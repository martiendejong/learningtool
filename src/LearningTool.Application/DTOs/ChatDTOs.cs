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
    public LearningPathUpdate? LearningPathUpdate { get; set; }
}

public class LearningPathUpdate
{
    public List<CreatedSkill> CreatedSkills { get; set; } = new();
    public List<CreatedTopic> CreatedTopics { get; set; } = new();
    public List<CreatedCourse> CreatedCourses { get; set; } = new();
}

public class CreatedSkill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreatedTopic
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SkillId { get; set; }
}

public class CreatedCourse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
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
