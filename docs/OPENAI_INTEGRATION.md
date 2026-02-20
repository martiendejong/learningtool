# OpenAI Integration Guide

## Overview

This document explains how to integrate OpenAI's GPT-4 API with the LearningTool chat system for advanced AI-powered conversations.

## Configuration

### 1. Get OpenAI API Key

1. Go to https://platform.openai.com/
2. Sign in or create account
3. Navigate to API Keys section
4. Create new secret key
5. Copy the key (starts with `sk-`)

### 2. Configure Application

**Add to `appsettings.json`:**
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here",
    "Model": "gpt-4",
    "MaxTokens": 500,
    "Temperature": 0.7,
    "SystemPrompt": "You are a helpful AI learning assistant. Your role is to help users learn new skills by adding them to their learning path, suggesting topics and courses, and answering questions about their progress."
  }
}
```

**For production, use environment variables:**
```bash
export OPENAI__APIKEY="sk-..."
```

Or in `appsettings.Production.json`:
```json
{
  "OpenAI": {
    "ApiKey": "${OPENAI_API_KEY}"
  }
}
```

## Implementation

### Step 1: Create OpenAI Service

Create `src/LearningTool.Application/Services/OpenAIChatService.cs`:

```csharp
using System.Text;
using System.Text.Json;
using LearningTool.Application.DTOs;
using Microsoft.Extensions.Configuration;

namespace LearningTool.Application.Services;

public class OpenAIChatService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly int _maxTokens;
    private readonly double _temperature;
    private readonly string _systemPrompt;

    public OpenAIChatService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _apiKey = configuration["OpenAI:ApiKey"]!;
        _model = configuration["OpenAI:Model"] ?? "gpt-4";
        _maxTokens = int.Parse(configuration["OpenAI:MaxTokens"] ?? "500");
        _temperature = double.Parse(configuration["OpenAI:Temperature"] ?? "0.7");
        _systemPrompt = configuration["OpenAI:SystemPrompt"] ??
            "You are a helpful AI learning assistant.";
    }

    public async Task<OpenAIResponse> GetCompletionAsync(
        List<ChatMessage> conversationHistory,
        string userMessage)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        // Build messages array
        var messages = new List<object>
        {
            new { role = "system", content = _systemPrompt }
        };

        // Add conversation history
        foreach (var msg in conversationHistory.TakeLast(10))
        {
            messages.Add(new
            {
                role = msg.Role.ToLower(),
                content = msg.Content
            });
        }

        // Add current user message
        messages.Add(new { role = "user", content = userMessage });

        // Define available functions
        var functions = new[]
        {
            new
            {
                name = "add_skill",
                description = "Add a new skill to the user's learning path",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new
                        {
                            type = "string",
                            description = "The name of the skill to add (e.g., 'Machine Learning', 'Python Programming')"
                        },
                        description = new
                        {
                            type = "string",
                            description = "Optional description of the skill"
                        }
                    },
                    required = new[] { "name" }
                }
            },
            new
            {
                name = "remove_skill",
                description = "Remove a skill from the user's learning path",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        skillId = new
                        {
                            type = "integer",
                            description = "The ID of the skill to remove"
                        }
                    },
                    required = new[] { "skillId" }
                }
            },
            new
            {
                name = "add_topic",
                description = "Add a topic to a skill",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        skillId = new
                        {
                            type = "integer",
                            description = "The ID of the parent skill"
                        },
                        name = new
                        {
                            type = "string",
                            description = "The name of the topic"
                        },
                        description = new
                        {
                            type = "string",
                            description = "Description of the topic"
                        }
                    },
                    required = new[] { "skillId", "name" }
                }
            },
            new
            {
                name = "get_user_skills",
                description = "Get the list of skills the user is learning",
                parameters = new
                {
                    type = "object",
                    properties = new { }
                }
            }
        };

        // Build request
        var requestBody = new
        {
            model = _model,
            messages = messages,
            functions = functions,
            function_call = "auto",
            temperature = _temperature,
            max_tokens = _maxTokens
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Call OpenAI API
        var response = await client.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            content
        );

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OpenAIApiResponse>(responseJson);

        return new OpenAIResponse
        {
            Message = result!.Choices[0].Message.Content,
            FunctionCall = result.Choices[0].Message.FunctionCall
        };
    }
}

public class OpenAIResponse
{
    public string? Message { get; set; }
    public OpenAIFunctionCall? FunctionCall { get; set; }
}

public class OpenAIFunctionCall
{
    public string Name { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

public class OpenAIApiResponse
{
    public Choice[] Choices { get; set; } = Array.Empty<Choice>();

    public class Choice
    {
        public Message Message { get; set; } = new();
    }

    public class Message
    {
        public string? Content { get; set; }
        public OpenAIFunctionCall? FunctionCall { get; set; }
    }
}
```

### Step 2: Update ChatService

Update `ChatService.cs` to use OpenAI:

```csharp
public class ChatService : IChatService
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly KnowledgeService _knowledgeService;
    private readonly UserLearningService _userLearningService;
    private readonly OpenAIChatService _openAIService; // Add this

    public ChatService(
        IChatMessageRepository chatMessageRepository,
        KnowledgeService knowledgeService,
        UserLearningService userLearningService,
        OpenAIChatService openAIService) // Add this
    {
        _chatMessageRepository = chatMessageRepository;
        _knowledgeService = knowledgeService;
        _userLearningService = userLearningService;
        _openAIService = openAIService;
    }

    public async Task<ChatResponse> ProcessMessageAsync(string userId, string message)
    {
        // Save user message
        var userMessage = new ChatMessage
        {
            UserId = userId,
            Role = "user",
            Content = message,
            Timestamp = DateTime.UtcNow
        };
        await _chatMessageRepository.CreateAsync(userMessage);

        // Get conversation history
        var history = await _chatMessageRepository.GetByUserIdAsync(userId, 10);

        // Call OpenAI
        var aiResponse = await _openAIService.GetCompletionAsync(history, message);

        // Check if function call is needed
        if (aiResponse.FunctionCall != null)
        {
            var toolCall = new ToolCall
            {
                Id = Guid.NewGuid().ToString(),
                ToolName = aiResponse.FunctionCall.Name,
                Arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    aiResponse.FunctionCall.Arguments
                )!
            };

            // Execute the tool
            var toolResults = await ExecuteToolCallsAsync(userId, new[] { toolCall });

            // Save assistant message with tool calls
            var assistantMessage = new ChatMessage
            {
                UserId = userId,
                Role = "assistant",
                Content = aiResponse.Message ?? "Executed action",
                ToolCalls = JsonSerializer.Serialize(new[] { toolCall }),
                Timestamp = DateTime.UtcNow
            };
            await _chatMessageRepository.CreateAsync(assistantMessage);

            return new ChatResponse
            {
                Message = aiResponse.Message ?? "I've updated your learning path!",
                ToolCalls = new List<ToolCall> { toolCall },
                RequiresAction = true,
                ToolResults = toolResults.ToList()
            };
        }
        else
        {
            // Regular message, no function call
            var assistantMessage = new ChatMessage
            {
                UserId = userId,
                Role = "assistant",
                Content = aiResponse.Message!,
                Timestamp = DateTime.UtcNow
            };
            await _chatMessageRepository.CreateAsync(assistantMessage);

            return new ChatResponse
            {
                Message = aiResponse.Message!,
                RequiresAction = false
            };
        }
    }

    // Keep existing ExecuteToolCallsAsync method
}
```

### Step 3: Register Service

Update `Program.cs`:

```csharp
// Add HTTP client factory
builder.Services.AddHttpClient();

// Register OpenAI service
builder.Services.AddScoped<OpenAIChatService>();

// ChatService already registered, now depends on OpenAIChatService
builder.Services.AddScoped<IChatService, ChatService>();
```

### Step 4: Update Configuration

Add your OpenAI API key to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=learningtool.db"
  },
  "Jwt": {
    "Key": "your-jwt-secret-key-at-least-32-characters-long",
    "Issuer": "LearningTool",
    "Audience": "LearningTool",
    "ExpiryDays": 7
  },
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key-here",
    "Model": "gpt-4",
    "MaxTokens": 500,
    "Temperature": 0.7,
    "SystemPrompt": "You are a helpful AI learning assistant. Help users learn new skills by managing their learning path. Be concise and friendly."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Testing the Integration

### 1. Build and Run

```bash
cd src/LearningTool.API
dotnet build
dotnet run
```

### 2. Test Chat Endpoint

```bash
# Get token
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'

# Chat with AI
curl -X POST https://localhost:5001/api/chat/message \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-token>" \
  -d '{"message":"I want to learn Python programming"}'
```

### 3. Expected Response

```json
{
  "message": "Great! I'll add Python Programming to your learning path.",
  "toolCalls": [{
    "id": "call-123",
    "toolName": "add_skill",
    "arguments": {
      "name": "Python Programming"
    }
  }],
  "requiresAction": true,
  "toolResults": [{
    "toolCallId": "call-123",
    "success": true,
    "result": "Added skill: Python Programming"
  }]
}
```

## Advanced Features

### 1. Conversation Context

The system maintains conversation context:
- Last 10 messages sent to OpenAI
- System prompt guides behavior
- Function calling for actions

### 2. Custom System Prompts

Customize the AI's personality:

```json
{
  "OpenAI": {
    "SystemPrompt": "You are an enthusiastic learning coach who loves helping people discover new skills. Use emojis occasionally and be encouraging!"
  }
}
```

### 3. Error Handling

Add retry logic and fallbacks:

```csharp
try
{
    var aiResponse = await _openAIService.GetCompletionAsync(history, message);
    // ... process response
}
catch (HttpRequestException ex)
{
    // Fallback to rule-based chat
    return await GenerateResponseAsync(userId, message);
}
```

### 4. Cost Optimization

**Monitor usage:**
```csharp
// Log token usage
_logger.LogInformation("OpenAI API call: {tokens} tokens used",
    response.Usage.TotalTokens);
```

**Set token limits:**
```json
{
  "OpenAI": {
    "MaxTokens": 300  // Reduce for cost savings
  }
}
```

**Use GPT-3.5-turbo for development:**
```json
{
  "OpenAI": {
    "Model": "gpt-3.5-turbo"  // Cheaper, faster
  }
}
```

## Troubleshooting

### Issue: 401 Unauthorized

**Cause:** Invalid API key
**Solution:** Verify API key in configuration

### Issue: 429 Rate Limit

**Cause:** Too many requests
**Solution:** Implement rate limiting, use exponential backoff

### Issue: High Latency

**Cause:** Large conversation history
**Solution:** Limit history to last 5-10 messages

### Issue: Unexpected Responses

**Cause:** System prompt needs tuning
**Solution:** Refine system prompt, add examples

## Cost Estimation

**GPT-4 Pricing (as of 2024):**
- Input: $0.03 per 1K tokens
- Output: $0.06 per 1K tokens

**Typical Chat Message:**
- Input: ~200 tokens (history + message + functions)
- Output: ~100 tokens
- Cost: ~$0.012 per message

**Monthly estimate for 1000 users:**
- 10 messages per user per day
- 30 days
- Total: 300,000 messages
- Cost: ~$3,600/month

**Optimization:**
- Use GPT-3.5-turbo: 10x cheaper (~$360/month)
- Cache frequent responses
- Limit conversation history

## Production Checklist

- [ ] API key stored in environment variable
- [ ] Error handling and retry logic
- [ ] Rate limiting implemented
- [ ] Cost monitoring dashboard
- [ ] Fallback to rule-based chat
- [ ] Logging of all API calls
- [ ] User consent for AI usage
- [ ] Privacy policy updated

## References

- [OpenAI API Documentation](https://platform.openai.com/docs)
- [Function Calling Guide](https://platform.openai.com/docs/guides/function-calling)
- [Best Practices](https://platform.openai.com/docs/guides/production-best-practices)

---

**Last Updated:** February 20, 2026
