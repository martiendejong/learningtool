namespace LearningTool.Domain.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;  // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
    public List<ToolCall>? ToolCalls { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ToolCall
{
    public string Name { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}
