using System.ComponentModel.DataAnnotations;

namespace SupportPulse.Api.DTOs
{
    public class UpdateArticleDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;
    }
}