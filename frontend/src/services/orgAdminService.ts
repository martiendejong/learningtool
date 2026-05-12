import api from './api';

export interface OrgOverview {
  orgName: string;
  totalStudents: number;
  activeStudents: number;
  totalCourses: number;
  avgCompletionPct: number;
  recentStudents: { id: string; email: string; joinedAt: string }[];
}

export interface OrgStudent {
  id: string;
  email: string;
  joinedAt: string;
  coursesCompleted: number;
  completionPct: number;
}

export interface OrgStudentDetail {
  id: string;
  email: string;
  joinedAt: string;
  totalCoursesCompleted: number;
  lastActivity?: string;
  completionPct: number;
  skills: {
    id: number;
    name: string;
    totalCourses: number;
    completedCourses: number;
    completionPct: number;
  }[];
}

export interface OrgInvite {
  id: number;
  expiresAt: string;
  maxUses: number;
  usedCount: number;
  createdAt: string;
  isExpired: boolean;
  isExhausted: boolean;
}

export interface CreateInviteResult {
  token: string;
  inviteUrl: string;
  expiresAt: string;
  maxUses: number;
}

export interface OrgBundle {
  id: number;
  bundleId: number;
  bundleName: string;
  bundleDescription?: string;
  maxUsers: number;
  isUnlimited: boolean;
  createdAt: string;
  skillCount: number;
  skills: { id: number; name: string }[];
  assignedUsers: number;
}

export interface BundleUser {
  id: string;
  email: string;
  isAssigned: boolean;
}

export interface OrgMember {
  id: string;
  email: string;
  role: string;
  joinedAt: string;
}

export const orgAdminService = {
  async getOverview(): Promise<OrgOverview> {
    const response = await api.get<OrgOverview>('/api/org/overview');
    return response.data;
  },

  async getStudents(search?: string): Promise<OrgStudent[]> {
    const response = await api.get<OrgStudent[]>('/api/org/students', { params: { search } });
    return response.data;
  },

  async getStudent(userId: string): Promise<OrgStudentDetail> {
    const response = await api.get<OrgStudentDetail>(`/api/org/students/${userId}`);
    return response.data;
  },

  async removeStudent(userId: string): Promise<void> {
    await api.delete(`/api/org/students/${userId}`);
  },

  async createInvite(expiresInDays = 7, maxUses = 10): Promise<CreateInviteResult> {
    const response = await api.post<CreateInviteResult>('/api/org/invite', { expiresInDays, maxUses });
    return response.data;
  },

  async getInvites(): Promise<OrgInvite[]> {
    const response = await api.get<OrgInvite[]>('/api/org/invites');
    return response.data;
  },

  async revokeInvite(inviteId: number): Promise<void> {
    await api.delete(`/api/org/invites/${inviteId}`);
  },

  async getBundles(): Promise<OrgBundle[]> {
    const response = await api.get<OrgBundle[]>('/api/org/bundles');
    return response.data;
  },

  async getBundleUsers(bundleId: number): Promise<BundleUser[]> {
    const response = await api.get<BundleUser[]>(`/api/org/bundles/${bundleId}/users`);
    return response.data;
  },

  async assignBundle(bundleId: number, userId: string): Promise<void> {
    await api.post(`/api/org/bundles/${bundleId}/users/${userId}`);
  },

  async revokeBundle(bundleId: number, userId: string): Promise<void> {
    await api.delete(`/api/org/bundles/${bundleId}/users/${userId}`);
  },

  async getMembers(): Promise<OrgMember[]> {
    const response = await api.get<OrgMember[]>('/api/org/members');
    return response.data;
  },
};
