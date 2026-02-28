import api from './api';

export interface User {
  id: string;
  email: string;
  userName: string;
  emailConfirmed: boolean;
  role: string;
  createdAt: string;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  role?: string;
}

export interface UpdateUserRequest {
  email?: string;
  role?: string;
}

export const userService = {
  // Get all users (admin only)
  async getUsers(): Promise<User[]> {
    const response = await api.get<User[]>('/user');
    return response.data;
  },

  // Create user (admin only)
  async createUser(request: CreateUserRequest): Promise<User> {
    const response = await api.post<User>('/user', request);
    return response.data;
  },

  // Update user (admin only)
  async updateUser(id: string, request: UpdateUserRequest): Promise<User> {
    const response = await api.put<User>(`/user/${id}`, request);
    return response.data;
  },

  // Delete user (admin only)
  async deleteUser(id: string): Promise<void> {
    await api.delete(`/user/${id}`);
  },

  // Get available roles
  async getRoles(): Promise<string[]> {
    const response = await api.get<string[]>('/user/roles');
    return response.data;
  },

  // Get current user info
  async getCurrentUser(): Promise<User> {
    const response = await api.get<User>('/user/current');
    return response.data;
  },
};
