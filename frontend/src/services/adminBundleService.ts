import api from './api';

export interface AdminBundle {
  id: number;
  name: string;
  description?: string;
  createdAt: string;
  skillCount: number;
  orgCount: number;
  skills: { id: number; name: string }[];
}

export interface AdminOrg {
  id: number;
  name: string;
}

export const adminBundleService = {
  async getBundles(): Promise<AdminBundle[]> {
    const response = await api.get<AdminBundle[]>('/api/admin/bundles');
    return response.data;
  },

  async createBundle(name: string, description?: string): Promise<AdminBundle> {
    const response = await api.post<AdminBundle>('/api/admin/bundles', { name, description });
    return response.data;
  },

  async updateBundle(id: number, name: string, description?: string): Promise<void> {
    await api.put(`/api/admin/bundles/${id}`, { name, description });
  },

  async deleteBundle(id: number): Promise<void> {
    await api.delete(`/api/admin/bundles/${id}`);
  },

  async addSkill(bundleId: number, skillId: number): Promise<void> {
    await api.post(`/api/admin/bundles/${bundleId}/skills/${skillId}`);
  },

  async removeSkill(bundleId: number, skillId: number): Promise<void> {
    await api.delete(`/api/admin/bundles/${bundleId}/skills/${skillId}`);
  },

  async assignToOrg(bundleId: number, orgId: number, maxUsers: number, isUnlimited: boolean): Promise<void> {
    await api.post(`/api/admin/bundles/${bundleId}/organizations/${orgId}`, { maxUsers, isUnlimited });
  },

  async updateOrgBundle(bundleId: number, orgId: number, maxUsers: number, isUnlimited: boolean): Promise<void> {
    await api.put(`/api/admin/bundles/${bundleId}/organizations/${orgId}`, { maxUsers, isUnlimited });
  },

  async unassignFromOrg(bundleId: number, orgId: number): Promise<void> {
    await api.delete(`/api/admin/bundles/${bundleId}/organizations/${orgId}`);
  },

  async getAdminStats() {
    const response = await api.get('/api/admin/stats');
    return response.data;
  },

  async getOrganizations(): Promise<AdminOrg[]> {
    const response = await api.get<AdminOrg[]>('/api/admin/organizations');
    return response.data;
  },

  async getSkills() {
    const response = await api.get('/api/admin/skills');
    return response.data;
  },
};
