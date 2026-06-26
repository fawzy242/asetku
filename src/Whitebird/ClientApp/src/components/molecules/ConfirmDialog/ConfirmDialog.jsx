import React, { useCallback, createContext, useContext } from 'react';
import { alertUtils, toastr } from '../../../lib/sweetAlert';

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

  static showInfo = (title, text) =>
    alertUtils.info(title, text);

  static showWarning = (title, text) =>
    alertUtils.warning(title, text);

  static toast = {
    success: (msg) => toastr.success(msg),
    error: (msg) => toastr.error(msg),
    info: (msg) => toastr.info(msg),
    warn: (msg) => toastr.warning(msg),
  };

  static close = () => {
    Swal.close();
  };
}

export default ConfirmDialog;