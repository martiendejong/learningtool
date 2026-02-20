# Complete System Architecture - LearningTool

## Table of Contents
1. [System Overview](#system-overview)
2. [Architecture Layers](#architecture-layers)
3. [Database Schema](#database-schema)
4. [API Design](#api-design)
5. [Frontend Architecture](#frontend-architecture)
6. [Authentication Flow](#authentication-flow)
7. [AI Chat System](#ai-chat-system)
8. [Data Flow](#data-flow)
9. [Deployment Architecture](#deployment-architecture)

---

## System Overview

LearningTool is a full-stack AI-powered learning management system built with clean architecture principles.

### Technology Stack

**Backend:**
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8.0
- SQLite (development) / SQL Server (production capable)
- ASP.NET Core Identity
- JWT Bearer Authentication
- OpenAI API integration

**Frontend:**
- React 19
- TypeScript 5.9
- Vite 7.3
- Zustand (state management)
- React Router v7
- Tailwind CSS 4.2
- Axios (HTTP client)

### Core Principles

1. **Clean Architecture** - Clear separation of concerns
2. **Dependency Inversion** - High-level modules don't depend on low-level modules
3. **Single Responsibility** - Each component has one reason to change
4. **DRY (Don't Repeat Yourself)** - Reusable components and services
5. **SOLID Principles** - Throughout the codebase

---

## Architecture Layers

### 1. Domain Layer (`LearningTool.Domain`)

**Purpose:** Core business entities and interfaces. Has NO dependencies on other layers.

**Components:**

#### Entities (`Entities/`)
- `Skill.cs` - Learning skill entity
  ```csharp
  public class Skill
  {
      public int Id { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public DifficultyLevel Difficulty { get; set; }
      public bool IsDeleted { get; set; }
      public DateTime CreatedAt { get; set; }
      public ICollection<Topic> Topics { get; set; }
  }
  ```

- `Topic.cs` - Learning topic under skill
- `Course.cs` - Individual course
- `UserSkill.cs` - User's skill associations
- `UserTopic.cs` - User's topic progress
- `UserCourse.cs` - User's course progress
- `ChatMessage.cs` - Chat history

#### Enums (`Enums/`)
- `DifficultyLevel` - Beginner, Intermediate, Advanced
- `LearningStatus` - Interested, Learning, Mastered
- `CourseStatus` - NotStarted, InProgress, Completed
- `ResourceType` - YouTube, Documentation, Tutorial, Article, Book
- `MessageRole` - User, Assistant, System

#### Interfaces (`Interfaces/`)
- `ISkillRepository`
- `ITopicRepository`
- `ICourseRepository`
- `IUserSkillRepository`
- `IUserTopicRepository`
- `IUserCourseRepository`
- `IChatMessageRepository`

**Key Design Decisions:**
- Entities are pure POCOs (Plain Old CLR Objects)
- No dependencies on frameworks
- Business rules embedded in entities
- Soft delete pattern (IsDeleted flag)

---

### 2. Application Layer (`LearningTool.Application`)

**Purpose:** Business logic and orchestration. Depends only on Domain layer.

**Components:**

#### Services (`Services/`)

**KnowledgeService** - Manages the knowledge catalog
```csharp
public interface IKnowledgeService
{
    Task<Skill> AddSkillToCatalogAsync(string name, string description, DifficultyLevel difficulty);
    Task<Skill?> FindOrCreateSkillAsync(string name);
    Task<List<Skill>> SearchSkillsAsync(string query);
    Task<Topic> AddTopicAsync(int skillId, string name, string description);
    Task<Topic?> FindOrCreateTopicAsync(int skillId, string name);
    Task<Course> AddCourseAsync(int topicId, string name, string description, ...);
}
```

**UserLearningService** - Manages user's learning progress
```csharp
public interface IUserLearningService
{
    Task<UserSkill> AddSkillToUserAsync(string userId, int skillId);
    Task RemoveSkillFromUserAsync(string userId, int skillId);
    Task<List<UserSkill>> GetUserSkillsAsync(string userId);
    Task<UserCourse> StartCourseAsync(string userId, int courseId);
    Task<UserCourse> CompleteCourseAsync(string userId, int courseId, int score);
}
```

**ChatService** - Handles AI conversation and tool calling
```csharp
public interface IChatService
{
    Task<ChatResponse> ProcessMessageAsync(string userId, string message);
    Task<List<ChatMessage>> GetChatHistoryAsync(string userId, int limit = 50);
}
```

#### DTOs (`DTOs/`)
- `ChatRequest`, `ChatResponse` - Chat communication
- `ToolCall`, `ToolResult` - AI tool calling
- Request/Response models for API

**Key Design Decisions:**
- Services coordinate between repositories
- Business logic lives here, not in controllers
- DTOs prevent entity exposure to API layer
- Validation happens in services

---

### 3. Infrastructure Layer (`LearningTool.Infrastructure`)

**Purpose:** Data access, external services. Depends on Domain and Application.

**Components:**

#### DbContext (`Data/`)

**LearningToolDbContext.cs** - EF Core context
```csharp
public class LearningToolDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Skill> Skills { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<UserSkill> UserSkills { get; set; }
    public DbSet<UserTopic> UserTopics { get; set; }
    public DbSet<UserCourse> UserCourses { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Soft delete global query filters
        modelBuilder.Entity<Skill>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Topic>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);

        // JSON converters for complex types
        // Cascade delete configurations
        // Indexes for performance
    }
}
```

#### Repositories (`Repositories/`)
Each repository implements its interface from Domain:
- `SkillRepository` - CRUD + search operations
- `TopicRepository` - Topic management
- `CourseRepository` - Course operations
- `UserSkillRepository` - User skill associations
- `UserTopicRepository` - User topic progress
- `UserCourseRepository` - Course progress tracking
- `ChatMessageRepository` - Message history

**Repository Pattern:**
```csharp
public class SkillRepository : ISkillRepository
{
    private readonly LearningToolDbContext _context;

    public async Task<Skill> CreateAsync(Skill entity)
    {
        _context.Skills.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<List<Skill>> GetAllAsync()
    {
        return await _context.Skills
            .Include(s => s.Topics)
                .ThenInclude(t => t.Courses)
            .ToListAsync();
    }
}
```

#### Data Seeding (`Data/`)

**DataSeeder.cs** - Populates initial data
- 5 skills (ML, Web Dev, Data Science, Cloud, Mobile)
- 8 topics across skills
- 5 courses with real resources
- Runs on first startup in development

**Key Design Decisions:**
- Repository pattern abstracts data access
- Soft delete via global query filters
- Eager loading for complex queries
- JSON storage for flexible data (Prerequisites, ResourceLinks)
- Auto-migration in development

---

### 4. API Layer (`LearningTool.API`)

**Purpose:** HTTP endpoints, authentication, request handling.

**Components:**

#### Controllers (`Controllers/`)

**AuthController** - Authentication endpoints
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
}
```

**SkillsController** - Skill management
```csharp
[Authorize]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    [HttpGet("catalog")]
    public async Task<IActionResult> SearchCatalog([FromQuery] string? query)

    [HttpGet("my-skills")]
    public async Task<IActionResult> GetMySkills()

    [HttpPost]
    public async Task<IActionResult> AddSkill([FromBody] AddSkillRequest request)

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveSkill(int id)
}
```

**TopicsController** - Topic operations
**CoursesController** - Course management
**ChatController** - AI chat interface

#### Program.cs - Dependency Injection

**Service Registration:**
```csharp
// Database
builder.Services.AddDbContext<LearningToolDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity + JWT
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<LearningToolDbContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT config */ });

// Repositories
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
// ... 6 more repositories

// Services
builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<IUserLearningService, UserLearningService>();
builder.Services.AddScoped<IChatService, ChatService>();

// OpenAI
builder.Services.AddHttpClient();
// OpenAI API key from configuration
```

**Key Design Decisions:**
- Controllers are thin (delegate to services)
- Claims-based authorization
- JWT token authentication
- CORS configuration for frontend
- Swagger for API documentation
- Dependency injection for all components

---

## Database Schema

### Entity Relationship Diagram

```
┌─────────────┐
│    User     │ (ASP.NET Identity)
│ (Identity)  │
└──────┬──────┘
       │
       ├──────────────┬──────────────┬──────────────┐
       │              │              │              │
       v              v              v              v
┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│  UserSkill  │ │  UserTopic  │ │ UserCourse  │ │ChatMessage  │
└──────┬──────┘ └──────┬──────┘ └──────┬──────┘ └─────────────┘
       │              │              │
       v              v              v
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│    Skill    │ │    Topic    │ │   Course    │
└──────┬──────┘ └──────┬──────┘ └─────────────┘
       │              │
       │              │
       └──────────────┘
```

### Tables

**Skills** - Knowledge catalog
- Id (PK, int)
- Name (string, indexed)
- Description (string)
- Difficulty (enum)
- IsDeleted (bool)
- CreatedAt (DateTime)

**Topics** - Learning topics
- Id (PK, int)
- SkillId (FK to Skills)
- Name (string)
- Description (string)
- IsDeleted (bool)
- CreatedAt (DateTime)

**Courses** - Individual courses
- Id (PK, int)
- TopicId (FK to Topics)
- Name (string)
- Description (string)
- Content (text, nullable)
- EstimatedMinutes (int)
- Prerequisites (JSON array)
- ResourceLinks (JSON array)
- IsDeleted (bool)
- CreatedAt (DateTime)

**UserSkills** - User's skill associations
- Id (PK, int)
- UserId (FK to Identity)
- SkillId (FK to Skills)
- Status (enum: Interested, Learning, Mastered)
- AddedAt (DateTime)
- IsDeleted (bool)

**UserTopics** - User's topic progress
- Id (PK, int)
- UserId (FK)
- TopicId (FK)
- Status (enum)
- AddedAt (DateTime)
- IsDeleted (bool)

**UserCourses** - Course progress
- Id (PK, int)
- UserId (FK)
- CourseId (FK)
- Status (enum: NotStarted, InProgress, Completed)
- StartedAt (DateTime, nullable)
- CompletedAt (DateTime, nullable)
- Score (int, nullable)
- IsDeleted (bool)

**ChatMessages** - Conversation history
- Id (PK, int)
- UserId (FK)
- Role (string: user/assistant/system)
- Content (text)
- ToolCalls (JSON, nullable)
- Timestamp (DateTime)

**Indexes:**
- Skills.Name (for search)
- UserSkills.UserId + SkillId (composite)
- UserCourses.UserId + Status (for filtering)
- ChatMessages.UserId + Timestamp (for history queries)

---

## API Design

### REST Principles

1. **Resource-based URLs** - `/api/skills`, `/api/courses/{id}`
2. **HTTP verbs** - GET (read), POST (create), PUT (update), DELETE (soft delete)
3. **Status codes** - 200 (OK), 201 (Created), 400 (Bad Request), 401 (Unauthorized), 404 (Not Found)
4. **JSON payloads** - All requests and responses use JSON

### Authentication Flow

```
1. User registers/logs in
   POST /api/auth/register or /api/auth/login
   → Returns JWT token + user info

2. Client stores token
   localStorage.setItem('token', token)

3. Subsequent requests include token
   Authorization: Bearer <token>

4. API validates token
   JWT middleware extracts claims
   → User identity available in controller
```

### Endpoint Categories

**Authentication** (`/api/auth`)
- POST `/register` - Create account
- POST `/login` - Get JWT token

**Skills** (`/api/skills`)
- GET `/catalog?query=search` - Search catalog
- GET `/my-skills` - User's skills
- POST `/` - Add skill to user
- DELETE `/{id}` - Remove skill

**Topics** (`/api/topics`)
- GET `/skill/{skillId}` - Topics for skill
- GET `/my-topics` - User's topics
- POST `/` - Create topic
- DELETE `/{id}` - Remove topic

**Courses** (`/api/courses`)
- GET `/{id}` - Course details
- GET `/in-progress` - Active courses
- GET `/completed` - Finished courses
- POST `/{id}/start` - Begin course
- POST `/{id}/complete` - Finish with score

**Chat** (`/api/chat`)
- POST `/message` - Send to AI
- GET `/history?limit=50` - Get history

### Request/Response Examples

**Register User:**
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}

Response 200:
{
  "token": "eyJhbGc...",
  "user": {
    "id": "user-guid",
    "email": "user@example.com",
    "userName": "user@example.com"
  }
}
```

**Chat with AI:**
```http
POST /api/chat/message
Authorization: Bearer <token>
Content-Type: application/json

{
  "message": "I want to learn Machine Learning"
}

Response 200:
{
  "message": "Great choice! Let me add Machine Learning...",
  "toolCalls": [{
    "id": "call-123",
    "toolName": "add_skill",
    "arguments": { "name": "Machine Learning" }
  }],
  "requiresAction": true,
  "toolResults": [{
    "toolCallId": "call-123",
    "success": true,
    "result": "Added skill: Machine Learning",
    "data": { /* UserSkill object */ }
  }]
}
```

---

## Frontend Architecture

### Component Structure

```
src/
├── components/
│   ├── Layout.tsx          # Navigation wrapper
│   ├── TreeView.tsx        # Hierarchical display
│   └── ChatInterface.tsx   # AI chat UI
├── pages/
│   ├── LoginPage.tsx       # Authentication
│   ├── RegisterPage.tsx    # Sign up
│   ├── ChatPage.tsx        # Main dashboard
│   ├── SkillsPage.tsx      # Skills management
│   ├── TimelinePage.tsx    # Completed courses
│   └── CoursePage.tsx      # Course details
├── services/
│   ├── api.ts              # Axios instance
│   ├── authService.ts      # Auth API calls
│   ├── knowledgeService.ts # Skills/Topics/Courses
│   └── chatService.ts      # Chat API
└── stores/
    └── authStore.ts        # Zustand auth state
```

### State Management

**Zustand Store Pattern:**
```typescript
interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<boolean>;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,

      login: async (email, password) => {
        const data = await authService.login({ email, password });
        set({
          user: data.user,
          token: data.token,
          isAuthenticated: true
        });
        return true;
      },

      logout: () => {
        localStorage.removeItem('token');
        set({ user: null, token: null, isAuthenticated: false });
      }
    }),
    { name: 'auth-storage' }
  )
);
```

### API Service Pattern

**Axios Configuration:**
```typescript
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL
});

// Request interceptor - add token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor - handle 401
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Redirect to login
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

### Routing

**React Router v7:**
```typescript
<BrowserRouter>
  <Routes>
    <Route path="/login" element={<LoginPage />} />
    <Route path="/register" element={<RegisterPage />} />
    <Route path="/" element={<ProtectedRoute><Layout /></ProtectedRoute>}>
      <Route index element={<Navigate to="/chat" />} />
      <Route path="chat" element={<ChatPage />} />
      <Route path="skills" element={<SkillsPage />} />
      <Route path="timeline" element={<TimelinePage />} />
      <Route path="course/:courseId" element={<CoursePage />} />
    </Route>
  </Routes>
</BrowserRouter>
```

---

## Authentication Flow

### Complete Authentication Sequence

```
1. User Registration
   ┌─────────┐    POST /api/auth/register    ┌──────────┐
   │ Client  │──────────────────────────────>│   API    │
   └─────────┘                                └────┬─────┘
       │                                           │
       │                                           v
       │                                    Create Identity User
       │                                           │
       │                                           v
       │                                    Generate JWT Token
       │                                           │
       │    { token, user }                        │
       │<──────────────────────────────────────────┘
       │
       v
   Store in localStorage + Zustand

2. Subsequent Requests
   ┌─────────┐    GET /api/skills/my-skills  ┌──────────┐
   │ Client  │──────────────────────────────>│   API    │
   │         │   Authorization: Bearer <token>│          │
   └─────────┘                                └────┬─────┘
       │                                           │
       │                                           v
       │                                    JWT Middleware
       │                                           │
       │                                           v
       │                                    Extract User Claims
       │                                           │
       │                                           v
       │                                    Controller Action
       │                                           │
       │    Skills data                            │
       │<──────────────────────────────────────────┘
```

### JWT Token Structure

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-guid",
    "email": "user@example.com",
    "jti": "token-id",
    "exp": 1234567890,
    "iss": "LearningTool",
    "aud": "LearningTool"
  },
  "signature": "..."
}
```

---

## AI Chat System

### Architecture

```
User Message
    │
    v
┌─────────────────────┐
│  ChatController     │
│  - Receives message │
│  - Gets user ID     │
└──────────┬──────────┘
           │
           v
┌─────────────────────┐
│   ChatService       │
│  - Save user msg    │
│  - Process intent   │
│  - Generate response│
│  - Execute tools    │
└──────────┬──────────┘
           │
           ├──> OpenAI API (optional)
           │
           v
┌─────────────────────┐
│   Tool Execution    │
│  - add_skill        │
│  - remove_skill     │
│  - add_topic        │
│  - get_user_skills  │
└──────────┬──────────┘
           │
           v
    Return response
```

### Tool Calling System

**Available Tools:**

1. **add_skill**
   - Input: `{ name: string, description?: string }`
   - Action: Find or create skill in catalog, add to user
   - Output: UserSkill object

2. **remove_skill**
   - Input: `{ skillId: number }`
   - Action: Soft delete user's skill association
   - Output: Success confirmation

3. **add_topic**
   - Input: `{ skillId: number, name: string, description?: string }`
   - Action: Create topic under skill
   - Output: Topic object

4. **get_user_skills**
   - Input: None
   - Action: Retrieve user's current skills
   - Output: List of UserSkill objects

**Tool Execution Flow:**
```csharp
private async Task<ToolResult> ExecuteToolAsync(string userId, ToolCall toolCall)
{
    switch (toolCall.ToolName)
    {
        case "add_skill":
            var name = toolCall.Arguments["name"].ToString();
            var skill = await _knowledgeService.FindOrCreateSkillAsync(name);
            var userSkill = await _userLearningService.AddSkillToUserAsync(userId, skill.Id);
            return new ToolResult {
                Success = true,
                Result = $"Added skill: {name}",
                Data = userSkill
            };
    }
}
```

### OpenAI Integration Points

**Configuration** (`appsettings.json`):
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Model": "gpt-4",
    "MaxTokens": 500,
    "Temperature": 0.7
  }
}
```

**Service Implementation:**
```csharp
public class OpenAIChatService : IChatService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public async Task<ChatResponse> ProcessMessageAsync(string userId, string message)
    {
        var client = _httpClientFactory.CreateClient();
        var apiKey = _configuration["OpenAI:ApiKey"];

        // Build messages array with history
        var messages = await BuildMessagesAsync(userId, message);

        // Call OpenAI API with function calling
        var request = new {
            model = "gpt-4",
            messages = messages,
            functions = GetAvailableFunctions(),
            function_call = "auto"
        };

        // Process response and execute tools
        // Return ChatResponse
    }
}
```

---

## Data Flow

### Complete Request Flow Example

**User adds skill via chat:**

```
1. Frontend
   User types: "I want to learn Python"
   ↓
   chatService.sendMessage("I want to learn Python")
   ↓
   POST /api/chat/message
   Headers: { Authorization: Bearer <token> }
   Body: { message: "I want to learn Python" }

2. API Layer
   ChatController.SendMessage()
   ↓
   Extract userId from JWT claims
   ↓
   chatService.ProcessMessageAsync(userId, message)

3. Application Layer
   ChatService
   ↓
   Save user message to database
   ↓
   Analyze message → detect skill intent
   ↓
   Extract skill name: "Python"
   ↓
   Create ToolCall: add_skill("Python")
   ↓
   Execute tool

4. Tool Execution
   knowledgeService.FindOrCreateSkillAsync("Python")
   ↓
   Check if "Python" exists in catalog
   ↓
   Create if not exists
   ↓
   userLearningService.AddSkillToUserAsync(userId, skillId)
   ↓
   Create UserSkill association
   ↓
   Return UserSkill

5. Response Construction
   ChatResponse {
     message: "Added Python to your learning path!",
     toolCalls: [{ id, toolName: "add_skill", ... }],
     toolResults: [{ success: true, data: UserSkill }]
   }
   ↓
   Save assistant message to database
   ↓
   Return to controller

6. Frontend
   Receive response
   ↓
   Update UI: show message + tool result
   ↓
   Refresh stats dashboard
   ↓
   Skill count increases
```

---

## Deployment Architecture

### Development Environment

```
┌──────────────┐     HTTP :5173     ┌──────────────┐
│   Vite Dev   │<──────────────────>│   Browser    │
│   Server     │                     └──────────────┘
└──────┬───────┘
       │ Proxy
       │ /api → localhost:5001
       v
┌──────────────┐     HTTPS :5001
│  ASP.NET API │
│   Kestrel    │
└──────┬───────┘
       │
       v
┌──────────────┐
│   SQLite     │
│   File DB    │
└──────────────┘
```

### Production Environment

```
                  ┌──────────────┐
                  │   CDN/Static │
                  │   (Frontend) │
                  └──────┬───────┘
                         │
                         v
┌──────────────┐     HTTPS      ┌──────────────┐
│   Nginx/     │<──────────────>│   Users      │
│   Reverse    │                 └──────────────┘
│   Proxy      │
└──────┬───────┘
       │
       │ Forward to API
       v
┌──────────────┐
│  ASP.NET API │
│   Kestrel    │
│   (Docker)   │
└──────┬───────┘
       │
       v
┌──────────────┐
│  SQL Server  │
│  /PostgreSQL │
└──────────────┘
```

### Docker Configuration

**Backend Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LearningTool.API/LearningTool.API.csproj", "LearningTool.API/"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LearningTool.API.dll"]
```

**Frontend Build:**
```bash
npm run build
# Outputs to dist/
# Deploy to CDN or static hosting
```

### Environment Variables

**Backend** (`.env` or `appsettings.Production.json`):
```
ConnectionStrings__DefaultConnection=<database-connection>
Jwt__Key=<jwt-secret-key>
Jwt__Issuer=LearningTool
Jwt__Audience=LearningTool
Jwt__ExpiryDays=7
OpenAI__ApiKey=<openai-api-key>
Authentication__Google__ClientId=<google-oauth-client-id>
Authentication__Google__ClientSecret=<google-oauth-secret>
```

**Frontend** (`.env.production`):
```
VITE_API_URL=https://api.yourproductiondomain.com/api
```

---

## Security Considerations

### Authentication
- JWT tokens with expiration
- Secure password hashing (ASP.NET Identity)
- HTTPS only in production
- CORS restricted to known origins

### Authorization
- Claims-based authorization
- User can only access own data
- Admin roles for catalog management

### Data Protection
- SQL injection prevention (EF Core parameterized queries)
- XSS prevention (React auto-escapes)
- CSRF protection (SameSite cookies)
- Soft delete (audit trail)

### API Security
- Rate limiting (add in production)
- API key rotation
- Secrets in environment variables
- No sensitive data in logs

---

## Performance Optimizations

### Database
- Indexes on frequently queried columns
- Eager loading for related entities
- Query filters for soft delete
- Connection pooling

### API
- Async/await throughout
- Efficient repository patterns
- Minimal data transfer (DTOs)
- Response caching where appropriate

### Frontend
- Code splitting (React lazy loading)
- Asset optimization (Vite)
- State persistence (Zustand + localStorage)
- Optimistic UI updates

---

## Monitoring & Logging

### Backend Logging
```csharp
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    config.AddApplicationInsights(); // Production
});
```

### Error Tracking
- Application Insights (Azure)
- Sentry (for exceptions)
- Custom error middleware

### Metrics
- API response times
- Database query performance
- User engagement (courses completed)
- Chat interactions

---

## Future Enhancements

### Phase 1 (Immediate)
- OpenAI GPT-4 integration
- Google OAuth completion
- Advanced course content editor

### Phase 2 (Near-term)
- Real-time notifications (SignalR)
- Course recommendations engine
- Progress analytics dashboard
- Mobile app (React Native)

### Phase 3 (Long-term)
- Video content integration
- Live instructor sessions
- Certification system
- Social learning features
- Gamification (badges, leaderboards)

---

## Appendices

### A. Code Style Guide
- C# naming conventions
- TypeScript/React patterns
- Comment standards
- Git commit messages

### B. Database Migration Guide
- Creating migrations
- Rolling back
- Production deployment

### C. Testing Strategy
- Unit tests (xUnit)
- Integration tests
- E2E tests (Playwright)
- Manual testing checklist

### D. Troubleshooting
- Common issues
- Debug procedures
- Support contacts

---

**Document Version:** 1.0
**Last Updated:** February 20, 2026
**Maintained by:** Development Team
