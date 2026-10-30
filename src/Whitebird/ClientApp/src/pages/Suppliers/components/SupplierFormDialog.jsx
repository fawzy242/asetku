// src/pages/Suppliers/components/SupplierFormDialog.jsx
import React from 'react';
import { Grid } from '@mui/material';
import CrudModal from '../../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../../components/atoms/FormSection/FormSection';
import Input from '../../../components/atoms/Input/Input';

const SupplierFormDialog = ({
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
          ? 'View Supplier'
          : editingRecord
            ? 'Edit Supplier'
            : 'Add Supplier'
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
        description="Supplier name and contact person"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Input
              label="Supplier Name"
              value={formData.supplierName || ''}
              onChange={(e) => setFormField('supplierName')(e.target.value)}
              error={errors?.supplierName || ''}
              disabled={isViewMode}
              required
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input
              label="Contact Person"
              value={formData.contactPerson || ''}
              onChange={(e) => setFormField('contactPerson')(e.target.value)}
              error={errors?.contactPerson || ''}
              disabled={isViewMode}
            />
          </Grid>
        </Grid>
      </FormSection>

      <FormSection
        title="Contact Information"
        description="Phone, email, and address"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Input
              label="Phone Number"
              value={formData.phoneNumber || ''}
              onChange={(e) => setFormField('phoneNumber')(e.target.value)}
              error={errors?.phoneNumber || ''}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input
              label="Email"
              type="email"
              value={formData.email || ''}
              onChange={(e) => setFormField('email')(e.target.value)}
              error={errors?.email || ''}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12}>
            <Input
              label="Address"
              value={formData.address || ''}
              onChange={(e) => setFormField('address')(e.target.value)}
              error={errors?.address || ''}
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

export default SupplierFormDialog;