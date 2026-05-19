import React, { forwardRef } from "react";
import { NumericFormat } from "react-number-format";
import { TextField } from "@mui/material";
import "./Input.scss";

/**
 * MUI Number Input wrapper menggunakan react-number-format.
 * Menggantikan <input type="number"> dengan number formatting yang proper.
 * 
 * Props:
 * - label: string
 * - value: string | number
 * - onChange: function (e) => setFormData({ ...formData, field: e.target.value })
 * - prefix: string (default: '') — contoh: 'Rp ' untuk currency
 * - suffix: string (default: '') — contoh: ' months', ' years'
 * - thousandSeparator: boolean (default: true)
 * - decimalScale: number (default: 0) — jumlah digit desimal
 * - allowNegative: boolean (default: false)
 * - min: number (default: undefined)
 * - max: number (default: undefined)
 * - placeholder: string
 * - required: boolean
 * - disabled: boolean
 * - error: string | boolean
 * - helperText: string
 * - name: string
 * - autoFocus: boolean
 * - fullWidth: boolean (default: true)
 * - size: string (default: 'small')
 * - variant: string (default: 'outlined')
 * - register: function (optional) — react-hook-form register
 * - registerOptions: object (optional)
 */
const NumberInput = forwardRef(({
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

  // Handle register jika ada
  const registerProps = register ? register(name, registerOptions) : {};

  const handleValueChange = (values) => {
    if (onChange) {
      // Kirim value asli (float) ke parent
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
});

NumberInput.displayName = "NumberInput";
export default NumberInput;