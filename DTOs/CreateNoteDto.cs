using System.ComponentModel.DataAnnotations;

namespace SupportPulse.Api.DTOs
{
    public class CreateNoteDto
    {
        [Required(ErrorMessage = "Note content is required.")]
        public string Note { get; set; } = string.Empty;
    }
}