namespace PsychologicalSupport.Domain.Entities;

public class Article
{
    public Guid Id { get; set; }
    public string TitleRu { get; set; } = null!;
    public string TitleTj { get; set; } = null!;
    public string ContentRu { get; set; } = null!;
    public string ContentTj { get; set; } = null!;
    public string? ImagePath { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
