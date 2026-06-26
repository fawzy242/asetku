import Swal from 'sweetalert2';
import toast from 'react-hot-toast';
import 'sweetalert2/dist/sweetalert2.min.css';

// Toast configuration
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

// SweetAlert2 configuration untuk confirm
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
  allowEscapeKey: false,
});

const forceCloseSwal = () => {
  const containers = document.querySelectorAll('.swal2-container');
  const popups = document.querySelectorAll('.swal2-popup');
  
  containers.forEach(el => {
    if (el && el.parentNode) {
      el.parentNode.removeChild(el);
    }
  });
  
  popups.forEach(el => {
    if (el && el.parentNode) {
      el.parentNode.removeChild(el);
    }
  });
  
  const backdrops = document.querySelectorAll('.swal2-backdrop');
  backdrops.forEach(el => {
    if (el && el.parentNode) {
      el.parentNode.removeChild(el);
    }
  });
  
  document.body.style.overflow = '';
  document.body.style.paddingRight = '';
  
  try {
    Swal.close();
  } catch (e) {}
};

export const alertUtils = {
  // CONFIRM - ADA TOMBOL CANCEL
  confirm: (options) => {
    return swal.fire({
      title: options.title || 'Are you sure?',
      text: options.text || '',
      icon: options.icon || 'warning',
      showCancelButton: true,
      showConfirmButton: true,
      confirmButtonText: options.confirmButtonText || 'Confirm',
      cancelButtonText: options.cancelButtonText || 'Cancel',
      confirmButtonColor: options.confirmButtonColor || '#dc2626',
    }).then((result) => {
      return result.isConfirmed;
    });
  },

  // DELETE - ADA TOMBOL CANCEL
  delete: (title = 'Delete Record', text = 'This action cannot be undone.') => {
    return alertUtils.confirm({
      title,
      text,
      icon: 'warning',
      showCancelButton: true,
      showConfirmButton: true,
      confirmButtonText: 'Delete',
      confirmButtonColor: '#ef4444',
    });
  },

  // ============================================================
  // SUCCESS, ERROR, INFO, WARNING - HANYA TOMBOL OK
  // ============================================================
  
  success: (title, text) => {
    return new Promise((resolve) => {
      forceCloseSwal();
      
      setTimeout(() => {
        Swal.fire({
          title: title || 'Success',
          text: text || '',
          icon: 'success',
          showCancelButton: false,
          showConfirmButton: true,
          confirmButtonText: 'OK',
          confirmButtonColor: '#10b981',
          allowOutsideClick: false,
          allowEscapeKey: false,
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
          buttonsStyling: false,
          didOpen: (modal) => {
            // HIDE cancel button if exists
            const cancelBtn = modal.querySelector('.swal2-cancel');
            if (cancelBtn) {
              cancelBtn.style.display = 'none';
            }
            
            const confirmBtn = modal.querySelector('.swal2-confirm');
            if (confirmBtn) {
              const newBtn = confirmBtn.cloneNode(true);
              confirmBtn.parentNode.replaceChild(newBtn, confirmBtn);
              newBtn.addEventListener('click', function(e) {
                e.stopPropagation();
                forceCloseSwal();
                resolve({ isConfirmed: true });
              });
            }
          },
          willClose: () => {
            forceCloseSwal();
            resolve({ isConfirmed: true });
          },
        }).catch(() => {
          forceCloseSwal();
          resolve({ isConfirmed: false });
        });
      }, 50);
    });
  },

  error: (title, text) => {
    return new Promise((resolve) => {
      forceCloseSwal();
      
      setTimeout(() => {
        Swal.fire({
          title: title || 'Error',
          text: text || '',
          icon: 'error',
          showCancelButton: false,
          showConfirmButton: true,
          confirmButtonText: 'OK',
          confirmButtonColor: '#ef4444',
          allowOutsideClick: false,
          allowEscapeKey: false,
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
          buttonsStyling: false,
          didOpen: (modal) => {
            const cancelBtn = modal.querySelector('.swal2-cancel');
            if (cancelBtn) {
              cancelBtn.style.display = 'none';
            }
            
            const confirmBtn = modal.querySelector('.swal2-confirm');
            if (confirmBtn) {
              const newBtn = confirmBtn.cloneNode(true);
              confirmBtn.parentNode.replaceChild(newBtn, confirmBtn);
              newBtn.addEventListener('click', function(e) {
                e.stopPropagation();
                forceCloseSwal();
                resolve({ isConfirmed: true });
              });
            }
          },
          willClose: () => {
            forceCloseSwal();
            resolve({ isConfirmed: true });
          },
        }).catch(() => {
          forceCloseSwal();
          resolve({ isConfirmed: false });
        });
      }, 50);
    });
  },

  info: (title, text) => {
    return new Promise((resolve) => {
      forceCloseSwal();
      
      setTimeout(() => {
        Swal.fire({
          title: title || 'Info',
          text: text || '',
          icon: 'info',
          showCancelButton: false,
          showConfirmButton: true,
          confirmButtonText: 'OK',
          confirmButtonColor: '#3b82f6',
          allowOutsideClick: false,
          allowEscapeKey: false,
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
          buttonsStyling: false,
          didOpen: (modal) => {
            const cancelBtn = modal.querySelector('.swal2-cancel');
            if (cancelBtn) {
              cancelBtn.style.display = 'none';
            }
            
            const confirmBtn = modal.querySelector('.swal2-confirm');
            if (confirmBtn) {
              const newBtn = confirmBtn.cloneNode(true);
              confirmBtn.parentNode.replaceChild(newBtn, confirmBtn);
              newBtn.addEventListener('click', function(e) {
                e.stopPropagation();
                forceCloseSwal();
                resolve({ isConfirmed: true });
              });
            }
          },
          willClose: () => {
            forceCloseSwal();
            resolve({ isConfirmed: true });
          },
        }).catch(() => {
          forceCloseSwal();
          resolve({ isConfirmed: false });
        });
      }, 50);
    });
  },

  warning: (title, text) => {
    return new Promise((resolve) => {
      forceCloseSwal();
      
      setTimeout(() => {
        Swal.fire({
          title: title || 'Warning',
          text: text || '',
          icon: 'warning',
          showCancelButton: false,
          showConfirmButton: true,
          confirmButtonText: 'OK',
          confirmButtonColor: '#f59e0b',
          allowOutsideClick: false,
          allowEscapeKey: false,
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
          buttonsStyling: false,
          didOpen: (modal) => {
            const cancelBtn = modal.querySelector('.swal2-cancel');
            if (cancelBtn) {
              cancelBtn.style.display = 'none';
            }
            
            const confirmBtn = modal.querySelector('.swal2-confirm');
            if (confirmBtn) {
              const newBtn = confirmBtn.cloneNode(true);
              confirmBtn.parentNode.replaceChild(newBtn, confirmBtn);
              newBtn.addEventListener('click', function(e) {
                e.stopPropagation();
                forceCloseSwal();
                resolve({ isConfirmed: true });
              });
            }
          },
          willClose: () => {
            forceCloseSwal();
            resolve({ isConfirmed: true });
          },
        }).catch(() => {
          forceCloseSwal();
          resolve({ isConfirmed: false });
        });
      }, 50);
    });
  },
};

export default { swal, toastr, alertUtils };