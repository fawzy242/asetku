// src/lib/sweetAlert.js
import Swal from 'sweetalert2';
import 'sweetalert2/dist/sweetalert2.min.css';

// SweetAlert2 default configuration untuk confirm (dengan reverseButtons)
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

// Toast tetap tersedia (untuk keperluan lain jika ada)
export const toastr = {
  success: (message, options = {}) => {
    if (!message) return;
    alertUtils.success('Success!', message);
  },
  error: (message, options = {}) => {
    if (!message) return;
    alertUtils.error('Error', message);
  },
  info: (message, options = {}) => {
    if (!message) return;
    alertUtils.info('Info', message);
  },
  warning: (message, options = {}) => {
    if (!message) return;
    alertUtils.info('Warning', message);
  },
};

// Alert utilities
export const alertUtils = {
  // Confirm - tetap menggunakan swal.mixin dengan reverseButtons
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

  // FIX: Success - Pakai Swal.fire langsung (BUKAN swal.mixin)
  // TAPI tetap pakai customClass yang sama agar styling konsisten
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

  // FIX: Error - Pakai Swal.fire langsung (BUKAN swal.mixin)
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