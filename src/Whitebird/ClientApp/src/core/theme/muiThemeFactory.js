import { createTheme } from '@mui/material/styles';

export const createAppTheme = (isDark) => {
  return createTheme({
    palette: {
      mode: isDark ? 'dark' : 'light',
      primary: { main: '#dc2626' },
      error: { main: '#ef4444' },
      warning: { main: '#f59e0b' },
      info: { main: '#3b82f6' },
      success: { main: '#10b981' },
      background: {
        default: isDark ? '#111827' : '#f9fafb',
        paper: isDark ? '#1f2937' : '#ffffff',
      },
      text: {
        primary: isDark ? '#f9fafb' : '#111827',
        secondary: isDark ? '#9ca3af' : '#6b7280',
      },
    },
    typography: {
      fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
      fontSize: 14,
    },
    shape: { borderRadius: 8 },
    components: {
      MuiTextField: {
        defaultProps: {
          variant: 'outlined',
          size: 'small',
          fullWidth: true,
        },
        styleOverrides: {
          root: {
            '& .MuiOutlinedInput-root': {
              backgroundColor: isDark ? '#374151' : '#ffffff',
              borderRadius: 8,
              '& .MuiOutlinedInput-notchedOutline': {
                borderColor: isDark ? '#4b5563' : '#d1d5db',
              },
              '&:hover .MuiOutlinedInput-notchedOutline': {
                borderColor: '#dc2626',
              },
              '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                borderColor: '#dc2626',
              },
              '&.Mui-disabled': {
                backgroundColor: isDark ? '#1f2937' : '#f3f4f6',
                opacity: 0.6,
              },
              '& .MuiInputBase-input': {
                color: isDark ? '#f9fafb' : '#111827',
                '&::placeholder': {
                  color: isDark ? '#6b7280' : '#9ca3af',
                  opacity: 1,
                },
                // FIX AUTOFILL BLUE BACKGROUND
                '&:-webkit-autofill': {
                  WebkitBoxShadow: isDark
                    ? '0 0 0 100px #374151 inset !important'
                    : '0 0 0 100px #ffffff inset !important',
                  WebkitTextFillColor: isDark ? '#f9fafb !important' : '#111827 !important',
                  caretColor: isDark ? '#f9fafb' : '#111827',
                  borderRadius: '8px',
                  transition: 'background-color 5000s ease-in-out 0s',
                },
                '&:-webkit-autofill:hover': {
                  WebkitBoxShadow: isDark
                    ? '0 0 0 100px #374151 inset !important'
                    : '0 0 0 100px #ffffff inset !important',
                },
                '&:-webkit-autofill:focus': {
                  WebkitBoxShadow: isDark
                    ? '0 0 0 100px #374151 inset !important'
                    : '0 0 0 100px #ffffff inset !important',
                },
                '&:-webkit-autofill:active': {
                  WebkitBoxShadow: isDark
                    ? '0 0 0 100px #374151 inset !important'
                    : '0 0 0 100px #ffffff inset !important',
                },
              },
            },
            '& .MuiInputLabel-root': {
              color: isDark ? '#9ca3af' : '#6b7280',
              '&.Mui-focused': { color: '#dc2626' },
              '&.Mui-error': { color: '#ef4444' },
            },
            '& .MuiFormHelperText-root': {
              color: '#ef4444',
              margin: '4px 0 0 0',
              fontSize: '12px',
            },
          },
        },
      },
      MuiOutlinedInput: {
        styleOverrides: {
          root: {
            backgroundColor: isDark ? '#374151' : '#ffffff',
            borderRadius: 8,
            '& .MuiOutlinedInput-notchedOutline': {
              borderColor: isDark ? '#4b5563' : '#d1d5db',
            },
            '&:hover .MuiOutlinedInput-notchedOutline': {
              borderColor: '#dc2626',
            },
            '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
              borderColor: '#dc2626',
            },
            '&.Mui-disabled': {
              backgroundColor: isDark ? '#1f2937' : '#f3f4f6',
              opacity: 0.6,
            },
            '& .MuiInputBase-input': {
              color: isDark ? '#f9fafb' : '#111827',
              '&:-webkit-autofill': {
                WebkitBoxShadow: isDark
                  ? '0 0 0 100px #374151 inset !important'
                  : '0 0 0 100px #ffffff inset !important',
                WebkitTextFillColor: isDark ? '#f9fafb !important' : '#111827 !important',
                borderRadius: '8px',
                transition: 'background-color 5000s ease-in-out 0s',
              },
            },
          },
        },
      },
      MuiInputLabel: {
        styleOverrides: {
          root: {
            color: isDark ? '#9ca3af' : '#6b7280',
            '&.Mui-focused': { color: '#dc2626' },
            '&.Mui-error': { color: '#ef4444' },
          },
        },
      },
      MuiFormControl: {
        styleOverrides: {
          root: {
            '& .MuiOutlinedInput-root': {
              backgroundColor: isDark ? '#374151' : '#ffffff',
              borderRadius: 8,
            },
          },
        },
      },
      MuiSelect: {
        styleOverrides: {
          icon: { color: isDark ? '#9ca3af' : '#6b7280' },
          select: {
            color: isDark ? '#f9fafb' : '#111827',
            borderRadius: 8,
          },
        },
      },
      MuiMenuItem: {
        styleOverrides: {
          root: {
            fontSize: '14px',
            borderRadius: 6,
            margin: '2px 4px',
            '&:hover': {
              backgroundColor: isDark ? '#374151' : '#f3f4f6',
              borderRadius: 6,
            },
            '&.Mui-selected': {
              backgroundColor: isDark ? 'rgba(220, 38, 38, 0.25)' : 'rgba(220, 38, 38, 0.1)',
              borderRadius: 6,
            },
          },
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: {
            backgroundImage: 'none',
            borderRadius: 8,
          },
        },
      },
      MuiList: {
        styleOverrides: {
          root: {
            padding: '4px',
            borderRadius: 8,
          },
        },
      },
      MuiFormHelperText: {
        styleOverrides: {
          root: { color: '#ef4444', margin: '4px 0 0 0', fontSize: '12px' },
        },
      },
      MuiButton: {
        defaultProps: { disableElevation: true },
        styleOverrides: {
          root: {
            textTransform: 'none',
            fontWeight: 500,
            borderRadius: 8,
          },
          outlined: {
            borderColor: isDark ? '#4b5563' : '#d1d5db',
            color: isDark ? '#f9fafb' : '#111827',
            '&:hover': {
              borderColor: '#dc2626',
              backgroundColor: isDark ? 'rgba(220, 38, 38, 0.08)' : 'rgba(220, 38, 38, 0.04)',
            },
          },
          text: {
            color: isDark ? '#f9fafb' : '#111827',
            '&:hover': {
              backgroundColor: isDark ? '#374151' : '#f3f4f6',
            },
          },
        },
      },
      MuiCheckbox: {
        styleOverrides: {
          root: {
            '&.Mui-checked': { color: '#dc2626' },
          },
        },
      },
      MuiChip: {
        styleOverrides: {
          root: {
            borderRadius: 6,
          },
          outlined: {
            borderColor: isDark ? '#4b5563' : '#d1d5db',
            color: isDark ? '#f9fafb' : '#111827',
          },
        },
      },
      MuiDataGrid: {
        styleOverrides: {
          root: {
            border: 'none',
            borderRadius: 8,
            '& .MuiDataGrid-columnHeaders': {
              backgroundColor: isDark ? '#374151' : '#f9fafb',
              color: isDark ? '#f9fafb' : '#374151',
              fontWeight: 600,
              fontSize: '0.875rem',
              borderBottom: `1px solid ${isDark ? '#4b5563' : '#e5e7eb'}`,
              borderRadius: '8px 8px 0 0',
            },
            '& .MuiDataGrid-cell': {
              borderBottom: `1px solid ${isDark ? '#374151' : '#f3f4f6'}`,
              fontSize: '0.875rem',
              '&:focus, &:focus-within': { outline: 'none' },
            },
            '& .MuiDataGrid-row:hover': {
              backgroundColor: isDark ? '#374151' : '#f9fafb',
            },
            '& .MuiDataGrid-footerContainer': {
              backgroundColor: isDark ? '#374151' : '#f9fafb',
              borderTop: `1px solid ${isDark ? '#4b5563' : '#e5e7eb'}`,
              borderRadius: '0 0 8px 8px',
            },
            '& .MuiDataGrid-columnSeparator, & .MuiDataGrid-menuIcon': {
              display: 'none',
            },
            '& .MuiDataGrid-overlay': {
              backgroundColor: isDark ? 'rgba(17, 24, 39, 0.7)' : 'rgba(255, 255, 255, 0.7)',
            },
          },
        },
      },
    },
  });
};

export default createAppTheme;