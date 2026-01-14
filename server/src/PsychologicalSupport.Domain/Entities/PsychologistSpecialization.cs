namespace PsychologicalSupport.Domain.Entities;

public class PsychologistSpecialization
{
    public Guid PsychologistId { get; set; }
    public Psychologist Psychologist { get; set; } = null!;

    public int SpecializationId { get; set; }
    public Specialization Specialization { get; set; } = null!;
}
