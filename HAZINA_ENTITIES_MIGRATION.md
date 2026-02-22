# Hazina Entities.yaml Migration Plan

## Current Functionality Analysis

### Domain Entities
1. **Skill** - Learning skill (e.g., "Web Development", "AI")
2. **Topic** - Sub-area within skill (e.g., "React Basics", "ChatGPT Usage")
3. **Course** - Actual learning content with AI-generated curriculum
4. **UserSkill** - User's learning path (skills they're learning)
5. **UserCourse** - User's course progress (status, minutes spent)
6. **ChatMessage** - Chat history (general + course-specific teaching)
7. **ResourceLink** - External learning resources (embedded in Course)

### Current Features (Must Preserve)
- ✅ Skills CRUD
- ✅ Topics CRUD (linked to Skills)
- ✅ Courses CRUD (linked to Topics)
- ✅ User learning paths (add/remove skills)
- ✅ Course progress tracking (status, time spent)
- ✅ Chat messages (general + course-specific)
- ✅ Library search (find existing skills/courses)
- ✅ AI course content generation (learning plan, system prompt, resources)
- ✅ Course teaching mode (separate chat per course)
- ✅ Soft delete (IsDeleted flag)

### Features We'll IMPROVE with Hazina
- 🚀 **Better Search** - RAG embeddings for semantic search
- 🚀 **Access Control** - Role-based permissions (Admin, User)
- 🚀 **Real-time** - SignalR updates when courses added
- 🚀 **Bulk Operations** - Import multiple courses at once
- 🚀 **Export** - CSV/Excel export of learning data
- 🚀 **API Discoverability** - Auto-generated Swagger docs

## Entities.yaml Design

### Skill Entity
```yaml
- name: Skill
  description: A learning skill (e.g., Web Development, AI)
  fields:
    - name: name
      type: string
      required: true
      maxLength: 200
      searchable: true
      indexed: true
    - name: description
      type: text
      searchable: true
    - name: difficulty
      type: enum
      description: "Beginner, Intermediate, Advanced"
      defaultValue: "Beginner"
  features:
    crud: true
    search: true
    embedding: true  # RAG search for skills
    softDelete: true
    pagination: true
    filtering: true
    sorting: true
```

### Topic Entity
```yaml
- name: Topic
  description: A topic within a skill
  fields:
    - name: skillId
      type: reference
      referencesEntity: Skill
      required: true
    - name: name
      type: string
      required: true
      maxLength: 200
      searchable: true
    - name: description
      type: text
      searchable: true
  features:
    crud: true
    search: true
    softDelete: true
```

### Course Entity
```yaml
- name: Course
  description: A course with AI-generated learning content
  fields:
    - name: topicId
      type: reference
      referencesEntity: Topic
      required: true
    - name: name
      type: string
      required: true
      maxLength: 200
      searchable: true
    - name: description
      type: text
      searchable: true
    - name: content
      type: text
      description: Course content/curriculum
    - name: learningPlan
      type: text
      description: AI-generated structured curriculum (markdown)
    - name: systemPrompt
      type: text
      description: AI teacher instructions for this course
    - name: estimatedMinutes
      type: int
      defaultValue: "60"
    - name: prerequisites
      type: json
      description: Array of prerequisite course names
    - name: resourceLinks
      type: json
      description: External learning resources (YouTube, docs, etc.)
    - name: contentGeneratedAt
      type: dateTime
      description: When AI content was generated
  features:
    crud: true
    search: true
    embedding: true  # RAG search for courses!
    softDelete: true
    pagination: true
    filtering: true
    sorting: true
  access:
    requiresAuth: false  # Public can browse courses
    createRoles: ["Admin"]
    updateRoles: ["Admin"]
    deleteRoles: ["Admin"]
```

### UserSkill Entity
```yaml
- name: UserSkill
  description: User's learning path (skills they're learning)
  fields:
    - name: userId
      type: string
      required: true
      indexed: true
    - name: skillId
      type: reference
      referencesEntity: Skill
      required: true
    - name: startedAt
      type: dateTime
      defaultValue: "now"
    - name: status
      type: enum
      description: "NotStarted, InProgress, Completed"
      defaultValue: "InProgress"
  features:
    crud: true
    softDelete: true
    filtering: true
  access:
    requiresAuth: true
    ownerOnly: true  # Users only see their own skills
```

### UserCourse Entity
```yaml
- name: UserCourse
  description: User's progress in a specific course
  fields:
    - name: userId
      type: string
      required: true
      indexed: true
    - name: courseId
      type: reference
      referencesEntity: Course
      required: true
    - name: startedAt
      type: dateTime
    - name: completedAt
      type: dateTime
    - name: minutesSpent
      type: int
      defaultValue: "0"
    - name: status
      type: enum
      description: "NotStarted, InProgress, Completed"
      defaultValue: "NotStarted"
    - name: progressPercentage
      type: int
      defaultValue: "0"
      minValue: 0
      maxValue: 100
  features:
    crud: true
    softDelete: true
    filtering: true
  access:
    requiresAuth: true
    ownerOnly: true
```

### ChatMessage Entity
```yaml
- name: ChatMessage
  description: Chat history (general + course-specific teaching)
  fields:
    - name: userId
      type: string
      required: true
      indexed: true
    - name: role
      type: enum
      description: "user, assistant, system"
      required: true
    - name: content
      type: text
      required: true
      searchable: true
    - name: courseId
      type: reference
      referencesEntity: Course
      description: "Null for general chat, set for course teaching"
    - name: toolCalls
      type: json
      description: AI tool calls made (if any)
  features:
    crud: true
    search: true
    softDelete: true
    pagination: true
    filtering: true
  access:
    requiresAuth: true
    ownerOnly: true  # Users only see their own chats
```

## Migration Steps

### Phase 1: Setup Hazina Generic API (1-2 hours)
1. Create `entities.yaml` in LearningTool.API
2. Install Hazina.API.Generic NuGet package
3. Configure in Program.cs
4. Test generated endpoints with Swagger

### Phase 2: Data Migration (1 hour)
1. Export existing SQLite data to JSON
2. Import via Hazina Generic API bulk endpoints
3. Verify data integrity

### Phase 3: Update ChatService (2-3 hours)
1. Replace repository calls with Hazina Generic API calls
2. Keep ChatService logic (AI generation)
3. Test chat functionality

### Phase 4: Update Frontend (2-3 hours)
1. Update API endpoints from `/api/skills` to `/api/entity/Skill`
2. Update DTOs to match Hazina response format
3. Test all UI features

### Phase 5: Testing & Deployment (1-2 hours)
1. Full integration test
2. Deploy to production
3. Verify all features work

## Expected Improvements

### Performance
- **Faster search** - PostgreSQL full-text search + embeddings
- **Better scaling** - Hazina handles pagination/filtering efficiently
- **Caching** - Built-in response caching

### Features
- **Semantic search** - "Find courses about machine learning" works better
- **Real-time updates** - Course library updates instantly
- **Export data** - Users can export their learning history
- **Bulk import** - Admin can import course catalog from CSV

### Developer Experience
- **Auto-generated Swagger docs** - API documentation for free
- **Type-safe API** - Generated TypeScript types for frontend
- **Less code** - No manual repository implementations

## Risk Mitigation

### Backup Strategy
1. Full SQLite backup before migration
2. Keep old Domain/Infrastructure code in git history
3. Deploy to staging first

### Rollback Plan
If migration fails:
1. Revert to previous git commit
2. Restore SQLite backup
3. Redeploy old version

### Testing Checklist
- [ ] Create skill
- [ ] Create topic under skill
- [ ] Create course under topic
- [ ] Add skill to user learning path
- [ ] Start course (track progress)
- [ ] General chat (create learning path)
- [ ] Course-specific chat (teaching mode)
- [ ] Search library for existing courses
- [ ] Generate AI course content
- [ ] View chat history
- [ ] Clear chat history

## Timeline

**Total estimated time: 8-12 hours**

- Phase 1: 2 hours (tonight)
- Phase 2: 1 hour (tonight)
- Phase 3: 3 hours (tomorrow morning)
- Phase 4: 3 hours (tomorrow afternoon)
- Phase 5: 2 hours (tomorrow evening)

**Target completion: February 23, 2026**
