namespace LearningTool.Domain.Entities;

public class Topic
{
    public int Id { get; set; }
    public int SkillId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Skill Skill { get; set; } = null!;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
