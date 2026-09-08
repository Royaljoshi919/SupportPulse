using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportPulse.Api.DTOs;
using SupportPulse.Api.Models;
using SupportPulse.Api.Services;

namespace SupportPulse.Api.Controllers;

[ApiController]
[Route("api/v1/tickets")]
[Authorize]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
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
        // Extract user identity and role from JWT token for BOLA check
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
}