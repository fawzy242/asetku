import React, { forwardRef, memo } from "react";
import { FormControl, InputLabel, Select as MuiSelect, MenuItem, FormHelperText } from "@mui/material";
import { useUIStore } from "../../../stores/uiStore";
import "./Select.scss";

const Select = memo(forwardRef(({
  label,
  options = [],
  value,
  onChange,
  onBlur,
  error = "",
  helperText = "",
  required = false,
  disabled = false,
  className = "",
  name = "",
  fullWidth = true,
  size = "small",
  variant = "outlined",
  register = null,
  registerOptions = {},
}, ref) => {
  const theme = useUIStore((s) => s.theme);
  const isDark = theme === 'dark';

  const safeOptions = Array.isArray(options)
    ? options.filter(opt =>
        opt !== null && opt !== undefined && typeof opt === 'object' &&
        opt.value !== undefined && opt.value !== null
      )
    : [];

  const safeValue = (value === null || value === undefined) ? "" : value;
  const registerProps = register ? register(name, registerOptions) : {};

  const finalOnChange = (e) => {
    registerProps.onChange?.(e);
    onChange?.(e);
  };

  const finalOnBlur = (e) => {
    registerProps.onBlur?.(e);
    onBlur?.(e);
  };

  const finalRef = (el) => {
    registerProps.ref?.(el);
    if (typeof ref === 'function') ref(el);
    else if (ref) ref.current = el;
  };

  const finalError = error || helperText;
  const errorId = finalError ? `${name}-error` : undefined;
  const labelId = `${name}-label`;

  return (
    <div className={`select-wrapper ${className}`}>
      <FormControl
        fullWidth={fullWidth}
        size={size}
        variant={variant}
        error={!!finalError}
        disabled={disabled}
        required={required}
      >
        <InputLabel id={labelId}>{label}</InputLabel>
        <MuiSelect
          inputRef={finalRef}
          value={safeValue}
          onChange={finalOnChange}
          onBlur={finalOnBlur}
          name={name}
          labelId={labelId}
          label={label}
          aria-describedby={errorId}
          MenuProps={{
            PaperProps: {
              sx: {
                maxHeight: '300px',
                backgroundColor: isDark ? '#1f2937' : '#ffffff',
                color: isDark ? '#f9fafb' : '#111827',
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
        >
          {safeOptions.map((option, index) => (
            <MenuItem
              key={`opt-${index}-${option.value}`}
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
              {option.label ?? String(option.value)}
            </MenuItem>
          ))}
        </MuiSelect>
        {finalError && (
          <FormHelperText id={errorId}>
            {finalError}
          </FormHelperText>
        )}
      </FormControl>
    </div>
  );
}));

Select.displayName = "Select";
export default Select;