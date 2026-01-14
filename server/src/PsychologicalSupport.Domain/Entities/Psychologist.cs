namespace PsychologicalSupport.Domain.Entities;

public class Psychologist
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string? PhotoPath { get; set; }
    public int ExperienceYears { get; set; }
    public string? Education { get; set; }
    public string? ApproachDescription { get; set; }
    public string Languages { get; set; } = "ru"; // comma-separated: "ru,tj,en"
    public string WorkFormats { get; set; } = "online"; // online,offline,both
    public decimal PricePerSession { get; set; }
    public string? MeetingLink { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<PsychologistSpecialization> Specializations { get; set; } = [];
    public ICollection<Availability> Availabilities { get; set; } = [];
    public ICollection<Session> Sessions { get; set; } = [];
}
