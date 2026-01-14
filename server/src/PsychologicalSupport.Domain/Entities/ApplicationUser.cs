using Microsoft.AspNetCore.Identity;

namespace PsychologicalSupport.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Language { get; set; } = "ru";
    public bool IsGuest { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Psychologist? Psychologist { get; set; }
    public ICollection<Session> ClientSessions { get; set; } = [];
    public ICollection<QuestionnaireResponse> QuestionnaireResponses { get; set; } = [];
}
