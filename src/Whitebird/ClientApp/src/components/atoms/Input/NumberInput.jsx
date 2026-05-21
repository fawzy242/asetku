import React, { forwardRef, memo } from "react";
import { NumericFormat } from "react-number-format";
import { TextField } from "@mui/material";
import "./Input.scss";

const NumberInput = memo(forwardRef(({
  label,
  value,
  onChange,
  prefix = '',
  suffix = '',
  thousandSeparator = true,
  decimalScale = 0,
  allowNegative = false,
  min,
  max,
  placeholder = '',
  required = false,
  disabled = false,
  error = '',
  helperText = '',
  name = '',
  autoFocus = false,
  fullWidth = true,
  size = "small",
  variant = "outlined",
  register = null,
  registerOptions = {},
  ...rest
}, ref) => {
  const finalError = !!(error || helperText);
  const finalHelperText = error || helperText;

  const registerProps = register ? register(name, registerOptions) : {};

  const handleValueChange = (values) => {
    if (onChange) {
      onChange({ target: { value: values.floatValue ?? '', name: name } });
    }
  };

  return (
    <div className="input-wrapper">
      <NumericFormat
        customInput={TextField}
        label={label}
        value={value !== null && value !== undefined ? value : ''}
        onValueChange={handleValueChange}
        prefix={prefix}
        suffix={suffix}
        thousandSeparator={thousandSeparator}
        decimalScale={decimalScale}
        allowNegative={allowNegative}
        isAllowed={(values) => {
          const { floatValue } = values;
          if (floatValue === undefined) return true;
          if (min !== undefined && floatValue < min) return false;
          if (max !== undefined && floatValue > max) return false;
          return true;
        }}
        placeholder={placeholder}
        required={required}
        disabled={disabled}
        error={finalError}
        helperText={finalHelperText}
        name={name}
        autoFocus={autoFocus}
        fullWidth={fullWidth}
        size={size}
        variant={variant}
        inputRef={ref}
        InputLabelProps={{ shrink: true }}
        InputProps={{
          sx: {
            '& .MuiOutlinedInput-root': {
              borderRadius: '8px',
            },
          },
        }}
        {...rest}
      />
    </div>
  );
}));

NumberInput.displayName = "NumberInput";
export default NumberInput;