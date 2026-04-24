import React from 'react';
import ReactDOM from 'react-dom/client';
import { ThemeProvider as MuiThemeProvider, createTheme } from '@mui/material/styles';
import App from './app';
import './styles/main.scss';

// Create theme that responds to CSS variables
const getMuiTheme = (mode) => createTheme({
  palette: {
    mode: mode,
    primary: { main: mode === 'dark' ? '#ef4444' : '#dc2626' },
    error: { main: mode === 'dark' ? '#f87171' : '#ef4444' },
    warning: { main: mode === 'dark' ? '#fbbf24' : '#f59e0b' },
    info: { main: mode === 'dark' ? '#60a5fa' : '#3b82f6' },
    success: { main: mode === 'dark' ? '#34d399' : '#10b981' },
    background: {
      default: mode === 'dark' ? '#111827' : '#ffffff',
      paper: mode === 'dark' ? '#1f2937' : '#ffffff',
    },
    text: {
      primary: mode === 'dark' ? '#f9fafb' : '#111827',
      secondary: mode === 'dark' ? '#9ca3af' : '#6b7280',
    },
  },
  typography: {
    fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
  },
  shape: { borderRadius: 8 },
  components: {
    MuiTextField: {
      defaultProps: { variant: 'outlined', size: 'small', fullWidth: true },
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            backgroundColor: mode === 'dark' ? '#374151' : '#ffffff',
          },
        },
      },
    },
    MuiSelect: {
      defaultProps: { variant: 'outlined', size: 'small', fullWidth: true },
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            backgroundColor: mode === 'dark' ? '#374151' : '#ffffff',
          },
        },
      },
    },
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: {
        root: { textTransform: 'none', fontWeight: 500 },
      },
    },
    MuiMenuItem: {
      styleOverrides: {
        root: {
          '&.Mui-selected': {
            backgroundColor: mode === 'dark' ? 'rgba(239, 68, 68, 0.15)' : 'rgba(220, 38, 38, 0.1)',
          },
        },
      },
    },
  },
});

const root = ReactDOM.createRoot(document.getElementById('root'));

// Render with initial theme
const initialMode = document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
let currentTheme = getMuiTheme(initialMode);

const render = () => {
  root.render(
    <React.StrictMode>
      <MuiThemeProvider theme={currentTheme}>
        <App />
      </MuiThemeProvider>
    </React.StrictMode>
  );
};

render();

// Listen for theme changes
const observer = new MutationObserver((mutations) => {
  mutations.forEach((mutation) => {
    if (mutation.attributeName === 'data-theme') {
      const newMode = document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
      currentTheme = getMuiTheme(newMode);
      render();
    }
  });
});

observer.observe(document.documentElement, { attributes: true });