using Microsoft.EntityFrameworkCore;
using SupportPulse.Api.Data;
using SupportPulse.Api.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SupportPulse.Api.Workers;

public class AIEnrichmentWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AIEnrichmentWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AIEnrichmentWorker(
        IServiceScopeFactory scopeFactory, 
        ILogger<AIEnrichmentWorker> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 AI Enrichment Background Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // 1. Database se 'PENDING' job nikalein
                    var job = await dbContext.AiJobs
                        .Where(j => j.Status == "PENDING")
                        .OrderBy(j => j.CreatedAt)
                        .FirstOrDefaultAsync(stoppingToken);

                    if (job != null)
                    {
                        _logger.LogInformation($"[Job #{job.Id}] Processing started for Ticket ID: {job.TicketId}.");

                        // 2. Status 'PROCESSING' mark karein
                        job.Status = "PROCESSING";
                        job.UpdatedAt = DateTime.UtcNow;
                        await dbContext.SaveChangesAsync(stoppingToken);

                        // -------------------------------------------------------------
                        // 3. Groq API Processing
                        // -------------------------------------------------------------
                        
                        var apiKey = _configuration["OpenAISettings:ApiKey"];
                        if (string.IsNullOrEmpty(apiKey))
                        {
                            throw new Exception("API Key appsettings.json mein nahi mili!");
                        }

                        // DB se Ticket aur uski related Files load karein
                        var ticket = await dbContext.Tickets
                            .Include(t => t.Files)
                            .FirstOrDefaultAsync(t => t.Id == job.TicketId, stoppingToken);

                        // Audio file filter karein (.mp3 ya .wav extension ke basis par)
                        var audioFile = ticket?.Files?.FirstOrDefault(f => 
                            f.FilePath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || 
                            f.FilePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
                        );

                        if (audioFile == null || string.IsNullOrEmpty(audioFile.FilePath))
                        {
                            _logger.LogWarning($"[Job #{job.Id}] Ticket #{job.TicketId} ke liye koi Audio file nahi mili.");
                            job.Status = "FAILED";
                            job.ErrorMessage = "Ticket ke sath koi valid audio file associated nahi hai.";
                            job.UpdatedAt = DateTime.UtcNow;
                            await dbContext.SaveChangesAsync(stoppingToken);
                            continue;
                        }

                        // Hardcoded path hatakar Database se dynamic path use kiya gaya hai
                        string audioFilePath = audioFile.FilePath;

                        if (!File.Exists(audioFilePath))
                        {
                            throw new FileNotFoundException($"File disk par nahi mili at path: {audioFilePath}");
                        }

                        string extractedTranscription = "";

                        try
                        {
                            var client = _httpClientFactory.CreateClient();
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                            using var content = new MultipartFormDataContent();
                            
                            // Groq Whisper Model
                            content.Add(new StringContent("whisper-large-v3"), "model");

                            // Audio File Add Karein with Proper Content-Type
                            byte[] audioBytes = await File.ReadAllBytesAsync(audioFilePath, stoppingToken);
                            var audioContent = new ByteArrayContent(audioBytes);
                            audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/mpeg");
                            
                            // Path.GetFileName pass karna bahut zaroori hai
                            content.Add(audioContent, "file", Path.GetFileName(audioFilePath));

                            _logger.LogInformation($"Groq API ko request SENT for File: {Path.GetFileName(audioFilePath)} (Size: {audioBytes.Length} bytes)");

                            var response = await client.PostAsync("https://api.groq.com/openai/v1/audio/transcriptions", content, stoppingToken);

                            if (response.IsSuccessStatusCode)
                            {
                                var responseString = await response.Content.ReadAsStringAsync(stoppingToken);
                                using var jsonDoc = JsonDocument.Parse(responseString);
                                extractedTranscription = jsonDoc.RootElement.GetProperty("text").GetString() ?? "No text found.";
                            }
                            else
                            {
                                var errorMsg = await response.Content.ReadAsStringAsync(stoppingToken);
                                throw new Exception($"Groq API Error: {response.StatusCode} - {errorMsg}");
                            }

                            // 4. DB mein result save karein (Sirf success hone par yahan aayega)
                            var aiResult = new TicketAiResult
                            {
                                TicketId = job.TicketId,
                                Transcription = extractedTranscription
                            };
                            dbContext.TicketAiResults.Add(aiResult);

                            // 5. Status 'COMPLETED' mark karein
                            job.Status = "COMPLETED";
                            job.ErrorMessage = null;
                            job.UpdatedAt = DateTime.UtcNow;
                            await dbContext.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation($"[Job #{job.Id}] Ticket ID: {job.TicketId} successfully completed! Transcription saved.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"[Job #{job.Id}] Groq API calling mein fail ho gayi: {ex.Message}");
                            
                            job.Status = "FAILED";
                            job.ErrorMessage = ex.Message;
                            job.UpdatedAt = DateTime.UtcNow;
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aayi AI Worker job process karte waqt.");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}