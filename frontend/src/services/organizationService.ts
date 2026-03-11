import api from './api';

export interface OrganizationMember {
  id: string;
  email: string;
  fullName?: string;
  roleInOrganization?: string;
  profilePictureUrl?: string;
  createdAt: string;
  lastLoginAt?: string;
}

export interface InviteResponse {
  message: string;
  token: string;
  inviteUrl: string;
  expiresAt: string;
}

export interface AcceptInviteRequest {
  token: string;
  email: string;
  password: string;
  fullName?: string;
}

export interface AcceptInviteResponse {
  token: string;
  user: {
    id: string;
    email: string;
    userName: string;
    fullName?: string;
    accountType?: string;
    organizationId?: string;
    roleInOrganization?: string;
    profilePictureUrl?: string;
  };
}

export const organizationService = {
  async getMyOrganization() {
    const response = await api.get('/organization/my');
    return response.data;
  },

  async getMembers(organizationId: string): Promise<OrganizationMember[]> {
    const response = await api.get(`/organization/${organizationId}/members`);
    return response.data;
  },

  async inviteUser(email: string, role: string = 'Student'): Promise<InviteResponse> {
    const response = await api.post('/organization/invite', { email, role });
    return response.data;
  },

  async revokeInvitation(token: string) {
    const response = await api.delete(`/organization/invite/${token}`);
    return response.data;
  },

  async removeMember(memberId: string) {
    const response = await api.delete(`/organization/members/${memberId}`);
    return response.data;
  },

  async acceptInvite(data: AcceptInviteRequest): Promise<AcceptInviteResponse> {
    const response = await api.post('/auth/accept-invite', data);
    return response.data;
  },
};
