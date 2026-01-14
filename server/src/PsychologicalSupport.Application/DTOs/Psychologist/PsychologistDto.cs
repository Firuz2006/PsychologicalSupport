namespace PsychologicalSupport.Application.DTOs.Psychologist;

public record PsychologistDto(
    Guid Id,
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? PhotoPath,
    int ExperienceYears,
    string? Education,
    string? ApproachDescription,
    List<string> Languages,
    List<string> WorkFormats,
    decimal PricePerSession,
    string? MeetingLink,
    bool IsVerified,
    List<SpecializationDto> Specializations
);

public record SpecializationDto(int Id, string Key, string Name);

public record CreatePsychologistProfileDto(
    string? Education,
    string? ApproachDescription,
    List<string> Languages,
    List<string> WorkFormats,
    decimal PricePerSession,
    string? MeetingLink,
    List<int> SpecializationIds
);

public record UpdatePsychologistProfileDto(
    string? Education,
    string? ApproachDescription,
    List<string> Languages,
    List<string> WorkFormats,
    decimal PricePerSession,
    string? MeetingLink,
    List<int> SpecializationIds
);

public record PsychologistFilterDto(
    string? Language,
    string? WorkFormat,
    List<int>? SpecializationIds,
    decimal? MaxPrice,
    bool OnlyVerified = true,
    int Page = 1,
    int PageSize = 10
);
