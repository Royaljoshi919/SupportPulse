using System.Text.Json.Serialization;

namespace SupportPulse.Api.DTOs
{
    public class TicketClassificationResult
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } = "General";

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "Medium";

        [JsonPropertyName("sentiment")]
        public string Sentiment { get; set; } = "Neutral";
    }
}