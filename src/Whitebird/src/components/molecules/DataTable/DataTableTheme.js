import { createTheme } from '@mui/material/styles';

/**
 * @deprecated Use the inline theme in DataTable.jsx directly.
 * Kept for backward compatibility if any external code imports this.
 */
export const createDataGridTheme = (isDark, minHeight = 400, hideFooter = true) => {
  return createTheme({
    palette: {
      mode: isDark ? 'dark' : 'light',
      primary: { main: '#dc2626' },
      background: {
        default: isDark ? '#111827' : '#ffffff',
        paper: isDark ? '#1f2937' : '#ffffff',
      },
      text: {
        primary: isDark ? '#f9fafb' : '#111827',
        secondary: isDark ? '#9ca3af' : '#6b7280',
      },
    },
    typography: {
      fontFamily: "'Inter', sans-serif",
      fontSize: 14,
    },
    shape: { borderRadius: 8 },
    components: {
      MuiDataGrid: {
        styleOverrides: {
          root: {
            border: 'none',
            backgroundColor: 'transparent',
            width: '100%',
            minWidth: '100%',
            minHeight: minHeight || 'auto',
            '& .MuiDataGrid-main': { width: '100%' },
            '& .MuiDataGrid-virtualScroller': { width: '100%' },
            '& .MuiDataGrid-columnHeaders': {
              backgroundColor: isDark ? '#374151' : '#f9fafb',
              color: isDark ? '#f9fafb' : '#374151',
              fontWeight: 600,
              fontSize: '0.875rem',
              borderBottom: `1px solid ${isDark ? '#4b5563' : '#e5e7eb'}`,
            },
            '& .MuiDataGrid-cell': {
              borderBottom: `1px solid ${isDark ? '#374151' : '#f3f4f6'}`,
              color: isDark ? '#f9fafb' : '#1f2937',
              fontSize: '0.875rem',
              '&:focus, &:focus-within': { outline: 'none' },
            },
            '& .MuiDataGrid-row:hover': {
              backgroundColor: isDark ? '#374151' : '#f9fafb',
            },
            '& .MuiDataGrid-footerContainer': {
              display: hideFooter ? 'none' : 'flex',
            },
            '& .MuiDataGrid-columnSeparator, & .MuiDataGrid-menuIcon': {
              display: 'none',
            },
          },
        },
      },
      MuiCheckbox: {
        styleOverrides: {
          root: {
            color: isDark ? '#9ca3af' : '#6b7280',
            '&.Mui-checked': { color: '#dc2626' },
          },
        },
      },
    },
  });
};