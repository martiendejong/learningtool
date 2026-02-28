import api from './api';

export interface Organization {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  userCount?: number;
}

export interface CreateOrganizationRequest {
  name: string;
  description?: string;
}

export interface UpdateOrganizationRequest {
  name?: string;
  description?: string;
  isActive?: boolean;
}

export const organizationService = {
  // Get all organizations (admin only)
  async getOrganizations(): Promise<Organization[]> {
    const response = await api.get<Organization[]>('/organization');
    return response.data;
  },

  // Get organization by ID (admin only)
  async getOrganization(id: number): Promise<Organization> {
    const response = await api.get<Organization>(`/organization/${id}`);
    return response.data;
  },

  // Create organization (admin only)
  async createOrganization(request: CreateOrganizationRequest): Promise<Organization> {
    const response = await api.post<Organization>('/organization', request);
    return response.data;
  },

  // Update organization (admin only)
  async updateOrganization(id: number, request: UpdateOrganizationRequest): Promise<Organization> {
    const response = await api.put<Organization>(`/organization/${id}`, request);
    return response.data;
  },

  // Delete organization (admin only)
  async deleteOrganization(id: number): Promise<void> {
    await api.delete(`/organization/${id}`);
  },
};
