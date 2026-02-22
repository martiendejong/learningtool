# Simple Data Migration Script
$ErrorActionPreference = "Stop"
$apiBase = "http://localhost:5028/api"

# Check if PSSQLite is available
if (-not (Get-Module -ListAvailable -Name PSSQLite)) {
    Write-Host "Installing PSSQLite module..." -ForegroundColor Yellow
    Install-Module -Name PSSQLite -Force -Scope CurrentUser
}

Import-Module PSSQLite

$dbPath = "C:\Projects\learningtool\src\LearningTool.API\learningtool.db"

Write-Host "=== DATA MIGRATION ===" -ForegroundColor Cyan

# Helper to POST
function PostEntity($entity, $body) {
    $json = $body | ConvertTo-Json -Depth 10 -Compress
    Invoke-RestMethod -Uri "$apiBase/$entity" -Method Post -Body $json -ContentType "application/json"
}

# Maps
$skillMap = @{}
$topicMap = @{}
$courseMap = @{}

# 1. Skills
Write-Host "[1/6] Skills..." -ForegroundColor Yellow
$skills = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM Skills WHERE IsDeleted = 0"
foreach ($s in $skills) {
    $new = PostEntity "skill" @{
        name = $s.Name
        description = $s.Description
        difficulty = "Beginner"
    }
    $skillMap[$s.Id] = $new.id
    Write-Host "  $($s.Name)"
}
Write-Host "  Done: $($skills.Count)" -ForegroundColor Green

# 2. Topics
Write-Host "[2/6] Topics..." -ForegroundColor Yellow
$topics = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM Topics WHERE IsDeleted = 0"
foreach ($t in $topics) {
    if ($skillMap.ContainsKey($t.SkillId)) {
        $new = PostEntity "topic" @{
            skillId = $skillMap[$t.SkillId]
            name = $t.Name
            description = $t.Description
        }
        $topicMap[$t.Id] = $new.id
        Write-Host "  $($t.Name)"
    }
}
Write-Host "  Done: $($topics.Count)" -ForegroundColor Green

# 3. Courses
Write-Host "[3/6] Courses..." -ForegroundColor Yellow
$courses = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM Courses WHERE IsDeleted = 0"
foreach ($c in $courses) {
    if ($topicMap.ContainsKey($c.TopicId)) {
        $new = PostEntity "course" @{
            topicId = $topicMap[$c.TopicId]
            name = $c.Name
            description = $c.Description
            content = $c.Content
            learningPlan = $c.LearningPlan
            systemPrompt = $c.SystemPrompt
            estimatedMinutes = $c.EstimatedMinutes
        }
        $courseMap[$c.Id] = $new.id
        Write-Host "  $($c.Name)"
    }
}
Write-Host "  Done: $($courses.Count)" -ForegroundColor Green

# 4. UserSkills
Write-Host "[4/6] UserSkills..." -ForegroundColor Yellow
$userSkills = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM UserSkills WHERE IsDeleted = 0"
foreach ($us in $userSkills) {
    if ($skillMap.ContainsKey($us.SkillId)) {
        PostEntity "userskill" @{
            userId = $us.UserId
            skillId = $skillMap[$us.SkillId]
            startedAt = $us.StartedAt
            status = "InProgress"
        } | Out-Null
    }
}
Write-Host "  Done: $($userSkills.Count)" -ForegroundColor Green

# 5. UserCourses
Write-Host "[5/6] UserCourses..." -ForegroundColor Yellow
$userCourses = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM UserCourses WHERE IsDeleted = 0"
foreach ($uc in $userCourses) {
    if ($courseMap.ContainsKey($uc.CourseId)) {
        PostEntity "usercourse" @{
            userId = $uc.UserId
            courseId = $courseMap[$uc.CourseId]
            startedAt = $uc.StartedAt
            completedAt = $uc.CompletedAt
            minutesSpent = $uc.MinutesSpent
            status = "NotStarted"
            progressPercentage = 0
        } | Out-Null
    }
}
Write-Host "  Done: $($userCourses.Count)" -ForegroundColor Green

# 6. ChatMessages
Write-Host "[6/6] ChatMessages..." -ForegroundColor Yellow
$msgs = Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM ChatMessages LIMIT 100"
foreach ($m in $msgs) {
    $courseId = $null
    if ($m.CourseId -and $courseMap.ContainsKey($m.CourseId)) {
        $courseId = $courseMap[$m.CourseId]
    }
    PostEntity "chatmessage" @{
        userId = $m.UserId
        role = $m.Role
        content = $m.Content
        courseId = $courseId
    } | Out-Null
}
Write-Host "  Done: $($msgs.Count)" -ForegroundColor Green

Write-Host ""
Write-Host "=== MIGRATION COMPLETE ===" -ForegroundColor Green
