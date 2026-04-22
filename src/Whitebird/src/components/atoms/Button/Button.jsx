import React from "react";
import { FiLoader } from "react-icons/fi";
import "./Button.scss";

const Button = ({
  children,
  variant = "primary",
  size = "md",
  loading = false,
  disabled = false,
  fullWidth = false,
  startIcon = null,
  endIcon = null,
  onClick,
  type = "button",
  className = "",
}) => {
  const classNames = [
    "btn",
    `btn--${variant}`,
    `btn--${size}`,
    fullWidth ? "btn--full-width" : "",
    loading ? "btn--loading" : "",
    className,
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <button
      className={classNames}
      onClick={onClick}
      disabled={disabled || loading}
      type={type}
    >
      {loading && (
        <span className="btn__spinner">
          <FiLoader className="btn__spinner-icon" />
        </span>
      )}
      {!loading && startIcon && <span className="btn__icon btn__icon--start">{startIcon}</span>}
      <span className="btn__text">{children}</span>
      {!loading && endIcon && <span className="btn__icon btn__icon--end">{endIcon}</span>}
    </button>
  );
};

export default Button;