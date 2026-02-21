import api from './api';

export interface Skill {
  id: number;
  name: string;
  description: string;
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced';
  createdAt: string;
  topics?: Topic[];
}

export interface Topic {
  id: number;
  skillId: number;
  name: string;
  description: string;
  createdAt: string;
  courses?: Course[];
}

export interface Course {
  id: number;
  topicId: number;
  name: string;
  description: string;
  content?: string;  // Course content for teaching
  prerequisites: string[];
  resourceLinks: ResourceLink[];
  estimatedMinutes: number;
  createdAt: string;
}

export interface ResourceLink {
  url: string;
  title: string;
  type: 'YouTube' | 'Article' | 'Documentation' | 'Tutorial' | 'Book';
}

export interface UserSkill {
  id: number;
  skillId: number;
  userId: string;
  status: 'Interested' | 'Learning' | 'Mastered';
  addedAt: string;
  skill: Skill;
}

export interface UserCourse {
  id: number;
  courseId: number;
  userId: string;
  status: 'NotStarted' | 'InProgress' | 'Completed';
  startedAt?: string;
  completedAt?: string;
  score?: number;
  course: Course;
}

export const knowledgeService = {
  // Skills catalog
  async getAllSkills(): Promise<Skill[]> {
    const response = await api.get<Skill[]>('/skills/catalog');
    return response.data;
  },

  async searchSkills(search: string): Promise<Skill[]> {
    const response = await api.get<Skill[]>('/skills/catalog', { params: { search } });
    return response.data;
  },

  // User skills
  async getMySkills(): Promise<UserSkill[]> {
    const response = await api.get<UserSkill[]>('/skills/my-skills');
    return response.data;
  },

  async getSkillById(skillId: number): Promise<Skill> {
    const response = await api.get<Skill>(`/skills/${skillId}`);
    return response.data;
  },

  async addSkill(name: string): Promise<UserSkill> {
    const response = await api.post<UserSkill>('/skills', { skillName: name });
    return response.data;
  },

  async addSkillById(skillId: number): Promise<UserSkill> {
    // For existing skills in catalog, we still use the same endpoint but with skill name
    // The API will find the skill by ID via the name lookup
    const skill = await this.getSkillById(skillId);
    return this.addSkill(skill.name);
  },

  async removeSkill(skillId: number): Promise<void> {
    await api.delete(`/skills/${skillId}`);
  },

  // Topics
  async getTopicsForSkill(skillId: number): Promise<Topic[]> {
    const response = await api.get<Topic[]>(`/topics/skill/${skillId}`);
    return response.data;
  },

  async getMyTopics(): Promise<Topic[]> {
    const response = await api.get<Topic[]>('/topics/my-topics');
    return response.data;
  },

  async getTopicById(topicId: number): Promise<Topic> {
    const response = await api.get<Topic>(`/topics/${topicId}`);
    return response.data;
  },

  async addTopic(skillId: number, name: string, description?: string): Promise<Topic> {
    const response = await api.post<Topic>('/topics', { skillId, name, description });
    return response.data;
  },

  async removeTopic(topicId: number): Promise<void> {
    await api.delete(`/topics/${topicId}`);
  },

  // Courses
  async getCourse(courseId: number): Promise<Course> {
    const response = await api.get<Course>(`/courses/${courseId}`);
    return response.data;
  },

  async getCourseById(courseId: number): Promise<Course> {
    return this.getCourse(courseId);
  },

  async getCoursesForTopic(topicId: number): Promise<Course[]> {
    const response = await api.get<Course[]>(`/courses/topic/${topicId}`);
    return response.data;
  },

  async getInProgressCourses(): Promise<UserCourse[]> {
    const response = await api.get<UserCourse[]>('/courses/in-progress');
    return response.data;
  },

  async getCompletedCourses(): Promise<UserCourse[]> {
    const response = await api.get<UserCourse[]>('/courses/completed');
    return response.data;
  },

  async startCourse(courseId: number): Promise<UserCourse> {
    const response = await api.post<UserCourse>(`/courses/${courseId}/start`);
    return response.data;
  },

  async completeCourse(courseId: number, score: number): Promise<UserCourse> {
    const response = await api.post<UserCourse>(`/courses/${courseId}/complete`, { score });
    return response.data;
  },
};
