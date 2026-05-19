import { useState, useCallback } from 'react';

/**
 * Custom hook untuk form CRUD modal
 * @param {Object} initialFormData - Default form values
 * @param {Object} dataService - Instance dari BaseData service
 * @param {Object} [options] - Additional options
 * @param {string} [options.idField] - Nama field ID (default: auto-detect)
 * @param {Function} [options.onBeforeSubmit] - Hook sebelum submit
 * @param {Function} [options.onAfterSubmit] - Hook setelah submit berhasil
 * @param {Function} [options.transformFormData] - Transform form data sebelum submit
 */
export const useCrudForm = (initialFormData, dataService, options = {}) => {
  const { idField, onBeforeSubmit, onAfterSubmit, transformFormData } = options;

  const [showModal, setShowModal] = useState(false);
  const [editingRecord, setEditingRecord] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formData, setFormData] = useState(initialFormData);

  const getRecordId = useCallback((record) => {
    if (!record) return null;
    if (idField && record[idField] !== undefined) return record[idField];
    const key = Object.keys(record).find(k => k.toLowerCase().includes('id'));
    return key ? record[key] : null;
  }, [idField]);

  const handleCreate = useCallback(() => {
    setEditingRecord(null);
    setFormData({ ...initialFormData });
    setShowModal(true);
  }, [initialFormData]);

  const handleEdit = useCallback(async (record) => {
    const recordId = getRecordId(record);
    if (!recordId) {
      console.error('useCrudForm: Cannot determine record ID');
      return;
    }
    const result = await dataService.fetchById(recordId);
    if (result.success) {
      setEditingRecord(result.data);
      setFormData({ ...result.data });
      setShowModal(true);
    } else {
      console.error('useCrudForm: Failed to fetch record for editing:', result.error);
    }
  }, [dataService, getRecordId]);

  const handleClose = useCallback(() => {
    if (!isSubmitting) {
      setShowModal(false);
    }
  }, [isSubmitting]);

  const handleSubmit = useCallback(async (e) => {
    if (e && typeof e.preventDefault === 'function') {
      e.preventDefault();
    }
    if (isSubmitting) return;

    if (onBeforeSubmit) {
      const canProceed = await onBeforeSubmit(formData);
      if (!canProceed) return;
    }

    setIsSubmitting(true);

    const submitData = transformFormData ? transformFormData({ ...formData }) : { ...formData };

    const result = editingRecord
      ? await dataService.update(getRecordId(editingRecord), submitData)
      : await dataService.create(submitData);

    setIsSubmitting(false);

    if (result.success) {
      setShowModal(false);
      if (onAfterSubmit) onAfterSubmit(result);
      return true;
    }
    return false;
  }, [isSubmitting, editingRecord, formData, dataService, getRecordId, onBeforeSubmit, onAfterSubmit, transformFormData]);

  const updateFormField = useCallback((field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  }, []);

  return {
    showModal,
    editingRecord,
    isSubmitting,
    formData,
    setFormData,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit,
    updateFormField,
  };
};