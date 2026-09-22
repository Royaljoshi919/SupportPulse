using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportPulse.Api.Models;

[Table("ticket_ai_results")]
public class TicketAiResult
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ticket_id")]
    public int TicketId { get; set; }

    [Column("transcription", TypeName = "text")]
    public string? Transcription { get; set; }

    [Column("image_description", TypeName = "text")]
    public string? ImageDescription { get; set; }

    // 👇 YAHAN ADD KARNA HAI SUGGESTED RESPONSE 👇
    [Column("suggested_response", TypeName = "text")]
    public string? SuggestedResponse { get; set; }
}