using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;

namespace LearningTool.Application.Services;

public class KnowledgeService : IKnowledgeService
{
    private readonly ISkillRepository _skillRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly ICourseRepository _courseRepository;

    public KnowledgeService(
        ISkillRepository skillRepository,
        ITopicRepository topicRepository,
        ICourseRepository courseRepository)
    {
        _skillRepository = skillRepository;
        _topicRepository = topicRepository;
        _courseRepository = courseRepository;
    }

    public async Task<Skill> AddSkillToCatalogAsync(string name, string description, DifficultyLevel difficulty)
    {
        var existing = await _skillRepository.GetByNameAsync(name);
        if (existing != null)
        {
            return existing;
        }

        var skill = new Skill
        {
            Name = name,
            Description = description,
            Difficulty = difficulty
        };

        return await _skillRepository.CreateAsync(skill);
    }

    public async Task<Skill?> FindOrCreateSkillAsync(string name)
    {
        var existing = await _skillRepository.GetByNameAsync(name);
        if (existing != null)
        {
            return existing;
        }

        var skill = new Skill
        {
            Name = name,
            Description = $"Learn {name}",
            Difficulty = DifficultyLevel.Beginner
        };

        return await _skillRepository.CreateAsync(skill);
    }

    public async Task<List<Skill>> SearchSkillsAsync(string query)
    {
        return await _skillRepository.SearchAsync(query);
    }

    public async Task<List<Skill>> GetAllSkillsAsync()
    {
        return await _skillRepository.GetAllAsync();
    }

    public async Task<Topic> AddTopicAsync(int skillId, string name, string description)
    {
        var topic = new Topic
        {
            SkillId = skillId,
            Name = name,
            Description = description
        };

        return await _topicRepository.CreateAsync(topic);
    }

    public async Task<Topic?> FindOrCreateTopicAsync(int skillId, string name)
    {
        var existing = await _topicRepository.GetByNameAsync(name);
        if (existing != null && existing.SkillId == skillId)
        {
            return existing;
        }

        var topic = new Topic
        {
            SkillId = skillId,
            Name = name,
            Description = $"Learn about {name}"
        };

        return await _topicRepository.CreateAsync(topic);
    }

    public async Task<List<Topic>> GetTopicsForSkillAsync(int skillId)
    {
        return await _topicRepository.GetBySkillIdAsync(skillId);
    }

    public async Task<Course> AddCourseAsync(int topicId, string name, string description,
        string content, int estimatedMinutes, List<string> prerequisites, List<ResourceLink> resourceLinks)
    {
        var course = new Course
        {
            TopicId = topicId,
            Name = name,
            Description = description,
            Content = content,
            EstimatedMinutes = estimatedMinutes,
            Prerequisites = prerequisites,
            ResourceLinks = resourceLinks
        };

        return await _courseRepository.CreateAsync(course);
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        return await _courseRepository.GetByIdAsync(id);
    }

    public async Task<List<Course>> GetCoursesForTopicAsync(int topicId)
    {
        return await _courseRepository.GetByTopicIdAsync(topicId);
    }
}
