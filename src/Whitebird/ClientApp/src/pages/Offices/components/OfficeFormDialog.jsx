// src/pages/Offices/components/OfficeFormDialog.jsx
import React from 'react';
import { Grid } from '@mui/material';
import CrudModal from '../../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../../components/atoms/FormSection/FormSection';
import Input from '../../../components/atoms/Input/Input';
import Select from '../../../components/atoms/Select/Select';

const OfficeFormDialog = ({
  isOpen,
  onClose,
  onSubmit,
  isSubmitting,
  isViewMode,
  editingRecord,
  formData,
  setFormField,
  officeTypeOptions,
  parentOfficeOptions,
}) => {
  return (
    <CrudModal
      isOpen={isOpen}
      onClose={onClose}
      title={
        isViewMode ? 'View Office' : editingRecord ? 'Edit Office' : 'Add Office'
      }
      onSubmit={onSubmit}
      isSubmitting={isSubmitting}
      submitText={isViewMode ? 'Close' : editingRecord ? 'Update' : 'Create'}
      cancelText={isViewMode ? 'Close' : 'Cancel'}
      disableSubmit={isViewMode}
      size="lg"
    >
      <FormSection
        title="Basic Information"
        description="Office name, code, and type"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Input
              label="Office Name"
              value={formData.officeName || ''}
              onChange={(e) => setFormField('officeName')(e.target.value)}
              disabled={isViewMode}
              required
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input
              label="Office Code"
              value={formData.officeCode || ''}
              onChange={(e) => setFormField('officeCode')(e.target.value)}
              disabled={isViewMode}
              placeholder="Optional"
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select
              label="Office Type"
              value={formData.officeType || ''}
              onChange={(e) => setFormField('officeType')(e.target.value)}
              options={officeTypeOptions}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select
              label="Parent Office"
              value={formData.parentOfficeId || ''}
              onChange={(e) =>
                setFormField('parentOfficeId')(e.target.value || null)
              }
              options={parentOfficeOptions}
              disabled={isViewMode}
            />
          </Grid>
        </Grid>
      </FormSection>

      <FormSection title="Location Details" description="City, address, and phone">
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Input
              label="City"
              value={formData.city || ''}
              onChange={(e) => setFormField('city')(e.target.value)}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input
              label="Phone"
              value={formData.phone || ''}
              onChange={(e) => setFormField('phone')(e.target.value)}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12}>
            <Input
              label="Address"
              value={formData.address || ''}
              onChange={(e) => setFormField('address')(e.target.value)}
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

export default OfficeFormDialog;