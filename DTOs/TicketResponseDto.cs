namespace SupportPulse.Api.DTOs
{
    public class TicketResponseDTos
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "OPEN";
        public string AiStatus { get; set; } = "PENDING"; // Day 8 AI Job Queue Status
        public DateTime CreatedAt { get; set; }
    }
}