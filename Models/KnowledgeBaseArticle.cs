using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportPulse.Api.Models
{
    [Table("knowledge_base")]
    public class KnowledgeBaseArticle
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("category")]
        public string Category { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}