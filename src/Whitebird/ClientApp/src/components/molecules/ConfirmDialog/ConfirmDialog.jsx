// src/components/molecules/ConfirmDialog/ConfirmDialog.jsx
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

  static confirmFileUpload = (fileNames, count) => {
    const names = Array.isArray(fileNames) ? fileNames.join(', ') : fileNames;
    return ConfirmDialog.show({
      title: `Upload ${count || 1} File(s)`,
      text: `Are you sure you want to upload:\n${names}`,
      icon: 'question',
      confirmButtonText: 'Yes, Upload',
      cancelButtonText: 'Cancel',
      confirmButtonColor: '#dc2626',
    });
  };

  static confirmFileDelete = (fileName) => {
    return ConfirmDialog.show({
      title: 'Delete File',
      text: `Are you sure you want to delete "${fileName}"?`,
      icon: 'warning',
      confirmButtonText: 'Yes, Delete',
      cancelButtonText: 'Cancel',
      confirmButtonColor: '#ef4444',
    });
  };

  static toast = {
    success: (msg) => {
      if (msg) {
        console.log('[ConfirmDialog.toast.success]', msg);
        toastr.success(msg);
      }
    },
    error: (msg) => {
      if (msg) {
        console.log('[ConfirmDialog.toast.error]', msg);
        toastr.error(msg);
      }
    },
    info: (msg) => {
      if (msg) {
        console.log('[ConfirmDialog.toast.info]', msg);
        toastr.info(msg);
      }
    },
    warning: (msg) => {
      if (msg) {
        console.log('[ConfirmDialog.toast.warning]', msg);
        toastr.warning(msg);
      }
    },
  };

  // REMOVED: close() - empty function, not needed
}

export default ConfirmDialog;