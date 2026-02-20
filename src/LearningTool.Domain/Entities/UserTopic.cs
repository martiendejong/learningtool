namespace LearningTool.Domain.Entities;

public class UserTopic
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int TopicId { get; set; }
    public UserTopicStatus Status { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime AddedAt { get; set; }

    // Navigation properties
    public Topic Topic { get; set; } = null!;
}

public enum UserTopicStatus
{
    NotStarted,
    InProgress,
    Completed
}
