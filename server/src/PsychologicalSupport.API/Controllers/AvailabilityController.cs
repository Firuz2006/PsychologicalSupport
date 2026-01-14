using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsychologicalSupport.Application.DTOs.Booking;
using PsychologicalSupport.Application.Interfaces;

namespace PsychologicalSupport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Psychologist")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;
    private readonly IPsychologistService _psychologistService;

    public AvailabilityController(IAvailabilityService availabilityService, IPsychologistService psychologistService)
    {
        _availabilityService = availabilityService;
        _psychologistService = psychologistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAvailability()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var psychologist = await _psychologistService.GetByUserIdAsync(userId);
        if (psychologist is null)
            return NotFound(new { error = "Psychologist profile not found" });

        var availability = await _availabilityService.GetPsychologistAvailabilityAsync(psychologist.Id);
        return Ok(availability);
    }

    [HttpGet("{psychologistId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPsychologistAvailability(Guid psychologistId)
    {
        var availability = await _availabilityService.GetPsychologistAvailabilityAsync(psychologistId);
        return Ok(availability);
    }

    [HttpPost]
    public async Task<IActionResult> SetAvailability([FromBody] SetAvailabilityDto dto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var psychologist = await _psychologistService.GetByUserIdAsync(userId);
        if (psychologist is null)
            return NotFound(new { error = "Psychologist profile not found" });

        var availability = await _availabilityService.SetAvailabilityAsync(psychologist.Id, dto);
        return Ok(availability);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveAvailability(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var psychologist = await _psychologistService.GetByUserIdAsync(userId);
        if (psychologist is null)
            return NotFound(new { error = "Psychologist profile not found" });

        var result = await _availabilityService.RemoveAvailabilityAsync(id, psychologist.Id);
        if (!result) return NotFound();
        return Ok(new { message = "Availability removed" });
    }
}
