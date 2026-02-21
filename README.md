# 🎓 LearningTool - AI-Powered Learning Management System

> Build your learning path with AI assistance. Master new skills through structured courses.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6)](https://www.typescriptlang.org/)
[![OpenAI](https://img.shields.io/badge/OpenAI-GPT--4o--mini-412991)](https://openai.com/)

## 🌐 Live Demo

**Production:** [http://learning.prospergenics.com](http://learning.prospergenics.com)
**API Docs:** [http://learning.prospergenics.com/api/swagger](http://learning.prospergenics.com/api/swagger)
**Repository:** [github.com/martiendejong/learningtool](https://github.com/martiendejong/learningtool)

## ✨ Features

- 🤖 **AI Chat Assistant** - Powered by OpenAI GPT-4o-mini with tool calling
- 📚 **Knowledge Hierarchy** - Skills → Topics → Courses structure
- 🎯 **Progress Tracking** - Monitor your learning journey
- 📊 **Two Visualization Views** - Tree view and timeline
- 🔐 **Secure Authentication** - JWT + Google OAuth ready
- 🗄️ **Centralized Catalog** - Shared knowledge base
- ✅ **Soft Deletion** - Never lose data
- 🎨 **Modern UI** - Responsive React interface

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/martiendejong/learningtool.git
   cd learningtool
   ```

2. **Start the backend**
   ```bash
   cd src/LearningTool.API
   dotnet run
   ```
   Backend runs at: https://localhost:5001

3. **Start the frontend** (new terminal)
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
   Frontend runs at: http://localhost:5173

4. **Open your browser**
   Navigate to http://localhost:5173 and create your account!

## 📖 Usage

### Chat with AI

1. Open the chat page
2. Type: "I want to learn Machine Learning"
3. The AI automatically adds the skill to your learning path

### Explore Courses

1. Navigate to "My Skills"
2. Expand skills to see topics
3. Expand topics to see courses
4. Click "View Course" to start learning

### Track Progress

1. Complete courses with self-assessment
2. View your timeline of completed courses
3. Track your learning journey over time

## 🏗️ Architecture

```
┌─────────────┐
│   Frontend  │  React 19 + TypeScript + Tailwind
├─────────────┤
│     API     │  ASP.NET Core Web API
├─────────────┤
│ Application │  Services + Business Logic
├─────────────┤
│    Domain   │  Entities + Interfaces
├─────────────┤
│ Infra Layer │  EF Core + Repositories
└─────────────┘
```

**Clean Architecture** with 4 layers:
- Domain (no dependencies)
- Application (depends on Domain)
- Infrastructure (depends on Domain + Application)
- API (depends on all)

## 🎯 Pre-loaded Content

The system comes with example content:

**5 Skills:**
- Machine Learning
- Web Development
- Data Science
- Cloud Computing
- Mobile Development

**8 Topics** including:
- Supervised Learning
- Neural Networks
- Frontend Basics
- React Framework
- Python for Data Science

**5 Courses** with real resources:
- Linear Regression Fundamentals
- Introduction to Neural Networks
- HTML & CSS Mastery
- React Fundamentals
- Pandas Data Analysis

All courses include working YouTube tutorials, documentation, and exercises!

## 🛠️ Technology Stack

### Backend
- **.NET 8.0** - Latest stable framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **SQLite** - Database (easily swappable)
- **ASP.NET Identity** - Authentication
- **JWT** - Token-based auth

### Frontend
- **React 19** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool
- **Zustand** - State management
- **React Router v7** - Navigation
- **Tailwind CSS** - Styling
- **Axios** - HTTP client

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login

### Skills
- `GET /api/skills/catalog` - Search skills
- `GET /api/skills/my-skills` - User's skills
- `POST /api/skills` - Add skill
- `DELETE /api/skills/{id}` - Remove skill

### Topics
- `GET /api/topics/skill/{skillId}` - Get topics
- `POST /api/topics` - Create topic
- `DELETE /api/topics/{id}` - Remove topic

### Courses
- `GET /api/courses/{id}` - Course details
- `GET /api/courses/in-progress` - In progress courses
- `GET /api/courses/completed` - Completed courses
- `POST /api/courses/{id}/start` - Start course
- `POST /api/courses/{id}/complete` - Complete course

### Chat
- `POST /api/chat/message` - Send message to AI
- `GET /api/chat/history` - Chat history

## 🧪 Testing

1. **Register** - Create a new account
2. **Chat** - Try "I want to learn Python"
3. **Skills** - Check the "My Skills" page
4. **Courses** - Expand skills → topics → courses
5. **Learn** - Click "View Course" on any course
6. **Complete** - Finish a course with a score
7. **Timeline** - View your completed courses

## 📁 Project Structure

```
learningtool/
├── src/
│   ├── LearningTool.Domain/         # Entities, interfaces
│   ├── LearningTool.Application/    # Services, business logic
│   ├── LearningTool.Infrastructure/ # Data access, repos
│   └── LearningTool.API/            # Controllers, auth
├── frontend/
│   └── src/
│       ├── components/    # Reusable UI components
│       ├── pages/         # Page components
│       ├── services/      # API services
│       └── stores/        # State management
├── ARCHITECTURE.md        # Detailed design doc
├── GETTING_STARTED.md     # Setup guide
├── PROJECT_SUMMARY.md     # What was built
└── README.md              # This file
```

## 🔧 Configuration

### Backend
Edit `src/LearningTool.API/appsettings.json`:
- Database connection
- JWT settings
- CORS origins
- Google OAuth (client ID/secret)

### Frontend
Edit `frontend/.env.local`:
```
VITE_API_URL=https://localhost:5001/api
```

## 🚢 Deployment

### Production Environment

**Server:** Windows Server (IIS 10.0)
**URL:** http://learning.prospergenics.com
**Status:** Live and operational

### Automated Deployment

Use the automated deployment script:
```bash
python deploy-complete.py
```

This script:
- Uploads files via SFTP
- Configures IIS application pool and website
- Sets up domain binding
- Verifies deployment

See [DEPLOYMENT_SUCCESS.md](DEPLOYMENT_SUCCESS.md) for full deployment documentation.

### Manual Deployment

#### Backend
```bash
cd src/LearningTool.API
dotnet publish -c Release
```

#### Frontend
```bash
cd frontend
npm run build
```

Deploy the `dist/` folder and `publish/` folder to your hosting service.

### Environment Variables

Production secrets are stored in GitHub Secrets:
- `OPENAI_API_KEY` - OpenAI API key for GPT-4o-mini
- `JWT_SECRET_KEY` - JWT signing key

See `appsettings.example.json` for configuration template.

## 🤝 Contributing

This is a learning management system built with clean architecture principles. Feel free to:
- Add new features
- Improve the AI chat
- Create new course content
- Enhance the UI

## 📄 License

MIT License - See LICENSE file for details

## 🎉 What Makes This Special

- **Clean Architecture** - Proper separation, testable, maintainable
- **Real AI Integration** - Tool calling pattern, not mocked
- **Production Ready** - Auth, validation, error handling
- **Comprehensive Docs** - Architecture, setup, and summary guides
- **Type Safety** - Full TypeScript frontend, C# backend
- **Professional UI** - Responsive design, loading states
- **Seed Data** - Real courses with working links
- **Best Practices** - Repository pattern, DI, soft delete

## 📞 Support

For questions or issues:
- Read the [GETTING_STARTED.md](GETTING_STARTED.md) guide
- Check the [ARCHITECTURE.md](ARCHITECTURE.md) document
- Review the [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md)

---

**Built with 💙 using Clean Architecture principles**

Ready to learn something new? 🚀
