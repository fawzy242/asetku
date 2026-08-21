import { zodResolver } from '@hookform/resolvers/zod';

/**
 * Create a react-hook-form resolver from a Zod schema
 * @param {z.ZodSchema} schema - Zod schema
 * @returns {Function} Zod resolver for react-hook-form
 */
export const createZodResolver = (schema) => {
  return zodResolver(schema);
};

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
 * Check if field has error
 * @param {Object} errors - react-hook-form errors object
 * @param {string} fieldName - Field name
 * @returns {boolean} True if field has error
 */
export const hasError = (errors, fieldName) => {
  return !!errors?.[fieldName];
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

/**
 * Get register options with error handling
 * @param {Object} register - react-hook-form register function
 * @param {string} fieldName - Field name
 * @param {Object} options - Additional register options
 * @returns {Object} Register props with error handling
 */
export const getRegisterProps = (register, fieldName, options = {}) => {
  return register(fieldName, options);
};

export default {
  createZodResolver,
  getErrorMessage,
  hasError,
  getFieldErrorProps,
  getRegisterProps,
};