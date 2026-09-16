using System.ComponentModel.DataAnnotations.Schema;

namespace SupportPulse.Api.Models
{
    [Table("ai_jobs")]
    public class AiJob
    {
        public int Id { get; set; }

        [Column("ticket_id")]
        public int TicketId { get; set; }

        [Column("status")]
        public string Status { get; set; } = "PENDING";

        [Column("attempt_count")]
        public int AttemptCount { get; set; } = 0;

        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}