using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsychologicalSupport.Application.DTOs.Psychologist;
using PsychologicalSupport.Application.Interfaces;

namespace PsychologicalSupport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PsychologistsController : ControllerBase
{
    private readonly IPsychologistService _psychologistService;

    public PsychologistsController(IPsychologistService psychologistService)
    {
        _psychologistService = psychologistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PsychologistFilterDto filter)
    {
        var psychologists = await _psychologistService.GetAllAsync(filter);
        return Ok(psychologists);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var psychologist = await _psychologistService.GetByIdAsync(id);
        if (psychologist is null) return NotFound();
        return Ok(psychologist);
    }

    [HttpGet("specializations")]
    public async Task<IActionResult> GetSpecializations([FromQuery] string language = "ru")
    {
        var specializations = await _psychologistService.GetAllSpecializationsAsync(language);
        return Ok(specializations);
    }

    [Authorize(Roles = "Psychologist")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var psychologist = await _psychologistService.GetByUserIdAsync(userId);
        if (psychologist is null) return NotFound();
        return Ok(psychologist);
    }

    [Authorize(Roles = "Psychologist")]
    [HttpPost("profile")]
    public async Task<IActionResult> CreateProfile([FromBody] CreatePsychologistProfileDto dto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // Check if profile already exists
        var existing = await _psychologistService.GetByUserIdAsync(userId);
        if (existing is not null)
            return BadRequest(new { error = "Profile already exists" });

        var psychologist = await _psychologistService.CreateProfileAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = psychologist.Id }, psychologist);
    }

    [Authorize(Roles = "Psychologist")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatePsychologistProfileDto dto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var existing = await _psychologistService.GetByUserIdAsync(userId);
        if (existing is null)
            return NotFound(new { error = "Profile not found" });

        var psychologist = await _psychologistService.UpdateProfileAsync(existing.Id, dto);
        return Ok(psychologist);
    }

    [Authorize(Roles = "Psychologist")]
    [HttpPost("photo")]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var existing = await _psychologistService.GetByUserIdAsync(userId);
        if (existing is null)
            return NotFound(new { error = "Profile not found" });

        await using var stream = file.OpenReadStream();
        var path = await _psychologistService.UploadPhotoAsync(existing.Id, stream, file.FileName);

        return Ok(new { photoPath = path });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/verify")]
    public async Task<IActionResult> VerifyPsychologist(Guid id, [FromQuery] bool verified = true)
    {
        var result = await _psychologistService.VerifyPsychologistAsync(id, verified);
        if (!result) return NotFound();
        return Ok(new { message = verified ? "Psychologist verified" : "Verification removed" });
    }
}
