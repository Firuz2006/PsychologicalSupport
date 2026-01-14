import { create } from 'zustand';

interface UiState {
  language: 'ru' | 'tj';
  sidebarOpen: boolean;
  setLanguage: (lang: 'ru' | 'tj') => void;
  toggleSidebar: () => void;
  setSidebarOpen: (open: boolean) => void;
}

export const useUiStore = create<UiState>((set) => ({
  language: (localStorage.getItem('language') as 'ru' | 'tj') || 'ru',
  sidebarOpen: false,

  setLanguage: (lang) => {
    localStorage.setItem('language', lang);
    set({ language: lang });
  },

  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
  setSidebarOpen: (open) => set({ sidebarOpen: open }),
}));
