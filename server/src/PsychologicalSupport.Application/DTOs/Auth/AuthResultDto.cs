namespace PsychologicalSupport.Application.DTOs.Auth;

public record AuthResultDto(
    bool Success,
    string? Token,
    UserDto? User,
    IEnumerable<string>? Errors = null
)
{
    public static AuthResultDto Successful(string token, UserDto user)
        => new(true, token, user);

    public static AuthResultDto Failed(IEnumerable<string> errors)
        => new(false, null, null, errors);

    public static AuthResultDto Failed(string error)
        => new(false, null, null, [error]);
}

public record UserDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string Language,
    bool IsGuest,
    IEnumerable<string> Roles
);
