using PsychologicalSupport.Domain.Enums;

namespace PsychologicalSupport.Domain.Entities;

public class Session
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }
    public ApplicationUser Client { get; set; } = null!;

    public Guid PsychologistId { get; set; }
    public Psychologist Psychologist { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Pending;
    public string? MeetingLink { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
