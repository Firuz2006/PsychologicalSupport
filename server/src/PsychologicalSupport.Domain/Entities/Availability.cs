namespace PsychologicalSupport.Domain.Entities;

public class Availability
{
    public Guid Id { get; set; }
    public Guid PsychologistId { get; set; }
    public Psychologist Psychologist { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; } = 60;
}
