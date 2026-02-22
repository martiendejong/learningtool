namespace LearningTool.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Content { get; set; }  // Full course content (markdown)
    public string? LearningPlan { get; set; }  // AI-generated learning plan (JSON/markdown)
    public string? SystemPrompt { get; set; }  // AI-generated course-specific teaching prompt
    public int EstimatedMinutes { get; set; }
    public List<string> Prerequisites { get; set; } = new();  // Course IDs as strings
    public List<ResourceLink> ResourceLinks { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ContentGeneratedAt { get; set; }  // When AI content was generated

    // Navigation properties
    public Topic Topic { get; set; } = null!;
}

public class ResourceLink
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
}

public enum ResourceType
{
    YouTube,
    Documentation,
    Tutorial,
    Article,
    Book
}
