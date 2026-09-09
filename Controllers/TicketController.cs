using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportPulse.Api.DTOs;
using SupportPulse.Api.Models;
using SupportPulse.Api.Services;
using Microsoft.EntityFrameworkCore;
using SupportPulse.Api.Data;

namespace SupportPulse.Api.Controllers;

[ApiController]
[Route("api/v1/tickets")]
[Authorize]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ApplicationDbContext _context; // 🟢 Change 1: Field add kiya

    // 🟢 Change 2: Constructor mein ApplicationDbContext inject kiya
    public TicketController(ITicketService ticketService, ApplicationDbContext context)
    {
        _ticketService = ticketService;
        _context = context;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateTicket([FromForm] CreateTicketDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        var response = await _ticketService.CreateTicketAsync(int.Parse(userIdClaim), dto);
        return CreatedAtAction(nameof(GetTicketById), new { id = response.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicketById(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "CUSTOMER";

        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        int currentUserId = int.Parse(userIdClaim);

        try
        {
            var response = await _ticketService.GetTicketAsync(id, currentUserId, userRole);
            if (response == null) return NotFound(new { message = "Ticket not found." });
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new { message = "Forbidden: You do not own this ticket." });
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "AGENT,ADMIN")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] TicketStatus newStatus)
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "AGENT";

        try
        {
            await _ticketService.UpdateTicketStatusAsync(id, newStatus, userRole);
            return Ok(new { message = "Ticket status updated successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("agent/tickets")]
    [Authorize(Roles = "AGENT,ADMIN")]
    public async Task<IActionResult> GetAgentTickets([FromQuery] TicketQueryParameters parameters)
    {
        // 🟢 Change 3: context ki jagah '_context' kiya
        var query = _context.Tickets.AsQueryable();

        // 1. Status Filter
        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            query = query.Where(t => t.Status.ToString() == parameters.Status);
        }

        // 2. Priority Filter
        if (!string.IsNullOrWhiteSpace(parameters.Priority))
        {
            query = query.Where(t => t.Priority.ToString() == parameters.Priority);
        }

        // // 3. Category Filter
        // if (!string.IsNullOrWhiteSpace(parameters.Category))
        // {
        //     query = query.Where(t => t.Category == parameters.Category);
        // }

        // // 4. Sentiment Filter
        // if (!string.IsNullOrWhiteSpace(parameters.Sentiment))
        // {
        //     query = query.Where(t => t.Sentiment == parameters.Sentiment);
        // }

        // 5. Text Search (Subject or Description)
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.ToLower();
            query = query.Where(t => t.Subject.ToLower().Contains(term) || 
                                     t.Description.ToLower().Contains(term));
        }

        var totalRecords = await query.CountAsync();

        // 6. Pagination (Skip & Take)
        var tickets = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return Ok(new
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)parameters.PageSize),
            Data = tickets
        });
    }
}