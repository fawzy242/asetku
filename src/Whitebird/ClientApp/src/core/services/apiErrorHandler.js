// src/core/services/apiErrorHandler.js
import { toastr } from '../../lib/sweetAlert';

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

  const result = {
    handled: false,
    status: 0,
    message: 'An unexpected error occurred',
    data: null,
  };

  // FIX: Jika error adalah cancel, jangan tampilkan apa-apa
  if (error?.name === 'CanceledError' || 
      error?.message === 'canceled' || 
      error?.isCanceled === true ||
      error?.code === 'ERR_CANCELED') {
    result.handled = true;
    result.message = '';
    return result;
  }

  // FIX: Skip toast untuk blob errors
  if (error?.__skipToast === true) {
    result.handled = true;
    result.message = '';
    return result;
  }

  // FIX: Jika 404 untuk profile-photo, jangan tampilkan error
  if (error.response?.status === 404 && 
      error.config?.url?.includes('/User/profile-photo')) {
    result.handled = true;
    result.message = '';
    return result;
  }

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

  let errorMessage = 'An unexpected error occurred';
  let errorData = null;

  if (data) {
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

  switch (status) {
    case 401:
      if (showToast) {
        toastr.error('Session expired. Please login again.');
      }
      localStorage.removeItem('whitebird_session_token');
      localStorage.removeItem('user');
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
      if (onUnauthorized) onUnauthorized(result);
      break;

    case 403:
      if (showToast) {
        toastr.error(errorMessage || 'Access denied. You do not have permission.');
      }
      if (onForbidden) onForbidden(result);
      break;

    case 404:
      if (showToast) {
        toastr.error(errorMessage || 'Resource not found.');
      }
      if (onNotFound) onNotFound(result);
      break;

    case 409:
      if (showToast) {
        toastr.error(errorMessage || 'Resource conflict. Please try again.');
      }
      if (onConflict) onConflict(result);
      break;

    case 422:
      if (showToast) {
        toastr.error(errorMessage || 'Validation failed. Please check your input.');
      }
      if (onValidationError) onValidationError(result);
      break;

    case 500:
    case 502:
    case 503:
    case 504:
      if (showToast) {
        toastr.error(errorMessage || 'Server error. Please try again later.');
      }
      if (onServerError) onServerError(result);
      break;

    default:
      if (showToast && errorMessage) {
        toastr.error(errorMessage);
      }
      break;
  }

  return result;
};

export const isCancelError = (error) => {
  return error?.name === 'CanceledError' || 
         error?.message === 'canceled' ||
         error?.isCanceled === true ||
         error?.code === 'ERR_CANCELED';
};

export const shouldIgnoreError = (error) => {
  return isCancelError(error) || error?.__skipToast === true;
};

export default handleApiError;