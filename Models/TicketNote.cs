using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportPulse.Api.Models
{
    [Table("ticket_notes")]
    public class TicketNote
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("ticket_id")]
        public int TicketId { get; set; }

        [Required]
        [Column("agent_id")]
        public int AgentId { get; set; }

        [Required]
        [Column("note")]
        public string Note { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("TicketId")]
        public Ticket? Ticket { get; set; }

        [ForeignKey("AgentId")]
        public User? Agent { get; set; }
    }
}