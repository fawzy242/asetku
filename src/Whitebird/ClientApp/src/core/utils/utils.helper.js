import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import { API_CONFIG, UPLOAD_CONFIG } from '../constants/appConstants';

dayjs.extend(relativeTime);

/**
 * Utility helper class for common operations
 * @class UtilsHelper
 */
class UtilsHelper {
  // ============================================================
  // DATE FORMATTING
  // ============================================================
  
  /**
   * Format date to YYYY-MM-DD format
   * @param {string|Date} date - Date to format
   * @param {string} [format='YYYY-MM-DD'] - Output format
   * @returns {string} Formatted date or '-' if invalid
   */
  formatDate(date, format = 'YYYY-MM-DD') {
    if (!date) return '-';
    const d = dayjs(date);
    if (!d.isValid()) return '-';
    return d.format(format);
  }

  /**
   * Format datetime to readable format
   * @param {string|Date} dateTime - DateTime to format
   * @param {string} [format='YYYY-MM-DD HH:mm'] - Output format
   * @returns {string} Formatted datetime or '-' if invalid
   */
  formatDateTime(dateTime, format = 'YYYY-MM-DD HH:mm') {
    if (!dateTime) return '-';
    const d = dayjs(dateTime);
    if (!d.isValid()) return '-';
    return d.format(format);
  }

  /**
   * Get relative time (e.g., "2 hours ago")
   * @param {string|Date} date - Date to convert
   * @returns {string} Relative time string
   */
  formatRelativeTime(date) {
    if (!date) return '-';
    const d = dayjs(date);
    if (!d.isValid()) return '-';
    return d.fromNow();
  }

  // ============================================================
  // NUMBER FORMATTING
  // ============================================================
  
  /**
   * Format number as currency (IDR)
   * @param {number} amount - Amount to format
   * @param {string} [currency='IDR'] - Currency code
   * @returns {string} Formatted currency or '-' if invalid
   */
  formatCurrency(amount, currency = 'IDR') {
    if (amount === null || amount === undefined) return '-';
    if (isNaN(amount)) return '-';
    
    return new Intl.NumberFormat('id-ID', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    }).format(amount);
  }

  /**
   * Format number with thousand separators
   * @param {number} number - Number to format
   * @param {number} [decimals=0] - Decimal places
   * @returns {string} Formatted number or '-' if invalid
   */
  formatNumber(number, decimals = 0) {
    if (number === null || number === undefined) return '-';
    if (isNaN(number)) return '-';
    
    return new Intl.NumberFormat('id-ID', {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals
    }).format(number);
  }

  // ============================================================
  // STRING UTILITIES
  // ============================================================
  
  /**
   * Truncate text to max length and add ellipsis
   * @param {string} text - Text to truncate
   * @param {number} [maxLength=50] - Maximum length
   * @returns {string} Truncated text
   */
  truncateText(text, maxLength = 50) {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
  }

  /**
   * Generate unique ID
   * @param {string} [prefix=''] - Prefix for ID
   * @returns {string} Unique ID
   */
  generateId(prefix = '') {
    return `${prefix}${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  // ============================================================
  // VALIDATION
  // ============================================================
  
  /**
   * Validate email format
   * @param {string} email - Email to validate
   * @returns {boolean} True if valid
   */
  validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
  }

  /**
   * Validate phone number format
   * @param {string} phone - Phone to validate
   * @returns {boolean} True if valid
   */
  validatePhone(phone) {
    const re = /^[0-9+\-\s()]{10,20}$/;
    return re.test(phone);
  }

  /**
   * Check if value is not empty
   * @param {any} value - Value to check
   * @returns {boolean} True if value has content
   */
  validateRequired(value) {
    return value !== null && value !== undefined && value.toString().trim() !== '';
  }

  // ============================================================
  // LOCAL STORAGE
  // ============================================================
  
  /**
   * Save value to localStorage
   * @param {string} key - Storage key
   * @param {any} value - Value to store
   * @returns {boolean} Success status
   */
  setLocalStorage(key, value) {
    try {
      localStorage.setItem(key, JSON.stringify(value));
      return true;
    } catch (e) {
      console.error('Failed to save to localStorage:', e);
      return false;
    }
  }

  /**
   * Get value from localStorage
   * @param {string} key - Storage key
   * @param {any} [defaultValue=null] - Default value if not found
   * @returns {any} Retrieved value or default
   */
  getLocalStorage(key, defaultValue = null) {
    try {
      const value = localStorage.getItem(key);
      return value ? JSON.parse(value) : defaultValue;
    } catch (e) {
      console.error('Failed to read from localStorage:', e);
      return defaultValue;
    }
  }

  /**
   * Remove value from localStorage
   * @param {string} key - Storage key
   * @returns {boolean} Success status
   */
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
  
  /**
   * Build query string from params object
   * @param {Object} params - Query parameters
   * @returns {string} Query string with leading '?'
   */
  buildQueryString(params) {
    const query = Object.entries(params)
      .filter(([_, value]) => value !== null && value !== undefined && value !== '')
      .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`)
      .join('&');
    
    return query ? `?${query}` : '';
  }

  /**
   * Parse query string to object
   * @param {string} queryString - Query string to parse
   * @returns {Object} Parsed parameters
   */
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
  
  /**
   * Extract readable error message from error object
   * @param {Error} error - Error object
   * @returns {string} Readable error message
   */
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
  
  /**
   * Trigger download of blob as file
   * @param {Blob} blob - File blob
   * @param {string} fileName - Name for downloaded file
   */
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

  /**
   * Format file size to human readable
   * @param {number} bytes - File size in bytes
   * @returns {string} Human readable file size
   */
  formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  /**
   * Check if file is an image based on extension
   * @param {string} fileName - File name
   * @returns {boolean} True if image file
   */
  isImageFile(fileName) {
    const extension = fileName?.split('.').pop()?.toLowerCase();
    return UPLOAD_CONFIG.ALLOWED_IMAGE_EXTENSIONS.includes(`.${extension}`);
  }
}

export default new UtilsHelper();