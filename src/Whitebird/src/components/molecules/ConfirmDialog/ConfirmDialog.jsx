import React, { useState, useCallback, createContext, useContext } from 'react';
import { FiAlertTriangle, FiCheckCircle, FiXCircle, FiInfo, FiHelpCircle } from 'react-icons/fi';
import Modal from '../Modal/Modal';
import Button from '../../atoms/Button/Button';
import toast from 'react-hot-toast';

// ============================================================
// CONFIRM CONTEXT
// ============================================================
const ConfirmContext = createContext(null);

export const useConfirm = () => {
  const context = useContext(ConfirmContext);
  if (!context) {
    // Fallback: return dummy functions untuk mencegah crash
    return {
      show: () => Promise.resolve(false),
    };
  }
  return context;
};

const ICON_MAP = {
  warning: { icon: FiAlertTriangle, color: '#f59e0b', bg: 'rgba(245, 158, 11, 0.1)' },
  success: { icon: FiCheckCircle, color: '#10b981', bg: 'rgba(16, 185, 129, 0.1)' },
  error: { icon: FiXCircle, color: '#ef4444', bg: 'rgba(239, 68, 68, 0.1)' },
  info: { icon: FiInfo, color: '#3b82f6', bg: 'rgba(59, 130, 246, 0.1)' },
  question: { icon: FiHelpCircle, color: '#dc2626', bg: 'rgba(220, 38, 38, 0.1)' },
};

export const ConfirmDialogProvider = ({ children }) => {
  const [state, setState] = useState({
    isOpen: false,
    title: '',
    text: '',
    icon: 'warning',
    confirmText: 'Confirm',
    cancelText: 'Cancel',
    confirmColor: '#dc2626',
    showCancel: true,
    resolve: null,
  });

  const show = useCallback((options) => {
    return new Promise((resolve) => {
      setState({
        isOpen: true,
        title: options.title || 'Are you sure?',
        text: options.text || '',
        icon: options.icon || 'warning',
        confirmText: options.confirmButtonText || 'Confirm',
        cancelText: options.cancelButtonText || 'Cancel',
        confirmColor: options.confirmButtonColor || '#dc2626',
        showCancel: options.showCancelButton !== false,
        resolve,
      });
    });
  }, []);

  const handleConfirm = useCallback(() => {
    if (state.resolve) {
      state.resolve(true);
    }
    setState({
      isOpen: false,
      title: '',
      text: '',
      icon: 'warning',
      confirmText: 'Confirm',
      cancelText: 'Cancel',
      confirmColor: '#dc2626',
      showCancel: true,
      resolve: null,
    });
  }, [state.resolve]);

  const handleCancel = useCallback(() => {
    if (state.resolve) {
      state.resolve(false);
    }
    setState({
      isOpen: false,
      title: '',
      text: '',
      icon: 'warning',
      confirmText: 'Confirm',
      cancelText: 'Cancel',
      confirmColor: '#dc2626',
      showCancel: true,
      resolve: null,
    });
  }, [state.resolve]);

  const iconData = ICON_MAP[state.icon] || ICON_MAP.warning;
  const IconComponent = iconData.icon;

  const contextValue = { show };

  return (
    <ConfirmContext.Provider value={contextValue}>
      {children}
      <Modal
        isOpen={state.isOpen}
        onClose={handleCancel}
        size="sm"
        showCloseButton={false}
        closeOnEsc={true}
      >
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          textAlign: 'center',
          gap: '16px',
          padding: '8px 0',
        }}>
          <div style={{
            width: '64px',
            height: '64px',
            borderRadius: '50%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            backgroundColor: iconData.bg,
            flexShrink: 0,
          }}>
            <IconComponent size={32} color={iconData.color} />
          </div>

          <h3 style={{
            fontSize: '18px',
            fontWeight: 600,
            color: 'var(--text-primary)',
            margin: 0,
          }}>
            {state.title}
          </h3>

          {state.text && (
            <p style={{
              fontSize: '14px',
              color: 'var(--text-secondary)',
              lineHeight: 1.6,
              margin: 0,
            }}>
              {state.text}
            </p>
          )}

          <div style={{
            display: 'flex',
            gap: '12px',
            marginTop: '8px',
            width: '100%',
          }}>
            {state.showCancel && (
              <Button variant="outline" onClick={handleCancel} fullWidth>
                {state.cancelText}
              </Button>
            )}
            <Button
              variant={state.confirmColor === '#ef4444' ? 'danger' : 'primary'}
              onClick={handleConfirm}
              fullWidth
            >
              {state.confirmText}
            </Button>
          </div>
        </div>
      </Modal>
    </ConfirmContext.Provider>
  );
};

// ============================================================
// STATIC API (kompatibel dengan kode existing)
// ============================================================
class ConfirmDialog {
  static show(options = {}) {
    // Fallback ke toast kalau ConfirmDialogProvider belum di-mount
    console.warn('ConfirmDialog.show() called before ConfirmDialogProvider mounted. Using fallback.');
    return Promise.resolve(window.confirm(options.title || 'Are you sure?'));
  }

  static showDelete = (title = 'Delete Record', text = 'This action cannot be undone.') =>
    ConfirmDialog.show({
      title,
      text,
      icon: 'warning',
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      confirmButtonColor: '#ef4444',
    });

  static showSuccess = (title, text) =>
    ConfirmDialog.show({
      title,
      text,
      icon: 'success',
      confirmButtonText: 'OK',
      showCancelButton: false,
      confirmButtonColor: '#10b981',
    });

  static showError = (title, text) =>
    ConfirmDialog.show({
      title,
      text,
      icon: 'error',
      confirmButtonText: 'OK',
      showCancelButton: false,
    });

  static toast = {
    success: (msg) => toast.success(msg, { position: 'bottom-right', duration: 3000 }),
    error: (msg) => toast.error(msg, { position: 'bottom-right', duration: 4000 }),
    info: (msg) => toast(msg, { icon: 'ℹ️', position: 'bottom-right', duration: 3000 }),
    warn: (msg) => toast(msg, { icon: '⚠️', position: 'bottom-right', duration: 3500 }),
  };

  static close = () => {};
}

export default ConfirmDialog;