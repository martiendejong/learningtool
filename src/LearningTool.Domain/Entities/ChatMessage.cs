namespace LearningTool.Domain.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;  // "user" or "assistant" or "system"
    public string Content { get; set; } = string.Empty;
    public string? ToolCalls { get; set; }  // JSON serialized tool calls
    public DateTime Timestamp { get; set; }
}
