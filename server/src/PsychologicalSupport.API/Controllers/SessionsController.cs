using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsychologicalSupport.Application.DTOs.Booking;
using PsychologicalSupport.Application.Interfaces;

namespace PsychologicalSupport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IAvailabilityService _availabilityService;

    public SessionsController(IBookingService bookingService, IAvailabilityService availabilityService)
    {
        _bookingService = bookingService;
        _availabilityService = availabilityService;
    }

    [HttpGet("slots/{psychologistId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSlots(Guid psychologistId, [FromQuery] DateOnly date)
    {
        var slots = await _bookingService.GetAvailableSlotsAsync(psychologistId, date);
        return Ok(slots);
    }

    [HttpPost]
    public async Task<IActionResult> BookSession([FromBody] BookSessionDto dto)
    {
        var clientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        try
        {
            var session = await _bookingService.BookSessionAsync(clientId, dto);
            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSession(Guid id)
    {
        var session = await _bookingService.GetSessionByIdAsync(id);
        if (session is null) return NotFound();

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (session.ClientId != userId && session.PsychologistId != userId)
            return Forbid();

        return Ok(session);
    }

    [HttpGet("client")]
    public async Task<IActionResult> GetClientSessions()
    {
        var clientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var sessions = await _bookingService.GetClientSessionsAsync(clientId);
        return Ok(sessions);
    }

    [HttpGet("psychologist")]
    [Authorize(Roles = "Psychologist")]
    public async Task<IActionResult> GetPsychologistSessions()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        // Need to get psychologist ID from user ID - simplified, assumes 1:1 mapping
        var sessions = await _bookingService.GetPsychologistSessionsAsync(userId);
        return Ok(sessions);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelSession(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _bookingService.CancelSessionAsync(id, userId);
        if (!result) return NotFound();
        return Ok(new { message = "Session cancelled" });
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Roles = "Psychologist")]
    public async Task<IActionResult> ConfirmSession(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _bookingService.ConfirmSessionAsync(id, userId);
        if (!result) return NotFound();
        return Ok(new { message = "Session confirmed" });
    }
}
