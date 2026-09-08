namespace SupportPulse.Api.DTOs;

public class CreateTicketDto
{
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IFormFile? AudioFile { get; set; }
    public IFormFile? Screenshot { get; set; }
}

public record TicketResponseDto(
    int Id,
    string Subject,
    string Description,
    string Status,
    string Priority,
    List<string> Files,
    DateTime CreatedAt);
