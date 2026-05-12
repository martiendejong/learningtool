import api from './api';

export interface ProgressSummary {
  totalCoursesCompleted: number;
  totalCoursesInProgress: number;
  totalCourses: number;
  overallCompletionPct: number;
  currentStreak: number;
  longestStreak: number;
}

export interface HeatmapEntry {
  date: string;
  count: number;
}

export interface Achievement {
  id: string;
  name: string;
  description: string;
  emoji: string;
  earned: boolean;
  earnedAt?: string;
}

export interface LeaderboardEntry {
  rank: number;
  userId: string;
  email: string;
  coursesCompleted: number;
  currentStreak: number;
}

export interface Certificate {
  skillId: number;
  skillName: string;
  certId: string;
  coursesCompleted: number;
  totalCourses: number;
  completionPct: number;
  issuedAt?: string;
}

export const progressService = {
  async getSummary(): Promise<ProgressSummary> {
    const response = await api.get<ProgressSummary>('/api/progress/summary');
    return response.data;
  },

  async getHeatmap(): Promise<HeatmapEntry[]> {
    const response = await api.get<HeatmapEntry[]>('/api/progress/heatmap');
    return response.data;
  },

  async getAchievements(): Promise<Achievement[]> {
    const response = await api.get<Achievement[]>('/api/progress/achievements');
    return response.data;
  },

  async getLeaderboard(): Promise<LeaderboardEntry[]> {
    const response = await api.get<LeaderboardEntry[]>('/api/progress/leaderboard');
    return response.data;
  },

  async getCertificates(): Promise<Certificate[]> {
    const response = await api.get<Certificate[]>('/api/progress/certificates');
    return response.data;
  },
};
