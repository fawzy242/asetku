import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';

dayjs.extend(relativeTime);

class UtilsHelper {
  formatDate(date, format = 'YYYY-MM-DD') {
    if (!date) return '-';
    return dayjs(date).format(format);
  }

  formatDateTime(dateTime, format = 'YYYY-MM-DD HH:mm') {
    if (!dateTime) return '-';
    return dayjs(dateTime).format(format);
  }

  formatCurrency(amount, currency = 'IDR') {
    if (amount === null || amount === undefined) return '-';
    
    return new Intl.NumberFormat('id-ID', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    }).format(amount);
  }

  formatNumber(number, decimals = 0) {
    if (number === null || number === undefined) return '-';
    
    return new Intl.NumberFormat('id-ID', {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals
    }).format(number);
  }

  formatRelativeTime(date) {
    if (!date) return '-';
    return dayjs(date).fromNow();
  }

  debounce(func, wait = 300) {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        clearTimeout(timeout);
        func(...args);
      };
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
    };
  }

  throttle(func, limit = 300) {
    let inThrottle;
    return function(...args) {
      if (!inThrottle) {
        func.apply(this, args);
        inThrottle = true;
        setTimeout(() => inThrottle = false, limit);
      }
    };
  }

  generateId(prefix = '') {
    return `${prefix}${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  getStatusColor(status) {
    const colors = {
      // Asset Status
      'Available': 'success',
      'Assigned': 'primary',
      'Under Repair': 'warning',
      'Maintenance': 'warning',
      'Retired': 'secondary',
      'Disposed': 'secondary',
      
      // Transaction Status
      'Pending': 'warning',
      'Approved': 'success',
      'Rejected': 'error',
      'Completed': 'success',
      'Cancelled': 'secondary',
      
      // Employee Status
      'Active': 'success',
      'Resigned': 'secondary',
      'On Leave': 'warning'
    };
    
    return colors[status] || 'secondary';
  }

  validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
  }

  validatePhone(phone) {
    const re = /^[0-9+\-\s()]{10,20}$/;
    return re.test(phone);
  }

  validateRequired(value) {
    return value !== null && value !== undefined && value.toString().trim() !== '';
  }

  getErrorMessage(error) {
    if (error.response?.data?.errors?.length > 0) {
      return error.response.data.errors.join('\n');
    }
    if (error.response?.data?.message) {
      return error.response.data.message;
    }
    return error.message || 'An unexpected error occurred';
  }

  setLocalStorage(key, value) {
    try {
      localStorage.setItem(key, JSON.stringify(value));
      return true;
    } catch (e) {
      console.error('Failed to save to localStorage:', e);
      return false;
    }
  }

  getLocalStorage(key, defaultValue = null) {
    try {
      const value = localStorage.getItem(key);
      return value ? JSON.parse(value) : defaultValue;
    } catch (e) {
      console.error('Failed to read from localStorage:', e);
      return defaultValue;
    }
  }

  removeLocalStorage(key) {
    try {
      localStorage.removeItem(key);
      return true;
    } catch (e) {
      console.error('Failed to remove from localStorage:', e);
      return false;
    }
  }

  buildQueryString(params) {
    const query = Object.entries(params)
      .filter(([_, value]) => value !== null && value !== undefined && value !== '')
      .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`)
      .join('&');
    
    return query ? `?${query}` : '';
  }

  parseQueryString(queryString) {
    const params = new URLSearchParams(queryString);
    const result = {};
    
    for (const [key, value] of params) {
      result[key] = value;
    }
    
    return result;
  }

  truncateText(text, maxLength = 50) {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
  }

  downloadFile(blob, fileName) {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  }
}

export default new UtilsHelper();