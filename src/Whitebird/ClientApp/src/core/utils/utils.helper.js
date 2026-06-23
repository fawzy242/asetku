import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import { API_CONFIG, UPLOAD_CONFIG } from '../constants/appConstants';

dayjs.extend(relativeTime);

class UtilsHelper {
  // ============================================================
  // DATE FORMATTING
  // ============================================================
  
  formatDate(date, format = 'YYYY-MM-DD') {
    if (!date) return '-';
    try {
      const parsed = dayjs(date);
      if (!parsed.isValid()) return '-';
      return parsed.format(format);
    } catch {
      return '-';
    }
  }

  formatDateTime(dateTime, format = 'YYYY-MM-DD HH:mm') {
    if (!dateTime) return '-';
    try {
      const parsed = dayjs(dateTime);
      if (!parsed.isValid()) return '-';
      return parsed.format(format);
    } catch {
      return '-';
    }
  }

  formatRelativeTime(date) {
    if (!date) return '-';
    try {
      const parsed = dayjs(date);
      if (!parsed.isValid()) return '-';
      return parsed.fromNow();
    } catch {
      return '-';
    }
  }

  // ============================================================
  // NUMBER FORMATTING
  // ============================================================
  
  formatCurrency(amount, currency = 'IDR') {
    if (amount === null || amount === undefined || amount === '') return '-';
    
    let numAmount = typeof amount === 'string' ? parseFloat(amount) : amount;
    
    if (isNaN(numAmount) || numAmount === 0) return '-';
    
    try {
      return new Intl.NumberFormat('id-ID', {
        style: 'currency',
        currency: currency,
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
      }).format(numAmount);
    } catch {
      return String(numAmount);
    }
  }

  formatNumber(number, decimals = 0) {
    if (number === null || number === undefined || number === '') return '-';
    
    let num = typeof number === 'string' ? parseFloat(number) : number;
    
    if (isNaN(num) || num === 0) return '-';
    
    try {
      return new Intl.NumberFormat('id-ID', {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
      }).format(num);
    } catch {
      return String(num);
    }
  }

  // ============================================================
  // STRING UTILITIES
  // ============================================================
  
  truncateText(text, maxLength = 50) {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
  }

  generateId(prefix = '') {
    return `${prefix}${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  // ============================================================
  // VALIDATION
  // ============================================================
  
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

  // ============================================================
  // LOCAL STORAGE
  // ============================================================
  
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

  // ============================================================
  // QUERY STRING
  // ============================================================
  
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

  // ============================================================
  // ERROR HANDLING
  // ============================================================
  
  getErrorMessage(error) {
    if (error.response?.data?.errors?.length > 0) {
      return error.response.data.errors.join('\n');
    }
    if (error.response?.data?.message) {
      return error.response.data.message;
    }
    return error.message || 'An unexpected error occurred';
  }

  // ============================================================
  // FILE HANDLING
  // ============================================================
  
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

  formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  isImageFile(fileName) {
    const extension = fileName?.split('.').pop()?.toLowerCase();
    return UPLOAD_CONFIG.ALLOWED_IMAGE_EXTENSIONS.includes(`.${extension}`);
  }

  getStatusColor(status) {
    console.warn('getStatusColor is deprecated. Use getStatusChipStyles from statusColors.js');
    const colors = {
      'Available': 'success',
      'Assigned': 'primary',
      'On Loan': 'purple',
      'In Maintenance': 'warning',
      'Under Repair': 'warning',
      'Damaged': 'error',
      'Retired': 'secondary',
      'Pending': 'warning',
      'Approved': 'success',
      'Rejected': 'error',
      'Active': 'success',
      'Resigned': 'secondary',
    };
    return colors[status] || 'secondary';
  }
}

export default new UtilsHelper();