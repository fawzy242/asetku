import { useCallback } from 'react';
import { alertUtils, toastr } from '../lib/sweetAlert';

/**
 * Hook for SweetAlert2 and toast utilities
 */
export const useSweetAlert = () => {
  // ============================================================
  // MODAL ALERT (Blocking) - Untuk Success/Error/Warning penting
  // ============================================================
  const modal = {
    success: useCallback((title, text) => {
      return alertUtils.success(title, text);
    }, []),
    error: useCallback((title, text) => {
      return alertUtils.error(title, text);
    }, []),
    info: useCallback((title, text) => {
      return alertUtils.info(title, text);
    }, []),
    warning: useCallback((title, text) => {
      return alertUtils.warning(title, text);
    }, []),
  };

  // ============================================================
  // CONFIRM (Blocking)
  // ============================================================
  const confirm = useCallback((options) => {
    return alertUtils.confirm(options);
  }, []);

  const confirmDelete = useCallback((title = 'Delete Record', text = 'This action cannot be undone.') => {
    return alertUtils.delete(title, text);
  }, []);

  // ============================================================
  // TOAST (Non-blocking) - Hanya untuk notifikasi ringan
  // ============================================================
  const toast = {
    success: useCallback((message) => toastr.success(message), []),
    error: useCallback((message) => toastr.error(message), []),
    info: useCallback((message) => toastr.info(message), []),
    warning: useCallback((message) => toastr.warning(message), []),
  };

  // ============================================================
  // DEPRECATED - Gunakan modal.* atau toast.*
  // ============================================================
  const showSuccess = useCallback((title, text) => alertUtils.success(title, text), []);
  const showError = useCallback((title, text) => alertUtils.error(title, text), []);
  const showInfo = useCallback((title, text) => alertUtils.info(title, text), []);
  const showWarning = useCallback((title, text) => alertUtils.warning(title, text), []);

  return {
    modal,
    confirm,
    confirmDelete,
    toast,
    showSuccess,
    showError,
    showInfo,
    showWarning,
  };
};

export default useSweetAlert;