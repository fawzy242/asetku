import Swal from 'sweetalert2';
import toast from 'react-hot-toast';
import 'sweetalert2/dist/sweetalert2.min.css';

// SweetAlert2 default configuration with full theming
export const swal = Swal.mixin({
  customClass: {
    confirmButton: 'swal-confirm-btn',
    cancelButton: 'swal-cancel-btn',
    popup: 'swal-popup',
    container: 'swal-container',
    title: 'swal-title',
    htmlContainer: 'swal-html',
    icon: 'swal-icon',
    actions: 'swal-actions',
    footer: 'swal-footer',
  },
  buttonsStyling: false,
  reverseButtons: true,
  // Blur effect sama seperti Modal component
  backdrop: `rgba(0, 0, 0, 0.6)`,
  allowOutsideClick: false,
  allowEscapeKey: true,
});

// Toast configuration (non-blocking notifications)
export const toastr = {
  success: (message, options = {}) => {
    toast.success(message, {
      position: 'bottom-right',
      duration: 3000,
      ...options,
    });
  },
  error: (message, options = {}) => {
    toast.error(message, {
      position: 'bottom-right',
      duration: 4000,
      ...options,
    });
  },
  info: (message, options = {}) => {
    toast(message, {
      icon: 'ℹ️',
      position: 'bottom-right',
      duration: 3000,
      ...options,
    });
  },
  warning: (message, options = {}) => {
    toast(message, {
      icon: '⚠️',
      position: 'bottom-right',
      duration: 3500,
      ...options,
    });
  },
};

// Alert utilities (blocking modals)
export const alertUtils = {
  confirm: async (options) => {
    const result = await swal.fire({
      title: options.title || 'Are you sure?',
      text: options.text || '',
      icon: options.icon || 'warning',
      showCancelButton: options.showCancelButton !== false,
      confirmButtonText: options.confirmButtonText || 'Confirm',
      cancelButtonText: options.cancelButtonText || 'Cancel',
      confirmButtonColor: options.confirmButtonColor || '#dc2626',
    });
    return result.isConfirmed;
  },

  delete: async (title = 'Delete Record', text = 'This action cannot be undone.') => {
    return alertUtils.confirm({
      title,
      text,
      icon: 'warning',
      confirmButtonText: 'Delete',
      confirmButtonColor: '#ef4444',
    });
  },

  success: async (title, text) => {
    await swal.fire({
      title,
      text,
      icon: 'success',
      confirmButtonText: 'OK',
      confirmButtonColor: '#10b981',
      backdrop: `rgba(0, 0, 0, 0.6)`,
    });
  },

  error: async (title, text) => {
    await swal.fire({
      title,
      text,
      icon: 'error',
      confirmButtonText: 'OK',
      backdrop: `rgba(0, 0, 0, 0.6)`,
    });
  },

  info: async (title, text) => {
    await swal.fire({
      title,
      text,
      icon: 'info',
      confirmButtonText: 'OK',
      backdrop: `rgba(0, 0, 0, 0.6)`,
    });
  },
};

export default { swal, toastr, alertUtils };