import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../client';

interface TimeSlot {
  startTime: string;
  endTime: string;
  durationMinutes: number;
}

interface Session {
  id: string;
  clientId: string;
  clientName?: string;
  psychologistId: string;
  psychologistName?: string;
  scheduledAt: string;
  status: 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled';
  meetingLink?: string;
  notes?: string;
}

interface BookSessionDto {
  psychologistId: string;
  scheduledAt: string;
  notes?: string;
}

export function useAvailableSlots(psychologistId: string, date: string) {
  return useQuery({
    queryKey: ['slots', psychologistId, date],
    queryFn: async () => {
      const { data } = await apiClient.get<TimeSlot[]>(`/sessions/slots/${psychologistId}?date=${date}`);
      return data;
    },
    enabled: !!psychologistId && !!date,
  });
}

export function useClientSessions() {
  return useQuery({
    queryKey: ['sessions', 'client'],
    queryFn: async () => {
      const { data } = await apiClient.get<Session[]>('/sessions/client');
      return data;
    },
  });
}

export function usePsychologistSessions() {
  return useQuery({
    queryKey: ['sessions', 'psychologist'],
    queryFn: async () => {
      const { data } = await apiClient.get<Session[]>('/sessions/psychologist');
      return data;
    },
  });
}

export function useBookSession() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (dto: BookSessionDto) => {
      const { data } = await apiClient.post<Session>('/sessions', dto);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sessions'] });
      queryClient.invalidateQueries({ queryKey: ['slots'] });
    },
  });
}

export function useCancelSession() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (sessionId: string) => {
      await apiClient.post(`/sessions/${sessionId}/cancel`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sessions'] });
    },
  });
}
