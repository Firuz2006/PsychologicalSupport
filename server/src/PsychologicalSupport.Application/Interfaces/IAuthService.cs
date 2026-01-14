using PsychologicalSupport.Application.DTOs.Auth;

namespace PsychologicalSupport.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto dto);
    Task<AuthResultDto> LoginAsync(LoginDto dto);
    Task<AuthResultDto> GoogleLoginAsync(GoogleAuthDto dto);
    Task<AuthResultDto> AppleLoginAsync(AppleAuthDto dto);
    Task<AuthResultDto> GuestLoginAsync(string language);
}
