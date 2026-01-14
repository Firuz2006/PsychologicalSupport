namespace PsychologicalSupport.Domain.Entities;

public class QuestionnaireResponse
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string? GuestSessionId { get; set; }

    public string Gender { get; set; } = null!;
    public int Age { get; set; }
    public string PreferredLanguage { get; set; } = null!;
    public string MainIssue { get; set; } = null!;
    public string UrgencyLevel { get; set; } = null!; // low, medium, high
    public string FormatPreference { get; set; } = null!; // online, offline, any
    public string? AdditionalInfo { get; set; }

    public string? LlmRecommendation { get; set; } // JSON with ranked psychologist IDs
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
