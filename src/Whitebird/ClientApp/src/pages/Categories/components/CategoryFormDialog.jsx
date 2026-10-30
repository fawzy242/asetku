// src/pages/Categories/components/CategoryFormDialog.jsx
import React from 'react';
import { Grid } from '@mui/material';
import CrudModal from '../../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../../components/atoms/FormSection/FormSection';
import Input from '../../../components/atoms/Input/Input';
import Select from '../../../components/atoms/Select/Select';

const CategoryFormDialog = ({
  isOpen,
  onClose,
  onSubmit,
  isSubmitting,
  isViewMode,
  editingRecord,
  formData,
  errors = {},
  setFormField,
  parentCategoryOptions,
}) => {
  return (
    <CrudModal
      isOpen={isOpen}
      onClose={onClose}
      title={
        isViewMode
          ? 'View Category'
          : editingRecord
            ? 'Edit Category'
            : 'Add Category'
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
        description="Category name and description"
      >
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Input
              label="Category Name"
              value={formData.categoryName || ''}
              onChange={(e) => setFormField('categoryName')(e.target.value)}
              error={errors?.categoryName || ''}
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
          <Grid item xs={12}>
            <Select
              label="Parent Category"
              value={formData.parentCategoryId || ''}
              onChange={(e) =>
                setFormField('parentCategoryId')(e.target.value || null)
              }
              error={errors?.parentCategoryId || ''}
              options={parentCategoryOptions}
              disabled={isViewMode}
            />
          </Grid>
        </Grid>
      </FormSection>
    </CrudModal>
  );
};

export default CategoryFormDialog;