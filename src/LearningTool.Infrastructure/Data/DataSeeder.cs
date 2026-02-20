using LearningTool.Domain.Entities;
using LearningTool.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(LearningToolDbContext context)
    {
        // Check if already seeded
        if (await context.Skills.AnyAsync())
        {
            return; // Database already has data
        }

        // Seed Skills
        var skills = new List<Skill>
        {
            new Skill
            {
                Name = "Machine Learning",
                Description = "Learn the fundamentals of machine learning, including algorithms, neural networks, and practical applications",
                Difficulty = DifficultyLevel.Intermediate,
                CreatedAt = DateTime.UtcNow
            },
            new Skill
            {
                Name = "Web Development",
                Description = "Master modern web development with HTML, CSS, JavaScript, and popular frameworks",
                Difficulty = DifficultyLevel.Beginner,
                CreatedAt = DateTime.UtcNow
            },
            new Skill
            {
                Name = "Data Science",
                Description = "Analyze and visualize data, build predictive models, and extract insights from large datasets",
                Difficulty = DifficultyLevel.Advanced,
                CreatedAt = DateTime.UtcNow
            },
            new Skill
            {
                Name = "Cloud Computing",
                Description = "Learn cloud platforms like AWS, Azure, and Google Cloud for scalable applications",
                Difficulty = DifficultyLevel.Intermediate,
                CreatedAt = DateTime.UtcNow
            },
            new Skill
            {
                Name = "Mobile Development",
                Description = "Build native and cross-platform mobile applications for iOS and Android",
                Difficulty = DifficultyLevel.Intermediate,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();

        // Seed Topics for Machine Learning
        var mlTopics = new List<Topic>
        {
            new Topic
            {
                SkillId = skills[0].Id,
                Name = "Supervised Learning",
                Description = "Learn classification and regression algorithms",
                CreatedAt = DateTime.UtcNow
            },
            new Topic
            {
                SkillId = skills[0].Id,
                Name = "Neural Networks",
                Description = "Deep learning and neural network architectures",
                CreatedAt = DateTime.UtcNow
            },
            new Topic
            {
                SkillId = skills[0].Id,
                Name = "Natural Language Processing",
                Description = "Process and understand human language with ML",
                CreatedAt = DateTime.UtcNow
            }
        };

        // Seed Topics for Web Development
        var webTopics = new List<Topic>
        {
            new Topic
            {
                SkillId = skills[1].Id,
                Name = "Frontend Basics",
                Description = "HTML, CSS, and JavaScript fundamentals",
                CreatedAt = DateTime.UtcNow
            },
            new Topic
            {
                SkillId = skills[1].Id,
                Name = "React Framework",
                Description = "Build modern UIs with React",
                CreatedAt = DateTime.UtcNow
            },
            new Topic
            {
                SkillId = skills[1].Id,
                Name = "Backend Development",
                Description = "Server-side programming with Node.js and APIs",
                CreatedAt = DateTime.UtcNow
            }
        };

        // Seed Topics for Data Science
        var dsTopics = new List<Topic>
        {
            new Topic
            {
                SkillId = skills[2].Id,
                Name = "Python for Data Science",
                Description = "NumPy, Pandas, and data manipulation",
                CreatedAt = DateTime.UtcNow
            },
            new Topic
            {
                SkillId = skills[2].Id,
                Name = "Statistical Analysis",
                Description = "Hypothesis testing, regression, and inference",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Topics.AddRange(mlTopics);
        context.Topics.AddRange(webTopics);
        context.Topics.AddRange(dsTopics);
        await context.SaveChangesAsync();

        // Seed Courses
        var courses = new List<Course>
        {
            // Machine Learning Courses
            new Course
            {
                TopicId = mlTopics[0].Id,
                Name = "Linear Regression Fundamentals",
                Description = "Master linear regression for predictive modeling",
                Prerequisites = new List<string>(),
                ResourceLinks = new List<ResourceLink>
                {
                    new ResourceLink { Url = "https://www.youtube.com/watch?v=nk2CQITm_eo", Title = "Linear Regression Explained", Type = ResourceType.YouTube },
                    new ResourceLink { Url = "https://scikit-learn.org/stable/modules/linear_model.html", Title = "Scikit-learn Documentation", Type = ResourceType.Documentation },
                    new ResourceLink { Url = "https://www.kaggle.com/learn/intro-to-machine-learning", Title = "Kaggle ML Course", Type = ResourceType.Tutorial }
                },
                EstimatedMinutes = 480,
                CreatedAt = DateTime.UtcNow
            },
            new Course
            {
                TopicId = mlTopics[1].Id,
                Name = "Introduction to Neural Networks",
                Description = "Build your first neural network from scratch",
                Prerequisites = new List<string> { "Python basics", "Linear algebra" },
                ResourceLinks = new List<ResourceLink>
                {
                    new ResourceLink { Url = "https://www.youtube.com/watch?v=aircAruvnKk", Title = "3Blue1Brown Neural Networks", Type = ResourceType.YouTube },
                    new ResourceLink { Url = "https://pytorch.org/tutorials/beginner/basics/intro.html", Title = "PyTorch Tutorials", Type = ResourceType.Documentation },
                    new ResourceLink { Url = "https://playground.tensorflow.org/", Title = "TensorFlow Playground", Type = ResourceType.Tutorial }
                },
                EstimatedMinutes = 900,
                CreatedAt = DateTime.UtcNow
            },

            // Web Development Courses
            new Course
            {
                TopicId = webTopics[0].Id,
                Name = "HTML & CSS Mastery",
                Description = "Build beautiful, responsive websites with HTML5 and CSS3",
                Prerequisites = new List<string>(),
                ResourceLinks = new List<ResourceLink>
                {
                    new ResourceLink { Url = "https://www.youtube.com/watch?v=G3e-cpL7ofc", Title = "HTML & CSS Full Course", Type = ResourceType.YouTube },
                    new ResourceLink { Url = "https://developer.mozilla.org/en-US/docs/Learn/HTML", Title = "MDN HTML Guide", Type = ResourceType.Documentation },
                    new ResourceLink { Url = "https://www.freecodecamp.org/learn/responsive-web-design/", Title = "FreeCodeCamp Exercises", Type = ResourceType.Tutorial }
                },
                EstimatedMinutes = 720,
                CreatedAt = DateTime.UtcNow
            },
            new Course
            {
                TopicId = webTopics[1].Id,
                Name = "React Fundamentals",
                Description = "Learn React hooks, components, and state management",
                Prerequisites = new List<string> { "JavaScript ES6", "HTML/CSS" },
                ResourceLinks = new List<ResourceLink>
                {
                    new ResourceLink { Url = "https://react.dev/learn", Title = "Official React Tutorial", Type = ResourceType.Documentation },
                    new ResourceLink { Url = "https://www.youtube.com/watch?v=Tn6-PIqc4UM", Title = "React in 100 Seconds", Type = ResourceType.YouTube },
                    new ResourceLink { Url = "https://react.dev/learn/tutorial-tic-tac-toe", Title = "Build Tic-Tac-Toe", Type = ResourceType.Tutorial }
                },
                EstimatedMinutes = 600,
                CreatedAt = DateTime.UtcNow
            },

            // Data Science Courses
            new Course
            {
                TopicId = dsTopics[0].Id,
                Name = "Pandas Data Analysis",
                Description = "Master data manipulation with Pandas library",
                Prerequisites = new List<string> { "Python basics" },
                ResourceLinks = new List<ResourceLink>
                {
                    new ResourceLink { Url = "https://www.youtube.com/watch?v=vmEHCJofslg", Title = "Pandas Tutorial", Type = ResourceType.YouTube },
                    new ResourceLink { Url = "https://pandas.pydata.org/docs/user_guide/index.html", Title = "Pandas User Guide", Type = ResourceType.Documentation },
                    new ResourceLink { Url = "https://www.kaggle.com/learn/pandas", Title = "Kaggle Pandas Course", Type = ResourceType.Tutorial }
                },
                EstimatedMinutes = 480,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();
    }
}
