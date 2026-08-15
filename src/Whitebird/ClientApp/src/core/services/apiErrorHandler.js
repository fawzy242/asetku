import { isCanceledError } from '../utils/error.utils';
import { STORAGE_KEYS } from '../constants/storageKeys';

export const handleApiError = (error, options = {}) => {
  const {
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
    shouldShowToast: true,
  };

  if (isCanceledError(error)) {
    result.handled = true;
    result.message = '';
    result.shouldShowToast = false;
    return result;
  }

  if (error?.__skipToast === true) {
    result.handled = true;
    result.message = '';
    result.shouldShowToast = false;
    return result;
  }

  if (error.response?.status === 404 && 
      error.config?.url?.includes('/User/profile-photo')) {
    result.handled = true;
    result.message = '';
    result.shouldShowToast = false;
    return result;
  }

  if (!error.response) {
    result.status = 0;
    result.message = error.message || 'Network error. Please check your connection.';
    result.handled = true;
    if (onNetworkError) onNetworkError(result);
    return result;
  }

  const { status, data } = error.response;
  result.status = status;

  let errorMessage = 'An unexpected error occurred';
  let errorData = null;

  if (data) {
    if (data.message) errorMessage = data.message;
    if (data.errors && Array.isArray(data.errors) && data.errors.length > 0) {
      errorMessage = data.errors.join('\n');
    }
    if (data.data) errorData = data.data;
  }

  result.message = errorMessage;
  result.data = errorData;
  result.handled = true;

  switch (status) {
    case 401:
      if (onUnauthorized) onUnauthorized(result);
      break;
    case 403:
      if (onForbidden) onForbidden(result);
      break;
    case 404:
      if (onNotFound) onNotFound(result);
      break;
    case 409:
      if (onConflict) onConflict(result);
      break;
    case 422:
      if (onValidationError) onValidationError(result);
      break;
    case 500:
    case 502:
    case 503:
    case 504:
      if (onServerError) onServerError(result);
      break;
    default:
      break;
  }

  return result;
};

export const isCancelError = isCanceledError;

export const shouldIgnoreError = (error) => {
  return isCanceledError(error) || error?.__skipToast === true;
};

export default handleApiError;