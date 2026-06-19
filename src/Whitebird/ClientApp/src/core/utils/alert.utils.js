import { toastr, alertUtils } from '../../lib/sweetAlert';

/**
 * Standardized alert utilities for the entire application
 * Use this instead of ConfirmDialog or direct toast calls
 */
export const showToast = {
  success: (message) => toastr.success(message),
  error: (message) => toastr.error(message),
  info: (message) => toastr.info(message),
  warning: (message) => toastr.warning(message),
};

export const showDialog = {
  confirm: (options) => alertUtils.confirm(options),
  delete: (title, text) => alertUtils.delete(title, text),
  success: (title, text) => alertUtils.success(title, text),
  error: (title, text) => alertUtils.error(title, text),
  info: (title, text) => alertUtils.info(title, text),
};

// For backward compatibility dengan kode existing
export const Confirm = {
  show: (options) => alertUtils.confirm(options),
  delete: () => alertUtils.delete(),
  toast: showToast,
};

export default { showToast, showDialog, Confirm };