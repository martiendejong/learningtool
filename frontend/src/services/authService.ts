import api from './api';

export interface RegisterRequest {
  email: string;
  password: string;
  fullName?: string;
  accountType?: 'Individual' | 'Organization';
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface User {
  id: string;
  email: string;
  userName: string;
  fullName?: string;
  accountType?: string;
  organizationId?: string;
  roleInOrganization?: string;
  profilePictureUrl?: string;
}

export interface AuthResponse {
  token: string;
  user: User;
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

  getGoogleLoginUrl(): string {
    const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5028/api';
    // Remove /api suffix if present, then add full path
    const baseUrl = apiUrl.replace(/\/api$/, '');
    return `${baseUrl}/api/auth/google`;
  },
};
