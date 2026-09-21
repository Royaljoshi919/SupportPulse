using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupportPulse.Api.Data;
using SupportPulse.Api.DTOs;
using SupportPulse.Api.Models;

namespace SupportPulse.Api.Workers
{
    public class AiEnrichmentWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AiEnrichmentWorker> _logger;

        public AiEnrichmentWorker(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<AiEnrichmentWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 AI Enrichment Background Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // 1. Fetch pending job
                    var job = await dbContext.AiJobs
                        .Where(j => j.Status.ToUpper() == "PENDING")
                        .OrderBy(j => j.CreatedAt)
                        .FirstOrDefaultAsync(stoppingToken);

                    if (job != null)
                    {
                        _logger.LogInformation($"[Job #{job.Id}] Processing started for Ticket ID: {job.TicketId}.");

                        // Status 'PROCESSING' mark karein
                        job.Status = "PROCESSING";
                        job.UpdatedAt = DateTime.UtcNow;
                        await dbContext.SaveChangesAsync(stoppingToken);

                        try
                        {
                            var groqApiKey = _configuration["GroqSettings:ApiKey"] ?? _configuration["OpenAISettings:ApiKey"];
                            var geminiKey = _configuration["GeminiSettings:ApiKey"];

                            // Load Ticket and associated files
                            var ticket = await dbContext.Tickets
                                .Include(t => t.Files)
                                .FirstOrDefaultAsync(t => t.Id == job.TicketId, stoppingToken);

                            if (ticket == null)
                                throw new Exception($"Ticket ID #{job.TicketId} database mein nahi mila.");

                            var audioFile = ticket.Files?.FirstOrDefault(f => 
                                f.FilePath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || 
                                f.FilePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase));

                            var imageFile = ticket.Files?.FirstOrDefault(f => 
                                f.FilePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || 
                                f.FilePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                                f.FilePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));

                            var httpClient = _httpClientFactory.CreateClient();
                          httpClient.Timeout = TimeSpan.FromMinutes(5);
                           string? audioResultText = null;
                            string? visionResultText = null;

                            // ==========================================
                            // 🎧 SCENARIO 1: AUDIO FILE PROCESSING (GROQ)
                            // ==========================================
                            if (audioFile != null)
                            {
                                if (string.IsNullOrEmpty(groqApiKey))
                                    throw new InvalidOperationException("Groq API Key configuration mein missing hai.");

                                string fullAudioPath = ResolveFilePath(audioFile.FilePath);
                                if (!File.Exists(fullAudioPath))
                                    throw new FileNotFoundException($"Audio file disk par nahi mili: {fullAudioPath}");

                                _logger.LogInformation($"[Job #{job.Id}] Audio processing started: {Path.GetFileName(fullAudioPath)}");

                                using var content = new MultipartFormDataContent();
                                content.Add(new StringContent("whisper-large-v3"), "model");

                                byte[] audioBytes = await File.ReadAllBytesAsync(fullAudioPath, stoppingToken);
                                var audioContent = new ByteArrayContent(audioBytes);
                              // Dynamic MIME type setting
                                string audioExt = Path.GetExtension(fullAudioPath).ToLower();
                                string audioMime = audioExt == ".wav" ? "audio/wav" : "audio/mpeg";
                               
                                content.Add(audioContent, "file", Path.GetFileName(fullAudioPath));

                                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/audio/transcriptions")
                                {
                                    Content = content
                                };
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", groqApiKey);

                                var response = await httpClient.SendAsync(request, stoppingToken);
                                if (response.IsSuccessStatusCode)
                                {
                                    var jsonStr = await response.Content.ReadAsStringAsync(stoppingToken);
                                    using var jsonDoc = JsonDocument.Parse(jsonStr);
                                    audioResultText = jsonDoc.RootElement.GetProperty("text").GetString();
                                }
                                else
                                {
                                    var errStr = await response.Content.ReadAsStringAsync(stoppingToken);
                                    throw new Exception($"Groq Whisper API Error ({response.StatusCode}): {errStr}");
                                }
                            }

                            // ==========================================
                            // 🖼️ SCENARIO 2: IMAGE PROCESSING (GEMINI)
                            // ==========================================
                        if (imageFile != null)
{
    if (string.IsNullOrEmpty(geminiKey))
        throw new InvalidOperationException("Gemini API Key configuration mein missing hai.");

    string fullImagePath = ResolveFilePath(imageFile.FilePath);
    if (!File.Exists(fullImagePath))
        throw new FileNotFoundException($"Image file disk par nahi mili: {fullImagePath}");

    _logger.LogInformation($"[Job #{job.Id}] Gemini Vision analysis started: {Path.GetFileName(fullImagePath)}");

    byte[] imageBytes = await File.ReadAllBytesAsync(fullImagePath, stoppingToken);
    string base64Image = Convert.ToBase64String(imageBytes);

    string extension = Path.GetExtension(fullImagePath).Replace(".", "").ToLower();
    string mimeType = extension == "jpg" ? "image/jpeg" : $"image/{extension}";

    var payload = new
    {
        contents = new[]
        {
            new
            {
                parts = new object[]
                {
                    new { text = "Analyze this support ticket screenshot. Describe any visible errors, UI issues, or key messages in 2-3 concise sentences." },
                    new
                    {
                        inline_data = new
                        {
                            mime_type = mimeType,
                            data = base64Image
                        }
                    }
                }
            }
        }
    };

    var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
    
    // 🛠️ UPDATE 1: Yahan gemini-1.5-flash karna hai
//string geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent?key={geminiKey}";
string geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.8-flash:generateContent?key={geminiKey}";

    var response = await httpClient.PostAsync(geminiUrl, jsonContent, stoppingToken);
    if (response.IsSuccessStatusCode)
    {
        var responseString = await response.Content.ReadAsStringAsync(stoppingToken);
        using var jsonDoc = JsonDocument.Parse(responseString);
        visionResultText = jsonDoc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        // ⏳ UPDATE 2: Yahan 4 second ka wait lagana hai taaki 429 TooManyRequests error na aaye
        _logger.LogInformation($"[Job #{job.Id}] Image analysis done. Waiting 4 seconds for API cooldown...");
        await Task.Delay(4000, stoppingToken);
    }
    else
    {
        var errStr = await response.Content.ReadAsStringAsync(stoppingToken);
        throw new Exception($"Gemini Vision API Error ({response.StatusCode}): {errStr}");
    }
}

                            // Save raw AI results in TicketAiResults
                            if (!string.IsNullOrEmpty(audioResultText) || !string.IsNullOrEmpty(visionResultText))
                            {
                                var aiResult = new TicketAiResult
                                {
                                    TicketId = job.TicketId,
                                    Transcription = audioResultText,
                                    ImageDescription = visionResultText
                                };
                                dbContext.TicketAiResults.Add(aiResult);
                                await dbContext.SaveChangesAsync(stoppingToken);
                            }

                            // ==========================================
                            // 🏷️ STEP 3: FINAL CLASSIFICATION & DB UPDATE
                            // ==========================================
                            _logger.LogInformation($"[Job #{job.Id}] Final AI Classification started...");
                            
                            var classification = await ClassifyTicketAsync(
                                ticket.Description,
                                audioResultText,
                                visionResultText,
                                httpClient,
                                geminiKey,
                                stoppingToken
                            );

                            _logger.LogInformation($"[Job #{job.Id}] Classification Done: {classification.Category} | Priority: {classification.Priority} | Sentiment: {classification.Sentiment}");

                            // Update ticket & job in same EF tracking context
                            ticket.Category = classification.Category;
                            ticket.Priority = MapPriority(classification.Priority);
                            ticket.Sentiment = classification.Sentiment;

                            job.Status = "COMPLETED";
                            job.ErrorMessage = null;
                            job.UpdatedAt = DateTime.UtcNow;

                            await dbContext.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"[Job #{job.Id}] Ticket updated & Job COMPLETED successfully!");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"[Job #{job.Id}] Processing failed: {ex.Message}");
                            job.Status = "FAILED";
                            job.ErrorMessage = ex.Message;
                            job.UpdatedAt = DateTime.UtcNow;
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("AI Worker shutting down gracefully.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in AI Worker execution loop.");
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

        // Helper Method: AI Classification with Structured Output
        private async Task<TicketClassificationResult> ClassifyTicketAsync(
            string? ticketDescription,
            string? audioTranscription,
            string? imageDescription,
            HttpClient httpClient,
            string? geminiKey,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(geminiKey))
            {
                return new TicketClassificationResult { Category = "General", Priority = "Medium", Sentiment = "Neutral" };
            }

            string prompt = $$"""
            You are an expert IT support ticket classifier.
            Analyze the following support ticket details and classify it accurately.

            Ticket Description: {{ticketDescription ?? "N/A"}}
            Audio Transcription: {{audioTranscription ?? "N/A"}}
            Image Description: {{imageDescription ?? "N/A"}}

            Respond ONLY with a valid JSON object matching this exact structure:
            {
                "category": "Technical | Billing | Account | General",
                "priority": "Low | Medium | High | Critical",
                "sentiment": "Positive | Neutral | Negative | Frustrated"
            }
            """;

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                },
                generationConfig = new
                {
                    response_mime_type = "application/json"
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            
            // Updated to gemini-3.6-flash
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={geminiKey}";

            var response = await httpClient.PostAsync(url, jsonContent, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Classification call failed ({response.StatusCode}). Using fallback defaults.");
                return new TicketClassificationResult { Category = "General", Priority = "Medium", Sentiment = "Neutral" };
            }

            var responseStr = await response.Content.ReadAsStringAsync(cancellationToken);
            using var jsonDoc = JsonDocument.Parse(responseStr);

            var rawText = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(rawText))
                return new TicketClassificationResult { Category = "General", Priority = "Medium", Sentiment = "Neutral" };

            var result = JsonSerializer.Deserialize<TicketClassificationResult>(rawText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new TicketClassificationResult { Category = "General", Priority = "Medium", Sentiment = "Neutral" };
        }

        // Helper Method: File path resolution
        private static string ResolveFilePath(string relativePath)
        {
            if (Path.IsPathRooted(relativePath))
                return relativePath;

            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/', '\\'));
        }

        private static TicketPriority MapPriority(string? priority)
        {
            if (string.Equals(priority, "Critical", StringComparison.OrdinalIgnoreCase))
                return TicketPriority.URGENT;

            return Enum.TryParse<TicketPriority>(priority, true, out var parsedPriority)
                ? parsedPriority
                : TicketPriority.MEDIUM;
        }
    }
}