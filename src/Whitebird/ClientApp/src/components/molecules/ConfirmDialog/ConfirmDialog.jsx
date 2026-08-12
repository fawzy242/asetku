// src/components/molecules/ConfirmDialog/ConfirmDialog.jsx
import React, { useCallback, createContext, useContext } from 'react';
import { alertUtils, toastr } from '../../../lib/sweetAlert';

// ============================================================
// CONFIRM CONTEXT - Using SweetAlert2
// ============================================================
const ConfirmContext = createContext(null);

export const useConfirm = () => {
  const context = useContext(ConfirmContext);
  if (!context) {
    return {
      show: (options) => alertUtils.confirm(options),
    };
  }
  return context;
};

export const ConfirmDialogProvider = ({ children }) => {
  const show = useCallback((options) => {
    return alertUtils.confirm(options);
  }, []);

  const contextValue = { show };

  return (
    <ConfirmContext.Provider value={contextValue}>
      {children}
    </ConfirmContext.Provider>
  );
};

// ============================================================
// STATIC API - FIXED: Gunakan toastr langsung
// ============================================================
class ConfirmDialog {
  static show(options = {}) {
    return alertUtils.confirm(options);
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
    alertUtils.success(title, text);

  static showError = (title, text) =>
    alertUtils.error(title, text);

  // FIX: Gunakan toastr langsung dari sweetAlert
  static toast = {
    success: (msg) => {
      if (msg) toastr.success(msg);
    },
    error: (msg) => {
      if (msg) toastr.error(msg);
    },
    info: (msg) => {
      if (msg) toastr.info(msg);
    },
    warning: (msg) => {
      if (msg) toastr.warning(msg);
    },
  };

  static close = () => {};
}

export default ConfirmDialog;