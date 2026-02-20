namespace LearningTool.Domain.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
