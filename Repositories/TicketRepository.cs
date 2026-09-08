using Microsoft.EntityFrameworkCore;
using SupportPulse.Api.Data;
using SupportPulse.Api.Models;

namespace SupportPulse.Api.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public TicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        return await _context.Tickets
            .Include(t => t.Files)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddTicketFileAsync(TicketFile file)
    {
        _context.TicketFiles.Add(file);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTicketAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }
}