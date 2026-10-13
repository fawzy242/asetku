// src/components/molecules/ConfirmDialog/ConfirmDialog.jsx
import React, { useCallback, createContext, useContext } from 'react';
import { alertUtils } from '../../../lib/sweetAlert';

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

  static showDelete = (
    title = 'Delete Record',
    text = 'This action cannot be undone.'
  ) =>
    ConfirmDialog.show({
      title,
      text,
      icon: 'warning',
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      confirmButtonColor: '#ef4444',
    });
}

export default ConfirmDialog;