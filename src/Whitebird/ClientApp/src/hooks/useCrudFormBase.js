import { useState, useCallback } from 'react';

// Flatten backend { fieldName: ["msg1", "msg2"] } to { fieldName: "msg1" }.
// Case-insensitive key matching: ModelState may return "AssetCode" while
// formData uses "assetCode"; we normalize to the formData key when possible
// so dialogs' `errors?.fieldName` lookups work regardless of server casing.
const flattenFieldErrors = (fieldErrors, formData) => {
  const flattened = {};
  if (!fieldErrors || typeof fieldErrors !== 'object' || Array.isArray(fieldErrors)) {
    return flattened;
  }

  const lowercaseToActualKey = {};
  Object.keys(formData || {}).forEach((k) => {
    lowercaseToActualKey[k.toLowerCase()] = k;
  });

  for (const [field, messages] of Object.entries(fieldErrors)) {
    let msg = null;
    if (Array.isArray(messages) && messages.length > 0) {
      msg = messages[0];
    } else if (typeof messages === 'string') {
      msg = messages;
    }
    if (!msg) continue;
    const actualKey = lowercaseToActualKey[field.toLowerCase()] || field;
    flattened[actualKey] = msg;
  }

  return flattened;
};

export const useCrudFormBase = (initialFormData, dataService, options = {}) => {
  const {
    idField,
    onBeforeSubmit,
    onAfterSubmit,
    transformFormData,
    onSuccess,
    onError,
    schema,
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
      setErrors(prev => {
        if (!prev[field]) return prev;
        const next = { ...prev };
        delete next[field];
        return next;
      });
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

    // 1. Client-side schema validation (optional; only runs if schema provided)
    if (schema) {
      const parseResult = schema.safeParse(formData);
      if (!parseResult.success) {
        const fieldErrors = {};
        for (const issue of parseResult.error.issues) {
          const field = issue.path[0];
          if (field && !fieldErrors[field]) {
            fieldErrors[field] = issue.message;
          }
        }
        setErrors(fieldErrors);
        return { success: false, errors: fieldErrors };
      }
    }

    // 2. Clear any client-side errors before proceeding to server call
    setErrors({});

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
      // Prefer backend fieldErrors (per-field) over flat errors array.
      if (result.fieldErrors) {
        const flattened = flattenFieldErrors(result.fieldErrors, formData);
        if (Object.keys(flattened).length > 0) {
          setErrors(flattened);
        } else if (result.errors) {
          if (Array.isArray(result.errors)) {
            setErrors({ _form: result.errors.join('\n') });
          } else if (typeof result.errors === 'object') {
            setErrors(result.errors);
          }
        }
      } else if (result.errors) {
        if (Array.isArray(result.errors)) {
          setErrors({ _form: result.errors.join('\n') });
        } else if (typeof result.errors === 'object') {
          setErrors(result.errors);
        }
      }
      if (onError) onError(result.error || 'Operation failed');
      return { 
        success: false, 
        error: result.error || 'Operation failed',
        errors: result.errors,
        fieldErrors: result.fieldErrors || null,
      };
    }
    return { success: false, error: 'Operation failed' };
  }, [isSubmitting, editingRecord, formData, dataService, getRecordId, transformFormData, onBeforeSubmit, onAfterSubmit, onSuccess, onError, initialFormData, schema]);

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