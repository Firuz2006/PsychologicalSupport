using System.ComponentModel.DataAnnotations;

namespace PsychologicalSupport.Application.DTOs.Matching;

public record QuestionnaireSubmitDto(
    [Required] string Gender,
    [Required][Range(1, 120)] int Age,
    [Required] string PreferredLanguage,
    [Required] string MainIssue,
    [Required] string UrgencyLevel,
    [Required] string FormatPreference,
    string? AdditionalInfo,
    string? GuestSessionId
);

public record PsychologistMatchDto(
    Guid Id,
    string Name,
    string? PhotoPath,
    int ExperienceYears,
    decimal Price,
    List<string> Specializations,
    string MatchReason,
    int MatchScore
);

public record LlmMatchResult(
    Guid PsychologistId,
    int Score,
    string Reason
);
