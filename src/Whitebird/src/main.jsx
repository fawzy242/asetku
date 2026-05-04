import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider as MuiThemeProvider } from '@mui/material/styles';
import { Toaster } from 'react-hot-toast';
import App from './app';
import { useUIStore } from './stores/uiStore';
import { createAppTheme } from './core/theme/muiThemeFactory';
import './styles/main.scss';

const stored = localStorage.getItem('theme');
const systemDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
const initialTheme = stored || (systemDark ? 'dark' : 'light');
document.documentElement.setAttribute('data-theme', initialTheme);

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,
      retry: 1,
      refetchOnWindowFocus: false,
      placeholderData: (previousData) => previousData,
    },
    mutations: {
      retry: 0,
    },
  },
});

const DynamicMuiThemeProvider = ({ children }) => {
  const theme = useUIStore((s) => s.theme);
  const isDark = theme === 'dark';
  const muiTheme = React.useMemo(() => createAppTheme(isDark), [isDark]);

  return (
    <MuiThemeProvider theme={muiTheme}>
      {children}
    </MuiThemeProvider>
  );
};

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <DynamicMuiThemeProvider>
        <App />
        <Toaster
          position="bottom-right"
          reverseOrder={false}
          gutter={8}
          containerClassName="toast-container"
          toastOptions={{
            duration: 3000,
            className: 'toast-custom',
            style: {
              background: 'var(--card-bg)',
              color: 'var(--text-primary)',
              border: '1px solid var(--border)',
              borderRadius: '12px',
              padding: '12px 16px',
              fontSize: '0.875rem',
              fontWeight: 500,
              boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.1)',
            },
            success: {
              iconTheme: {
                primary: '#10b981',
                secondary: 'white',
              },
            },
            error: {
              iconTheme: {
                primary: '#ef4444',
                secondary: 'white',
              },
            },
          }}
        />
      </DynamicMuiThemeProvider>
    </QueryClientProvider>
  </React.StrictMode>
);