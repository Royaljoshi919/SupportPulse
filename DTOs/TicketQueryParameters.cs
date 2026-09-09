namespace SupportPulse.Api.DTOs;

public class TicketQueryParameters
{
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Filters
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Sentiment { get; set; }

    // Text Search (Subject or Description)
    public string? SearchTerm { get; set; }
}