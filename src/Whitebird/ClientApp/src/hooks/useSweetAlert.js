import { useCallback } from 'react';
import { alertUtils, toastr } from '../lib/sweetAlert';

/**
 * Hook for SweetAlert2 and toast utilities
 */
export const useSweetAlert = () => {
  const confirm = useCallback(async (options) => {
    return alertUtils.confirm(options);
  }, []);

  const confirmDelete = useCallback(async (title = 'Delete Record', text = 'This action cannot be undone.') => {
    return alertUtils.delete(title, text);
  }, []);

  const showSuccess = useCallback((title, text) => {
    alertUtils.success(title, text);
  }, []);

  const showError = useCallback((title, text) => {
    alertUtils.error(title, text);
  }, []);

  const showInfo = useCallback((title, text) => {
    alertUtils.info(title, text);
  }, []);

  const toast = {
    success: useCallback((message) => toastr.success(message), []),
    error: useCallback((message) => toastr.error(message), []),
    info: useCallback((message) => toastr.info(message), []),
    warning: useCallback((message) => toastr.warning(message), []),
  };

  return {
    confirm,
    confirmDelete,
    showSuccess,
    showError,
    showInfo,
    toast,
  };
};

export default useSweetAlert;