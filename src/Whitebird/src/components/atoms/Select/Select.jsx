import React, { forwardRef } from "react";
import { FormControl, InputLabel, Select as MuiSelect, MenuItem, FormHelperText } from "@mui/material";
import "./Select.scss";

const Select = forwardRef(({
  label,
  options = [],
  value = "",
  onChange,
  onBlur,
  error = "",
  required = false,
  disabled = false,
  placeholder = "",
  className = "",
  name = "",
  fullWidth = true,
  size = "small",
  variant = "outlined",
}, ref) => {
  // Filter invalid options
  const safeOptions = Array.isArray(options)
    ? options.filter(opt => opt !== null && opt !== undefined && typeof opt === 'object' && opt.value !== undefined && opt.value !== null)
    : [];

  // Replace null/undefined with empty string
  const safeValue = (value === null || value === undefined) ? "" : value;

  return (
    <div className={`select-wrapper ${className}`}>
      <FormControl
        fullWidth={fullWidth}
        size={size}
        variant={variant}
        error={!!error}
        disabled={disabled}
        required={required}
        sx={{
          '& .MuiOutlinedInput-root': {
            borderRadius: '8px',
            backgroundColor: 'var(--input-bg)',
            '& .MuiOutlinedInput-notchedOutline': { borderColor: error ? 'var(--error)' : 'var(--input-border)' },
            '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: error ? 'var(--error)' : 'var(--primary)' },
            '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: error ? 'var(--error)' : 'var(--primary)' },
          },
          '& .MuiSelect-select': { color: 'var(--text-primary)', py: 1 },
          '& .MuiInputLabel-root': { color: 'var(--text-secondary)', '&.Mui-focused': { color: error ? 'var(--error)' : 'var(--primary)' }, '&.Mui-error': { color: 'var(--error)' } },
          '& .MuiSvgIcon-root': { color: 'var(--text-secondary)' },
          '&.Mui-disabled': { opacity: 0.6 },
        }}
      >
        <InputLabel>{label}</InputLabel>
        <MuiSelect
          ref={ref}
          value={safeValue}
          onChange={onChange}
          onBlur={onBlur}
          name={name}
          label={label}
          MenuProps={{
            PaperProps: {
              sx: {
                backgroundColor: 'var(--card-bg)', color: 'var(--text-primary)', borderRadius: '8px',
                border: '1px solid var(--border)', maxHeight: '300px',
                '& .MuiMenuItem-root': { fontSize: '14px', '&:hover': { backgroundColor: 'var(--surface)' }, '&.Mui-selected': { backgroundColor: 'rgba(220, 38, 38, 0.1)' } },
              },
            },
          }}
        >
          {safeOptions.map((option, index) => (
            <MenuItem key={`opt-${index}-${option.value}`} value={option.value}>{option.label}</MenuItem>
          ))}
        </MuiSelect>
        {error && <FormHelperText sx={{ color: 'var(--error)', ml: 0 }}>{error}</FormHelperText>}
      </FormControl>
    </div>
  );
});

Select.displayName = "Select";
export default Select;