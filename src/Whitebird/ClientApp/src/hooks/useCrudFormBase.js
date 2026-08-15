import { useState, useCallback } from 'react';

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
      const operation = editingRecord ? 'update' : 'create';
      const entityName = dataService.getEntityName ? dataService.getEntityName() : 'record';
      
      // Close modal and reset form
      setShowModal(false);
      setEditingRecord(null);
      setFormData({ ...initialFormData });
      setErrors({});

      // Call callbacks - NO UI calls
      if (onAfterSubmit) onAfterSubmit(result);
      if (onSuccess) onSuccess(result);

      return { 
        success: true, 
        message: `${entityName} ${operation}d successfully`,
        data: result.data,
      };
    } else if (!result.cancelled) {
      if (result.errors) {
        setErrors(result.errors);
      }
      if (onError) onError(result.error || 'Operation failed');
      return { 
        success: false, 
        error: result.error || 'Operation failed',
        errors: result.errors,
      };
    }
    return { success: false, error: 'Operation failed' };
  }, [isSubmitting, editingRecord, formData, dataService, getRecordId, transformFormData, onBeforeSubmit, onAfterSubmit, onSuccess, onError, initialFormData]);

  const resetForm = useCallback(() => {
    setFormData({ ...initialFormData });
    setErrors({});
    setEditingRecord(null);
  }, [initialFormData]);

  return {
    showModal,
    editingRecord,
    isSubmitting,
    formData,
    errors,
    setFormData,
    setErrors,
    setFieldValue,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit,
    resetForm,
    getRecordId,
  };
};

export default useCrudFormBase;