# Data Migration Script: Old DB -> Hazina Dynamic API
# Migrates Skills, Topics, Courses, UserSkills, UserCourses, ChatMessages

$ErrorActionPreference = "Stop"
$apiBase = "http://localhost:5028/api"

# Install SQLite module if needed
if (-not (Get-Module -ListAvailable -Name PSSQLite)) {
    Install-Module -Name PSSQLite -Force -Scope CurrentUser
}

Import-Module PSSQLite

$dbPath = "C:\Projects\learningtool\src\LearningTool.API\learningtool.db"

Write-Host "=== DATA MIGRATION STARTED ===" -ForegroundColor Cyan
Write-Host "Source: $dbPath"
Write-Host "Target: $apiBase"
Write-Host ""

# Helper function to POST to Hazina API
function Post-HazinaEntity {
    param(
        [string]$Entity,
        [hashtable]$Data
    )

    $json = $Data | ConvertTo-Json -Depth 10
    $response = Invoke-RestMethod -Uri "$apiBase/$Entity" -Method Post -Body $json -ContentType "application/json"
    return $response
}

# Track ID mappings (old ID -> new GUID)
$skillMap = @{}
$topicMap = @{}
$courseMap = @{}

# 1. Migrate Skills
Write-Host "[1/6] Migrating Skills..." -ForegroundColor Yellow
$skills = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM Skills WHERE IsDeleted = 0"
$skillCount = 0
foreach ($skill in $skills) {
    $data = @{
        name = $skill.Name
        description = $skill.Description
        difficulty = if ($skill.Difficulty) { $skill.Difficulty } else { "Beginner" }
    }
    $newSkill = Post-HazinaEntity -Entity "skill" -Data $data
    $skillMap[$skill.Id] = $newSkill.id
    $skillCount++
    Write-Host "  ✓ $($skill.Name) -> $($newSkill.id)"
}
Write-Host "  Migrated $skillCount skills" -ForegroundColor Green
Write-Host ""

# 2. Migrate Topics
Write-Host "[2/6] Migrating Topics..." -ForegroundColor Yellow
$topics = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM Topics WHERE IsDeleted = 0"
$topicCount = 0
foreach ($topic in $topics) {
    if ($skillMap.ContainsKey($topic.SkillId)) {
        $data = @{
            skillId = $skillMap[$topic.SkillId]
            name = $topic.Name
            description = $topic.Description
        }
        $newTopic = Post-HazinaEntity -Entity "topic" -Data $data
        $topicMap[$topic.Id] = $newTopic.id
        $topicCount++
        Write-Host "  ✓ $($topic.Name) -> $($newTopic.id)"
    }
}
Write-Host "  Migrated $topicCount topics" -ForegroundColor Green
Write-Host ""

# 3. Migrate Courses
Write-Host "[3/6] Migrating Courses..." -ForegroundColor Yellow
$courses = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM Courses WHERE IsDeleted = 0"
$courseCount = 0
foreach ($course in $courses) {
    if ($topicMap.ContainsKey($course.TopicId)) {
        # Parse JSON fields
        $prerequisites = if ($course.Prerequisites) {
            $course.Prerequisites | ConvertFrom-Json
        } else {
            @()
        }

        $resourceLinks = if ($course.ResourceLinks) {
            $course.ResourceLinks | ConvertFrom-Json
        } else {
            @()
        }

        $data = @{
            topicId = $topicMap[$course.TopicId]
            name = $course.Name
            description = $course.Description
            content = $course.Content
            learningPlan = $course.LearningPlan
            systemPrompt = $course.SystemPrompt
            estimatedMinutes = $course.EstimatedMinutes
            prerequisites = $prerequisites
            resourceLinks = $resourceLinks
            contentGeneratedAt = $course.ContentGeneratedAt
        }

        $newCourse = Post-HazinaEntity -Entity "course" -Data $data
        $courseMap[$course.Id] = $newCourse.id
        $courseCount++
        Write-Host "  ✓ $($course.Name) -> $($newCourse.id)"
    }
}
Write-Host "  Migrated $courseCount courses" -ForegroundColor Green
Write-Host ""

# 4. Migrate UserSkills
Write-Host "[4/6] Migrating UserSkills..." -ForegroundColor Yellow
$userSkills = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM UserSkills WHERE IsDeleted = 0"
$userSkillCount = 0
foreach ($userSkill in $userSkills) {
    if ($skillMap.ContainsKey($userSkill.SkillId)) {
        $data = @{
            userId = $userSkill.UserId
            skillId = $skillMap[$userSkill.SkillId]
            startedAt = $userSkill.StartedAt
            status = if ($userSkill.Status) { $userSkill.Status } else { "InProgress" }
        }

        $newUserSkill = Post-HazinaEntity -Entity "userskill" -Data $data
        $userSkillCount++
        Write-Host "  ✓ User $($userSkill.UserId) -> Skill $($newUserSkill.skillId)"
    }
}
Write-Host "  Migrated $userSkillCount user skills" -ForegroundColor Green
Write-Host ""

# 5. Migrate UserCourses
Write-Host "[5/6] Migrating UserCourses..." -ForegroundColor Yellow
$userCourses = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM UserCourses WHERE IsDeleted = 0"
$userCourseCount = 0
foreach ($userCourse in $userCourses) {
    if ($courseMap.ContainsKey($userCourse.CourseId)) {
        $data = @{
            userId = $userCourse.UserId
            courseId = $courseMap[$userCourse.CourseId]
            startedAt = $userCourse.StartedAt
            completedAt = $userCourse.CompletedAt
            minutesSpent = $userCourse.MinutesSpent
            status = if ($userCourse.Status) { $userCourse.Status } else { "NotStarted" }
            progressPercentage = if ($userCourse.ProgressPercentage) { $userCourse.ProgressPercentage } else { 0 }
        }

        $newUserCourse = Post-HazinaEntity -Entity "usercourse" -Data $data
        $userCourseCount++
        Write-Host "  ✓ User $($userCourse.UserId) -> Course $($newUserCourse.courseId)"
    }
}
Write-Host "  Migrated $userCourseCount user courses" -ForegroundColor Green
Write-Host ""

# 6. Migrate ChatMessages
Write-Host "[6/6] Migrating ChatMessages..." -ForegroundColor Yellow
$chatMessages = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM ChatMessages WHERE IsDeleted = 0"
$chatCount = 0
foreach ($msg in $chatMessages) {
    $toolCalls = if ($msg.ToolCalls) {
        $msg.ToolCalls | ConvertFrom-Json
    } else {
        $null
    }

    $data = @{
        userId = $msg.UserId
        role = $msg.Role
        content = $msg.Content
        courseId = if ($msg.CourseId -and $courseMap.ContainsKey($msg.CourseId)) {
            $courseMap[$msg.CourseId]
        } else {
            $null
        }
        toolCalls = $toolCalls
    }

    $newMsg = Post-HazinaEntity -Entity "chatmessage" -Data $data
    $chatCount++
}
Write-Host "  Migrated $chatCount chat messages" -ForegroundColor Green
Write-Host ""

Write-Host "=== MIGRATION COMPLETE ===" -ForegroundColor Cyan
Write-Host "Skills: $skillCount" -ForegroundColor White
Write-Host "Topics: $topicCount" -ForegroundColor White
Write-Host "Courses: $courseCount" -ForegroundColor White
Write-Host "UserSkills: $userSkillCount" -ForegroundColor White
Write-Host "UserCourses: $userCourseCount" -ForegroundColor White
Write-Host "ChatMessages: $chatCount" -ForegroundColor White
Write-Host ""
Write-Host "✓ All data migrated successfully!" -ForegroundColor Green
