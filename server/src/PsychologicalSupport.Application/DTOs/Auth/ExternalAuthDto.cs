using System.ComponentModel.DataAnnotations;

namespace PsychologicalSupport.Application.DTOs.Auth;

public record GoogleAuthDto([Required] string IdToken, string Language = "ru");

public record AppleAuthDto([Required] string AuthCode, string? FirstName, string? LastName, string Language = "ru");
