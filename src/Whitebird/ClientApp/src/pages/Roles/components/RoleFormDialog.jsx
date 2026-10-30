// src/pages/Roles/components/RoleFormDialog.jsx
import React from 'react';
import { Grid } from '@mui/material';
import CrudModal from '../../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../../components/atoms/FormSection/FormSection';
import Input from '../../../components/atoms/Input/Input';

const RoleFormDialog = ({
  isOpen,
  onClose,
  onSubmit,
  isSubmitting,
  isViewMode,
  editingRecord,
  formData,
  setFormField,
}) => {
  return (
    <CrudModal
      isOpen={isOpen}
      onClose={onClose}
      title={
        isViewMode ? 'View Role' : editingRecord ? 'Edit Role' : 'Add Role'
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
        description="Role code, name, and description"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Input
              label="Role Code"
              value={formData.roleCode || ''}
              onChange={(e) => setFormField('roleCode')(e.target.value)}
              disabled={isViewMode}
              placeholder="Optional (max 50 chars)"
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input
              label="Role Name"
              value={formData.roleName || ''}
              onChange={(e) => setFormField('roleName')(e.target.value)}
              disabled={isViewMode}
              required
            />
          </Grid>
          <Grid item xs={12}>
            <Input
              label="Description"
              value={formData.description || ''}
              onChange={(e) => setFormField('description')(e.target.value)}
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

export default RoleFormDialog;