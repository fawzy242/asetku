// src/core/services/apiErrorHandler.js
import { toastr } from '../../lib/sweetAlert';

/**
 * Centralized API Error Handler
 * 
 * @param {Error} error - Error object dari axios
 * @param {Object} options - Optional configuration
 * @param {boolean} options.showToast - Show toast notification (default: true)
 * @param {Function} options.onUnauthorized - Callback when 401
 * @param {Function} options.onForbidden - Callback when 403
 * @param {Function} options.onNotFound - Callback when 404
 * @param {Function} options.onConflict - Callback when 409
 * @param {Function} options.onValidationError - Callback when 422
 * @param {Function} options.onServerError - Callback when 500
 * @param {Function} options.onNetworkError - Callback when network error
 * @returns {Object} { handled: boolean, status: number, message: string, data: any }
 */
export const handleApiError = (error, options = {}) => {
  const {
    showToast = true,
    onUnauthorized,
    onForbidden,
    onNotFound,
    onConflict,
    onValidationError,
    onServerError,
    onNetworkError,
  } = options;

  // Default error response
  const result = {
    handled: false,
    status: 0,
    message: 'An unexpected error occurred',
    data: null,
  };

  // Network error (no response)
  if (!error.response) {
    result.status = 0;
    result.message = error.message || 'Network error. Please check your connection.';
    result.handled = true;

    if (showToast) {
      toastr.error(result.message);
    }
    if (onNetworkError) onNetworkError(result);
    return result;
  }

  const { status, data } = error.response;
  result.status = status;

  // Extract error message from backend
  let errorMessage = 'An unexpected error occurred';
  let errorData = null;

  if (data) {
    // Backend ServiceResult format
    if (data.message) {
      errorMessage = data.message;
    }
    if (data.errors && Array.isArray(data.errors) && data.errors.length > 0) {
      errorMessage = data.errors.join('\n');
    }
    if (data.data) {
      errorData = data.data;
    }
  }

  result.message = errorMessage;
  result.data = errorData;
  result.handled = true;

  // Handle specific status codes
  switch (status) {
    case 401:
      // Unauthorized - session expired
      if (showToast) {
        toastr.error('Session expired. Please login again.');
      }
      // Clear session
      localStorage.removeItem('whitebird_session_token');
      localStorage.removeItem('user');
      // Redirect to login if not already there
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
      if (onUnauthorized) onUnauthorized(result);
      break;

    case 403:
      // Forbidden
      if (showToast) {
        toastr.error(errorMessage || 'Access denied. You do not have permission.');
      }
      if (onForbidden) onForbidden(result);
      break;

    case 404:
      // Not Found
      if (showToast) {
        toastr.error(errorMessage || 'Resource not found.');
      }
      if (onNotFound) onNotFound(result);
      break;

    case 409:
      // Conflict
      if (showToast) {
        toastr.error(errorMessage || 'Resource conflict. Please try again.');
      }
      if (onConflict) onConflict(result);
      break;

    case 422:
      // Validation Error
      if (showToast) {
        toastr.error(errorMessage || 'Validation failed. Please check your input.');
      }
      if (onValidationError) onValidationError(result);
      break;

    case 500:
    case 502:
    case 503:
    case 504:
      // Server Error
      if (showToast) {
        toastr.error(errorMessage || 'Server error. Please try again later.');
      }
      if (onServerError) onServerError(result);
      break;

    default:
      // Other errors
      if (showToast && errorMessage) {
        toastr.error(errorMessage);
      }
      break;
  }

  return result;
};

/**
 * Check if error is a cancellation error
 */
export const isCancelError = (error) => {
  return error?.name === 'CanceledError' || error?.message === 'canceled';
};

/**
 * Check if error should be ignored (cancellation, etc.)
 */
export const shouldIgnoreError = (error) => {
  return isCancelError(error);
};

export default handleApiError;