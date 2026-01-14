using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PsychologicalSupport.Application.DTOs.Auth;
using PsychologicalSupport.Application.Interfaces;
using PsychologicalSupport.Domain.Entities;

namespace PsychologicalSupport.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            return AuthResultDto.Failed("User with this email already exists");

        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Language = dto.Language,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return AuthResultDto.Failed(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, "Client");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return AuthResultDto.Successful(token, MapToUserDto(user, roles));
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return AuthResultDto.Failed("Invalid email or password");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return AuthResultDto.Failed("Invalid email or password");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return AuthResultDto.Successful(token, MapToUserDto(user, roles));
    }

    public async Task<AuthResultDto> GoogleLoginAsync(GoogleAuthDto dto)
    {
        var payload = await ValidateGoogleTokenAsync(dto.IdToken);
        if (payload is null)
            return AuthResultDto.Failed("Invalid Google token");

        var user = await _userManager.FindByEmailAsync(payload.Email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = payload.Email,
                UserName = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                Language = dto.Language,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return AuthResultDto.Failed(result.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(user, "Client");
            await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", payload.Sub, "Google"));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return AuthResultDto.Successful(token, MapToUserDto(user, roles));
    }

    public async Task<AuthResultDto> AppleLoginAsync(AppleAuthDto dto)
    {
        // Apple Sign In requires server-side token exchange
        // For MVP, we'll validate the auth code and extract user info
        var appleUser = await ValidateAppleAuthCodeAsync(dto.AuthCode);
        if (appleUser is null)
            return AuthResultDto.Failed("Invalid Apple auth code");

        var user = await _userManager.FindByEmailAsync(appleUser.Email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = appleUser.Email,
                UserName = appleUser.Email,
                FirstName = dto.FirstName ?? appleUser.GivenName,
                LastName = dto.LastName ?? appleUser.FamilyName,
                Language = dto.Language,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return AuthResultDto.Failed(result.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(user, "Client");
            await _userManager.AddLoginAsync(user, new UserLoginInfo("Apple", appleUser.Sub, "Apple"));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return AuthResultDto.Successful(token, MapToUserDto(user, roles));
    }

    public async Task<AuthResultDto> GuestLoginAsync(string language)
    {
        var guestId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Email = $"guest_{guestId}@temp.local",
            UserName = $"guest_{guestId}",
            Language = language,
            IsGuest = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
            return AuthResultDto.Failed(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, "Client");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return AuthResultDto.Successful(token, MapToUserDto(user, roles));
    }

    private async Task<GoogleTokenPayload?> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");

            if (!response.IsSuccessStatusCode)
                return null;

            var payload = await response.Content.ReadFromJsonAsync<GoogleTokenPayload>();

            var clientId = _configuration["Authentication:Google:ClientId"];
            if (payload?.Aud != clientId)
                return null;

            return payload;
        }
        catch
        {
            return null;
        }
    }

    private async Task<AppleUserInfo?> ValidateAppleAuthCodeAsync(string authCode)
    {
        // Apple token validation requires JWT client secret generation
        // This is a simplified implementation for MVP
        try
        {
            var clientId = _configuration["Authentication:Apple:ClientId"];
            var teamId = _configuration["Authentication:Apple:TeamId"];
            var keyId = _configuration["Authentication:Apple:KeyId"];
            var privateKey = _configuration["Authentication:Apple:PrivateKey"];

            // Generate client secret JWT for Apple
            var clientSecret = GenerateAppleClientSecret(clientId!, teamId!, keyId!, privateKey!);

            var tokenRequest = new Dictionary<string, string>
            {
                ["client_id"] = clientId!,
                ["client_secret"] = clientSecret,
                ["code"] = authCode,
                ["grant_type"] = "authorization_code"
            };

            var response = await _httpClient.PostAsync(
                "https://appleid.apple.com/auth/token",
                new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
                return null;

            var tokenResponse = await response.Content.ReadFromJsonAsync<AppleTokenResponse>();
            if (tokenResponse?.IdToken is null)
                return null;

            // Decode the ID token to get user info
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenResponse.IdToken);

            return new AppleUserInfo
            {
                Sub = jwt.Subject,
                Email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "",
                GivenName = jwt.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value,
                FamilyName = jwt.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value
            };
        }
        catch
        {
            return null;
        }
    }

    private static string GenerateAppleClientSecret(string clientId, string teamId, string keyId, string privateKey)
    {
        // Apple requires ES256 signed JWT as client secret
        // This is a placeholder - in production use proper ECDSA signing
        var now = DateTime.UtcNow;
        var claims = new Dictionary<string, object>
        {
            ["iss"] = teamId,
            ["iat"] = new DateTimeOffset(now).ToUnixTimeSeconds(),
            ["exp"] = new DateTimeOffset(now.AddMonths(6)).ToUnixTimeSeconds(),
            ["aud"] = "https://appleid.apple.com",
            ["sub"] = clientId
        };

        // For full implementation, use System.Security.Cryptography.ECDsa
        // to sign the JWT with the private key
        return "";
    }

    private static UserDto MapToUserDto(ApplicationUser user, IList<string> roles) => new(
        user.Id,
        user.Email!,
        user.FirstName,
        user.LastName,
        user.Language,
        user.IsGuest,
        roles
    );

    private record GoogleTokenPayload
    {
        public string Sub { get; init; } = "";
        public string Email { get; init; } = "";
        public string? GivenName { get; init; }
        public string? FamilyName { get; init; }
        public string? Aud { get; init; }
    }

    private record AppleTokenResponse
    {
        public string? IdToken { get; init; }
    }

    private record AppleUserInfo
    {
        public string Sub { get; init; } = "";
        public string Email { get; init; } = "";
        public string? GivenName { get; init; }
        public string? FamilyName { get; init; }
    }
}
