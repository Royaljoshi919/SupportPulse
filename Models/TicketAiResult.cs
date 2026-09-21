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

    // 👇 YEH NAYI LINE ADD KAREIN 👇
    [Column("image_description", TypeName = "text")]
    public string? ImageDescription { get; set; }

    // (Agar Summary ya AiRemark jaisi aur properties hain toh unhe delete mat karna, waise hi rehne dena)
}