using SupportPulse.Api.DTOs;
using SupportPulse.Api.Models;
using SupportPulse.Api.Repositories;

namespace SupportPulse.Api.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _repository;
    private readonly IWebHostEnvironment _env;

    public TicketService(ITicketRepository repository, IWebHostEnvironment env)
    {
        _repository = repository;
        _env = env;
    }

    public async Task<TicketResponseDto> CreateTicketAsync(int userId, CreateTicketDto dto)
    {
        // File validations (already implemented)
        var ticket = new Ticket
        {
            UserId = userId,
            Subject = dto.Subject,
            Description = dto.Description,
            Status = TicketStatus.OPEN,
            Priority = TicketPriority.MEDIUM
        };

        var createdTicket = await _repository.CreateTicketAsync(ticket);
        var filePaths = new List<string>();

        var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        if (dto.AudioFile != null && dto.AudioFile.Length > 0)
        {
            var path = await SaveFileAsync(dto.AudioFile, uploadsFolder);
            await _repository.AddTicketFileAsync(new TicketFile { TicketId = createdTicket.Id, FilePath = path, FileType = FileType.AUDIO });
            filePaths.Add(path);
        }

        if (dto.Screenshot != null && dto.Screenshot.Length > 0)
        {
            var path = await SaveFileAsync(dto.Screenshot, uploadsFolder);
            await _repository.AddTicketFileAsync(new TicketFile { TicketId = createdTicket.Id, FilePath = path, FileType = FileType.IMAGE });
            filePaths.Add(path);
        }

        return MapToDto(createdTicket, filePaths);
    }

    // --- DAY 4: BOLA / IDOR Protection for Ticket Retrieval ---
    public async Task<TicketResponseDto?> GetTicketAsync(int ticketId, int currentUserId, string currentUserRole)
    {
        var ticket = await _repository.GetTicketByIdAsync(ticketId);
        if (ticket == null) return null;

        // If user is a CUSTOMER, verify ownership
        if (currentUserRole == "CUSTOMER" && ticket.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("ACCESS_DENIED_BOLA"); // Handled as 403 Forbidden
        }

        var filePaths = ticket.Files.Select(f => f.FilePath).ToList();
        return MapToDto(ticket, filePaths);
    }

    // --- DAY 4: Status State Machine Validation ---
    public async Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, string currentUserRole)
    {
        var ticket = await _repository.GetTicketByIdAsync(ticketId);
        if (ticket == null) return false;

        // Validate State Machine Transitions
        bool isValidTransition = (ticket.Status, newStatus) switch
        {
            (TicketStatus.OPEN, TicketStatus.IN_PROGRESS) => true,
            (TicketStatus.IN_PROGRESS, TicketStatus.RESOLVED) => true,
            (TicketStatus.RESOLVED, TicketStatus.CLOSED) => true,
            _ => false
        };

        if (!isValidTransition)
        {
            throw new InvalidOperationException($"Invalid status transition from {ticket.Status} to {newStatus}.");
        }

        ticket.Status = newStatus;
        await _repository.UpdateTicketAsync(ticket);
        return true;
    }

    private async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(folder, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return filePath;
    }

    private TicketResponseDto MapToDto(Ticket ticket, List<string> filePaths)
    {
        return new TicketResponseDto(
            ticket.Id, 
            ticket.Subject, 
            ticket.Description, 
            ticket.Status.ToString(), 
            ticket.Priority.ToString(), 
            filePaths, 
            ticket.CreatedAt
        );
    }
}