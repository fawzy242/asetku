// src/core/services/successHandler.js
import { alertUtils } from '../../lib/sweetAlert';

/**
 * Centralized Success Handler - Menggunakan SweetAlert2 Modal
 * 
 * @param {Object} options
 * @param {string} options.message - Success message to display
 * @param {string} options.operation - Operation type (create, update, delete, etc.)
 * @param {string} options.entityName - Entity name for default messages
 * @param {Function} options.onSuccess - Callback after success
 * @param {boolean} options.showAlert - Show SweetAlert modal (default: true)
 * @param {Function} options.onCloseModal - Close modal callback
 * @param {Function} options.onRefreshData - Refresh data callback
 * @param {Function} options.onResetForm - Reset form callback
 * @param {Object} options.result - API result data
 * @returns {Object} { success: true, message: string }
 */
export const handleSuccess = async (options = {}) => {
  const {
    message,
    operation = 'operation',
    entityName = 'record',
    onSuccess,
    showAlert = true,
    onCloseModal,
    onRefreshData,
    onResetForm,
    result,
  } = options;

  // Generate default message based on operation
  let defaultMessage = message;
  if (!defaultMessage) {
    const operationMap = {
      create: `${entityName} created successfully`,
      update: `${entityName} updated successfully`,
      delete: `${entityName} deleted successfully`,
      activate: `${entityName} activated successfully`,
      deactivate: `${entityName} deactivated successfully`,
      approve: `${entityName} approved successfully`,
      reject: `${entityName} rejected successfully`,
      complete: `${entityName} completed successfully`,
      uncomplete: `${entityName} uncompleted successfully`,
      return: `${entityName} returned successfully`,
      'post-maintenance': `Post-maintenance completed successfully`,
      import: `Import completed successfully`,
      generate: `Generated successfully`,
      upload: `Upload completed successfully`,
      'change-password': `Password changed successfully`,
      'update-profile': `Profile updated successfully`,
      login: `Login successful`,
      logout: `Logged out successfully`,
      'reset-password': `Password reset successfully`,
    };
    defaultMessage = operationMap[operation] || `${entityName} ${operation}ed successfully`;
  }

  const resultObject = {
    success: true,
    message: defaultMessage,
    data: result?.data || result,
  };

  // FIX: Gunakan SweetAlert2 modal (bukan toast)
  if (showAlert && defaultMessage) {
    await alertUtils.success('Success!', defaultMessage);
  }

  // Execute callbacks
  if (onCloseModal) {
    onCloseModal();
  }

  if (onRefreshData) {
    onRefreshData();
  }

  if (onResetForm) {
    onResetForm();
  }

  if (onSuccess) {
    onSuccess(resultObject);
  }

  return resultObject;
};

export const crudSuccess = (operation, entityName, options = {}) => {
  return handleSuccess({
    operation,
    entityName,
    ...options,
  });
};

export default handleSuccess;