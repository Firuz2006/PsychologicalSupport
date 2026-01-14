import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface User {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  language: string;
  isGuest: boolean;
  roles: string[];
}

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isPsychologist: boolean;
  isAdmin: boolean;
  setAuth: (user: User, token: string) => void;
  logout: () => void;
  updateUser: (user: Partial<User>) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      token: null,
      isAuthenticated: false,
      isPsychologist: false,
      isAdmin: false,

      setAuth: (user, token) => {
        localStorage.setItem('token', token);
        set({
          user,
          token,
          isAuthenticated: true,
          isPsychologist: user.roles.includes('Psychologist'),
          isAdmin: user.roles.includes('Admin'),
        });
      },

      logout: () => {
        localStorage.removeItem('token');
        set({
          user: null,
          token: null,
          isAuthenticated: false,
          isPsychologist: false,
          isAdmin: false,
        });
      },

      updateUser: (updates) => {
        const current = get().user;
        if (current) {
          set({ user: { ...current, ...updates } });
        }
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({ user: state.user, token: state.token }),
      onRehydrate: () => (state) => {
        if (state?.token) {
          localStorage.setItem('token', state.token);
          state.isAuthenticated = true;
          state.isPsychologist = state.user?.roles.includes('Psychologist') ?? false;
          state.isAdmin = state.user?.roles.includes('Admin') ?? false;
        }
      },
    }
  )
);
