import React, { forwardRef } from 'react';
import { TextField, InputAdornment, IconButton } from '@mui/material';
import { FiEye, FiEyeOff } from 'react-icons/fi';

/**
 * Reusable Form Input component with react-hook-form integration
 * 
 * @param {Object} props
 * @param {string} props.name - Field name (for react-hook-form)
 * @param {string} props.label - Input label
 * @param {string} props.type - Input type (text, password, email, etc)
 * @param {string} props.placeholder - Placeholder text
 * @param {boolean} props.required - Required flag
 * @param {boolean} props.disabled - Disabled flag
 * @param {string} props.error - Error message
 * @param {any} props.register - react-hook-form register function
 * @param {Object} props.registerOptions - Register options (validation)
 * @param {React.ReactNode} props.startAdornment - Start adornment
 * @param {React.ReactNode} props.endAdornment - End adornment
 * @param {boolean} props.multiline - Multiline flag
 * @param {number} props.rows - Number of rows for multiline
 */

const FormInput = forwardRef(({
  name,
  label,
  type = 'text',
  placeholder = '',
  required = false,
  disabled = false,
  error = '',
  register,
  registerOptions = {},
  startAdornment = null,
  endAdornment = null,
  multiline = false,
  rows = 1,
  ...rest
}, ref) => {
  const [showPassword, setShowPassword] = React.useState(false);
  const inputType = type === 'password' && showPassword ? 'text' : type;

  const registerProps = register ? register(name, {
    required: required ? `${label} is required` : false,
    ...registerOptions
  }) : {};

  const finalError = error || registerProps.error?.message;

  return (
    <TextField
      {...registerProps}
      inputRef={ref}
      name={name}
      label={label}
      type={inputType}
      placeholder={placeholder}
      required={required}
      disabled={disabled}
      error={!!finalError}
      helperText={finalError}
      fullWidth
      size="small"
      multiline={multiline}
      rows={multiline ? rows : undefined}
      InputProps={{
        startAdornment: startAdornment ? (
          <InputAdornment position="start">{startAdornment}</InputAdornment>
        ) : undefined,
        endAdornment: type === 'password' ? (
          <InputAdornment position="end">
            <IconButton
              onClick={() => setShowPassword(!showPassword)}
              edge="end"
              size="small"
              aria-label={showPassword ? 'Hide password' : 'Show password'}
            >
              {showPassword ? <FiEyeOff size={18} /> : <FiEye size={18} />}
            </IconButton>
          </InputAdornment>
        ) : endAdornment ? (
          <InputAdornment position="end">{endAdornment}</InputAdornment>
        ) : undefined,
      }}
      {...rest}
    />
  );
});

FormInput.displayName = 'FormInput';
export default FormInput;