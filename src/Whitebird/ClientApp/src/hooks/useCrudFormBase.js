import { useState, useCallback } from 'react';

/**
 * Enhanced CRUD form hook for consistent form handling across all entities
 * @param {Object} initialFormData - Default form values
 * @param {Object} dataService - Instance of BaseData service
 * @param {Object} options - Additional options
 * @param {string} options.idField - Nama field ID (default: auto-detect from entity)
 * @param {Function} options.onBeforeSubmit - Hook sebelum submit
 * @param {Function} options.onAfterSubmit - Hook setelah submit berhasil
 * @param {Function} options.transformFormData - Transform form data sebelum submit
 * @param {Function} options.onSuccess - Callback after successful submit
 * @param {Function} options.onError - Callback after failed submit
 */
export const useCrudFormBase = (initialFormData, dataService, options = {}) => {
  const {
    idField,
    onBeforeSubmit,
    onAfterSubmit,
    transformFormData,
    onSuccess,
    onError,
  } = options;

  const [showModal, setShowModal] = useState(false);
  const [editingRecord, setEditingRecord] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formData, setFormData] = useState(initialFormData);
  const [errors, setErrors] = useState({});

  const getRecordId = useCallback((record) => {
    if (!record) return null;
    if (idField && record[idField] !== undefined) return record[idField];
    // Auto-detect ID field
    const possibleIdFields = ['id', `${record.constructor?.name?.toLowerCase()}Id`];
    for (const field of possibleIdFields) {
      if (record[field] !== undefined) return record[field];
    }
    const idKey = Object.keys(record).find(k => k.toLowerCase().includes('id'));
    return idKey ? record[idKey] : null;
  }, [idField]);

  const handleCreate = useCallback(() => {
    setEditingRecord(null);
    setFormData({ ...initialFormData });
    setErrors({});
    setShowModal(true);
  }, [initialFormData]);

  const handleEdit = useCallback(async (record) => {
    const recordId = getRecordId(record);
    if (!recordId) {
      console.error('useCrudFormBase: Cannot determine record ID');
      return;
    }
    
    const result = await dataService.fetchById(recordId);
    if (result.success && result.data) {
      setEditingRecord(result.data);
      setFormData({ ...result.data });
      setErrors({});
      setShowModal(true);
    } else {
      const errorMsg = result.error || 'Failed to fetch record for editing';
      console.error('useCrudFormBase:', errorMsg);
      if (onError) onError(errorMsg);
    }
  }, [dataService, getRecordId, onError]);

  const handleClose = useCallback(() => {
    if (!isSubmitting) {
      setShowModal(false);
      setEditingRecord(null);
      setFormData({ ...initialFormData });
      setErrors({});
    }
  }, [isSubmitting, initialFormData]);

  const setFieldValue = useCallback((field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear error for this field when user types
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  }, [errors]);

  const setFormField = useCallback((field) => (value) => {
    setFieldValue(field, value);
  }, [setFieldValue]);

  const handleSubmit = useCallback(async (event) => {
    if (event && typeof event.preventDefault === 'function') {
      event.preventDefault();
    }
    
    if (isSubmitting) return;

    if (onBeforeSubmit) {
      const canProceed = await onBeforeSubmit(formData);
      if (!canProceed) return;
    }

    setIsSubmitting(true);

    const submitData = transformFormData 
      ? transformFormData({ ...formData }) 
      : { ...formData };

    // Remove empty strings and undefined values for ID fields
    Object.keys(submitData).forEach(key => {
      if (submitData[key] === '' || submitData[key] === undefined) {
        submitData[key] = null;
      }
    });

    const result = editingRecord
      ? await dataService.update(getRecordId(editingRecord), submitData)
      : await dataService.create(submitData);

    setIsSubmitting(false);

    if (result.success) {
      setShowModal(false);
      setEditingRecord(null);
      setFormData({ ...initialFormData });
      setErrors({});
      if (onAfterSubmit) onAfterSubmit(result);
      if (onSuccess) onSuccess(result);
      return true;
    } else if (!result.cancelled) {
      if (result.errors) {
        setErrors(result.errors);
      }
      if (onError) onError(result.error || 'Operation failed');
    }
    return false;
  }, [isSubmitting, editingRecord, formData, dataService, getRecordId, transformFormData, onBeforeSubmit, onAfterSubmit, onSuccess, onError, initialFormData]);

  const resetForm = useCallback(() => {
    setFormData({ ...initialFormData });
    setErrors({});
    setEditingRecord(null);
  }, [initialFormData]);

  return {
    // State
    showModal,
    editingRecord,
    isSubmitting,
    formData,
    errors,
    
    // Setters
    setFormData,
    setErrors,
    setFieldValue,
    setFormField,
    
    // Actions
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit,
    resetForm,
    
    // Helpers
    getRecordId,
  };
};

export default useCrudFormBase;