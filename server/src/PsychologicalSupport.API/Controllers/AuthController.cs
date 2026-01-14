using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsychologicalSupport.Application.DTOs.Auth;
using PsychologicalSupport.Application.Interfaces;

namespace PsychologicalSupport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { token = result.Token, user = result.User });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (!result.Success)
            return Unauthorized(new { errors = result.Errors });

        return Ok(new { token = result.Token, user = result.User });
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto dto)
    {
        var result = await _authService.GoogleLoginAsync(dto);
        if (!result.Success)
            return Unauthorized(new { errors = result.Errors });

        return Ok(new { token = result.Token, user = result.User });
    }

    [HttpPost("apple")]
    public async Task<IActionResult> AppleLogin([FromBody] AppleAuthDto dto)
    {
        var result = await _authService.AppleLoginAsync(dto);
        if (!result.Success)
            return Unauthorized(new { errors = result.Errors });

        return Ok(new { token = result.Token, user = result.User });
    }

    [HttpPost("guest")]
    public async Task<IActionResult> GuestLogin([FromQuery] string language = "ru")
    {
        var result = await _authService.GuestLoginAsync(language);
        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { token = result.Token, user = result.User });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);

        return Ok(new { userId, email, roles });
    }
}
