namespace LearningTool.Domain.Entities;

public class UserCourse
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public UserCourseStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? AssessmentScore { get; set; }  // 0-100
    public int MinutesSpent { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Course Course { get; set; } = null!;
}

public enum UserCourseStatus
{
    NotStarted,
    InProgress,
    Completed
}
