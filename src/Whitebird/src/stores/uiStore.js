import { create } from 'zustand';
import { persist } from 'zustand/middleware';

const getSystemTheme = () =>
  typeof window !== 'undefined' && window.matchMedia('(prefers-color-scheme: dark)').matches
    ? 'dark'
    : 'light';

const applyTheme = (theme) => {
  if (typeof document !== 'undefined') {
    document.documentElement.setAttribute('data-theme', theme);
  }
};

const applySidebarWidth = (collapsed) => {
  if (typeof document !== 'undefined') {
    const width = collapsed ? '80px' : '260px';
    document.documentElement.style.setProperty('--sidebar-width', width);
  }
};

export const useUIStore = create(
  persist(
    (set, get) => ({
      sidebarCollapsed: typeof window !== 'undefined'
        ? localStorage.getItem('sidebar-collapsed') === 'true'
        : false,

      theme: typeof window !== 'undefined'
        ? (localStorage.getItem('theme') || getSystemTheme())
        : 'light',

      toggleSidebar: () => set((state) => {
        const newValue = !state.sidebarCollapsed;
        localStorage.setItem('sidebar-collapsed', String(newValue));
        applySidebarWidth(newValue);
        return { sidebarCollapsed: newValue };
      }),

      toggleTheme: () => set((state) => {
        const newTheme = state.theme === 'light' ? 'dark' : 'light';
        localStorage.setItem('theme', newTheme);
        applyTheme(newTheme);
        return { theme: newTheme };
      }),

      init: () => {
        const state = get();
        // PASTIKAN theme di-apply ke document.documentElement SESEGERA MUNGKIN
        applyTheme(state.theme);
        applySidebarWidth(state.sidebarCollapsed);

        if (typeof window !== 'undefined') {
          const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
          const handler = (e) => {
            const stored = localStorage.getItem('theme');
            if (!stored) {
              const newTheme = e.matches ? 'dark' : 'light';
              applyTheme(newTheme);
              set({ theme: newTheme });
            }
          };
          mediaQuery.addEventListener('change', handler);
        }
      },
    }),
    {
      name: 'whitebird-ui-store',
      partialize: (state) => ({
        sidebarCollapsed: state.sidebarCollapsed,
        theme: state.theme,
      }),
    }
  )
);

// INITIALIZE IMMEDIATELY saat module load
if (typeof window !== 'undefined') {
  const stored = localStorage.getItem('theme');
  const initialTheme = stored || getSystemTheme();
  applyTheme(initialTheme);
  useUIStore.getState().init();
}

export default useUIStore;