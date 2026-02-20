# LearningTool - Getting Started

## Overview

LearningTool is an AI-powered learning management system that helps users learn new skills through structured courses with AI assistance.

## Features

### ✅ Complete Implementation

1. **Google OAuth Authentication**
   - User registration and login
   - JWT token-based authentication
   - Secure session management

2. **AI Chat Agent with Tool Calling**
   - Natural language interaction
   - Automatic skill detection and creation
   - Tool calling capabilities:
     - `add_skill`: Create and add skills to user's learning path
     - `remove_skill`: Remove skills from user
     - `add_topic`: Create topics under skills
     - `get_user_skills`: Retrieve user's learning progress

3. **Knowledge Hierarchy (Skills → Topics → Courses)**
   - Three-level learning structure
   - Centralized knowledge catalog
   - User-specific progress tracking
   - Soft deletion (IsDeleted flag)

4. **Course Taking System**
   - View course details and prerequisites
   - Access learning resources (videos, articles, documentation, exercises)
   - Start/complete courses with progress tracking
   - Self-assessment scoring (0-100%)
   - Completion timestamps

5. **Two UI Views**
   - **Tree View**: Hierarchical expandable view of Skills → Topics → Courses
   - **Timeline View**: Chronological display of completed courses with scores

6. **Clean Architecture**
   - Domain Layer (entities, interfaces)
   - Application Layer (services, DTOs)
   - Infrastructure Layer (repositories, EF Core)
   - API Layer (controllers, authentication)
   - Frontend Layer (React, TypeScript, Tailwind)

## Technology Stack

### Backend
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8.0
- SQLite (for simplicity, easily swappable)
- ASP.NET Core Identity + JWT
- Google OAuth (ready for configuration)

### Frontend
- React 19
- TypeScript
- Vite
- Zustand (state management)
- React Router v7
- Tailwind CSS
- Axios

## Project Structure

```
learningtool/
├── src/
│   ├── LearningTool.Domain/          # Entities, interfaces, enums
│   ├── LearningTool.Application/     # Services, business logic
│   ├── LearningTool.Infrastructure/  # Data access, repositories
│   └── LearningTool.API/             # Controllers, authentication
├── frontend/
│   ├── src/
│   │   ├── components/   # Reusable UI components
│   │   ├── pages/        # Page components
│   │   ├── services/     # API services
│   │   └── stores/       # Zustand stores
│   └── public/
├── ARCHITECTURE.md       # Detailed system design
└── GETTING_STARTED.md    # This file
```

## Running the Application

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+ and npm
- Git

### Backend Setup

1. Navigate to API directory:
   ```bash
   cd C:\Projects\learningtool\src\LearningTool.API
   ```

2. Run the API:
   ```bash
   dotnet run
   ```

   The API will start at:
   - HTTPS: https://localhost:5001
   - HTTP: http://localhost:5000

3. Access Swagger UI:
   - Open browser to https://localhost:5001/swagger

### Frontend Setup

1. Navigate to frontend directory:
   ```bash
   cd C:\Projects\learningtool\frontend
   ```

2. Install dependencies (if not already done):
   ```bash
   npm install
   ```

3. Run the development server:
   ```bash
   npm run dev
   ```

   The frontend will start at:
   - http://localhost:5173

### First Use

1. Open http://localhost:5173 in your browser
2. Click "Sign up" to create an account
3. Log in with your credentials
4. You'll be greeted by the AI learning assistant!

## Using the System

### Chat with AI Assistant
- Type messages like "I want to learn Machine Learning"
- The AI will detect skills and automatically add them to your learning path
- Ask about your progress: "How am I doing?"

### Manage Skills
- Navigate to "My Skills" to see your learning hierarchy
- Expand skills to see topics
- Expand topics to see courses
- Click "View Course" to start learning

### Take Courses
- View course details, prerequisites, and resources
- Click "Start Learning" to begin tracking progress
- Complete the course with a self-assessment score
- View your timeline to see all completed courses

### Timeline
- See all your completed courses chronologically
- View scores and completion dates
- Track your learning journey

## Database

The system uses SQLite with Entity Framework Core migrations.

Database location: `src/LearningTool.API/learningtool.db`

### Migrations
To create a new migration:
```bash
cd src/LearningTool.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../LearningTool.API
```

## API Endpoints

### Authentication
- POST `/api/auth/register` - Register new user
- POST `/api/auth/login` - Login and get JWT token

### Skills
- GET `/api/skills/catalog?query={search}` - Search skills catalog
- GET `/api/skills/my-skills` - Get user's skills
- POST `/api/skills` - Add skill to user
- DELETE `/api/skills/{id}` - Remove skill (soft delete)

### Topics
- GET `/api/topics/skill/{skillId}` - Get topics for skill
- GET `/api/topics/my-topics` - Get user's topics
- POST `/api/topics` - Create new topic
- DELETE `/api/topics/{id}` - Remove topic (soft delete)

### Courses
- GET `/api/courses/{id}` - Get course details
- GET `/api/courses/in-progress` - Get courses in progress
- GET `/api/courses/completed` - Get completed courses
- POST `/api/courses/{id}/start` - Start course
- POST `/api/courses/{id}/complete` - Complete course with score

### Chat
- POST `/api/chat/message` - Send message to AI
- GET `/api/chat/history?limit={n}` - Get chat history

## Future Enhancements

While all core requirements are implemented, here are potential improvements:

1. **OpenAI Integration**: Replace rule-based chat with GPT-4 for advanced conversations
2. **Real Knowledge Assessment**: Interactive quizzes and assessments
3. **Course Content Creation**: Rich text editor for course materials
4. **Google OAuth**: Complete OAuth configuration with client ID/secret
5. **Social Features**: Share progress, compete with friends
6. **Recommendations**: AI-powered course recommendations
7. **Certificates**: Generate completion certificates
8. **Mobile App**: React Native version

## ClickUp Board

Project management: See ClickUp board "LearningTool" for task tracking and roadmap.

## Contributing

This is a learning management system built with clean architecture principles.
Feel free to extend and customize for your needs!

## License

[Your License Here]
