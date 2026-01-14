namespace PsychologicalSupport.Domain.Entities;

public class Specialization
{
    public int Id { get; set; }
    public string Key { get; set; } = null!;
    public string NameRu { get; set; } = null!;
    public string NameTj { get; set; } = null!;

    public ICollection<PsychologistSpecialization> Psychologists { get; set; } = [];
}
