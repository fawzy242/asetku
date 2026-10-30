// src/pages/Employees/components/EmployeeFormDialog.jsx
import React from 'react';
import { Grid, Box, Avatar, Typography, IconButton } from '@mui/material';
import { FiCamera, FiX } from 'react-icons/fi';
import CrudModal from '../../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../../components/atoms/FormSection/FormSection';
import Input from '../../../components/atoms/Input/Input';
import Select from '../../../components/atoms/Select/Select';
import DatePickerInput from '../../../components/atoms/Input/DatePickerInput';
import FileUploader from '../../../components/molecules/FileUploader/FileUploader';
import { getFieldErrorProps } from '../../../core/utils/validation.utils';

const EmployeeFormDialog = ({
  isOpen,
  onClose,
  onSubmit,
  isSubmitting,
  isViewMode,
  isEditMode,
  editingEmployee,
  currentEmployeeId,
  formData,
  setFormField,
  register,
  formErrors,
  photoPreview,
  handlePhotoChange,
  handleRemovePhoto,
  attachmentFiles,
  existingAttachments,
  handleFilesChange,
  handleUploadComplete,
  handleFileDeleted,
  departmentOptions,
  positionOptions,
  statusOptions,
  officeOptions,
}) => {
  const showPhotoSection = isEditMode && currentEmployeeId;
  const showFileUploads = isEditMode && currentEmployeeId && !isViewMode;

  return (
    <CrudModal
      isOpen={isOpen}
      onClose={onClose}
      title={
        isViewMode
          ? 'View Employee'
          : editingEmployee
            ? 'Edit Employee'
            : 'Add Employee'
      }
      onSubmit={onSubmit}
      isSubmitting={isSubmitting}
      submitText={
        isViewMode ? 'Close' : editingEmployee ? 'Update' : 'Create'
      }
      cancelText={isViewMode ? 'Close' : 'Cancel'}
      disableSubmit={isViewMode}
      size="xl"
    >
      {showPhotoSection && (
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 3,
            mb: 3,
            pb: 3,
            borderBottom: '1px solid var(--border)',
          }}
        >
          <Box sx={{ position: 'relative' }}>
            <Avatar
              src={
                photoPreview ||
                editingEmployee?.photoProfileUrl ||
                undefined
              }
              sx={{
                width: 100,
                height: 100,
                bgcolor: 'var(--primary)',
                fontSize: 40,
                fontWeight: 600,
                border: '3px solid var(--border)',
              }}
            >
              {!photoPreview &&
                !editingEmployee?.photoProfileUrl &&
                (formData.fullName?.charAt(0) || 'E')}
            </Avatar>
            {!isViewMode && (
              <Box sx={{ position: 'absolute', bottom: -4, right: -4 }}>
                <label
                  htmlFor="employee-photo-upload"
                  style={{ cursor: 'pointer' }}
                >
                  <IconButton
                    component="span"
                    size="small"
                    sx={{
                      bgcolor: 'var(--primary)',
                      color: 'white',
                      '&:hover': { bgcolor: 'var(--primary-hover)' },
                      width: 32,
                      height: 32,
                    }}
                  >
                    <FiCamera size={16} />
                  </IconButton>
                  <input
                    id="employee-photo-upload"
                    type="file"
                    accept="image/jpeg,image/png,image/gif,image/webp"
                    style={{ display: 'none' }}
                    onChange={handlePhotoChange}
                  />
                </label>
                {photoPreview && (
                  <IconButton
                    size="small"
                    onClick={handleRemovePhoto}
                    sx={{
                      position: 'absolute',
                      top: -8,
                      right: -8,
                      bgcolor: 'var(--error)',
                      color: 'white',
                      '&:hover': { bgcolor: 'var(--error-dark)' },
                      width: 24,
                      height: 24,
                    }}
                  >
                    <FiX size={14} />
                  </IconButton>
                )}
              </Box>
            )}
          </Box>
          <Box>
            <Typography variant="subtitle1" fontWeight={600}>
              {isViewMode ? 'Photo' : 'Update Photo'}
            </Typography>
            {!isViewMode && (
              <Typography variant="caption" color="text.secondary">
                Click the camera icon to upload or change photo. Max 5MB.
              </Typography>
            )}
          </Box>
        </Box>
      )}

      <FormSection
        title="Basic Information"
        description="Employee code and full name"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Input
              label="Employee Code"
              {...register('employeeCode')}
              value={formData.employeeCode || ''}
              onChange={(e) => setFormField('employeeCode')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'employeeCode')}
              disabled={isViewMode}
              required
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input
              label="Full Name"
              {...register('fullName')}
              value={formData.fullName || ''}
              onChange={(e) => setFormField('fullName')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'fullName')}
              disabled={isViewMode}
              required
            />
          </Grid>
        </Grid>
      </FormSection>

      <FormSection
        title="Contact Information"
        description="Address, phone, and email"
      >
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Input
              label="Address"
              {...register('address')}
              value={formData.address || ''}
              onChange={(e) => setFormField('address')(e.target.value)}
              multiline
              rows={2}
              {...getFieldErrorProps(formErrors, 'address')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input
              label="Phone Number"
              {...register('phoneNumber')}
              value={formData.phoneNumber || ''}
              onChange={(e) => setFormField('phoneNumber')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'phoneNumber')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input
              label="Email"
              type="email"
              {...register('email')}
              value={formData.email || ''}
              onChange={(e) => setFormField('email')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'email')}
              disabled={isViewMode}
            />
          </Grid>
        </Grid>
      </FormSection>

      <FormSection
        title="Employment Details"
        description="Department, position, status, and office"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Select
              label="Department"
              {...register('departmentId')}
              value={formData.departmentId || ''}
              onChange={(e) => setFormField('departmentId')(e.target.value)}
              options={departmentOptions}
              {...getFieldErrorProps(formErrors, 'departmentId')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select
              label="Position"
              {...register('position')}
              value={formData.position || ''}
              onChange={(e) => setFormField('position')(e.target.value)}
              options={positionOptions}
              {...getFieldErrorProps(formErrors, 'position')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select
              label="Employment Status"
              {...register('employmentStatus')}
              value={formData.employmentStatus || ''}
              onChange={(e) =>
                setFormField('employmentStatus')(e.target.value)
              }
              options={statusOptions}
              {...getFieldErrorProps(formErrors, 'employmentStatus')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select
              label="Office"
              {...register('officeId')}
              value={formData.officeId || ''}
              onChange={(e) => setFormField('officeId')(e.target.value)}
              options={officeOptions}
              {...getFieldErrorProps(formErrors, 'officeId')}
              disabled={isViewMode}
            />
          </Grid>
        </Grid>
      </FormSection>

      <FormSection title="Dates" description="Join and resignation dates">
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <DatePickerInput
              label="Join Date"
              {...register('joinDate')}
              value={formData.joinDate || ''}
              onChange={(e) => setFormField('joinDate')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'joinDate')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <DatePickerInput
              label="Resign Date"
              {...register('resignDate')}
              value={formData.resignDate || ''}
              onChange={(e) => setFormField('resignDate')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'resignDate')}
              disabled={isViewMode}
            />
          </Grid>
        </Grid>
      </FormSection>

      <FormSection title="Attachments" description="Supporting documents">
        <Grid container spacing={2}>
          {showFileUploads && (
            <Grid item xs={12}>
              <FileUploader
                referenceType="Employee"
                referenceId={currentEmployeeId}
                entityType="employee"
                isAttachment={true}
                existingFiles={[...existingAttachments, ...attachmentFiles]}
                onFilesChange={handleFilesChange}
                onUploadComplete={handleUploadComplete}
                onFileDeleted={handleFileDeleted}
              />
            </Grid>
          )}

          {isViewMode && existingAttachments.length > 0 && (
            <Grid item xs={12}>
              <Typography variant="subtitle2" sx={{ mb: 1 }}>
                Attachments ({existingAttachments.length})
              </Typography>
              {existingAttachments.map((att, idx) => (
                <div key={att.fileAttachmentId || idx} style={{ padding: '4px 0' }}>
                  <a
                    href={att.fileUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    {att.originalFileName || att.fileName || 'File'}
                  </a>
                </div>
              ))}
            </Grid>
          )}
        </Grid>
      </FormSection>
    </CrudModal>
  );
};

export default EmployeeFormDialog;