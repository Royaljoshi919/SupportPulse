using System.ComponentModel.DataAnnotations.Schema;

namespace SupportPulse.Api.Models
{
    [Table("audit_logs")]
    public class AuditLog
    {
        public int Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("entity")]
        public string Entity { get; set; } = string.Empty;

        [Column("entity_id")]
        public string? EntityId { get; set; }

        [Column("old_value")]
        public string? OldValue { get; set; }

        [Column("new_value")]
        public string? NewValue { get; set; }

        [Column("correlation_id")]
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

        [Column("created_at")] // ---> Yeh database ke exact `created_at` column se map karega
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}