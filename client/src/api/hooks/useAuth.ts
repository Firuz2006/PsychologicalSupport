import { useMutation } from '@tanstack/react-query';
import { apiClient } from '../client';
import { useAuthStore } from '../../stores/authStore';

interface LoginDto {
  email: string;
  password: string;
}

interface RegisterDto {
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
  language?: string;
}

interface AuthResponse {
  token: string;
  user: {
    id: string;
    email: string;
    firstName?: string;
    lastName?: string;
    language: string;
    isGuest: boolean;
    roles: string[];
  };
}

export function useLogin() {
  const { setAuth } = useAuthStore();

  return useMutation({
    mutationFn: async (dto: LoginDto) => {
      const { data } = await apiClient.post<AuthResponse>('/auth/login', dto);
      return data;
    },
    onSuccess: (data) => {
      setAuth(data.user, data.token);
    },
  });
}

export function useRegister() {
  const { setAuth } = useAuthStore();

  return useMutation({
    mutationFn: async (dto: RegisterDto) => {
      const { data } = await apiClient.post<AuthResponse>('/auth/register', dto);
      return data;
    },
    onSuccess: (data) => {
      setAuth(data.user, data.token);
    },
  });
}

export function useGuestLogin() {
  const { setAuth } = useAuthStore();

  return useMutation({
    mutationFn: async (language: string = 'ru') => {
      const { data } = await apiClient.post<AuthResponse>(`/auth/guest?language=${language}`);
      return data;
    },
    onSuccess: (data) => {
      setAuth(data.user, data.token);
    },
  });
}
