namespace SupportPulse.Api.Models;

public enum TicketStatus
{
    OPEN,
    IN_PROGRESS,
    RESOLVED,
    CLOSED
}

public enum TicketPriority
{
    LOW,
    MEDIUM,
    HIGH,
    URGENT
}

public enum FileType
{
    AUDIO,
    IMAGE
}

public class Ticket
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.OPEN;
    public TicketPriority Priority { get; set; } = TicketPriority.MEDIUM;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<TicketFile> Files { get; set; } = new();
     public string? Category { get; set; }
    public string? Sentiment { get; set; }
}

public class TicketFile
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public Ticket Ticket { get; set; } = null!;
}

