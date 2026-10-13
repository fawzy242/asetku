import { useCallback } from 'react';
import toast from 'react-hot-toast';
import { alertUtils } from '../lib/sweetAlert';

/**
 * Hook for SweetAlert2 confirmations and react-hot-toast notifications.
 */
export const useSweetAlert = () => {
  const confirm = useCallback(async (options) => {
    return alertUtils.confirm(options);
  }, []);

  const confirmDelete = useCallback(
    async (title = 'Delete Record', text = 'This action cannot be undone.') => {
      return alertUtils.delete(title, text);
    },
    []
  );

  const showSuccess = useCallback((title, text) => {
    alertUtils.success(title, text);
  }, []);

  const showError = useCallback((title, text) => {
    alertUtils.error(title, text);
  }, []);

  const showInfo = useCallback((title, text) => {
    alertUtils.info(title, text);
  }, []);

  const toastApi = {
    success: useCallback((message) => {
      if (message) toast.success(message);
    }, []),
    error: useCallback((message) => {
      if (message) toast.error(message);
    }, []),
    info: useCallback((message) => {
      if (message) toast(message, { icon: 'ℹ️' });
    }, []),
    warning: useCallback((message) => {
      if (message) toast(message, { icon: '⚠️' });
    }, []),
  };

  return {
    confirm,
    confirmDelete,
    showSuccess,
    showError,
    showInfo,
    toast: toastApi,
  };
};

export default useSweetAlert;