export const getErrorMessage = (error) => {
  if (error?.response?.data?.errors?.length > 0) {
    return error.response.data.errors.join('\n');
  }
  if (error?.response?.data?.message) {
    return error.response.data.message;
  }
  if (error?.userMessage) {
    return error.userMessage;
  }
  return error?.message || 'An unexpected error occurred';
};

export const isCanceledError = (error) => {
  return error?.name === 'CanceledError' ||
         error?.message === 'canceled' ||
         error?.isCanceled === true ||
         error?.code === 'ERR_CANCELED';
};

export const isNotFoundError = (error) => {
  return error?.response?.status === 404 || error?.isNotFound === true;
};

export const isUnauthorizedError = (error) => {
  return error?.response?.status === 401;
};

export const isForbiddenError = (error) => {
  return error?.response?.status === 403;
};

export const isValidationError = (error) => {
  return error?.response?.status === 422;
};

export const isServerError = (error) => {
  const status = error?.response?.status;
  return status >= 500 && status <= 599;
};