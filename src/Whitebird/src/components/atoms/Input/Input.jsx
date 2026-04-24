import React, { forwardRef, useState } from "react";
import { TextField, InputAdornment, IconButton } from "@mui/material";
import { FiEye, FiEyeOff } from "react-icons/fi";
import "./Input.scss";

const Input = forwardRef(({
  label,
  type = "text",
  placeholder = "",
  value = "",
  onChange,
  onBlur,
  error = "",
  required = false,
  disabled = false,
  readOnly = false,
  startAdornment = null,
  endAdornment = null,
  className = "",
  name = "",
  autoFocus = false,
  fullWidth = true,
  size = "small",
  variant = "outlined",
  multiline = false,
  rows = 1,
  autoComplete = "off",
}, ref) => {
  const [showPassword, setShowPassword] = useState(false);
  const [focused, setFocused] = useState(false);
  const inputType = type === "password" && showPassword ? "text" : type;
  const hasValue = value !== null && value !== undefined && value !== "";

  const adornments = {
    endAdornment: (type === "password" || endAdornment) && (
      <InputAdornment position="end">
        {type === "password" && (
          <IconButton onClick={() => setShowPassword(!showPassword)} edge="end" size="small">
            {showPassword ? <FiEyeOff size={18} /> : <FiEye size={18} />}
          </IconButton>
        )}
        {endAdornment && type !== "password" && endAdornment}
      </InputAdornment>
    ),
    startAdornment: startAdornment && (
      <InputAdornment position="start">{startAdornment}</InputAdornment>
    ),
  };

  return (
    <div className={`input-wrapper ${className}`}>
      <TextField
        inputRef={ref}
        label={label}
        type={inputType}
        placeholder={(!focused && !hasValue) ? label : ""}
        value={value}
        onChange={onChange}
        onBlur={(e) => { setFocused(false); onBlur?.(e); }}
        onFocus={() => setFocused(true)}
        error={!!error}
        helperText={error}
        required={required}
        disabled={disabled}
        name={name}
        autoFocus={autoFocus}
        fullWidth={fullWidth}
        size={size}
        variant={variant}
        multiline={multiline}
        rows={multiline ? rows : undefined}
        autoComplete={autoComplete}
        InputLabelProps={{
          shrink: focused || hasValue,
          sx: {
            color: 'var(--text-secondary)',
            fontSize: size === 'medium' ? '16px' : '14px',
            '&.Mui-focused': { color: error ? 'var(--error)' : 'var(--primary)' },
            '&.Mui-error': { color: 'var(--error)' },
          },
        }}
        InputProps={{
          ...adornments,
          readOnly: readOnly,
          autoComplete: autoComplete,
          sx: {
            borderRadius: '8px',
            backgroundColor: 'var(--input-bg)',
            '& .MuiOutlinedInput-notchedOutline': {
              borderColor: error ? 'var(--error)' : 'var(--input-border)',
            },
            '&:hover .MuiOutlinedInput-notchedOutline': {
              borderColor: error ? 'var(--error)' : 'var(--primary)',
            },
            '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
              borderColor: error ? 'var(--error)' : 'var(--primary)',
            },
            '& .MuiInputBase-input': {
              color: 'var(--text-primary)',
              fontSize: size === 'medium' ? '16px' : '14px',
              padding: size === 'medium' ? '14px 14px' : '10px 14px',
              '&::placeholder': {
                color: 'var(--text-secondary)',
                opacity: 0.6,
              },
              '&:-webkit-autofill, &:-webkit-autofill:hover, &:-webkit-autofill:focus, &:-webkit-autofill:active': {
                WebkitBoxShadow: '0 0 0 100px var(--input-bg) inset !important',
                WebkitTextFillColor: 'var(--text-primary) !important',
                caretColor: 'var(--text-primary)',
                transition: 'background-color 5000s ease-in-out 0s',
              },
            },
            '&.Mui-disabled': { backgroundColor: 'var(--surface)', opacity: 0.6 },
            '& input[type=number]': {
              MozAppearance: 'textfield',
              '&::-webkit-outer-spin-button, &::-webkit-inner-spin-button': {
                WebkitAppearance: 'none',
                margin: 0,
              },
            },
          },
        }}
        FormHelperTextProps={{
          sx: {
            color: 'var(--error)',
            margin: '4px 0 0 0',
            fontSize: '12px',
          },
        }}
      />
    </div>
  );
});

Input.displayName = "Input";
export default Input;