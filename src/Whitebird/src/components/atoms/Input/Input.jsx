import React, { forwardRef, useState } from "react";
import { FiEye, FiEyeOff } from "react-icons/fi";
import "./Input.scss";

const Input = forwardRef(
  (
    {
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
    },
    ref
  ) => {
    const [showPassword, setShowPassword] = useState(false);

    const inputType = type === "password" && showPassword ? "text" : type;

    const inputClasses = [
      "input",
      error ? "input--error" : "",
      startAdornment ? "input--with-start" : "",
      endAdornment || type === "password" ? "input--with-end" : "",
      className,
    ]
      .filter(Boolean)
      .join(" ");

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
            type={inputType}
            placeholder={placeholder}
            value={value}
            onChange={onChange}
            onBlur={onBlur}
            disabled={disabled}
            readOnly={readOnly}
            name={name}
            autoFocus={autoFocus}
          />
          {type === "password" && (
            <button
              type="button"
              className="input__password-toggle"
              onClick={() => setShowPassword(!showPassword)}
              tabIndex={-1}
            >
              {showPassword ? <FiEyeOff size={18} /> : <FiEye size={18} />}
            </button>
          )}
          {endAdornment && type !== "password" && (
            <span className="input__adornment input__adornment--end">{endAdornment}</span>
          )}
        </div>
        {error && <span className="input__error">{error}</span>}
      </div>
    );
  }
);

Input.displayName = "Input";

export default Input;