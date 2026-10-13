// src/lib/sweetAlert.js
import Swal from 'sweetalert2';
import 'sweetalert2/dist/sweetalert2.min.css';
import toast from 'react-hot-toast';

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
  backdrop: `rgba(0, 0, 0, 0.6)`,
  allowOutsideClick: false,
  allowEscapeKey: true,
});

// Thin toast wrappers over react-hot-toast
export const toastr = {
  success: (message) => {
    if (!message) return;
    toast.success(message);
  },
  error: (message) => {
    if (!message) return;
    toast.error(message);
  },
  info: (message) => {
    if (!message) return;
    toast(message, { icon: 'ℹ️' });
  },
  warning: (message) => {
    if (!message) return;
    toast(message, { icon: '⚠️' });
  },
};

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
    await Swal.fire({
      title: title || 'Success!',
      text: text || 'Operation completed successfully.',
      icon: 'success',
      confirmButtonText: 'OK',
      confirmButtonColor: '#10b981',
      showCancelButton: false,
      customClass: {
        confirmButton: 'swal-confirm-btn',
        popup: 'swal-popup',
        container: 'swal-container',
        title: 'swal-title',
        htmlContainer: 'swal-html',
        icon: 'swal-icon',
        actions: 'swal-actions',
        footer: 'swal-footer',
      },
      backdrop: `rgba(0, 0, 0, 0.6)`,
      allowOutsideClick: false,
    });
  },

  error: async (title, text) => {
    await Swal.fire({
      title: title || 'Error!',
      text: text || 'An error occurred.',
      icon: 'error',
      confirmButtonText: 'OK',
      showCancelButton: false,
      customClass: {
        confirmButton: 'swal-confirm-btn',
        popup: 'swal-popup',
        container: 'swal-container',
        title: 'swal-title',
        htmlContainer: 'swal-html',
        icon: 'swal-icon',
        actions: 'swal-actions',
        footer: 'swal-footer',
      },
      backdrop: `rgba(0, 0, 0, 0.6)`,
      allowOutsideClick: false,
    });
  },

  info: async (title, text) => {
    await Swal.fire({
      title: title || 'Info',
      text: text || '',
      icon: 'info',
      confirmButtonText: 'OK',
      showCancelButton: false,
      customClass: {
        confirmButton: 'swal-confirm-btn',
        popup: 'swal-popup',
        container: 'swal-container',
        title: 'swal-title',
        htmlContainer: 'swal-html',
        icon: 'swal-icon',
        actions: 'swal-actions',
        footer: 'swal-footer',
      },
      backdrop: `rgba(0, 0, 0, 0.6)`,
      allowOutsideClick: false,
    });
  },
};

export default { swal, toastr, alertUtils };