-- Seed data for LearningTool
-- Skills, Topics, and Courses

-- Clear existing data (optional - comment out if you want to keep existing data)
-- DELETE FROM UserCourses;
-- DELETE FROM UserSkills;
-- DELETE FROM Courses;
-- DELETE FROM Topics;
-- DELETE FROM Skills;

-- ===== SKILLS =====

INSERT OR IGNORE INTO Skills (Name, Description, Difficulty, IsDeleted, CreatedAt) VALUES
('AI (Artificial Intelligence)', 'Learn to use and build with AI tools', 0, 0, datetime('now')),
('Web Development', 'Build websites and web applications', 0, 0, datetime('now')),
('Mobile Development', 'Create mobile apps for iOS and Android', 1, 0, datetime('now')),
('Data Science', 'Analyze data and build predictive models', 2, 0, datetime('now')),
('Design', 'UI/UX design and graphic design', 0, 0, datetime('now')),
('Business & Entrepreneurship', 'Start and grow your business', 0, 0, datetime('now')),
('Digital Marketing', 'Market your products and services online', 0, 0, datetime('now'));

-- ===== AI SKILL - TOPICS & COURSES =====

-- AI Chatbots
INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'AI Chatbots', 'Learn to use and build AI chatbots', 0, datetime('now')
FROM Skills WHERE Name = 'AI (Artificial Intelligence)';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Using ChatGPT', 'Master ChatGPT for productivity and creativity', 'Learn the fundamentals of ChatGPT', 60, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'AI Chatbots';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Building Custom Chatbots', 'Create your own AI-powered chatbots', 'Build chatbots using OpenAI API', 120, '["Using ChatGPT"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'AI Chatbots';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Prompt Engineering', 'Write effective prompts for better AI results', 'Master the art of prompt engineering', 90, '["Using ChatGPT"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'AI Chatbots';

-- AI Image Generation
INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'AI Image Generation', 'Create images using AI', 0, datetime('now')
FROM Skills WHERE Name = 'AI (Artificial Intelligence)';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Using DALL-E', 'Generate images with DALL-E', 'Learn to create images using DALL-E', 45, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'AI Image Generation';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Using Midjourney', 'Create stunning images with Midjourney', 'Master Midjourney for image generation', 60, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'AI Image Generation';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Using Stable Diffusion', 'Generate images locally with Stable Diffusion', 'Learn Stable Diffusion basics', 90, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'AI Image Generation';

-- AI Website Builders
INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'AI Website Builders', 'Build websites using AI tools', 0, datetime('now')
FROM Skills WHERE Name = 'AI (Artificial Intelligence)';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Using v0 by Vercel', 'Generate React components with v0', 'Learn to use v0 for rapid prototyping', 60, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'AI Website Builders';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Using Cursor IDE', 'Code with AI assistance in Cursor', 'Master AI-powered coding with Cursor', 75, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'AI Website Builders';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Using Claude Code', 'Build software with Claude Code', 'Learn autonomous coding with Claude', 90, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'AI Website Builders';

-- ===== WEB DEVELOPMENT - TOPICS & COURSES =====

-- Frontend Basics
INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'Frontend Basics', 'HTML, CSS, and JavaScript fundamentals', 0, datetime('now')
FROM Skills WHERE Name = 'Web Development';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'HTML Fundamentals', 'Learn HTML from scratch', 'Master HTML structure and elements', 120, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Frontend Basics';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'CSS Fundamentals', 'Style websites with CSS', 'Learn CSS styling and layouts', 150, '["HTML Fundamentals"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Frontend Basics';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'JavaScript Fundamentals', 'Make websites interactive with JavaScript', 'Learn JavaScript basics', 180, '["HTML Fundamentals"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Frontend Basics';

-- Backend Development
INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'Backend Development', 'Server-side programming', 0, datetime('now')
FROM Skills WHERE Name = 'Web Development';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Python Basics', 'Learn Python programming', 'Master Python fundamentals', 200, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Backend Development';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'C# Fundamentals', 'Learn C# and .NET', 'Master C# programming', 220, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Backend Development';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Node.js Basics', 'Build backend with Node.js', 'Learn server-side JavaScript', 180, '["JavaScript Fundamentals"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Backend Development';

-- Frontend Frameworks
INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'Frontend Frameworks', 'Modern JavaScript frameworks', 0, datetime('now')
FROM Skills WHERE Name = 'Web Development';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'React Basics', 'Build UIs with React', 'Learn React component development', 240, '["JavaScript Fundamentals"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Frontend Frameworks';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Vue.js Basics', 'Build apps with Vue.js', 'Learn Vue.js framework', 200, '["JavaScript Fundamentals"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Frontend Frameworks';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'ASP.NET Core Basics', 'Build web apps with ASP.NET Core', 'Learn ASP.NET Core MVC', 260, '["C# Fundamentals"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Frontend Frameworks';

-- ===== MOBILE DEVELOPMENT - TOPICS & COURSES =====

INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'iOS Development', 'Build apps for iPhone and iPad', 0, datetime('now')
FROM Skills WHERE Name = 'Mobile Development';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Swift Fundamentals', 'Learn Swift programming', 'Master Swift for iOS', 200, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'iOS Development';

INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'Android Development', 'Build apps for Android devices', 0, datetime('now')
FROM Skills WHERE Name = 'Mobile Development';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Kotlin Fundamentals', 'Learn Kotlin for Android', 'Master Kotlin programming', 200, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Android Development';

INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'Cross-Platform Development', 'Build apps for iOS and Android together', 0, datetime('now')
FROM Skills WHERE Name = 'Mobile Development';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'React Native Basics', 'Build mobile apps with React Native', 'Learn cross-platform development', 240, '["JavaScript Fundamentals"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Cross-Platform Development';

-- ===== DATA SCIENCE - TOPICS & COURSES =====

INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'Data Analysis', 'Analyze and visualize data', 0, datetime('now')
FROM Skills WHERE Name = 'Data Science';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Python for Data Science', 'Use Python for data analysis', 'Learn pandas and numpy', 180, '["Python Basics"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Data Analysis';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Data Visualization', 'Create charts and dashboards', 'Master data visualization tools', 120, '["Python for Data Science"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Data Analysis';

INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'Machine Learning', 'Build predictive models', 0, datetime('now')
FROM Skills WHERE Name = 'Data Science';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Machine Learning Basics', 'Introduction to ML algorithms', 'Learn supervised and unsupervised learning', 300, '["Python for Data Science"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Machine Learning';

-- ===== DESIGN - TOPICS & COURSES =====

INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'UI/UX Design', 'Design user interfaces and experiences', 0, datetime('now')
FROM Skills WHERE Name = 'Design';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Figma Basics', 'Design with Figma', 'Learn Figma interface and tools', 90, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'UI/UX Design';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'UX Design Principles', 'Create great user experiences', 'Master UX fundamentals', 120, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'UI/UX Design';

INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'Graphic Design', 'Create visual content', 0, datetime('now')
FROM Skills WHERE Name = 'Design';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Adobe Photoshop Basics', 'Edit images with Photoshop', 'Learn photo editing fundamentals', 150, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Graphic Design';

-- ===== BUSINESS - TOPICS & COURSES =====

INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'Starting a Business', 'Launch your own business', 0, datetime('now')
FROM Skills WHERE Name = 'Business & Entrepreneurship';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Business Planning', 'Create a solid business plan', 'Learn business planning fundamentals', 180, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Starting a Business';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Finding Your First Customers', 'Acquire early customers', 'Learn customer acquisition strategies', 120, '["Business Planning"]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Starting a Business';

-- ===== MARKETING - TOPICS & COURSES =====

INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'Social Media Marketing', 'Market on social platforms', 0, datetime('now')
FROM Skills WHERE Name = 'Digital Marketing';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Instagram Marketing', 'Grow your Instagram presence', 'Master Instagram for business', 90, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Social Media Marketing';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'LinkedIn Marketing', 'Build your professional brand', 'Use LinkedIn for business growth', 90, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Social Media Marketing';

INSERT OR IGNORE INTO Topics (SkillId, Name, Description, IsDeleted, CreatedAt)
SELECT Id, 'Content Marketing', 'Create content that converts', 0, datetime('now')
FROM Skills WHERE Name = 'Digital Marketing';

INSERT OR IGNORE INTO Courses (TopicId, Name, Description, Content, EstimatedMinutes, Prerequisites, ResourceLinks, IsDeleted, CreatedAt)
SELECT Id, 'Blogging for Business', 'Drive traffic with blog content', 'Learn content marketing through blogging', 120, '[]', '[]', 0, datetime('now')
FROM Topics WHERE Name = 'Content Marketing';

-- Verify data
SELECT 'Skills created: ' || COUNT(*) FROM Skills WHERE IsDeleted = 0;
SELECT 'Topics created: ' || COUNT(*) FROM Topics WHERE IsDeleted = 0;
SELECT 'Courses created: ' || COUNT(*) FROM Courses WHERE IsDeleted = 0;
