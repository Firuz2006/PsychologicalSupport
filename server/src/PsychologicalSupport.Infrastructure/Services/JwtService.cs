using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PsychologicalSupport.Application.Interfaces;
using PsychologicalSupport.Domain.Entities;

namespace PsychologicalSupport.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new("language", user.Language),
            new("isGuest", user.IsGuest.ToString().ToLower())
        };

        if (user.FirstName is not null)
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "1440");

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
