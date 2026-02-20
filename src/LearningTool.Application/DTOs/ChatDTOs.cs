namespace LearningTool.Application.DTOs;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public List<ToolCall>? ToolCalls { get; set; }
    public bool RequiresAction { get; set; }
}

public class ToolCall
{
    public string Id { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, object> Arguments { get; set; } = new();
}

public class ToolResult
{
    public string ToolCallId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Result { get; set; } = string.Empty;
    public object? Data { get; set; }
}
