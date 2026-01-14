using System.ComponentModel.DataAnnotations;

namespace PsychologicalSupport.Application.DTOs.Auth;

public record LoginDto(
    [Required][EmailAddress] string Email,
    [Required] string Password
);
