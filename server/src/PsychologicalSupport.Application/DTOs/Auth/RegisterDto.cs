using System.ComponentModel.DataAnnotations;

namespace PsychologicalSupport.Application.DTOs.Auth;

public record RegisterDto(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password,
    string? FirstName,
    string? LastName,
    string Language = "ru"
);
