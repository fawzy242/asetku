// src/pages/Departments/components/DepartmentFormDialog.jsx
import React from 'react';
import { Grid } from '@mui/material';
import CrudModal from '../../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../../components/atoms/FormSection/FormSection';
import Input from '../../../components/atoms/Input/Input';

const DepartmentFormDialog = ({
  isOpen,
  onClose,
  onSubmit,
  isSubmitting,
  isViewMode,
  editingRecord,
  formData,
  errors = {},
  setFormField,
}) => {
  return (
    <CrudModal
      isOpen={isOpen}
      onClose={onClose}
      title={
        isViewMode
          ? 'View Department'
          : editingRecord
            ? 'Edit Department'
            : 'Add Department'
      }
      onSubmit={onSubmit}
      isSubmitting={isSubmitting}
      submitText={isViewMode ? 'Close' : editingRecord ? 'Update' : 'Create'}
      cancelText={isViewMode ? 'Close' : 'Cancel'}
      disableSubmit={isViewMode}
      size="md"
    >
      <FormSection
        title="Basic Information"
        description="Department code and name"
      >
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Input
              label="Department Code"
              value={formData.departmentCode || ''}
              onChange={(e) => setFormField('departmentCode')(e.target.value)}
              error={errors?.departmentCode || ''}
              disabled={isViewMode}
              placeholder="Optional (max 100 chars)"
            />
          </Grid>
          <Grid item xs={12}>
            <Input
              label="Department Name"
              value={formData.departmentName || ''}
              onChange={(e) => setFormField('departmentName')(e.target.value)}
              error={errors?.departmentName || ''}
              disabled={isViewMode}
              required
            />
          </Grid>
          <Grid item xs={12}>
            <Input
              label="Description"
              value={formData.description || ''}
              onChange={(e) => setFormField('description')(e.target.value)}
              error={errors?.description || ''}
              disabled={isViewMode}
              multiline
              rows={2}
            />
          </Grid>
        </Grid>
      </FormSection>
    </CrudModal>
  );
};

export default DepartmentFormDialog;