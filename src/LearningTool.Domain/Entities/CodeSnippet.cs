using System.Text.Json.Serialization;

namespace LearningTool.Domain.Entities;

public class CodeSnippet
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = "plaintext";
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    [JsonIgnore]
    public Course? Course { get; set; }
}
