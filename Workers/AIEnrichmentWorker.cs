using Microsoft.EntityFrameworkCore;
using SupportPulse.Api.Data;
using SupportPulse.Api.Models; // NAYA: TicketAiResult model use karne ke liye
using System.Net.Http;         // NAYA: HttpClient use karne ke liye

namespace SupportPulse.Api.Workers;

public class AIEnrichmentWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AIEnrichmentWorker> _logger;
    private readonly IConfiguration _configuration; // NAYA: appsettings.json read karne ke liye
    private readonly HttpClient _httpClient;       // NAYA: OpenAI API call karne ke liye

    // NAYA: Constructor update kiya gaya hai
    public AIEnrichmentWorker(
        IServiceScopeFactory scopeFactory, 
        ILogger<AIEnrichmentWorker> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 AI Enrichment Background Worker start ho gaya hai.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // 1. Database se sabse purana 'PENDING' job nikalein
                    var job = await dbContext.AiJobs
                        .Where(j => j.Status == "PENDING")
                        .OrderBy(j => j.CreatedAt)
                        .FirstOrDefaultAsync(stoppingToken);

                    if (job != null)
                    {
                        _logger.LogInformation($"[Job #{job.Id}] Processing start hui Ticket ID: {job.TicketId} ke liye.");

                        // 2. Status 'PROCESSING' mark karein (Job Lock)
                        job.Status = "PROCESSING";
                        job.UpdatedAt = DateTime.UtcNow;
                        await dbContext.SaveChangesAsync(stoppingToken);

                        // -------------------------------------------------------------
                        // 3. DAY 10 NAYA CODE: AI Processing (Audio -> Text)
                        // -------------------------------------------------------------
                        
                        // API key read kar rahe hain
                        var apiKey = _configuration["OpenAISettings:ApiKey"];

                        // Yahan hum 3 second ka wait simulate kar rahe hain (API call ka time)
                        await Task.Delay(3000, stoppingToken);
                        
                        // Abhi ke liye Mock text save kar rahe hain (Baad mein yahan real API ka code aayega)
                        string mockTranscription = "Yeh ek test transcription hai jo Whisper API se aayegi.";

                        var aiResult = new TicketAiResult
                        {
                            TicketId = job.TicketId,
                            Transcription = mockTranscription
                        };
                        dbContext.TicketAiResults.Add(aiResult);
                        // -------------------------------------------------------------

                        // 4. Status 'COMPLETED' mark karein aur changes save karein
                        job.Status = "COMPLETED";
                        job.UpdatedAt = DateTime.UtcNow;
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation($"[Job #{job.Id}] Ticket ID: {job.TicketId} successfully complete aur transcribe ho gaya!");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aayi AI Worker job process karte waqt.");
            }

            // Har 5 second baad agla check karein
            await Task.Delay(5000, stoppingToken);
        }
    }
}