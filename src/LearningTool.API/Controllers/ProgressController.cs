using LearningTool.Domain.Entities;
using LearningTool.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/progress")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly LearningToolDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProgressController(LearningToolDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    // ── Summary ────────────────────────────────────────────────────────────────

    // GET /api/progress/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = UserId;

        var userCourses = await _context.UserCourses
            .Where(uc => uc.UserId == userId)
            .Select(uc => new { uc.Status, uc.CompletedAt, uc.MinutesSpent })
            .ToListAsync();

        var completed = userCourses.Count(uc => uc.Status == UserCourseStatus.Completed);
        var inProgress = userCourses.Count(uc => uc.Status == UserCourseStatus.InProgress);
        var totalMinutes = userCourses.Sum(uc => uc.MinutesSpent);
        var lastActivity = userCourses
            .Where(uc => uc.CompletedAt.HasValue)
            .Select(uc => uc.CompletedAt)
            .Max();

        var completedDates = userCourses
            .Where(uc => uc.CompletedAt.HasValue)
            .Select(uc => uc.CompletedAt!.Value);
        var (currentStreak, longestStreak) = CalculateStreaks(completedDates);

        return Ok(new
        {
            coursesCompleted = completed,
            coursesInProgress = inProgress,
            totalMinutesSpent = totalMinutes,
            lastActivity,
            currentStreak,
            longestStreak
        });
    }

    // ── Streak ────────────────────────────────────────────────────────────────

    // GET /api/progress/streak
    [HttpGet("streak")]
    public async Task<IActionResult> GetStreak()
    {
        var userId = UserId;

        var completedDates = await _context.UserCourses
            .Where(uc => uc.UserId == userId && uc.Status == UserCourseStatus.Completed && uc.CompletedAt.HasValue)
            .Select(uc => uc.CompletedAt!.Value)
            .ToListAsync();

        var (current, longest) = CalculateStreaks(completedDates);
        var lastActivity = completedDates.Count > 0 ? completedDates.Max() : (DateTime?)null;

        return Ok(new
        {
            currentStreak = current,
            longestStreak = longest,
            totalCompleted = completedDates.Count,
            lastActivity
        });
    }

    // ── Heatmap ───────────────────────────────────────────────────────────────

    // GET /api/progress/heatmap
    [HttpGet("heatmap")]
    public async Task<IActionResult> GetHeatmap()
    {
        var userId = UserId;
        var since = DateTime.UtcNow.AddDays(-364);

        var completions = await _context.UserCourses
            .Where(uc => uc.UserId == userId
                && uc.Status == UserCourseStatus.Completed
                && uc.CompletedAt.HasValue
                && uc.CompletedAt >= since)
            .Select(uc => uc.CompletedAt!.Value)
            .ToListAsync();

        var heatmap = completions
            .GroupBy(d => DateOnly.FromDateTime(d))
            .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() })
            .OrderBy(x => x.date)
            .ToList();

        return Ok(heatmap);
    }

    // ── Achievements ──────────────────────────────────────────────────────────

    // GET /api/progress/achievements
    [HttpGet("achievements")]
    public async Task<IActionResult> GetAchievements()
    {
        var userId = UserId;

        // Gather all data in as few queries as possible
        var userCourses = await _context.UserCourses
            .Where(uc => uc.UserId == userId)
            .Select(uc => new { uc.Status, uc.CompletedAt, uc.CourseId })
            .ToListAsync();

        var completedCourseIds = userCourses
            .Where(uc => uc.Status == UserCourseStatus.Completed)
            .Select(uc => uc.CourseId)
            .ToHashSet();

        var completedDates = userCourses
            .Where(uc => uc.Status == UserCourseStatus.Completed && uc.CompletedAt.HasValue)
            .Select(uc => uc.CompletedAt!.Value)
            .ToList();

        var bookmarkCount = await _context.Bookmarks.CountAsync(b => b.UserId == userId);

        var (currentStreak, longestStreak) = CalculateStreaks(completedDates);
        var totalCompleted = completedCourseIds.Count;

        // Find completed skills (all courses in skill done)
        var skillCourseMap = await _context.Skills
            .Select(s => new
            {
                s.Id,
                CourseIds = s.Topics.SelectMany(t => t.Courses).Select(c => c.Id).ToList()
            })
            .ToListAsync();

        var completedSkillCount = skillCourseMap
            .Count(s => s.CourseIds.Count > 0 && s.CourseIds.All(id => completedCourseIds.Contains(id)));

        var badges = new[]
        {
            Badge("First Step",      "Complete your first course",                  totalCompleted >= 1,                   totalCompleted, 1),
            Badge("On a Roll",       "Complete 5 courses",                          totalCompleted >= 5,                   totalCompleted, 5),
            Badge("Dedicated",       "Complete 20 courses",                         totalCompleted >= 20,                  totalCompleted, 20),
            Badge("Centurion",       "Complete 50 courses",                         totalCompleted >= 50,                  totalCompleted, 50),
            Badge("Week Warrior",    "Maintain a 7-day learning streak",            longestStreak >= 7,                    longestStreak, 7),
            Badge("Streak Master",   "Maintain a 30-day learning streak",           longestStreak >= 30,                   longestStreak, 30),
            Badge("Scholar",         "Complete all courses in a skill",             completedSkillCount >= 1,              completedSkillCount, 1),
            Badge("Completionist",   "Complete all courses in 3 or more skills",    completedSkillCount >= 3,              completedSkillCount, 3),
            Badge("Bookworm",        "Bookmark 5 or more courses",                  bookmarkCount >= 5,                    bookmarkCount, 5),
        };

        return Ok(badges);
    }

    // ── Leaderboard ───────────────────────────────────────────────────────────

    // GET /api/progress/leaderboard
    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        var currentUserId = UserId;

        var top = await _context.UserCourses
            .Where(uc => uc.Status == UserCourseStatus.Completed)
            .GroupBy(uc => uc.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        var result = new List<object>();
        int rank = 1;
        foreach (var entry in top)
        {
            var user = await _userManager.FindByIdAsync(entry.UserId);
            var displayName = user?.Email?.Split('@')[0] ?? "user";
            result.Add(new
            {
                rank,
                displayName,
                coursesCompleted = entry.Count,
                isCurrentUser = entry.UserId == currentUserId
            });
            rank++;
        }

        return Ok(result);
    }

    // ── Certificates ──────────────────────────────────────────────────────────

    // GET /api/progress/certificates
    [HttpGet("certificates")]
    public async Task<IActionResult> GetCertificates()
    {
        var userId = UserId;

        var completedCourseIds = await _context.UserCourses
            .Where(uc => uc.UserId == userId && uc.Status == UserCourseStatus.Completed)
            .Select(uc => new { uc.CourseId, uc.CompletedAt })
            .ToListAsync();

        var completedSet = completedCourseIds.ToDictionary(x => x.CourseId, x => x.CompletedAt);

        var skills = await _context.Skills
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Difficulty,
                Courses = s.Topics.SelectMany(t => t.Courses).Select(c => c.Id).ToList()
            })
            .ToListAsync();

        var certificates = skills
            .Where(s => s.Courses.Count > 0 && s.Courses.All(id => completedSet.ContainsKey(id)))
            .Select(s =>
            {
                var earnedAt = s.Courses
                    .Select(id => completedSet[id])
                    .Where(d => d.HasValue)
                    .Max();

                // Deterministic certificate ID: SHA256(userId:skillId)
                var certificateId = Convert.ToHexString(
                    SHA256.HashData(Encoding.UTF8.GetBytes($"{userId}:{s.Id}")));

                return new
                {
                    skillId = s.Id,
                    skillName = s.Name,
                    difficulty = s.Difficulty.ToString(),
                    courseCount = s.Courses.Count,
                    earnedAt,
                    certificateId
                };
            })
            .OrderBy(c => c.skillName)
            .ToList();

        return Ok(certificates);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (int current, int longest) CalculateStreaks(IEnumerable<DateTime> completionDates)
    {
        var dates = completionDates
            .Select(d => DateOnly.FromDateTime(d))
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (dates.Count == 0) return (0, 0);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        // Current streak — only counts if last activity was today or yesterday
        int current = 0;
        if (dates[0] == today || dates[0] == yesterday)
        {
            current = 1;
            for (int i = 1; i < dates.Count; i++)
            {
                if (dates[i] == dates[i - 1].AddDays(-1))
                    current++;
                else
                    break;
            }
        }

        // Longest streak — full pass
        int longest = dates.Count > 0 ? 1 : 0;
        int run = 1;
        for (int i = 1; i < dates.Count; i++)
        {
            run = dates[i] == dates[i - 1].AddDays(-1) ? run + 1 : 1;
            if (run > longest) longest = run;
        }

        return (current, Math.Max(longest, current));
    }

    private static object Badge(string name, string description, bool earned, int progress, int goal) => new
    {
        name,
        description,
        earned,
        progress = Math.Min(progress, goal),
        goal
    };
}
