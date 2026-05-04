import React, { forwardRef } from "react";
import { FormControl, InputLabel, Select as MuiSelect, MenuItem, FormHelperText } from "@mui/material";
import "./Select.scss";

const Select = forwardRef(({
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
              },
            },
          }}
        >
          {safeOptions.map((option, index) => (
            <MenuItem key={`opt-${index}-${option.value}`} value={option.value}>
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
});

Select.displayName = "Select";
export default Select;