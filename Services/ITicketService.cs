using SupportPulse.Api.DTOs;

namespace SupportPulse.Api.Services;

public interface ITicketService
{
    Task<TicketResponseDto> CreateTicketAsync(int userId, CreateTicketDto dto);
    Task<TicketResponseDto?> GetTicketAsync(int ticketId, int currentUserId, string currentUserRole);
    Task<bool> UpdateTicketStatusAsync(int ticketId, Models.TicketStatus newStatus, string currentUserRole);
}