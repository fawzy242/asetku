// src/pages/UserRoles/components/AssignRoleDialog.jsx
import React from 'react';
import { Grid, Typography, Box } from '@mui/material';
import CrudModal from '../../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../../components/atoms/FormSection/FormSection';
import NumberInput from '../../../components/atoms/Input/NumberInput';
import Select from '../../../components/atoms/Select/Select';

const AssignRoleDialog = ({
  isOpen,
  onClose,
  onSubmit,
  isSubmitting,
  formData,
  setFormField,
  roleOptions,
}) => {
  return (
    <CrudModal
      isOpen={isOpen}
      onClose={onClose}
      title="Assign Role to User"
      onSubmit={onSubmit}
      isSubmitting={isSubmitting}
      submitText="Assign"
      cancelText="Cancel"
      disableSubmit={false}
      size="md"
    >
      <FormSection
        title="Assignment Details"
        description="Specify the user and the role to assign"
      >
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <NumberInput
              label="User ID"
              value={formData.userId}
              onChange={(e) => setFormField('userId')(e.target.value)}
              prefix="#"
              thousandSeparator={false}
              decimalScale={0}
              min={1}
              required
            />
          </Grid>
          <Grid item xs={12}>
            <Select
              label="Role"
              value={formData.roleId || ''}
              onChange={(e) => setFormField('roleId')(e.target.value)}
              options={roleOptions}
              required
            />
          </Grid>
          <Grid item xs={12}>
            <Box
              sx={{
                p: 2,
                bgcolor: 'var(--surface)',
                borderRadius: 2,
                border: '1px solid var(--border)',
              }}
            >
              <Typography variant="caption" color="text.secondary">
                User IDs are numeric account identifiers. If a role is already
                assigned, the server will reactivate the existing assignment.
              </Typography>
            </Box>
          </Grid>
        </Grid>
      </FormSection>
    </CrudModal>
  );
};

export default AssignRoleDialog;