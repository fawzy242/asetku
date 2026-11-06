// src/pages/Assets/components/AssetFormDialog.jsx
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
import NumberInput from '../../../components/atoms/Input/NumberInput';
import FileUploader from '../../../components/molecules/FileUploader/FileUploader';
import AttachmentList from '../../../components/molecules/AttachmentList';
import { getFieldErrorProps } from '../../../core/utils/validation.utils';
import { useFileBlob } from '../../../hooks/useFileBlob';

const AssetFormDialog = ({
  isOpen,
  onClose,
  onSubmit,
  isSubmitting,
  isViewMode,
  isEditMode,
  editingAsset,
  currentAssetId,
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
  categoryFormOptions,
  supplierFormOptions,
  conditionPurchaseOptions,
}) => {
  const showPhotoSection = isEditMode && currentAssetId;
  const showFileUploads = isEditMode && currentAssetId && !isViewMode;

  const { blobUrl: fetchedPhotoUrl } = useFileBlob(formData.photoProfileUrl);

  const effectivePhotoSrc = photoPreview || fetchedPhotoUrl || null;
  const showAvatarImage = Boolean(effectivePhotoSrc);

  const canDeleteBackendPhoto =
    showAvatarImage && !photoPreview && !isViewMode && Boolean(onDeletePhoto);

  return (
    <CrudModal
      isOpen={isOpen}
      onClose={onClose}
      title={
        isViewMode ? 'View Asset' : editingAsset ? 'Edit Asset' : 'Add Asset'
      }
      onSubmit={onSubmit}
      isSubmitting={isSubmitting}
      submitText={isViewMode ? 'Close' : editingAsset ? 'Update' : 'Create'}
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
              {!showAvatarImage && (formData.assetName?.charAt(0) || 'A')}
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
                  htmlFor="asset-photo-upload"
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
                    id="asset-photo-upload"
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
        description="Asset code, name, and category"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Input
              label="Asset Code"
              {...register('assetCode')}
              value={formData.assetCode || ''}
              onChange={(e) => setFormField('assetCode')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'assetCode')}
              disabled={isViewMode}
              required
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input
              label="Asset Name"
              {...register('assetName')}
              value={formData.assetName || ''}
              onChange={(e) => setFormField('assetName')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'assetName')}
              disabled={isViewMode}
              required
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select
              label="Category"
              {...register('categoryId')}
              value={formData.categoryId || ''}
              onChange={(e) => setFormField('categoryId')(e.target.value)}
              options={categoryFormOptions}
              {...getFieldErrorProps(formErrors, 'categoryId')}
              disabled={isViewMode}
              required
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select
              label="Supplier"
              {...register('supplierId')}
              value={formData.supplierId || ''}
              onChange={(e) => setFormField('supplierId')(e.target.value)}
              options={supplierFormOptions}
              {...getFieldErrorProps(formErrors, 'supplierId')}
              disabled={isViewMode}
            />
          </Grid>
        </Grid>
      </FormSection>

      <FormSection
        title="Technical Details"
        description="Brand, model, serial numbers, and network information"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Input
              label="Brand"
              {...register('brand')}
              value={formData.brand || ''}
              onChange={(e) => setFormField('brand')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'brand')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input
              label="Model"
              {...register('model')}
              value={formData.model || ''}
              onChange={(e) => setFormField('model')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'model')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input
              label="Serial Number"
              {...register('serialNumber')}
              value={formData.serialNumber || ''}
              onChange={(e) => setFormField('serialNumber')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'serialNumber')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input
              label="IMEI"
              {...register('imei')}
              value={formData.imei || ''}
              onChange={(e) => setFormField('imei')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'imei')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input
              label="MAC Address"
              {...register('macAddress')}
              value={formData.macAddress || ''}
              onChange={(e) => setFormField('macAddress')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'macAddress')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input
              label="Hostname"
              {...register('hostname')}
              value={formData.hostname || ''}
              onChange={(e) => setFormField('hostname')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'hostname')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input
              label="IP Address"
              {...register('ipAddress')}
              value={formData.ipAddress || ''}
              onChange={(e) => setFormField('ipAddress')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'ipAddress')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input
              label="Invoice Number"
              {...register('invoiceNumber')}
              value={formData.invoiceNumber || ''}
              onChange={(e) => setFormField('invoiceNumber')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'invoiceNumber')}
              disabled={isViewMode}
            />
          </Grid>
        </Grid>
      </FormSection>

      <FormSection
        title="Purchase Information"
        description="Purchase date, price, warranty, and condition"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={4}>
            <DatePickerInput
              label="Purchase Date"
              {...register('purchaseDate')}
              value={formData.purchaseDate || ''}
              onChange={(e) => setFormField('purchaseDate')(e.target.value)}
              {...getFieldErrorProps(formErrors, 'purchaseDate')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <NumberInput
              label="Purchase Price"
              {...register('purchasePrice')}
              value={formData.purchasePrice}
              onChange={(e) => setFormField('purchasePrice')(e.target.value)}
              prefix="Rp "
              thousandSeparator
              decimalScale={0}
              {...getFieldErrorProps(formErrors, 'purchasePrice')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <NumberInput
              label="Warranty (Months)"
              {...register('warrantyPeriod')}
              value={formData.warrantyPeriod}
              onChange={(e) => setFormField('warrantyPeriod')(e.target.value)}
              suffix=" months"
              decimalScale={0}
              min={0}
              {...getFieldErrorProps(formErrors, 'warrantyPeriod')}
              disabled={isViewMode}
            />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Select
              label="Purchase Condition"
              {...register('assetConditionPurchase')}
              value={formData.assetConditionPurchase || ''}
              onChange={(e) =>
                setFormField('assetConditionPurchase')(e.target.value)
              }
              options={conditionPurchaseOptions}
              {...getFieldErrorProps(formErrors, 'assetConditionPurchase')}
              disabled={isViewMode}
            />
          </Grid>
        </Grid>
      </FormSection>

      <FormSection
        title="Notes & Attachments"
        description="Additional information and file attachments"
      >
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Input
              label="Notes"
              {...register('notes')}
              value={formData.notes || ''}
              onChange={(e) => setFormField('notes')(e.target.value)}
              multiline
              rows={3}
              {...getFieldErrorProps(formErrors, 'notes')}
              disabled={isViewMode}
            />
          </Grid>

          {showFileUploads && (
            <Grid item xs={12}>
              <FileUploader
                referenceType="Asset"
                referenceId={currentAssetId}
                entityType="asset"
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

export default AssetFormDialog;