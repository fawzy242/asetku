import React, { forwardRef } from 'react';
import './Input.scss';

const Input = forwardRef(({
  label,
  type = 'text',
  placeholder = '',
  value = '',
  onChange,
  onBlur,
  error = '',
  required = false,
  disabled = false,
  readOnly = false,
  startAdornment = null,
  endAdornment = null,
  className = '',
  name = '',
  autoFocus = false
}, ref) => {
  const inputClasses = [
    'input',
    error ? 'input--error' : '',
    startAdornment ? 'input--with-start' : '',
    endAdornment ? 'input--with-end' : '',
    className
  ].filter(Boolean).join(' ');

  return (
    <div className="input-wrapper">
      {label && (
        <label className="input__label">
          {label}
          {required && <span className="input__required">*</span>}
        </label>
      )}
      <div className="input__container">
        {startAdornment && (
          <span className="input__adornment input__adornment--start">{startAdornment}</span>
        )}
        <input
          ref={ref}
          className={inputClasses}
          type={type}
          placeholder={placeholder}
          value={value}
          onChange={onChange}
          onBlur={onBlur}
          disabled={disabled}
          readOnly={readOnly}
          name={name}
          autoFocus={autoFocus}
        />
        {endAdornment && (
          <span className="input__adornment input__adornment--end">{endAdornment}</span>
        )}
      </div>
      {error && <span className="input__error">{error}</span>}
    </div>
  );
});

Input.displayName = 'Input';

export default Input;