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

export default {
  createZodResolver,
  getErrorMessage,
  hasError,
};