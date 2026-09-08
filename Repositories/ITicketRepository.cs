using SupportPulse.Api.Models;

namespace SupportPulse.Api.Repositories;

public interface ITicketRepository
{
    Task<Ticket> CreateTicketAsync(Ticket ticket);
    Task<Ticket?> GetTicketByIdAsync(int id);
    Task AddTicketFileAsync(TicketFile file);
    Task UpdateTicketAsync(Ticket ticket);
}