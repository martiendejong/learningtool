import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { authService } from '../services/authService';
import type { AuthResponse, User } from '../services/authService';

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;

  login: (email: string, password: string) => Promise<boolean>;
  register: (email: string, password: string, fullName?: string, accountType?: 'Individual' | 'Organization') => Promise<boolean>;
  logout: () => void;
  setAuth: (data: AuthResponse) => void;
  isOrgAdmin: () => boolean;
  hasRole: (role: string) => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,

      setAuth: (data: AuthResponse) => {
        localStorage.setItem('token', data.token);
        set({
          user: data.user,
          token: data.token,
          isAuthenticated: true,
        });
      },

      login: async (email: string, password: string) => {
        set({ isLoading: true });
        try {
          const data = await authService.login({ email, password });
          localStorage.setItem('token', data.token);
          set({
            user: data.user,
            token: data.token,
            isAuthenticated: true,
            isLoading: false,
          });
          return true;
        } catch (error) {
          console.error('Login failed:', error);
          set({ isLoading: false });
          return false;
        }
      },

      register: async (email: string, password: string, fullName?: string, accountType?: 'Individual' | 'Organization') => {
        set({ isLoading: true });
        try {
          const data = await authService.register({ email, password, fullName, accountType });
          localStorage.setItem('token', data.token);
          set({
            user: data.user,
            token: data.token,
            isAuthenticated: true,
            isLoading: false,
          });
          return true;
        } catch (error) {
          console.error('Registration failed:', error);
          set({ isLoading: false });
          return false;
        }
      },

      logout: () => {
        localStorage.removeItem('token');
        set({
          user: null,
          token: null,
          isAuthenticated: false,
        });
      },

      isOrgAdmin: (): boolean => {
        const { user } = get();
        return user?.accountType === 'Organization' && user?.roleInOrganization === 'Admin';
      },

      hasRole: (role: string): boolean => {
        const { user } = get();
        return user?.roleInOrganization === role;
      },
    }),
    {
      name: 'auth-storage',
    }
  )
);
