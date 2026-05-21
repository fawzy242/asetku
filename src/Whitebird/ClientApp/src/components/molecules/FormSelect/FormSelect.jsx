import React, { forwardRef } from 'react';
import { FormControl, InputLabel, Select as MuiSelect, MenuItem, FormHelperText } from '@mui/material';
import { useUIStore } from '../../../stores/uiStore';

/**
 * Reusable Form Select component with react-hook-form integration
 * 
 * @param {Object} props
 * @param {string} props.name - Field name (for react-hook-form)
 * @param {string} props.label - Select label
 * @param {Array} props.options - Options array { value, label }
 * @param {boolean} props.required - Required flag
 * @param {boolean} props.disabled - Disabled flag
 * @param {string} props.error - Error message
 * @param {any} props.register - react-hook-form register function
 * @param {Object} props.registerOptions - Register options
 * @param {any} props.value - Controlled value (if not using register)
 * @param {Function} props.onChange - onChange handler (if not using register)
 */

const FormSelect = forwardRef(({
  name,
  label,
  options = [],
  required = false,
  disabled = false,
  error = '',
  register,
  registerOptions = {},
  value,
  onChange,
  ...rest
}, ref) => {
  const theme = useUIStore((s) => s.theme);
  const isDark = theme === 'dark';

  const registerProps = register ? register(name, {
    required: required ? `${label} is required` : false,
    ...registerOptions
  }) : {};

  const finalError = error || registerProps.error?.message;
  const errorId = finalError ? `${name}-error` : undefined;
  const labelId = `${name}-label`;

  const handleChange = (e) => {
    if (registerProps.onChange) registerProps.onChange(e);
    if (onChange) onChange(e);
  };

  const selectValue = value !== undefined ? value : registerProps.value;

  return (
    <FormControl
      fullWidth
      size="small"
      error={!!finalError}
      disabled={disabled}
      required={required}
    >
      <InputLabel id={labelId}>{label}</InputLabel>
      <MuiSelect
        {...registerProps}
        inputRef={ref}
        labelId={labelId}
        label={label}
        value={selectValue || ''}
        onChange={handleChange}
        name={name}
        aria-describedby={errorId}
        MenuProps={{
          PaperProps: {
            sx: {
              maxHeight: '300px',
              backgroundColor: isDark ? '#1f2937' : '#ffffff',
              '&::-webkit-scrollbar': {
                width: '6px',
              },
              '&::-webkit-scrollbar-track': {
                background: isDark ? '#111827' : '#f3f4f6',
              },
              '&::-webkit-scrollbar-thumb': {
                background: isDark ? '#4b5563' : '#d1d5db',
                borderRadius: '3px',
              },
              '&::-webkit-scrollbar-thumb:hover': {
                background: isDark ? '#6b7280' : '#9ca3af',
              },
            },
          },
        }}
        {...rest}
      >
        {options.map((option, index) => (
          <MenuItem
            key={`${name}-opt-${index}-${option.value}`}
            value={option.value}
            sx={{
              fontSize: '14px',
              borderRadius: '6px',
              margin: '2px 4px',
              color: isDark ? '#f9fafb' : '#111827',
              '&:hover': {
                backgroundColor: isDark ? '#374151' : '#f3f4f6',
              },
              '&.Mui-selected': {
                backgroundColor: isDark ? 'rgba(220, 38, 38, 0.25)' : 'rgba(220, 38, 38, 0.1)',
              },
            }}
          >
            {option.label}
          </MenuItem>
        ))}
      </MuiSelect>
      {finalError && (
        <FormHelperText id={errorId}>
          {finalError}
        </FormHelperText>
      )}
    </FormControl>
  );
});

FormSelect.displayName = 'FormSelect';
export default FormSelect;