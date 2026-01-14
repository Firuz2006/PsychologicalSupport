using PsychologicalSupport.Domain.Entities;

namespace PsychologicalSupport.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}
