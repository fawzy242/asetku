/**
 * Get error message from react-hook-form errors
 * @param {Object} errors - react-hook-form errors object
 * @param {string} fieldName - Field name
 * @returns {string} Error message or empty string
 */
export const getErrorMessage = (errors, fieldName) => {
  return errors?.[fieldName]?.message || '';
};

/**
 * Get field error props for Input components
 * @param {Object} errors - react-hook-form errors object
 * @param {string} fieldName - Field name
 * @returns {Object} { error: string, helperText: string }
 */
export const getFieldErrorProps = (errors, fieldName) => {
  const message = getErrorMessage(errors, fieldName);
  return {
    error: message,
    helperText: message,
  };
};