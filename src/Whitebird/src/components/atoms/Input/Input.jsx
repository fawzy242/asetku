import React, { forwardRef, useState, useRef, useCallback } from "react";
import { TextField, InputAdornment, IconButton } from "@mui/material";
import { FiEye, FiEyeOff } from "react-icons/fi";
import "./Input.scss";

const Input = forwardRef(({
  label,
  type = "text",
  placeholder = "",
  value,
  onChange,
  onBlur,
  error = "",
  helperText = "",
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
  InputLabelProps = {},
  inputProps: customInputProps = {},
  register = null,
  registerOptions = {},
}, ref) => {
  const [showPassword, setShowPassword] = useState(false);
  const [focused, setFocused] = useState(false);
  const internalRef = useRef(null);
  const inputType = type === "password" && showPassword ? "text" : type;
  const hasValue = value !== null && value !== undefined && value !== "";

  const registerProps = register ? register(name, registerOptions) : {};

  const mergeRefs = useCallback((element) => {
    internalRef.current = element;
    if (registerProps.ref) {
      if (typeof registerProps.ref === 'function') registerProps.ref(element);
      else registerProps.ref.current = element;
    }
    if (ref) {
      if (typeof ref === 'function') ref(element);
      else ref.current = element;
    }
  }, [registerProps.ref, ref]);

  const finalOnChange = (e) => {
    registerProps.onChange?.(e);
    onChange?.(e);
  };

  const finalOnBlur = (e) => {
    registerProps.onBlur?.(e);
    onBlur?.(e);
    setFocused(false);
  };

  const finalError = error || helperText;

  return (
    <div className={`input-wrapper ${className}`}>
      <TextField
        inputRef={mergeRefs}
        label={label}
        type={inputType}
        placeholder={(!focused && !hasValue) ? placeholder || label : ""}
        value={value}
        onChange={finalOnChange}
        onBlur={finalOnBlur}
        onFocus={() => setFocused(true)}
        error={!!finalError}
        helperText={finalError}
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
          ...InputLabelProps,
        }}
        InputProps={{
          readOnly: readOnly,
          autoComplete: autoComplete,
          startAdornment: startAdornment ? (
            <InputAdornment position="start">{startAdornment}</InputAdornment>
          ) : undefined,
          endAdornment: (type === "password" || endAdornment) ? (
            <InputAdornment position="end">
              {type === "password" && (
                <IconButton
                  onClick={() => setShowPassword(!showPassword)}
                  edge="end"
                  size="small"
                  aria-label={showPassword ? "Hide password" : "Show password"}
                >
                  {showPassword ? <FiEyeOff size={18} /> : <FiEye size={18} />}
                </IconButton>
              )}
              {endAdornment && type !== "password" && endAdornment}
            </InputAdornment>
          ) : undefined,
          ...customInputProps,
        }}
      />
    </div>
  );
});

Input.displayName = "Input";
export default Input;