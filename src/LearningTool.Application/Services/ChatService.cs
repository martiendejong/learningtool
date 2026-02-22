using LearningTool.Application.DTOs;
using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;
using System.Text.Json;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using DTOToolCall = LearningTool.Application.DTOs.ToolCall;
using OpenAIChatMessage = OpenAI.Chat.ChatMessage;
using DomainChatMessage = LearningTool.Domain.Entities.ChatMessage;
using CreatedSkill = LearningTool.Application.DTOs.CreatedSkill;
using CreatedTopic = LearningTool.Application.DTOs.CreatedTopic;
using CreatedCourse = LearningTool.Application.DTOs.CreatedCourse;
using LearningPathUpdate = LearningTool.Application.DTOs.LearningPathUpdate;

namespace LearningTool.Application.Services;

public interface IChatService
{
    Task<ChatResponse> ProcessMessageAsync(string userId, string message);
    Task<List<DomainChatMessage>> GetChatHistoryAsync(string userId, int limit = 50);
    Task ClearChatHistoryAsync(string userId);

    // Course-specific chat methods
    Task<ChatResponse> ProcessCourseMessageAsync(string userId, int courseId, string message);
    Task<List<DomainChatMessage>> GetCourseChatHistoryAsync(string userId, int courseId, int limit = 50);
    Task ClearCourseChatHistoryAsync(string userId, int courseId);
    Task<(string learningPlan, string systemPrompt, List<ResourceLink> resources)> GenerateCourseContentAsync(Course course);
}

public class ChatService : IChatService
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IKnowledgeService _knowledgeService;
    private readonly IUserLearningService _userLearningService;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _systemPrompt;
    private readonly int _maxTokens;
    private readonly float _temperature;

    public ChatService(
        IChatMessageRepository chatMessageRepository,
        IKnowledgeService knowledgeService,
        IUserLearningService userLearningService,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _chatMessageRepository = chatMessageRepository;
        _knowledgeService = knowledgeService;
        _userLearningService = userLearningService;

        // Read OpenAI configuration
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key not configured");
        _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        _systemPrompt = configuration["OpenAI:SystemPrompt"] ?? @"You are an intelligent AI learning assistant that helps users build personalized learning paths.

CRITICAL: Your ONLY job in this chat is to create learning paths (Skills → Topics → Courses). You do NOT teach courses here.

When a user expresses interest in learning something, you MUST follow this sequence:
1. Call add_skill to create the skill
2. Call add_topic 2-3 times to create topics for that skill
3. Call add_course AT LEAST ONCE for EACH topic you created (this is MANDATORY!)
4. Present the created courses to the user
5. Tell them to click the 'Start Course' button to begin learning

Example workflow for 'I want to learn React':
- add_skill(name='React', description='JavaScript library for building user interfaces')
- add_topic(skillName='React', topicName='React Basics', description='Core concepts and components')
- add_course(topicName='React Basics', courseName='Introduction to React', description='Learn React fundamentals including JSX, components, and props')
- add_topic(skillName='React', topicName='React Hooks', description='Modern React with hooks')
- add_course(topicName='React Hooks', courseName='Mastering React Hooks', description='Deep dive into useState, useEffect, and custom hooks')

Then say: 'I've created these courses for you! To start learning, navigate to the Skills page and click the Start Course button on the course you want to begin.'

IMPORTANT:
- DO NOT teach course content in this chat
- DO NOT start courses for the user
- DO NOT try to be a teacher here
- Your role is ONLY to build the learning path structure
- Users will access actual course teaching in a separate dedicated course chat

If user says 'start' or 'begin', remind them: 'Please use the Start Course button on the course page to begin your learning journey. Each course has its own dedicated learning environment.'";
        _maxTokens = int.Parse(configuration["OpenAI:MaxTokens"] ?? "500");
        _temperature = float.Parse(configuration["OpenAI:Temperature"] ?? "0.7", System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task<ChatResponse> ProcessMessageAsync(string userId, string message)
    {
        // Save user message
        var userMessage = new DomainChatMessage
        {
            UserId = userId,
            Role = "user",
            Content = message,
            Timestamp = DateTime.UtcNow
        };
        await _chatMessageRepository.CreateAsync(userMessage);

        // Process message with OpenAI
        var response = await GenerateResponseAsync(userId, message);

        // Save assistant message
        var assistantMessage = new DomainChatMessage
        {
            UserId = userId,
            Role = "assistant",
            Content = response.Message,
            ToolCalls = response.ToolCalls != null ? JsonSerializer.Serialize(response.ToolCalls) : null,
            Timestamp = DateTime.UtcNow
        };
        await _chatMessageRepository.CreateAsync(assistantMessage);

        return response;
    }

    public async Task<List<DomainChatMessage>> GetChatHistoryAsync(string userId, int limit = 50)
    {
        return await _chatMessageRepository.GetByUserIdAsync(userId, limit);
    }

    public async Task ClearChatHistoryAsync(string userId)
    {
        await _chatMessageRepository.DeleteByUserIdAsync(userId);
    }

    private async Task<ChatResponse> GenerateResponseAsync(string userId, string message)
    {
        // Initialize OpenAI client
        var client = new OpenAIClient(new ApiKeyCredential(_apiKey));
        var chatClient = client.GetChatClient(_model);

        // Get conversation history
        var history = await GetChatHistoryAsync(userId, 20);

        // Get user's current skills for context
        var userSkills = await _userLearningService.GetUserSkillsAsync(userId);
        var skillsContext = userSkills.Any()
            ? $"\n\nUser is currently learning: {string.Join(", ", userSkills.Select(s => s.Skill.Name))}"
            : "\n\nUser hasn't started learning any skills yet.";

        // Build messages array
        var messages = new List<OpenAIChatMessage>();

        // System message with context
        messages.Add(OpenAIChatMessage.CreateSystemMessage(_systemPrompt + skillsContext));

        // Add conversation history
        foreach (var historyMsg in history.OrderBy(m => m.Timestamp))
        {
            if (historyMsg.Role == "user")
            {
                messages.Add(OpenAIChatMessage.CreateUserMessage(historyMsg.Content));
            }
            else if (historyMsg.Role == "assistant")
            {
                messages.Add(OpenAIChatMessage.CreateAssistantMessage(historyMsg.Content));
            }
        }

        // Add current user message
        messages.Add(OpenAIChatMessage.CreateUserMessage(message));

        // Define available functions
        var tools = new List<ChatTool>
        {
            ChatTool.CreateFunctionTool(
                functionName: "add_skill",
                functionDescription: "Add a new skill to the user's learning path",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "name": {
                            "type": "string",
                            "description": "The name of the skill to add"
                        },
                        "description": {
                            "type": "string",
                            "description": "A brief description of what this skill involves"
                        },
                        "difficulty": {
                            "type": "string",
                            "enum": ["Beginner", "Intermediate", "Advanced"],
                            "description": "The difficulty level of the skill"
                        }
                    },
                    "required": ["name", "description"],
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "remove_skill",
                functionDescription: "Remove a skill from the user's learning path",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "name": {
                            "type": "string",
                            "description": "The name of the skill to remove"
                        }
                    },
                    "required": ["name"],
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "get_user_progress",
                functionDescription: "Get the user's learning progress and statistics",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {},
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "list_available_topics",
                functionDescription: "List topics available for a specific skill",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "skillName": {
                            "type": "string",
                            "description": "The name of the skill to get topics for"
                        }
                    },
                    "required": ["skillName"],
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "add_topic",
                functionDescription: "Add a new topic to an existing skill",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "skillName": {
                            "type": "string",
                            "description": "The name of the skill to add the topic to"
                        },
                        "topicName": {
                            "type": "string",
                            "description": "The name of the topic to add"
                        },
                        "description": {
                            "type": "string",
                            "description": "A brief description of what this topic covers"
                        }
                    },
                    "required": ["skillName", "topicName", "description"],
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "add_course",
                functionDescription: "Create a new course under a topic. You MUST call this at least once for each topic you create. Courses are what users actually learn from.",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "topicName": {
                            "type": "string",
                            "description": "The exact name of the topic this course belongs to (must match a topic you already created)"
                        },
                        "courseName": {
                            "type": "string",
                            "description": "A clear, descriptive name for the course (e.g., 'Introduction to React', 'Python Data Structures')"
                        },
                        "description": {
                            "type": "string",
                            "description": "A detailed description of what the student will learn in this course (2-3 sentences)"
                        },
                        "url": {
                            "type": "string",
                            "description": "Optional URL to external course materials"
                        }
                    },
                    "required": ["topicName", "courseName", "description"],
                    "additionalProperties": false
                }
                """)
            )
        };

        // Create chat completion options
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _maxTokens,
            Temperature = _temperature
        };

        foreach (var tool in tools)
        {
            options.Tools.Add(tool);
        }

        // Call OpenAI API
        var completion = await chatClient.CompleteChatAsync(messages, options);

        var responseMessage = string.Empty;
        var toolCalls = new List<DTOToolCall>();
        var learningPathUpdate = new LearningPathUpdate();

        // Check if the model wants to call functions
        if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
        {
            foreach (var toolCall in completion.Value.ToolCalls)
            {
                var functionToolCall = toolCall as ChatToolCall;
                if (functionToolCall != null)
                {
                    // Parse function arguments
                    var args = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        functionToolCall.FunctionArguments.ToString());

                    toolCalls.Add(new DTOToolCall
                    {
                        Id = functionToolCall.Id,
                        ToolName = functionToolCall.FunctionName,
                        Arguments = args ?? new Dictionary<string, object>()
                    });
                }
            }

            // Execute tool calls and collect learning path updates
            var toolResults = new List<string>();
            foreach (var toolCall in toolCalls)
            {
                var (result, pathUpdate) = await ExecuteToolCallWithTrackingAsync(userId, toolCall);
                toolResults.Add(result);

                // Collect created items
                if (pathUpdate != null)
                {
                    if (pathUpdate.CreatedSkill != null)
                        learningPathUpdate.CreatedSkills.Add(pathUpdate.CreatedSkill);
                    if (pathUpdate.CreatedTopic != null)
                        learningPathUpdate.CreatedTopics.Add(pathUpdate.CreatedTopic);
                    if (pathUpdate.CreatedCourse != null)
                        learningPathUpdate.CreatedCourses.Add(pathUpdate.CreatedCourse);
                }
            }

            // Generate final response with tool results
            var finalMessages = new List<OpenAIChatMessage>(messages);

            // Add assistant message with tool calls
            var assistantMessage = OpenAIChatMessage.CreateAssistantMessage(completion.Value.ToolCalls);
            finalMessages.Add(assistantMessage);

            // Add tool results
            foreach (var (toolCall, result) in toolCalls.Zip(toolResults))
            {
                finalMessages.Add(OpenAIChatMessage.CreateToolMessage(toolCall.Id, result));
            }

            // Get final response from OpenAI
            var finalCompletion = await chatClient.CompleteChatAsync(finalMessages);
            responseMessage = finalCompletion.Value.Content.Count > 0 ? finalCompletion.Value.Content[0].Text : string.Empty;
        }
        else
        {
            // No tool calls, get text response directly
            responseMessage = completion.Value.Content.Count > 0 ? completion.Value.Content[0].Text : string.Empty;
        }

        return new ChatResponse
        {
            Message = responseMessage,
            RequiresAction = false, // Tools already executed
            ToolCalls = null, // Don't return tool calls since they're already executed
            LearningPathUpdate = learningPathUpdate.CreatedSkills.Any() || learningPathUpdate.CreatedTopics.Any() || learningPathUpdate.CreatedCourses.Any()
                ? learningPathUpdate
                : null
        };
    }

    private class ToolExecutionResult
    {
        public CreatedSkill? CreatedSkill { get; set; }
        public CreatedTopic? CreatedTopic { get; set; }
        public CreatedCourse? CreatedCourse { get; set; }
    }

    private async Task<(string result, ToolExecutionResult? pathUpdate)> ExecuteToolCallWithTrackingAsync(string userId, DTOToolCall toolCall)
    {
        try
        {
            switch (toolCall.ToolName)
            {
                case "add_skill":
                    var name = toolCall.Arguments["name"].ToString()!;
                    var description = toolCall.Arguments["description"].ToString()!;
                    var difficulty = toolCall.Arguments.ContainsKey("difficulty")
                        ? toolCall.Arguments["difficulty"].ToString()!
                        : "Beginner";

                    // Check if skill already exists in the system
                    var existingSkills = await _knowledgeService.GetAllSkillsAsync();
                    var skill = existingSkills.FirstOrDefault(s =>
                        s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (skill == null)
                    {
                        // Create new skill in the knowledge base
                        var difficultyEnum = difficulty.ToLower() switch
                        {
                            "beginner" => DifficultyLevel.Beginner,
                            "intermediate" => DifficultyLevel.Intermediate,
                            "advanced" => DifficultyLevel.Advanced,
                            _ => DifficultyLevel.Beginner
                        };
                        skill = await _knowledgeService.AddSkillToCatalogAsync(name, description, difficultyEnum);
                    }

                    // Add to user's learning path
                    await _userLearningService.AddSkillToUserAsync(userId, skill.Id);

                    return ($"Successfully added '{name}' to your learning path. Skill ID: {skill.Id}",
                        new ToolExecutionResult
                        {
                            CreatedSkill = new CreatedSkill { Id = skill.Id, Name = skill.Name }
                        });

                case "remove_skill":
                    var skillName = toolCall.Arguments["name"].ToString()!;
                    var userSkills = await _userLearningService.GetUserSkillsAsync(userId);
                    var userSkill = userSkills.FirstOrDefault(us =>
                        us.Skill.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

                    if (userSkill != null)
                    {
                        await _userLearningService.RemoveSkillFromUserAsync(userId, userSkill.SkillId);
                        return ($"Successfully removed '{skillName}' from your learning path.", null);
                    }
                    return ($"Skill '{skillName}' not found in your learning path.", null);

                case "get_user_progress":
                    var skills = await _userLearningService.GetUserSkillsAsync(userId);
                    var completedCourses = await _userLearningService.GetCompletedCoursesAsync(userId);

                    return ($"You are currently learning {skills.Count} skill(s) and have completed {completedCourses.Count} course(s).", null);

                case "list_available_topics":
                    var targetSkill = toolCall.Arguments["skillName"].ToString()!;
                    var allSkills = await _knowledgeService.GetAllSkillsAsync();
                    var foundSkill = allSkills.FirstOrDefault(s =>
                        s.Name.Equals(targetSkill, StringComparison.OrdinalIgnoreCase));

                    if (foundSkill != null)
                    {
                        var topics = await _knowledgeService.GetTopicsForSkillAsync(foundSkill.Id);
                        if (topics.Any())
                        {
                            var topicList = string.Join(", ", topics.Select(t => t.Name));
                            return ($"Topics for {targetSkill}: {topicList}", null);
                        }
                        return ($"No topics found for {targetSkill} yet.", null);
                    }
                    return ($"Skill '{targetSkill}' not found.", null);

                case "add_topic":
                    var skillNameForTopic = toolCall.Arguments["skillName"].ToString()!;
                    var topicName = toolCall.Arguments["topicName"].ToString()!;
                    var topicDescription = toolCall.Arguments["description"].ToString()!;

                    var skillsForTopic = await _knowledgeService.GetAllSkillsAsync();
                    var skillForTopic = skillsForTopic.FirstOrDefault(s =>
                        s.Name.Equals(skillNameForTopic, StringComparison.OrdinalIgnoreCase));

                    if (skillForTopic != null)
                    {
                        var topic = await _knowledgeService.AddTopicAsync(skillForTopic.Id, topicName, topicDescription);
                        return ($"Successfully added topic '{topicName}' to skill '{skillNameForTopic}'. Topic ID: {topic.Id}",
                            new ToolExecutionResult
                            {
                                CreatedTopic = new CreatedTopic
                                {
                                    Id = topic.Id,
                                    Name = topic.Name,
                                    SkillId = skillForTopic.Id
                                }
                            });
                    }
                    return ($"Skill '{skillNameForTopic}' not found. Please add the skill first.", null);

                case "add_course":
                    var topicNameForCourse = toolCall.Arguments["topicName"].ToString()!;
                    var courseName = toolCall.Arguments["courseName"].ToString()!;
                    var courseUrl = toolCall.Arguments.ContainsKey("url")
                        ? toolCall.Arguments["url"].ToString()!
                        : "";
                    var courseDescription = toolCall.Arguments["description"].ToString()!;

                    // Find the topic across all skills
                    var allSkillsForCourse = await _knowledgeService.GetAllSkillsAsync();
                    Domain.Entities.Topic? foundTopic = null;
                    foreach (var skillItem in allSkillsForCourse)
                    {
                        var topicsForSkill = await _knowledgeService.GetTopicsForSkillAsync(skillItem.Id);
                        foundTopic = topicsForSkill.FirstOrDefault(t =>
                            t.Name.Equals(topicNameForCourse, StringComparison.OrdinalIgnoreCase));
                        if (foundTopic != null) break;
                    }

                    if (foundTopic != null)
                    {
                        var resourceLinks = new List<ResourceLink>();
                        if (!string.IsNullOrEmpty(courseUrl))
                        {
                            resourceLinks.Add(new ResourceLink
                            {
                                Title = courseName,
                                Url = courseUrl,
                                Type = ResourceType.Tutorial
                            });
                        }

                        var course = await _knowledgeService.AddCourseAsync(
                            foundTopic.Id,
                            courseName,
                            courseDescription,
                            courseDescription, // Use description as content
                            60, // default 60 minutes
                            new List<string>(),
                            resourceLinks);
                        return ($"Successfully added course '{courseName}' to topic '{topicNameForCourse}'. Course ID: {course.Id}",
                            new ToolExecutionResult
                            {
                                CreatedCourse = new CreatedCourse
                                {
                                    Id = course.Id,
                                    Name = course.Name,
                                    Description = course.Description,
                                    TopicId = foundTopic.Id,
                                    TopicName = foundTopic.Name
                                }
                            });
                    }
                    return ($"Topic '{topicNameForCourse}' not found. Please add the topic first.", null);

                default:
                    return ("Unknown tool call.", null);
            }
        }
        catch (Exception ex)
        {
            return ($"Error executing {toolCall.ToolName}: {ex.Message}", null);
        }
    }

    // Course-specific chat implementation
    public async Task<ChatResponse> ProcessCourseMessageAsync(string userId, int courseId, string message)
    {
        // Save user message with CourseId
        var userMessage = new DomainChatMessage
        {
            UserId = userId,
            Role = "user",
            Content = message,
            CourseId = courseId,
            Timestamp = DateTime.UtcNow
        };
        await _chatMessageRepository.CreateAsync(userMessage);

        // Process message with OpenAI (teaching mode)
        var response = await GenerateCourseResponseAsync(userId, courseId, message);

        // Save assistant message with CourseId
        var assistantMessage = new DomainChatMessage
        {
            UserId = userId,
            Role = "assistant",
            Content = response.Message,
            CourseId = courseId,
            ToolCalls = response.ToolCalls != null ? JsonSerializer.Serialize(response.ToolCalls) : null,
            Timestamp = DateTime.UtcNow
        };
        await _chatMessageRepository.CreateAsync(assistantMessage);

        return response;
    }

    public async Task<List<DomainChatMessage>> GetCourseChatHistoryAsync(string userId, int courseId, int limit = 50)
    {
        return await _chatMessageRepository.GetByCourseIdAsync(userId, courseId, limit);
    }

    public async Task ClearCourseChatHistoryAsync(string userId, int courseId)
    {
        await _chatMessageRepository.DeleteByCourseIdAsync(userId, courseId);
    }

    private async Task<ChatResponse> GenerateCourseResponseAsync(string userId, int courseId, string message)
    {
        // Initialize OpenAI client
        var client = new OpenAIClient(new ApiKeyCredential(_apiKey));
        var chatClient = client.GetChatClient(_model);

        // Get course details
        var course = await _knowledgeService.GetCourseByIdAsync(courseId);
        if (course == null)
        {
            throw new InvalidOperationException($"Course {courseId} not found");
        }

        // Get course chat history
        var history = await GetCourseChatHistoryAsync(userId, courseId, 20);

        // Get user's course progress
        var userCourse = await _userLearningService.GetUserCourseAsync(userId, courseId);

        // Calculate progress percentage based on time spent vs estimated duration
        var progressPercentage = userCourse != null && course.EstimatedMinutes > 0
            ? Math.Min(100, (int)((userCourse.MinutesSpent / (float)course.EstimatedMinutes) * 100))
            : 0;

        // Build comprehensive teaching context with AI-generated content
        var learningPlanSection = !string.IsNullOrEmpty(course.LearningPlan)
            ? $@"

LEARNING PLAN:
{course.LearningPlan}"
            : "";

        var resourcesSection = course.ResourceLinks != null && course.ResourceLinks.Any()
            ? $@"

EXTERNAL RESOURCES AVAILABLE:
{string.Join("\n", course.ResourceLinks.Select(r => $"- {r.Title} ({r.Type}): {r.Url}"))}"
            : "";

        // Build teaching system prompt - use AI-generated course-specific prompt if available
        var teachingPrompt = !string.IsNullOrEmpty(course.SystemPrompt)
            ? $@"{course.SystemPrompt}

Course: ""{course.Name}""
Description: {course.Description}
{learningPlanSection}
{resourcesSection}

Student Progress: {progressPercentage}% complete
Started: {(userCourse?.StartedAt?.ToString("yyyy-MM-dd") ?? "Just now")}

You can reference the learning plan and external resources when teaching. Direct students to specific resources when relevant."
            : $@"You are an expert programming teacher teaching the course ""{course.Name}"".

Course Description: {course.Description}
{learningPlanSection}
{resourcesSection}

Teaching Guidelines:
1. Explain concepts clearly with practical code examples in markdown code blocks (```language)
2. Keep explanations focused - teach ONE concept at a time
3. ALWAYS end your message with an interactive question or exercise
4. Questions should test understanding or encourage practice
5. Be patient, encouraging, and conversational
6. Occasionally use longer explanations for complex topics, but usually keep responses concise

Student Progress: {progressPercentage}% complete
Started: {(userCourse?.StartedAt?.ToString("yyyy-MM-dd") ?? "Just now")}

CRITICAL: Every response must end with a question, comprehension check, or exercise. Make learning interactive!";

        // Build messages array
        var messages = new List<OpenAIChatMessage>();
        messages.Add(OpenAIChatMessage.CreateSystemMessage(teachingPrompt));

        // Add conversation history
        foreach (var historyMsg in history.OrderBy(m => m.Timestamp))
        {
            if (historyMsg.Role == "user")
            {
                messages.Add(OpenAIChatMessage.CreateUserMessage(historyMsg.Content));
            }
            else if (historyMsg.Role == "assistant")
            {
                messages.Add(OpenAIChatMessage.CreateAssistantMessage(historyMsg.Content));
            }
        }

        // Add current user message
        messages.Add(OpenAIChatMessage.CreateUserMessage(message));

        // Define teaching tools
        var tools = new List<ChatTool>
        {
            ChatTool.CreateFunctionTool(
                functionName: "get_course_content",
                functionDescription: "Retrieve the full course content and structure",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "section": {
                            "type": "string",
                            "description": "Optional specific section to retrieve"
                        }
                    },
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "mark_section_complete",
                functionDescription: "Mark a course section as completed by the student",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "sectionName": {
                            "type": "string",
                            "description": "The name of the section completed"
                        }
                    },
                    "required": ["sectionName"],
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "get_student_progress",
                functionDescription: "Get detailed progress information for this course",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {},
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "get_learning_plan",
                functionDescription: "Get the detailed learning plan and curriculum for this course",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {},
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "get_external_resources",
                functionDescription: "Get external learning resources (videos, tutorials, documentation) for this course",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {},
                    "additionalProperties": false
                }
                """)
            )
        };

        // Create chat completion options
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 1500,  // More tokens for detailed teaching
            Temperature = 0.7f
        };

        foreach (var tool in tools)
        {
            options.Tools.Add(tool);
        }

        // Call OpenAI API
        var completion = await chatClient.CompleteChatAsync(messages, options);

        var responseMessage = "";
        var toolCalls = new List<DTOToolCall>();

        // Check if the model wants to call functions
        if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
        {
            foreach (var toolCall in completion.Value.ToolCalls)
            {
                var functionToolCall = toolCall as ChatToolCall;
                if (functionToolCall != null)
                {
                    var args = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        functionToolCall.FunctionArguments.ToString());

                    toolCalls.Add(new DTOToolCall
                    {
                        Id = functionToolCall.Id,
                        ToolName = functionToolCall.FunctionName,
                        Arguments = args ?? new Dictionary<string, object>()
                    });
                }
            }

            return new ChatResponse
            {
                Message = responseMessage,
                ToolCalls = toolCalls,
                RequiresAction = true
            };
        }

        // Get the response text when no tool calls
        responseMessage = completion.Value.Content.Count > 0 && completion.Value.Content[0].Text != null
            ? completion.Value.Content[0].Text
            : "I'm ready to help you learn!";

        return new ChatResponse
        {
            Message = responseMessage,
            RequiresAction = false
        };
    }

    /// <summary>
    /// Generate comprehensive course content when a course is first started
    /// </summary>
    public async Task<(string learningPlan, string systemPrompt, List<ResourceLink> resources)> GenerateCourseContentAsync(Course course)
    {
        var client = new OpenAIClient(new ApiKeyCredential(_apiKey));
        var chatClient = client.GetChatClient("gpt-4o-mini");

        var prompt = $@"You are an expert curriculum designer. Create comprehensive learning content for the course: '{course.Name}'.

Course Description: {course.Description}

Generate the following:

1. LEARNING PLAN: A detailed, structured curriculum with:
   - 3-5 main modules/sections
   - Each module should have 2-4 specific lessons
   - Clear learning objectives for each lesson
   - Estimated time for each section
   - Format as markdown with clear hierarchy

2. EXTERNAL RESOURCES: Provide 5-10 high-quality external learning resources:
   - YouTube videos (actual URLs)
   - Documentation links
   - Tutorials
   - Articles
   - Books
   - Format as JSON array with title, url, and type

3. TEACHING SYSTEM PROMPT: Create a detailed system prompt (300-500 words) for an AI teacher that will teach this course. Include:
   - Teaching style and approach
   - How to explain concepts
   - How to use examples relevant to this topic
   - How to check understanding
   - How to encourage practice
   - Specific pedagogical strategies for this subject

Return your response in this EXACT JSON format:
{{
  ""learningPlan"": ""markdown string here"",
  ""systemPrompt"": ""teaching instructions here"",
  ""resources"": [
    {{""title"": ""Resource Name"", ""url"": ""https://..."", ""type"": ""YouTube""}},
    {{""title"": ""Resource Name"", ""url"": ""https://..."", ""type"": ""Documentation""}}
  ]
}}

IMPORTANT: Return ONLY valid JSON, no markdown code blocks or extra text.";

        var messages = new List<OpenAIChatMessage>
        {
            OpenAIChatMessage.CreateSystemMessage("You are an expert curriculum designer. Always respond with valid JSON only."),
            OpenAIChatMessage.CreateUserMessage(prompt)
        };

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 2000,
            Temperature = 0.7f
        };

        var completion = await chatClient.CompleteChatAsync(messages, options);
        var jsonResponse = completion.Value.Content[0].Text ?? "{}";

        // Parse the JSON response
        var doc = JsonDocument.Parse(jsonResponse);
        var root = doc.RootElement;

        var learningPlan = root.GetProperty("learningPlan").GetString() ?? "";
        var systemPrompt = root.GetProperty("systemPrompt").GetString() ?? "";

        var resources = new List<ResourceLink>();
        if (root.TryGetProperty("resources", out var resourcesArray))
        {
            foreach (var resource in resourcesArray.EnumerateArray())
            {
                var typeString = resource.GetProperty("type").GetString() ?? "Tutorial";
                var resourceType = Enum.TryParse<ResourceType>(typeString, out var parsed) ? parsed : ResourceType.Tutorial;

                resources.Add(new ResourceLink
                {
                    Title = resource.GetProperty("title").GetString() ?? "",
                    Url = resource.GetProperty("url").GetString() ?? "",
                    Type = resourceType
                });
            }
        }

        return (learningPlan, systemPrompt, resources);
    }
}
