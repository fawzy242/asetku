import React, { useEffect, useRef, useMemo } from "react";
import { FiX } from "react-icons/fi";
import { createPortal } from "react-dom";
import { ThemeProvider } from "@mui/material/styles";
import { useUIStore } from "../../../stores/uiStore";
import { createAppTheme } from "../../../core/theme/muiThemeFactory";
import "./Modal.scss";

const Modal = ({
  isOpen = false,
  onClose,
  title,
  children,
  size = "md",
  showCloseButton = true,
  closeOnOverlayClick = false,
  closeOnEsc = true,
  actions = null,
  className = "",
}) => {
  const overlayRef = useRef(null);
  const theme = useUIStore((s) => s.theme);
  const isDark = theme === "dark";

  const muiTheme = useMemo(() => createAppTheme(isDark), [isDark]);
  const modalId = useMemo(() => `modal-${Date.now()}-${Math.random().toString(36).substr(2, 5)}`, []);

  useEffect(() => {
    const handleEsc = (e) => {
      if (e.key === "Escape" && closeOnEsc && isOpen) onClose();
    };
    if (isOpen) {
      document.addEventListener("keydown", handleEsc);
      document.body.style.overflow = "hidden";
    }
    return () => {
      document.removeEventListener("keydown", handleEsc);
      document.body.style.overflow = "";
    };
  }, [isOpen, closeOnEsc, onClose]);

  if (!isOpen) return null;

  const handleOverlayClick = (e) => {
    if (closeOnOverlayClick && e.target === overlayRef.current) onClose();
  };

  return createPortal(
    <div
      className="modal-overlay"
      ref={overlayRef}
      onClick={handleOverlayClick}
      role="dialog"
      aria-modal="true"
      aria-labelledby={`${modalId}-title`}
    >
      <div
        className={`modal modal--${size} ${className}`}
        onClick={(e) => e.stopPropagation()}
        style={{
          backgroundColor: isDark ? "#1f2937" : "#ffffff",
          color: isDark ? "#f9fafb" : "#111827",
          borderColor: isDark ? "#374151" : "#e5e7eb",
        }}
      >
        <ThemeProvider theme={muiTheme}>
          {(title || showCloseButton) && (
            <div
              className="modal__header"
              style={{
                borderBottomColor: isDark ? "#374151" : "#e5e7eb",
              }}
            >
              {title && (
                <h3
                  className="modal__title"
                  id={`${modalId}-title`}
                  style={{
                    color: isDark ? "#f9fafb" : "#111827",
                  }}
                >
                  {title}
                </h3>
              )}
              {showCloseButton && (
                <button
                  className="modal__close"
                  onClick={onClose}
                  type="button"
                  aria-label="Close modal"
                >
                  <FiX size={20} aria-hidden="true" />
                </button>
              )}
            </div>
          )}
          <div className="modal__body">{children}</div>
          {actions && (
            <div
              className="modal__footer"
              style={{
                borderTopColor: isDark ? "#374151" : "#e5e7eb",
              }}
            >
              {actions}
            </div>
          )}
        </ThemeProvider>
      </div>
    </div>,
    document.body
  );
};

export default Modal;