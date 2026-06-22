import React, { memo } from 'react';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs from 'dayjs';
import { TextField } from '@mui/material';

const DatePickerInput = memo(({ 
  label, 
  value, 
  onChange, 
  required = false, 
  disabled = false,
  helperText = '',
  error = '',
  sx = {},
  ...rest 
}) => {
  const dayjsValue = value ? dayjs(value) : null;
  const hasValue = value !== null && value !== undefined && value !== '';

  const handleChange = (newValue) => {
    if (onChange) {
      const formattedValue = newValue ? dayjs(newValue).format('YYYY-MM-DD') : '';
      onChange({ target: { value: formattedValue } });
    }
  };

  const finalError = !!(error || helperText);
  const finalHelperText = error || helperText;

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <DatePicker
        label={label}
        value={dayjsValue}
        onChange={handleChange}
        disabled={disabled}
        slotProps={{
          textField: {
            required: required,
            error: finalError,
            helperText: finalHelperText,
            fullWidth: true,
            size: 'small',
            variant: 'outlined',
            // FIX: Always set shrink to true when value exists
            // This fixes the autofill issue where label doesn't float up
            InputLabelProps: {
              shrink: hasValue || true,
            },
            sx: {
              '& .MuiOutlinedInput-root': {
                borderRadius: '8px',
              },
              ...sx,
            },
          },
        }}
        {...rest}
      />
    </LocalizationProvider>
  );
});

DatePickerInput.displayName = "DatePickerInput";
export default DatePickerInput;