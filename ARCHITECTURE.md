# LearningTool - Architecture Design

**Version:** 1.0
**Date:** 2026-02-20
**Author:** Jengo

---

## Vision

Build an AI-powered learning management system that's "1000x better than client-manager" through:
- **Clean Architecture** from the start (proper separation of concerns)
- **Intelligent AI Tutor** with tool-calling capabilities
- **Knowledge Graph** (Skills → Topics → Courses) with smart recommendations
- **Modern Stack** with best practices baked in
- **Scalable Design** ready for multi-tenancy and advanced features

---

## Core Features

### 1. Google OAuth Authentication
- Single sign-on with Google
- JWT token-based session management
- User profile management

### 2. AI Chat Agent
- **Greets user by name** on first interaction
- **Asks about learning goals** and maintains skill wishlist
- **Tool-calling capabilities**:
  - `add_skill` / `remove_skill`
  - `add_topic` / `remove_topic`
  - `add_course` / `remove_course`
  - `search_catalog` (find existing knowledge items)
  - `start_course` / `complete_course`
  - `assess_knowledge` (quiz-style assessment)
  - `recommend_resources` (YouTube, websites, docs)

### 3. Knowledge Hierarchy

**Three-level structure:**
- **Skills** (e.g., "Python Programming", "Machine Learning", "Web Development")
  - **Topics** (e.g., "Django Framework", "Neural Networks", "CSS Grid")
    - **Courses** (e.g., "Build a REST API with Django", "Implement a CNN from Scratch")

**Catalog vs User Data:**
- **Centralized Catalog** (user-independent):
  - `Skills`, `Topics`, `Courses` tables
  - Shared across all users (deduplicated)
  - Rich metadata (descriptions, difficulty, prerequisites, tags)

- **User Associations**:
  - `UserSkills`, `UserTopics`, `UserCourses` tables
  - Track user-specific state (in_progress, completed, deleted)
  - Progress tracking, assessment scores, completion dates

**Soft Deletion:**
- All entities have `IsDeleted` flag
- Deleted items remain in database for history/recovery
- User sees only active items by default

### 4. Course "Taking" System

**Learning Flow:**
1. **Introduction**: Agent explains course objectives
2. **Content Delivery**:
   - Structured lessons with explanations
   - Links to external resources (YouTube videos, documentation, tutorials)
   - Code examples, diagrams, exercises
3. **Knowledge Checks**:
   - Periodic quizzes/questions to validate understanding
   - Adaptive: if user struggles, provide more examples
4. **Assessment**:
   - Final assessment to validate course completion
   - Score tracked in `UserCourses.AssessmentScore`
5. **Completion**:
   - Course marked complete
   - Certificate/badge generated (future feature)
   - Appears in timeline

### 5. UI Views

**A. Chat Interface**
- Full-screen chat with AI tutor
- Message history with tool call results
- Streaming responses
- Rich formatting (markdown, code blocks, links)

**B. Skills Tree View**
- Hierarchical display: Skills → Topics → Courses
- Visual indicators:
  - ✓ Completed
  - ⟳ In Progress
  - ○ Not Started
  - 🗑️ Soft-Deleted (toggle visibility)
- Expand/collapse functionality
- Quick actions (add, remove, start)
- Progress bars for each skill/topic

**C. Timeline View**
- Chronological list of completed courses
- Card-based layout with:
  - Course name + parent topic + parent skill
  - Completion date
  - Duration (time spent)
  - Assessment score
  - Link to review course content
- Filter by date range, skill, topic
- Export to PDF/CSV

---

## Technical Architecture

### Stack Selection

**Backend:**
- **Framework**: ASP.NET Core 8.0 (Web API)
- **Language**: C# 12
- **Database**: SQLite with Entity Framework Core 9.0
- **AI Integration**: Hazina framework (multi-provider LLM orchestration)
- **Authentication**: ASP.NET Core Identity + Google OAuth + JWT
- **Real-time**: SignalR for streaming chat responses
- **Background Jobs**: Hangfire (future: scheduled learning reminders)

**Frontend:**
- **Framework**: React 19 + TypeScript
- **Build Tool**: Vite 7
- **State Management**: Zustand (persistent stores)
- **Routing**: React Router v7
- **UI Library**: Radix UI + Tailwind CSS
- **HTTP Client**: Axios with interceptors
- **Real-time**: SignalR client
- **Markdown**: react-markdown for chat rendering

**Infrastructure:**
- **Version Control**: Git (C:\Projects\learningtool)
- **Project Management**: ClickUp (List ID: 901215905273)
- **Documentation**: Markdown in `/docs`

---

## Clean Architecture Layers

### 1. Domain Layer (`LearningTool.Domain`)
**Pure business logic, no dependencies on infrastructure.**

**Entities:**
```csharp
// Catalog entities (shared across users)
public class Skill {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? IconUrl { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<Topic> Topics { get; set; }
}

public class Topic {
    public int Id { get; set; }
    public int SkillId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public Skill Skill { get; set; }
    public ICollection<Course> Courses { get; set; }
}

public class Course {
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Content { get; set; }  // Full course content (markdown)
    public int EstimatedMinutes { get; set; }
    public List<string> Prerequisites { get; set; }  // Course IDs
    public List<string> ResourceLinks { get; set; }  // YouTube, docs, tutorials
    public bool IsDeleted { get; set; }

    // Navigation
    public Topic Topic { get; set; }
}

// User-specific associations
public class UserSkill {
    public int Id { get; set; }
    public string UserId { get; set; }
    public int SkillId { get; set; }
    public UserSkillStatus Status { get; set; }  // WantToLearn, InProgress, Mastered
    public bool IsDeleted { get; set; }
    public DateTime AddedAt { get; set; }

    // Navigation
    public Skill Skill { get; set; }
}

public class UserTopic {
    public int Id { get; set; }
    public string UserId { get; set; }
    public int TopicId { get; set; }
    public UserTopicStatus Status { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime AddedAt { get; set; }

    // Navigation
    public Topic Topic { get; set; }
}

public class UserCourse {
    public int Id { get; set; }
    public string UserId { get; set; }
    public int CourseId { get; set; }
    public UserCourseStatus Status { get; set; }  // NotStarted, InProgress, Completed
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? AssessmentScore { get; set; }  // 0-100
    public int MinutesSpent { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public Course Course { get; set; }
}

// Enums
public enum DifficultyLevel { Beginner, Intermediate, Advanced, Expert }
public enum UserSkillStatus { WantToLearn, InProgress, Mastered }
public enum UserTopicStatus { NotStarted, InProgress, Completed }
public enum UserCourseStatus { NotStarted, InProgress, Completed }
```

**Interfaces:**
```csharp
public interface ISkillRepository {
    Task<Skill?> GetByIdAsync(int id);
    Task<Skill?> GetByNameAsync(string name);
    Task<List<Skill>> SearchAsync(string query);
    Task<Skill> CreateAsync(Skill skill);
    Task UpdateAsync(Skill skill);
    Task SoftDeleteAsync(int id);
}

// Similar interfaces for ITopic, ICourse, IUserSkill, IUserTopic, IUserCourse repositories
```

### 2. Application Layer (`LearningTool.Application`)
**Orchestrates business logic, defines use cases.**

**Services:**
```csharp
public interface IKnowledgeService {
    // Skill management
    Task<Skill> AddSkillToCatalogAsync(string name, string description, DifficultyLevel difficulty);
    Task<Skill?> FindOrCreateSkillAsync(string name);
    Task<List<Skill>> SearchSkillsAsync(string query);

    // Topic management
    Task<Topic> AddTopicAsync(int skillId, string name, string description);
    Task<List<Topic>> GetTopicsForSkillAsync(int skillId);

    // Course management
    Task<Course> AddCourseAsync(int topicId, CourseCreateDto dto);
    Task<Course?> GetCourseByIdAsync(int id);
    Task<List<Course>> GetCoursesForTopicAsync(int topicId);
}

public interface IUserLearningService {
    // User skill management
    Task<UserSkill> AddSkillToUserAsync(string userId, int skillId);
    Task RemoveSkillFromUserAsync(string userId, int skillId);  // Soft delete
    Task<List<UserSkill>> GetUserSkillsAsync(string userId, bool includeDeleted = false);

    // User topic management
    Task<UserTopic> AddTopicToUserAsync(string userId, int topicId);
    Task RemoveTopicFromUserAsync(string userId, int topicId);

    // User course management
    Task<UserCourse> StartCourseAsync(string userId, int courseId);
    Task<UserCourse> CompleteCourseAsync(string userId, int courseId, int assessmentScore);
    Task<List<UserCourse>> GetCompletedCoursesAsync(string userId);
    Task<List<UserCourse>> GetInProgressCoursesAsync(string userId);
}

public interface IChatService {
    Task<ChatMessage> SendMessageAsync(string userId, string message, CancellationToken cancellationToken);
    Task<List<ChatMessage>> GetChatHistoryAsync(string userId, int limit = 50);
    Task ClearChatHistoryAsync(string userId);
}

public interface ICourseDeliveryService {
    Task<CourseLesson> GetNextLessonAsync(string userId, int courseId);
    Task<AssessmentQuestion> GenerateAssessmentQuestionAsync(int courseId, string topic);
    Task<bool> ValidateAnswerAsync(int questionId, string answer);
    Task<List<ResourceLink>> GetResourceLinksAsync(int courseId);
}
```

**DTOs:**
```csharp
public record ChatMessage(string Role, string Content, DateTime Timestamp, List<ToolCall>? ToolCalls = null);
public record ToolCall(string Name, string Arguments, string Result);
public record CourseCreateDto(string Name, string Description, string Content, int EstimatedMinutes, List<string> Prerequisites, List<string> ResourceLinks);
public record CourseLesson(string Title, string Content, int LessonNumber, int TotalLessons);
public record AssessmentQuestion(int Id, string Question, List<string> Options, string? Hint);
public record ResourceLink(string Title, string Url, ResourceType Type);

public enum ResourceType { YouTube, Documentation, Tutorial, Article, Book }
```

### 3. Infrastructure Layer (`LearningTool.Infrastructure`)
**Implements external dependencies (database, AI, auth).**

**Database:**
```csharp
public class LearningToolDbContext : DbContext {
    // Catalog tables
    public DbSet<Skill> Skills { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Course> Courses { get; set; }

    // User association tables
    public DbSet<UserSkill> UserSkills { get; set; }
    public DbSet<UserTopic> UserTopics { get; set; }
    public DbSet<UserCourse> UserCourses { get; set; }

    // Chat history
    public DbSet<ChatMessageEntity> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // Soft delete global filter
        modelBuilder.Entity<Skill>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Topic>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);

        // Cascade delete: Skill → Topics → Courses
        modelBuilder.Entity<Skill>()
            .HasMany(s => s.Topics)
            .WithOne(t => t.Skill)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Topic>()
            .HasMany(t => t.Courses)
            .WithOne(c => c.Topic)
            .OnDelete(DeleteBehavior.Cascade);

        // User associations are soft-deleted, not cascaded
    }
}

// Database location
// Development: C:\Projects\learningtool\LearningTool.API\learningtool.db
// Production: Configurable via appsettings.json
```

**AI Integration:**
```csharp
public class HazinaLLMService : ILLMService {
    private readonly IProviderOrchestrator _orchestrator;
    private readonly IToolsContext _toolsContext;

    public HazinaLLMService(IProviderOrchestrator orchestrator) {
        _orchestrator = orchestrator;
        _toolsContext = BuildToolsContext();
    }

    private IToolsContext BuildToolsContext() {
        var context = new ToolsContext();

        // Add skills management tools
        context.Add(new HazinaChatTool(
            name: "add_skill",
            description: "Add a skill to the user's learning wishlist",
            parameters: [
                new("skill_name", "string", "Name of the skill", required: true)
            ],
            execute: async (messages, call, cancel) => {
                var args = JsonSerializer.Deserialize<AddSkillArgs>(call.Arguments);
                var skill = await _knowledgeService.FindOrCreateSkillAsync(args.skill_name);
                var userSkill = await _userLearningService.AddSkillToUserAsync(GetUserId(), skill.Id);
                return $"Added '{skill.Name}' to your learning goals!";
            }
        ));

        context.Add(new HazinaChatTool(
            name: "remove_skill",
            description: "Remove a skill from the user's learning wishlist",
            parameters: [
                new("skill_name", "string", "Name of the skill to remove", required: true)
            ],
            execute: async (messages, call, cancel) => {
                // Implementation
            }
        ));

        context.Add(new HazinaChatTool(
            name: "start_course",
            description: "Start a course for the user",
            parameters: [
                new("course_id", "integer", "ID of the course to start", required: true)
            ],
            execute: async (messages, call, cancel) => {
                // Implementation
            }
        ));

        // ... more tools (search_catalog, recommend_resources, assess_knowledge, etc.)

        return context;
    }

    public async Task<string> ChatAsync(string userId, string message, List<ChatMessage> history, CancellationToken cancel) {
        var messages = BuildMessages(userId, message, history);
        var response = await _orchestrator.ChatCompletionAsync(messages, _toolsContext, cancel);
        return response.Content;
    }
}
```

**Repositories:**
```csharp
public class SkillRepository : ISkillRepository {
    private readonly LearningToolDbContext _context;

    public async Task<Skill?> GetByIdAsync(int id) =>
        await _context.Skills
            .Include(s => s.Topics)
                .ThenInclude(t => t.Courses)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Skill?> GetByNameAsync(string name) =>
        await _context.Skills.FirstOrDefaultAsync(s => s.Name == name);

    public async Task<List<Skill>> SearchAsync(string query) =>
        await _context.Skills
            .Where(s => EF.Functions.Like(s.Name, $"%{query}%") ||
                        EF.Functions.Like(s.Description, $"%{query}%"))
            .ToListAsync();

    // ... more methods
}
```

### 4. API Layer (`LearningTool.API`)
**Exposes HTTP endpoints.**

**Controllers:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkillsController : ControllerBase {
    private readonly IKnowledgeService _knowledgeService;
    private readonly IUserLearningService _userLearningService;

    [HttpGet]
    public async Task<IActionResult> GetMySkills() {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var skills = await _userLearningService.GetUserSkillsAsync(userId);
        return Ok(skills);
    }

    [HttpPost]
    public async Task<IActionResult> AddSkill([FromBody] AddSkillRequest request) {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var skill = await _knowledgeService.FindOrCreateSkillAsync(request.Name);
        var userSkill = await _userLearningService.AddSkillToUserAsync(userId, skill.Id);
        return CreatedAtAction(nameof(GetMySkills), new { id = userSkill.Id }, userSkill);
    }

    // ... more endpoints
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase {
    private readonly IChatService _chatService;

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request, CancellationToken cancellationToken) {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var response = await _chatService.SendMessageAsync(userId, request.Message, cancellationToken);
        return Ok(response);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 50) {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var messages = await _chatService.GetChatHistoryAsync(userId, limit);
        return Ok(messages);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase {
    private readonly IUserLearningService _userLearningService;
    private readonly ICourseDeliveryService _courseDeliveryService;

    [HttpPost("{courseId}/start")]
    public async Task<IActionResult> StartCourse(int courseId) {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var userCourse = await _userLearningService.StartCourseAsync(userId, courseId);
        return Ok(userCourse);
    }

    [HttpPost("{courseId}/complete")]
    public async Task<IActionResult> CompleteCourse(int courseId, [FromBody] CompleteCourseRequest request) {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var userCourse = await _userLearningService.CompleteCourseAsync(userId, courseId, request.AssessmentScore);
        return Ok(userCourse);
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedCourses() {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var courses = await _userLearningService.GetCompletedCoursesAsync(userId);
        return Ok(courses);
    }
}
```

**Program.cs (DI Configuration):**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<LearningToolDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity + Google OAuth
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<LearningToolDbContext>();

builder.Services.AddAuthentication()
    .AddGoogle(options => {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Hazina AI services
builder.Services.AddProviderRegistry();  // Multi-provider LLM orchestration
builder.Services.AddDefaultLLMClient();
builder.Services.AddSingleton<ILLMService, HazinaLLMService>();

// Application services
builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<IUserLearningService, UserLearningService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ICourseDeliveryService, CourseDeliveryService>();

// Repositories
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserSkillRepository, UserSkillRepository>();
builder.Services.AddScoped<IUserTopicRepository, UserTopicRepository>();
builder.Services.AddScoped<IUserCourseRepository, UserCourseRepository>();

// SignalR (for streaming chat)
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins("http://localhost:5173")  // Vite dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");  // SignalR endpoint

app.Run();
```

---

## Frontend Architecture

### State Management (Zustand)

**Auth Store:**
```typescript
interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;

  login: (token: string) => void;
  logout: () => void;
  initialize: () => Promise<void>;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      token: null,
      isAuthenticated: false,

      login: (token) => {
        localStorage.setItem('token', token);
        set({ token, isAuthenticated: true });
      },

      logout: () => {
        localStorage.removeItem('token');
        set({ user: null, token: null, isAuthenticated: false });
      },

      initialize: async () => {
        const token = localStorage.getItem('token');
        if (!token) return;

        try {
          const user = await api.getCurrentUser();
          set({ user, token, isAuthenticated: true });
        } catch {
          get().logout();
        }
      }
    }),
    { name: 'auth-storage' }
  )
);
```

**Skills Store:**
```typescript
interface SkillsState {
  skills: UserSkill[];
  loading: boolean;

  fetchSkills: () => Promise<void>;
  addSkill: (name: string) => Promise<void>;
  removeSkill: (id: number) => Promise<void>;
}

export const useSkillsStore = create<SkillsState>((set, get) => ({
  skills: [],
  loading: false,

  fetchSkills: async () => {
    set({ loading: true });
    const skills = await api.getMySkills();
    set({ skills, loading: false });
  },

  addSkill: async (name) => {
    await api.addSkill(name);
    await get().fetchSkills();  // Refresh
  },

  removeSkill: async (id) => {
    await api.removeSkill(id);
    await get().fetchSkills();
  }
}));
```

**Chat Store:**
```typescript
interface ChatState {
  messages: ChatMessage[];
  isStreaming: boolean;

  sendMessage: (content: string) => Promise<void>;
  clearHistory: () => void;
}

export const useChatStore = create<ChatState>((set, get) => ({
  messages: [],
  isStreaming: false,

  sendMessage: async (content) => {
    const userMessage = { role: 'user', content, timestamp: new Date() };
    set({ messages: [...get().messages, userMessage], isStreaming: true });

    const response = await api.sendChatMessage(content);

    set({
      messages: [...get().messages, {
        role: 'assistant',
        content: response.content,
        timestamp: new Date(),
        toolCalls: response.toolCalls
      }],
      isStreaming: false
    });
  },

  clearHistory: () => set({ messages: [] })
}));
```

### Components Structure

```
src/
├── components/
│   ├── auth/
│   │   ├── GoogleLoginButton.tsx
│   │   ├── ProtectedRoute.tsx
│   │   └── AuthCallback.tsx
│   ├── chat/
│   │   ├── ChatInterface.tsx
│   │   ├── MessageList.tsx
│   │   ├── MessageInput.tsx
│   │   ├── ToolCallResult.tsx
│   │   └── TypingIndicator.tsx
│   ├── skills/
│   │   ├── SkillsTreeView.tsx
│   │   ├── SkillNode.tsx
│   │   ├── TopicNode.tsx
│   │   ├── CourseNode.tsx
│   │   └── AddSkillModal.tsx
│   ├── timeline/
│   │   ├── TimelineView.tsx
│   │   ├── CourseCard.tsx
│   │   └── TimelineFilter.tsx
│   └── shared/
│       ├── Layout.tsx
│       ├── Navbar.tsx
│       └── LoadingSpinner.tsx
├── stores/
│   ├── authStore.ts
│   ├── skillsStore.ts
│   ├── chatStore.ts
│   └── timelineStore.ts
├── services/
│   ├── api.ts (Axios instance)
│   ├── authService.ts
│   ├── skillsService.ts
│   ├── chatService.ts
│   └── coursesService.ts
├── types/
│   ├── auth.ts
│   ├── skills.ts
│   ├── chat.ts
│   └── courses.ts
├── App.tsx
└── main.tsx
```

### Routing

```typescript
<Routes>
  {/* Public routes */}
  <Route path="/" element={<LandingPage />} />
  <Route path="/login" element={<LoginPage />} />
  <Route path="/auth/callback" element={<AuthCallback />} />

  {/* Protected routes */}
  <Route element={<ProtectedRoute />}>
    <Route element={<Layout />}>
      <Route path="/chat" element={<ChatInterface />} />
      <Route path="/skills" element={<SkillsTreeView />} />
      <Route path="/timeline" element={<TimelineView />} />
      <Route path="/profile" element={<ProfilePage />} />
    </Route>
  </Route>
</Routes>
```

---

## Database Schema

```sql
-- Catalog tables (shared across users)
CREATE TABLE Skills (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT NOT NULL,
    IconUrl TEXT,
    Difficulty INTEGER NOT NULL,  -- 0=Beginner, 1=Intermediate, 2=Advanced, 3=Expert
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL
);

CREATE TABLE Topics (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SkillId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT NOT NULL,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (SkillId) REFERENCES Skills(Id) ON DELETE CASCADE
);

CREATE TABLE Courses (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TopicId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT NOT NULL,
    Content TEXT,  -- Full course content (markdown)
    EstimatedMinutes INTEGER NOT NULL,
    Prerequisites TEXT,  -- JSON array of course IDs
    ResourceLinks TEXT,  -- JSON array of resource objects
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (TopicId) REFERENCES Topics(Id) ON DELETE CASCADE
);

-- User association tables
CREATE TABLE UserSkills (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    SkillId INTEGER NOT NULL,
    Status INTEGER NOT NULL,  -- 0=WantToLearn, 1=InProgress, 2=Mastered
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    AddedAt TEXT NOT NULL,
    FOREIGN KEY (SkillId) REFERENCES Skills(Id),
    UNIQUE(UserId, SkillId)
);

CREATE TABLE UserTopics (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    TopicId INTEGER NOT NULL,
    Status INTEGER NOT NULL,  -- 0=NotStarted, 1=InProgress, 2=Completed
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    AddedAt TEXT NOT NULL,
    FOREIGN KEY (TopicId) REFERENCES Topics(Id),
    UNIQUE(UserId, TopicId)
);

CREATE TABLE UserCourses (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    CourseId INTEGER NOT NULL,
    Status INTEGER NOT NULL,  -- 0=NotStarted, 1=InProgress, 2=Completed
    StartedAt TEXT,
    CompletedAt TEXT,
    AssessmentScore INTEGER,  -- 0-100
    MinutesSpent INTEGER NOT NULL DEFAULT 0,
    Notes TEXT,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id),
    UNIQUE(UserId, CourseId)
);

-- Chat history
CREATE TABLE ChatMessages (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId TEXT NOT NULL,
    Role TEXT NOT NULL,  -- 'user' or 'assistant'
    Content TEXT NOT NULL,
    ToolCalls TEXT,  -- JSON array of tool calls
    Timestamp TEXT NOT NULL
);

-- Indexes for performance
CREATE INDEX idx_userskills_userid ON UserSkills(UserId);
CREATE INDEX idx_usertopics_userid ON UserTopics(UserId);
CREATE INDEX idx_usercourses_userid ON UserCourses(UserId);
CREATE INDEX idx_chatmessages_userid ON ChatMessages(UserId);
CREATE INDEX idx_topics_skillid ON Topics(SkillId);
CREATE INDEX idx_courses_topicid ON Courses(TopicId);
```

---

## AI Agent System Prompt

```
You are a helpful AI tutor for the LearningTool platform. Your role is to:

1. **Greet new users** warmly and ask about their learning goals
2. **Manage skills, topics, and courses** using the provided tools
3. **Deliver course content** in a structured, engaging way
4. **Assess knowledge** through quizzes and questions
5. **Recommend resources** (YouTube, documentation, tutorials)

## Available Tools:

- add_skill(skill_name: string): Add a skill to user's learning wishlist
- remove_skill(skill_name: string): Remove a skill from wishlist (soft delete)
- add_topic(skill_name: string, topic_name: string): Add a topic under a skill
- remove_topic(topic_name: string): Remove a topic
- add_course(topic_name: string, course_name: string, description: string): Add a course
- start_course(course_id: int): Begin a course for the user
- complete_course(course_id: int, assessment_score: int): Mark course complete
- search_catalog(query: string): Search existing skills/topics/courses
- recommend_resources(course_id: int): Get YouTube videos, docs, tutorials

## Behavior Guidelines:

1. **First interaction**: Greet by name, ask "What skills would you like to learn?"
2. **Adding skills**: Search catalog first. If exists, reuse. If not, create new.
3. **Course delivery**:
   - Break content into lessons (introduction, main content, practice, assessment)
   - Provide YouTube links and documentation
   - Check understanding before moving forward
4. **Assessment**:
   - Ask 3-5 questions to validate learning
   - If user scores <70%, offer to review material
   - If ≥70%, mark course complete
5. **Encouragement**: Be supportive, celebrate progress, suggest next steps

## Example Interactions:

**User (first time)**: "Hi"
**Assistant**: "Hello [Name]! Welcome to LearningTool. I'm here to help you learn new skills. What would you like to learn? For example: programming languages, data science, design, business skills, or anything else!"

**User**: "I want to learn Python"
**Assistant**: [Uses add_skill("Python Programming")] "Great choice! I've added Python Programming to your learning goals. Would you like me to suggest some topics to get started? For example: Python Basics, Web Development with Django, Data Analysis with Pandas, or Machine Learning?"

**User**: "Yes, Python Basics"
**Assistant**: [Uses add_topic("Python Programming", "Python Basics")] "Perfect! I've added Python Basics. Let me create some beginner courses for you: 1) Introduction to Python Syntax, 2) Variables and Data Types, 3) Control Flow (if/else, loops), 4) Functions and Modules. Which one would you like to start with?"
```

---

## Development Phases

### Phase 1: Foundation (Week 1)
**Goal**: Project structure + authentication working

- ✅ Initialize repository
- [ ] Set up backend project structure (4 layers)
- [ ] Configure database (SQLite + EF Core)
- [ ] Implement Google OAuth + JWT authentication
- [ ] Set up frontend (React + Vite + TypeScript)
- [ ] Basic auth flow (login, logout, protected routes)

### Phase 2: Knowledge Hierarchy (Week 2)
**Goal**: Skills/Topics/Courses CRUD working

- [ ] Domain models for Skills, Topics, Courses
- [ ] Repositories for all entities
- [ ] KnowledgeService + UserLearningService
- [ ] API controllers with full CRUD
- [ ] Database migrations
- [ ] Basic UI for viewing/adding skills

### Phase 3: AI Agent (Week 3)
**Goal**: Chat working with tool calling

- [ ] Integrate Hazina LLM orchestration
- [ ] Define AI tools (add_skill, remove_skill, etc.)
- [ ] ChatService with message history
- [ ] SignalR for streaming responses
- [ ] Chat UI with tool call visualization
- [ ] System prompt configuration

### Phase 4: Course Delivery (Week 4)
**Goal**: Taking courses end-to-end

- [ ] CourseDeliveryService
- [ ] Lesson content generation/retrieval
- [ ] Assessment question generation
- [ ] Resource recommendations (YouTube API integration)
- [ ] Progress tracking (time spent, status updates)
- [ ] Course completion flow

### Phase 5: UI Views (Week 5)
**Goal**: Tree view + Timeline working

- [ ] SkillsTreeView component (hierarchical display)
- [ ] Expand/collapse functionality
- [ ] Progress indicators
- [ ] TimelineView component
- [ ] Filtering and search
- [ ] Export functionality

### Phase 6: Polish & Deploy (Week 6)
**Goal**: Production-ready

- [ ] Error handling and validation
- [ ] Loading states and animations
- [ ] Responsive design (mobile-friendly)
- [ ] Performance optimization
- [ ] Documentation (user guide, API docs)
- [ ] Deployment (Docker + CI/CD)

---

## Success Criteria

This system is "1000x better than client-manager" if:

1. **Clean Architecture**: No more 2,683-line Program.cs, proper layer separation
2. **AI Excellence**: Smart agent that genuinely helps users learn
3. **User Experience**: Intuitive, fast, delightful to use
4. **Code Quality**: Well-tested, documented, maintainable
5. **Scalability**: Ready for 1000+ users, easy to add features
6. **Learning Effectiveness**: Users actually complete courses and gain skills

---

## Next Steps

1. ✅ Create ClickUp list (ID: 901215905273)
2. ✅ Write this architecture document
3. [ ] Set up backend project structure
4. [ ] Configure database and migrations
5. [ ] Implement authentication
6. [ ] Start building Knowledge Hierarchy

---

**End of Architecture Document**
