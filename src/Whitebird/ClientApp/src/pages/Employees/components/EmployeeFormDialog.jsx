// src/pages/Employees/components/EmployeeFormDialog.jsx
import React from 'react';
import {
  Grid,
  Box,
  Avatar,
  Typography,
  IconButton,
  CircularProgress,
} from '@mui/material';
import { FiCamera, FiX, FiTrash2 } from 'react-icons/fi';
import CrudModal from '../../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../../components/atoms/FormSection/FormSection';
import Input from '../../../components/atoms/Input/Input';
import Select from '../../../components/atoms/Select/Select';
import DatePickerInput from '../../../components/atoms/Input/DatePickerInput';
import FileUploader from '../../../components/molecules/FileUploader/FileUploader';
import AttachmentList from '../../../components/molecules/AttachmentList';
import { getFieldErrorProps } from '../../../core/utils/validation.utils';
import { useFileBlob } from '../../../hooks/useFileBlob';

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
  onDeletePhoto,
  deletingPhoto,
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

  // formData.photoProfileUrl is the raw backend path
  // ("/api/Employee/5/photo") or "". useFileBlob performs the authenticated
  // fetch and returns a renderable blob URL (or null while loading / when
  // no photo / on fetch error). photoPreview (a data URL from a just-staged
  // File) takes precedence when the user has picked a new image but not saved.
  const { blobUrl: fetchedPhotoUrl } = useFileBlob(formData.photoProfileUrl);

  const effectivePhotoSrc = photoPreview || fetchedPhotoUrl || null;
  const showAvatarImage = Boolean(effectivePhotoSrc);

  // Delete button visible when a *backend* photo exists and no staged file
  // is overriding it. When a staged file is present (photoPreview), the X
  // button is shown instead — clicking X reverts to the backend photo.
  const canDeleteBackendPhoto =
    showAvatarImage && !photoPreview && !isViewMode && Boolean(onDeletePhoto);

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
              src={showAvatarImage ? effectivePhotoSrc : undefined}
              sx={{
                width: 100,
                height: 100,
                bgcolor: 'var(--primary)',
                fontSize: 40,
                fontWeight: 600,
                border: '3px solid var(--border)',
              }}
            >
              {!showAvatarImage && (formData.fullName?.charAt(0) || 'E')}
            </Avatar>
            {!isViewMode && (
              <Box
                sx={{
                  position: 'absolute',
                  bottom: -4,
                  right: -4,
                  display: 'flex',
                  gap: 1,
                }}
              >
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
                {canDeleteBackendPhoto && (
                  <IconButton
                    size="small"
                    onClick={onDeletePhoto}
                    disabled={deletingPhoto}
                    aria-label="Delete profile photo"
                    title="Delete profile photo"
                    sx={{
                      bgcolor: 'var(--error)',
                      color: 'white',
                      '&:hover': { opacity: 0.9 },
                      '&.Mui-disabled': { bgcolor: 'var(--error)', opacity: 0.6 },
                      width: 32,
                      height: 32,
                    }}
                  >
                    {deletingPhoto ? (
                      <CircularProgress size={14} sx={{ color: 'white' }} />
                    ) : (
                      <FiTrash2 size={14} />
                    )}
                  </IconButton>
                )}
                {photoPreview && (
                  <IconButton
                    size="small"
                    onClick={handleRemovePhoto}
                    aria-label="Discard staged photo"
                    title="Discard staged photo"
                    sx={{
                      bgcolor: 'var(--error)',
                      color: 'white',
                      '&:hover': { bgcolor: 'var(--error-dark)' },
                      width: 32,
                      height: 32,
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
                Click the camera icon to upload or change photo. Click the trash
                icon to remove the current photo. Max 5MB.
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
              <AttachmentList attachments={existingAttachments} />
            </Grid>
          )}
        </Grid>
      </FormSection>
    </CrudModal>
  );
};

export default EmployeeFormDialog;