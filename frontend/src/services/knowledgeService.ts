import api from './api';

// Hazina paged response
export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// Hazina Dynamic API returns entities with this structure
interface HazinaEntity<T> {
  id: string;
  createdAt: string;
  updatedAt: string | null;
  isDeleted: boolean;
  properties: T;
}

// Helper to flatten Hazina entity structure
function flattenHazinaEntity<T>(entity: HazinaEntity<T>): T & { id: string; createdAt: string; updatedAt?: string } {
  return {
    ...entity.properties,
    id: entity.id,
    createdAt: entity.createdAt,
    updatedAt: entity.updatedAt || undefined
  } as T & { id: string; createdAt: string; updatedAt?: string };
}

export interface Skill {
  id: string;  // GUID
  name: string;
  description: string;
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced';
  createdAt: string;
  updatedAt?: string;
  topics?: Topic[];
}

export interface Topic {
  id: string;  // GUID
  skillId: string;  // GUID
  name: string;
  description: string;
  createdAt: string;
  updatedAt?: string;
  courses?: Course[];
  skill?: Skill;
}

export interface Course {
  id: string;  // GUID
  topicId: string;  // GUID
  name: string;
  description: string;
  content?: string;
  learningPlan?: string;
  systemPrompt?: string;
  prerequisites?: string[];
  resourceLinks?: ResourceLink[];
  estimatedMinutes: number;
  createdAt: string;
  updatedAt?: string;
  topic?: Topic;
}

export interface ResourceLink {
  url: string;
  title: string;
  type: 'YouTube' | 'Article' | 'Documentation' | 'Tutorial' | 'Book';
}

export interface UserSkill {
  id: string;  // GUID
  skillId: string;  // GUID
  userId: string;
  status: 'Interested' | 'Learning' | 'Mastered' | 'InProgress';
  startedAt?: string;
  createdAt: string;
  updatedAt?: string;
  skill?: Skill;
}

export interface UserCourse {
  id: string;  // GUID
  courseId: string;  // GUID
  userId: string;
  status: 'NotStarted' | 'InProgress' | 'Completed';
  startedAt?: string;
  completedAt?: string;
  minutesSpent?: number;
  progressPercentage?: number;
  createdAt: string;
  updatedAt?: string;
  course?: Course;
}

export const knowledgeService = {
  // Skills catalog - get all skills
  async getAllSkills(): Promise<Skill[]> {
    const response = await api.get<PagedResponse<Skill>>('/skill', {
      params: { pageSize: 1000 }
    });
    return response.data.items;
  },

  async searchSkills(search: string): Promise<Skill[]> {
    const response = await api.get<PagedResponse<Skill>>('/skill', {
      params: { search, pageSize: 100 }
    });
    return response.data.items;
  },

  // User skills - filter by current user
  async getMySkills(): Promise<UserSkill[]> {
    // Get current user from auth store or token
    const [userSkillsResponse, skillsResponse] = await Promise.all([
      api.get<PagedResponse<HazinaEntity<UserSkill>>>('/userskill', {
        params: { pageSize: 1000 }
      }),
      api.get<PagedResponse<HazinaEntity<Skill>>>('/skill', {
        params: { pageSize: 1000 }
      })
    ]);

    // Flatten Hazina response structure
    const userSkills = userSkillsResponse.data.items.map(flattenHazinaEntity);
    const skills = skillsResponse.data.items.map(flattenHazinaEntity);

    // Map skills by ID for easy lookup
    const skillsMap = new Map(skills.map(s => [s.id, s]));

    // Attach skill objects to userSkills
    return userSkills.map(us => ({
      ...us,
      skill: skillsMap.get(us.skillId)
    }));
  },

  async getSkillById(skillId: string): Promise<Skill> {
    const response = await api.get<Skill>(`/skill/${skillId}`);
    return response.data;
  },

  async addSkill(name: string, description?: string): Promise<UserSkill> {
    // First create the skill if it doesn't exist
    const skill = await api.post<Skill>('/skill', {
      name,
      description: description || '',
      difficulty: 'Beginner'
    });

    // Then add to user's skills
    const userSkill = await api.post<UserSkill>('/userskill', {
      skillId: skill.data.id,
      status: 'Learning',
      startedAt: new Date().toISOString()
    });

    return userSkill.data;
  },

  async addSkillById(skillId: string): Promise<UserSkill> {
    const response = await api.post<UserSkill>('/userskill', {
      skillId,
      status: 'Learning',
      startedAt: new Date().toISOString()
    });
    return response.data;
  },

  async removeSkill(skillId: string): Promise<void> {
    // Find and delete the UserSkill entry (soft delete)
    await api.delete(`/userskill/${skillId}`);
  },

  // Topics
  async getTopicsForSkill(skillId: string): Promise<Topic[]> {
    const response = await api.get<PagedResponse<Topic>>('/topic', {
      params: { skillId, pageSize: 100 }
    });
    return response.data.items;
  },

  async getMyTopics(): Promise<Topic[]> {
    // Get topics for user's skills
    const userSkills = await this.getMySkills();
    const skillIds = userSkills.map(us => us.skillId);

    if (skillIds.length === 0) return [];

    // Get all topics for these skills
    const allTopics: Topic[] = [];
    for (const skillId of skillIds) {
      const topics = await this.getTopicsForSkill(skillId);
      allTopics.push(...topics);
    }

    return allTopics;
  },

  async getTopicById(topicId: string): Promise<Topic> {
    const response = await api.get<Topic>(`/topic/${topicId}`);
    return response.data;
  },

  async addTopic(skillId: string, name: string, description?: string): Promise<Topic> {
    const response = await api.post<Topic>('/topic', {
      skillId,
      name,
      description: description || ''
    });
    return response.data;
  },

  async removeTopic(topicId: string): Promise<void> {
    await api.delete(`/topic/${topicId}`);
  },

  // Courses
  async getCourse(courseId: string): Promise<Course> {
    const response = await api.get<Course>(`/course/${courseId}`);
    return response.data;
  },

  async getCourseById(courseId: string): Promise<Course> {
    return this.getCourse(courseId);
  },

  async getCoursesForTopic(topicId: string): Promise<Course[]> {
    const response = await api.get<PagedResponse<Course>>('/course', {
      params: { topicId, pageSize: 100 }
    });
    return response.data.items;
  },

  async getInProgressCourses(): Promise<UserCourse[]> {
    const response = await api.get<PagedResponse<UserCourse>>('/usercourse', {
      params: { status: 'InProgress', pageSize: 100 }
    });
    return response.data.items;
  },

  async getCompletedCourses(): Promise<UserCourse[]> {
    const response = await api.get<PagedResponse<UserCourse>>('/usercourse', {
      params: { status: 'Completed', pageSize: 100 }
    });
    return response.data.items;
  },

  async startCourse(courseId: string): Promise<UserCourse> {
    const response = await api.post<UserCourse>('/usercourse', {
      courseId,
      status: 'InProgress',
      startedAt: new Date().toISOString(),
      progressPercentage: 0,
      minutesSpent: 0
    });
    return response.data;
  },

  async completeCourse(courseId: string, minutesSpent: number): Promise<UserCourse> {
    // Find existing UserCourse by courseId
    const userCourses = await api.get<PagedResponse<UserCourse>>('/usercourse', {
      params: { courseId }
    });

    if (userCourses.data.items.length === 0) {
      throw new Error('Course not started');
    }

    const userCourse = userCourses.data.items[0];

    // Update to completed
    const response = await api.put<UserCourse>(`/usercourse/${userCourse.id}`, {
      ...userCourse,
      status: 'Completed',
      completedAt: new Date().toISOString(),
      progressPercentage: 100,
      minutesSpent
    });

    return response.data;
  },
};
