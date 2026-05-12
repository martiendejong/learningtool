import api from './api';
import type { User } from '../stores/authStore';

export interface RegisterRequest {
  email: string;
  password: string;
  inviteToken?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface RegisterOrgRequest {
  email: string;
  password: string;
  organizationName: string;
}

export interface RegisterOrgResponse extends AuthResponse {
  organization: { id: number; name: string };
}

export const authService = {
  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/register', data);
    return response.data;
  },

  async login(data: LoginRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/login', data);
    return response.data;
  },

  async registerOrganization(data: RegisterOrgRequest): Promise<RegisterOrgResponse> {
    const response = await api.post<RegisterOrgResponse>('/auth/register-organization', data);
    return response.data;
  },

  async verifyGoogle(email: string, code: string): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/verify-google', { email, code });
    return response.data;
  },

  async logout(): Promise<void> {
    await api.post('/auth/logout');
  },
};
