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
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public TicketController(ITicketService ticketService, ApplicationDbContext context, IAuditService auditService)
    {
        _ticketService = ticketService;
        _context = context;
        _auditService = auditService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateTicket([FromForm] CreateTicketDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        int currentUserId = int.Parse(userIdClaim);
        
        // 1. Ticket database mein save hoga
        var response = await _ticketService.CreateTicketAsync(currentUserId, dto);

        // -----------------------------------------------------------------
        // ---> DAY 8: AI Job Entry (PENDING status ke sath DB queue mein)
        // -----------------------------------------------------------------
        var aiJob = new AiJob
        {
            TicketId = response.Id,
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow
        };

        _context.AiJobs.Add(aiJob);
        await _context.SaveChangesAsync();
        // -----------------------------------------------------------------

        // 2. Ticket creation ko audit log mein save karein
        await _auditService.LogAsync(currentUserId, "TICKET_CREATED", "Ticket", response.Id.ToString());

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
            await _auditService.LogAsync(currentUserId, "UNAUTHORIZED_TICKET_ACCESS", "Ticket", id.ToString());
            
            return StatusCode(403, new { message = "Forbidden: You do not own this ticket." });
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "AGENT,ADMIN")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] TicketStatus newStatus)
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "AGENT";
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        int currentUserId = int.Parse(userIdClaim);

        try
        {
            await _ticketService.UpdateTicketStatusAsync(id, newStatus, userRole);
            
            await _auditService.LogAsync(currentUserId, "TICKET_STATUS_CHANGED", "Ticket", id.ToString(), null, newStatus.ToString());
            
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

        // 3. Category Filter
        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            query = query.Where(t => t.Category == parameters.Category);
        }

        // 4. Sentiment Filter
        if (!string.IsNullOrWhiteSpace(parameters.Sentiment))
        {
            query = query.Where(t => t.Sentiment == parameters.Sentiment);
        }

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

    /// <summary>
    /// Adds an internal note to a ticket (AGENT and ADMIN only).
    /// Endpoint: POST /api/v1/tickets/{id}/notes
    /// </summary>
    [HttpPost("{id}/notes")]
    [Authorize(Roles = "AGENT,ADMIN")]
    public async Task<IActionResult> AddInternalNote(int id, [FromBody] CreateNoteDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1. Check if Ticket exists
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound(new { message = $"Ticket with ID {id} not found." });
        }

        // 2. Extract Agent ID from Claims
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        int currentUserId = int.Parse(userIdClaim);

        // 3. Create Note Entity
        var noteEntity = new TicketNote
        {
            TicketId = id,
            AgentId = currentUserId,
            Note = dto.Note,
            CreatedAt = DateTime.UtcNow
        };

        _context.TicketNotes.Add(noteEntity);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(currentUserId, "INTERNAL_NOTE_ADDED", "TicketNote", noteEntity.Id.ToString(), null, "Note added to Ticket ID: " + id);

        // 4. Return DTO Response
        var response = new NoteResponseDto
        {
            Id = noteEntity.Id,
            TicketId = noteEntity.TicketId,
            AgentId = noteEntity.AgentId,
            Note = noteEntity.Note,
            CreatedAt = noteEntity.CreatedAt
        };

        return CreatedAtAction(nameof(GetTicketById), new { id = noteEntity.TicketId }, response);
    }
}