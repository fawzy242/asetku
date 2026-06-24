import React, { forwardRef, memo, useMemo } from "react";
import { Autocomplete, TextField } from "@mui/material";
import { useUIStore } from "../../../stores/uiStore";
import "./Select.scss";

const Select = memo(forwardRef(({
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
  placeholder = "",
  freeSolo = false,
  register = null,
  registerOptions = {},
}, ref) => {
  const theme = useUIStore((s) => s.theme);
  const isDark = theme === "dark";

  const safeOptions = useMemo(() => {
    if (!Array.isArray(options)) return [];
    return options.filter(opt =>
      opt !== null && opt !== undefined && typeof opt === "object" &&
      opt.value !== undefined && opt.value !== null
    );
  }, [options]);

  const safeValue = useMemo(() => {
    if (value === null || value === undefined || value === "") return null;
    const found = safeOptions.find(opt => opt.value === value);
    return found || null;
  }, [value, safeOptions]);

  const registerProps = register ? register(name, registerOptions) : {};
  const finalError = error || helperText;
  const errorId = finalError ? `${name}-error` : undefined;
  const labelId = `${name}-label`;

  const handleChange = (event, newValue) => {
    const newVal = newValue ? newValue.value : "";
    registerProps.onChange?.({ target: { value: newVal, name } });
    onChange?.({ target: { value: newVal, name } });
  };

  const handleBlur = () => {
    registerProps.onBlur?.();
    onBlur?.();
  };

  const finalRef = (el) => {
    registerProps.ref?.(el);
    if (typeof ref === "function") ref(el);
    else if (ref) ref.current = el;
  };

  return (
    <div className={`select-wrapper ${className}`}>
      <Autocomplete
        options={safeOptions}
        getOptionLabel={(option) => option?.label || ""}
        isOptionEqualToValue={(option, val) => option?.value === val?.value}
        value={safeValue}
        onChange={handleChange}
        onBlur={handleBlur}
        disabled={disabled}
        freeSolo={freeSolo}
        disableClearable={!freeSolo && required}
        loading={safeOptions.length === 0}
        loadingText="Loading..."
        noOptionsText="No options"
        sx={{
          "& .MuiOutlinedInput-root": {
            borderRadius: "8px",
            backgroundColor: isDark ? "#374151" : "#ffffff",
            "& fieldset": {
              borderColor: isDark ? "#4b5563" : "#d1d5db",
            },
            "&:hover fieldset": {
              borderColor: "var(--primary)",
            },
            "&.Mui-focused fieldset": {
              borderColor: "var(--primary)",
            },
            "& .MuiAutocomplete-input": {
              color: isDark ? "#f9fafb" : "#111827",
            },
          },
          "& .MuiInputLabel-root": {
            color: isDark ? "#9ca3af" : "#6b7280",
            "&.Mui-focused": {
              color: "var(--primary)",
            },
            "&.Mui-error": {
              color: "var(--error)",
            },
          },
          "& .MuiFormHelperText-root": {
            color: "var(--error)",
            margin: "4px 0 0 0",
            fontSize: "12px",
          },
        }}
        renderInput={(params) => (
          <TextField
            {...params}
            inputRef={finalRef}
            label={label}
            name={name}
            required={required}
            error={!!finalError}
            helperText={finalError}
            placeholder={placeholder}
            size={size}
            fullWidth={fullWidth}
            variant={variant}
            InputLabelProps={{
              shrink: true,
              id: labelId,
            }}
            aria-describedby={errorId}
          />
        )}
      />
    </div>
  );
}));

Select.displayName = "Select";
export default Select;