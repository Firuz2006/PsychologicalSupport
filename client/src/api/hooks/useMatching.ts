import { useMutation } from '@tanstack/react-query';
import { apiClient } from '../client';

interface QuestionnaireSubmit {
  gender: string;
  age: number;
  preferredLanguage: string;
  mainIssue: string;
  urgencyLevel: string;
  formatPreference: string;
  additionalInfo?: string;
  guestSessionId?: string;
}

interface PsychologistMatch {
  id: string;
  name: string;
  photoPath?: string;
  experienceYears: number;
  price: number;
  specializations: string[];
  matchReason: string;
  matchScore: number;
}

export function useSubmitQuestionnaire() {
  return useMutation({
    mutationFn: async (dto: QuestionnaireSubmit) => {
      const { data } = await apiClient.post<PsychologistMatch[]>('/matching/questionnaire', dto);
      return data;
    },
  });
}
