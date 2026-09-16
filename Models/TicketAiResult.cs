using System.ComponentModel.DataAnnotations.Schema;

namespace SupportPulse.Api.Models
{
    [Table("ticket_ai_results")]
    public class TicketAiResult
    {
        public int Id { get; set; }
        
        [Column("ticket_id")]
        public int TicketId { get; set; }
        
        [Column("transcription")]
        public string? Transcription { get; set; } // Yahan Audio ka text save hoga
        
        // Baaki fields Day 11 aur 12 ke liye:
        [Column("summary")]
        public string? Summary { get; set; }
        
        [Column("sentiment")]
        public string? Sentiment { get; set; }
    }
}