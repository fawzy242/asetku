import React, { forwardRef } from "react";
import "./Select.scss";

const Select = forwardRef(
  (
    {
      label,
      options = [],
      value = "",
      onChange,
      onBlur,
      error = "",
      required = false,
      disabled = false,
      placeholder = "Select an option",
      className = "",
      name = "",
    },
    ref
  ) => {
    const selectClasses = ["select", error ? "select--error" : "", className]
      .filter(Boolean)
      .join(" ");

    return (
      <div className="select-wrapper">
        {label && (
          <label className="select__label">
            {label}
            {required && <span className="select__required">*</span>}
          </label>
        )}
        <div className="select__container">
          <select
            ref={ref}
            className={selectClasses}
            value={value}
            onChange={onChange}
            onBlur={onBlur}
            disabled={disabled}
            name={name}
          >
            <option value="" disabled>
              {placeholder}
            </option>
            {options.map((option, index) => (
              <option key={`${option.value}-${index}`} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
          <span className="select__arrow">▼</span>
        </div>
        {error && <span className="select__error">{error}</span>}
      </div>
    );
  }
);

Select.displayName = "Select";

export default Select;