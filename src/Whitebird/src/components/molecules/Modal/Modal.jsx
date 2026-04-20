import React, { useEffect, useRef } from 'react';
import './Modal.scss';

const Modal = ({
  isOpen = false,
  onClose,
  title,
  children,
  size = 'md',
  showCloseButton = true,
  closeOnOverlayClick = true,
  closeOnEsc = true,
  actions = null,
  className = ''
}) => {
  const modalRef = useRef(null);

  useEffect(() => {
    const handleEsc = (e) => {
      if (e.key === 'Escape' && closeOnEsc && isOpen) {
        onClose();
      }
    };

    document.addEventListener('keydown', handleEsc);
    
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    }

    return () => {
      document.removeEventListener('keydown', handleEsc);
      document.body.style.overflow = '';
    };
  }, [isOpen, closeOnEsc, onClose]);

  if (!isOpen) return null;

  const handleOverlayClick = (e) => {
    if (closeOnOverlayClick && e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <div className="modal-overlay" onClick={handleOverlayClick}>
      <div 
        ref={modalRef}
        className={`modal modal--${size} ${className}`}
        role="dialog"
        aria-modal="true"
      >
        {(title || showCloseButton) && (
          <div className="modal__header">
            {title && <h3 className="modal__title">{title}</h3>}
            {showCloseButton && (
              <button className="modal__close" onClick={onClose} aria-label="Close">
                ×
              </button>
            )}
          </div>
        )}
        
        <div className="modal__body">
          {children}
        </div>
        
        {actions && (
          <div className="modal__footer">
            {actions}
          </div>
        )}
      </div>
    </div>
  );
};

export default Modal;