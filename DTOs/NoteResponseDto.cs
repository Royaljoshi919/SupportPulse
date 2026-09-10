namespace SupportPulse.Api.DTOs
{
    public class NoteResponseDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int AgentId { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}   