import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider as MuiThemeProvider } from '@mui/material/styles';
import App from './app';
import { useUIStore } from './stores/uiStore';
import { createAppTheme } from './core/theme/muiThemeFactory';
import './styles/main.scss';

// SweetAlert2 styles
import 'sweetalert2/dist/sweetalert2.min.css';

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
      </DynamicMuiThemeProvider>
    </QueryClientProvider>
  </React.StrictMode>
);