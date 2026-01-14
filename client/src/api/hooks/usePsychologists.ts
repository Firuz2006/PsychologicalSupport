import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../client';

interface Specialization {
  id: number;
  key: string;
  name: string;
}

interface Psychologist {
  id: string;
  userId: string;
  firstName?: string;
  lastName?: string;
  photoPath?: string;
  experienceYears: number;
  education?: string;
  approachDescription?: string;
  languages: string[];
  workFormats: string[];
  pricePerSession: number;
  meetingLink?: string;
  isVerified: boolean;
  specializations: Specialization[];
}

interface PsychologistFilter {
  language?: string;
  workFormat?: string;
  specializationIds?: number[];
  maxPrice?: number;
  onlyVerified?: boolean;
  page?: number;
  pageSize?: number;
}

export function usePsychologists(filter?: PsychologistFilter) {
  return useQuery({
    queryKey: ['psychologists', filter],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (filter?.language) params.append('language', filter.language);
      if (filter?.workFormat) params.append('workFormat', filter.workFormat);
      if (filter?.maxPrice) params.append('maxPrice', String(filter.maxPrice));
      if (filter?.onlyVerified !== undefined) params.append('onlyVerified', String(filter.onlyVerified));
      if (filter?.page) params.append('page', String(filter.page));
      if (filter?.pageSize) params.append('pageSize', String(filter.pageSize));
      filter?.specializationIds?.forEach(id => params.append('specializationIds', String(id)));

      const { data } = await apiClient.get<Psychologist[]>(`/psychologists?${params}`);
      return data;
    },
  });
}

export function usePsychologist(id: string) {
  return useQuery({
    queryKey: ['psychologist', id],
    queryFn: async () => {
      const { data } = await apiClient.get<Psychologist>(`/psychologists/${id}`);
      return data;
    },
    enabled: !!id,
  });
}

export function useSpecializations(language: string = 'ru') {
  return useQuery({
    queryKey: ['specializations', language],
    queryFn: async () => {
      const { data } = await apiClient.get<Specialization[]>(`/psychologists/specializations?language=${language}`);
      return data;
    },
  });
}
