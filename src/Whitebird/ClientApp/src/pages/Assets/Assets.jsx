// src/pages/Assets/Assets.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Grid, Box, Avatar, Typography, IconButton } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { FiCamera, FiX, FiUpload, FiCheckSquare } from 'react-icons/fi';
import { AssetsData } from '../../core/services';
import { assetSchema, getAssetDefaultValues } from '../../core/schemas/assetSchema';
import { getFieldErrorProps } from '../../core/utils/validation.utils';
import GridView from '../../components/organisms/GridView/GridView';
import CrudModal from '../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../components/atoms/FormSection/FormSection';
import Input from '../../components/atoms/Input/Input';
import Select from '../../components/atoms/Select/Select';
import DatePickerInput from '../../components/atoms/Input/DatePickerInput';
import NumberInput from '../../components/atoms/Input/NumberInput';
import Spinner from '../../components/atoms/Spinner/Spinner';
import ImportModal from '../../components/molecules/ImportModal/ImportModal';
import BulkActivateModal from '../../components/molecules/BulkActivateModal/BulkActivateModal';
import Button from '../../components/atoms/Button/Button';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import FileUploader from '../../components/molecules/FileUploader/FileUploader';
import { ASSET_TABS } from '../../core/constants/assetStatuses';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useReferenceData } from '../../hooks/useReferenceData';
import { useOptions } from '../../hooks/useOptions';
import { getAssetColumns } from '../../core/constants/columns';
import './Assets.scss';

const assetsData = new AssetsData();

const Assets = () => {
  const [activeTab, setActiveTab] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [showImportModal, setShowImportModal] = useState(false);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [editingAsset, setEditingAsset] = useState(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [currentAssetId, setCurrentAssetId] = useState(null);
  
  const [filterCategoryId, setFilterCategoryId] = useState('');
  const [filterSupplierId, setFilterSupplierId] = useState('');
  
  const [photoFile, setPhotoFile] = useState(null);
  const [photoPreview, setPhotoPreview] = useState(null);
  const [attachmentFiles, setAttachmentFiles] = useState([]);
  const [existingAttachments, setExistingAttachments] = useState([]);

  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirm } = useSweetAlert();

  const { categories, suppliers, assetConditionPurchases } = useReferenceData();

  // ============================================================
  // REACT HOOK FORM WITH ZOD
  // ============================================================
  const {
    register,
    handleSubmit: handleFormSubmit,
    formState: { errors: formErrors, isSubmitting },
    setValue,
    reset,
    watch,
  } = useForm({
    resolver: zodResolver(assetSchema),
    defaultValues: getAssetDefaultValues(),
  });

  const formData = watch();

  const setFormField = useCallback((field) => (value) => {
    setValue(field, value, { shouldValidate: true });
  }, [setValue]);

  // ============================================================
  // CRUD OPERATIONS
  // ============================================================
  const handleCreate = useCallback(() => {
    reset(getAssetDefaultValues());
    setIsEditMode(false);
    setCurrentAssetId(null);
    setAttachmentFiles([]);
    setExistingAttachments([]);
    setPhotoFile(null);
    setPhotoPreview(null);
    setEditingAsset(null);
    setShowModal(true);
  }, [reset]);

  const handleEdit = useCallback(async (record) => {
    const recordId = record?.assetId;
    if (!recordId) return;
    
    setIsEditMode(true);
    setCurrentAssetId(recordId);
    setPhotoFile(null);
    setPhotoPreview(null);
    setEditingAsset(record);
    
    try {
      const result = await assetsData.fetchById(recordId);
      if (result.success && result.data) {
        const data = result.data;
        const defaults = getAssetDefaultValues();
        Object.keys(defaults).forEach(key => {
          if (data[key] !== undefined && data[key] !== null) {
            setValue(key, data[key]);
          } else {
            setValue(key, '');
          }
        });
        
        if (data.attachments && Array.isArray(data.attachments)) {
          setExistingAttachments(data.attachments);
        } else {
          setExistingAttachments([]);
        }
        setShowModal(true);
      } else {
        setExistingAttachments([]);
        toast.error('Failed to load asset data');
      }
    } catch (error) {
      console.warn('Failed to fetch asset:', error);
      setExistingAttachments([]);
      toast.error('Failed to load asset data');
    }
  }, [setValue, toast]);

  const handleClose = useCallback(() => {
    if (!isSubmitting) {
      setShowModal(false);
      setIsEditMode(false);
      setCurrentAssetId(null);
      setEditingAsset(null);
      reset(getAssetDefaultValues());
      setPhotoFile(null);
      setPhotoPreview(null);
      setAttachmentFiles([]);
      setExistingAttachments([]);
    }
  }, [isSubmitting, reset]);

  const onSubmit = useCallback(async (data) => {
    if (isSubmitting) return;

    const submitData = {
      ...data,
      photoFile: photoFile,
      attachmentFiles: attachmentFiles,
      attachmentIdsToKeep: isEditMode ? existingAttachments.map(a => a.fileAttachmentId) : [],
      replaceAttachments: isEditMode ? true : false,
    };

    try {
      const result = isEditMode && editingAsset
        ? await assetsData.update(editingAsset.assetId, submitData)
        : await assetsData.create(submitData);
      
      if (result.success) {
        setPhotoFile(null);
        setPhotoPreview(null);
        setAttachmentFiles([]);
        setExistingAttachments([]);
        setIsEditMode(false);
        setCurrentAssetId(null);
        setEditingAsset(null);
        setShowModal(false);
        reset(getAssetDefaultValues());
        
        queryClient.invalidateQueries({ queryKey: ['assets'] });
        queryClient.invalidateQueries({ queryKey: ['reference', 'assets'] });
        reload();
        clearSelection();
        
        const message = isEditMode && editingAsset 
          ? 'Asset updated successfully' 
          : 'Asset created successfully';
        toast.success(message);
      } else {
        const errorMsg = result.error || result.errors?.join('\n') || 'Failed to save asset';
        toast.error(errorMsg);
      }
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to save asset';
      toast.error(errorMsg);
    }
  }, [isSubmitting, isEditMode, editingAsset, photoFile, attachmentFiles, existingAttachments, queryClient, reload, clearSelection, toast, reset]);

  // ============================================================
  // BULK SELECTION
  // ============================================================
  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } =
    useBulkSelection({ idField: 'assetId' });

  const showCheckbox = useMemo(() => {
    return activeTab === 'active' || activeTab === 'inactive';
  }, [activeTab]);

  // ============================================================
  // OPTIONS
  // ============================================================
  const categoryFilterOptions = useOptions(categories, 'All Categories');
  const supplierFilterOptions = useOptions(suppliers, 'All Suppliers');
  const categoryFormOptions = useOptions(categories, 'Select Category');
  const supplierFormOptions = useOptions(suppliers, 'Select Supplier');
  const conditionPurchaseOptions = useOptions(assetConditionPurchases, 'Select Purchase Condition');

  // ============================================================
  // PHOTO HANDLING
  // ============================================================
  const handlePhotoChange = useCallback((e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    
    if (!file.type.startsWith('image/')) {
      toast.error('Please select an image file');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      toast.error('Image size must be less than 5MB');
      return;
    }
    
    setPhotoFile(file);
    const reader = new FileReader();
    reader.onload = (event) => {
      setPhotoPreview(event.target.result);
    };
    reader.readAsDataURL(file);
    e.target.value = '';
  }, [toast]);

  const handleRemovePhoto = useCallback(() => {
    setPhotoFile(null);
    setPhotoPreview(null);
  }, []);

  // ============================================================
  // BUILD FILTERS
  // ============================================================
  const buildFilters = useCallback(() => {
    const filters = {};
    
    switch (activeTab) {
      case 'all':
        break;
      case 'active':
        filters.isActive = true;
        break;
      case 'inactive':
        filters.isActive = false;
        break;
      case 'available':
        filters.status = 'Available';
        break;
      case 'assigned':
        filters.status = 'Assigned';
        break;
      case 'onloan':
        filters.status = 'On Loan';
        break;
      case 'inmaintenance':
        filters.status = 'In Maintenance';
        break;
      case 'damaged':
        filters.status = 'Damaged';
        break;
      default:
        break;
    }
    
    if (searchTerm) filters.search = searchTerm;
    if (filterCategoryId) filters.categoryId = parseInt(filterCategoryId, 10);
    if (filterSupplierId) filters.supplierId = parseInt(filterSupplierId, 10);
    
    return filters;
  }, [activeTab, searchTerm, filterCategoryId, filterSupplierId]);

  // ============================================================
  // FETCH GRID DATA
  // ============================================================
  const fetchGridData = useCallback(
    async (params) => {
      const filters = buildFilters();
      const requestParams = {
        page: params.page || 1,
        pageSize: params.pageSize || 10,
        ...filters,
        ...params,
      };
      const result = await assetsData.fetchGridData(requestParams);
      return result;
    },
    [buildFilters]
  );

  const {
    data: rawAssets,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    reload,
  } = useGridData(['assets', activeTab, searchTerm, filterCategoryId, filterSupplierId], fetchGridData);

  // ============================================================
  // EFFECTS
  // ============================================================
  useEffect(() => {
    isMountedRef.current = true;
    setPage(1);
    clearSelection();
    reload();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, setPage, clearSelection, filterCategoryId, filterSupplierId]);

  // ============================================================
  // DATA TRANSFORM
  // ============================================================
  const assets = useMemo(() => {
    return (rawAssets || []).map(asset => ({
      ...asset,
      id: asset.assetId,
      displayStatus: asset.currentStatus || asset.status || 'Available',
    }));
  }, [rawAssets]);

  // ============================================================
  // HANDLERS
  // ============================================================
  const handleSearch = useCallback(
    (search) => {
      setSearchTerm(search);
      setPage(1);
      clearSelection();
    },
    [setPage, clearSelection]
  );

  const handleDelete = useCallback(
    async (asset) => {
      const confirmed = await confirm({
        title: 'Delete Asset',
        text: `Are you sure you want to delete "${asset.assetName}"?`,
        icon: 'warning',
        confirmButtonText: 'Yes, Delete',
        confirmButtonColor: '#ef4444',
      });
      if (!confirmed) return;
      const r = await assetsData.delete(asset.assetId);
      if (r.success && isMountedRef.current) {
        toast.success('Asset deleted successfully');
        queryClient.invalidateQueries({ queryKey: ['assets'] });
        reload();
        clearSelection();
      }
    },
    [reload, queryClient, confirm, clearSelection, toast]
  );

  const handleTabChange = useCallback(
    (tab) => {
      setActiveTab(tab);
      setPage(1);
      setSearchTerm('');
      clearSelection();
    },
    [setPage, clearSelection]
  );

  const handleBulkActivate = useCallback(
    async (ids, activate) => {
      const actionText = activate ? 'activate' : 'deactivate';
      const confirmed = await confirm({
        title: activate ? 'Activate Assets' : 'Deactivate Assets',
        text: `Are you sure you want to ${actionText} ${ids.length} asset(s)?`,
        confirmButtonText: activate ? 'Yes, Activate' : 'Yes, Deactivate',
      });
      if (!confirmed) return;
      const r = await assetsData.bulkActivate(ids, activate);
      if (r.success && isMountedRef.current) {
        const count = r.data || ids.length;
        toast.success(`${count} asset(s) ${actionText}d successfully`);
        clearSelection();
        queryClient.invalidateQueries({ queryKey: ['assets'] });
        reload();
      }
    },
    [reload, queryClient, confirm, clearSelection, toast]
  );

  const handleImport = useCallback(
    async (file) => {
      setIsImporting(true);
      const r = await assetsData.importAssets(file);
      setIsImporting(false);
      if (r.success && isMountedRef.current) {
        setImportResult(r.data);
        if (r.data?.errorCount > 0) {
          toast.info(`Import completed: ${r.data.successCount} success, ${r.data.errorCount} errors`);
        } else {
          toast.success(`Import completed: ${r.data?.successCount || 0} assets imported`);
        }
        queryClient.invalidateQueries({ queryKey: ['assets'] });
        reload();
        clearSelection();
      }
    },
    [reload, queryClient, clearSelection, toast]
  );

  const handleDownloadTemplate = useCallback(async () => {
    const result = await assetsData.downloadTemplate();
    if (!result.success) {
      toast.error(result.error || 'Failed to download template');
    }
  }, [toast]);

  const handleFilesChange = useCallback((files) => {
    setAttachmentFiles(files.filter(f => f.isNew));
    const existing = files.filter(f => !f.isNew);
    if (existing.length > 0) {
      setExistingAttachments(existing);
    }
  }, []);

  const handleUploadComplete = useCallback((newFiles) => {
    console.log('Files added, ready for save:', newFiles);
  }, []);

  const handleFileDeleted = useCallback(() => {
    const fetchAttachments = async () => {
      try {
        const result = await assetsData.fetchById(currentAssetId);
        if (result.success && result.data) {
          const data = result.data;
          if (data.attachments && Array.isArray(data.attachments)) {
            setExistingAttachments(data.attachments);
          }
        }
      } catch (error) {
        console.warn('Failed to refresh attachments:', error);
      }
    };
    fetchAttachments();
  }, [currentAssetId]);

  // ============================================================
  // GRID ACTIONS
  // ============================================================
  const handleGridAction = useCallback(
    (actionType, row) => {
      if (actionType === ACTION_TYPES.EDIT) {
        setAttachmentFiles([]);
        setExistingAttachments([]);
        setPhotoFile(null);
        setPhotoPreview(null);
        handleEdit(row);
      } else if (actionType === ACTION_TYPES.DELETE) {
        handleDelete(row);
      }
    },
    [handleEdit, handleDelete]
  );

  const getConditionalActions = useCallback((row) => {
    const actions = [];
    if (row.canEdit !== false) actions.push(ACTION_TYPES.EDIT);
    if (row.canDelete !== false) actions.push(ACTION_TYPES.DELETE);
    return actions;
  }, []);

  const { actionColumn } = useGridActions({
    actions: [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleGridAction,
    getConditionalActions,
    rowIdField: 'assetId',
  });

  // ============================================================
  // COLUMNS
  // ============================================================
  const columns = useMemo(() => {
    return getAssetColumns(actionColumn);
  }, [actionColumn]);

  // ============================================================
  // RENDER HELPERS
  // ============================================================
  const extraActions = (
    <>
      <Button
        variant="outline"
        size="sm"
        onClick={() => setShowImportModal(true)}
        className="u-inline-flex u-btn-gap"
      >
        <FiUpload size={16} /> Import
      </Button>
      {hasSelection && showCheckbox && (
        <Button
          variant="primary"
          size="sm"
          onClick={() => setShowBulkActivateModal(true)}
          className="u-inline-flex u-btn-gap"
        >
          <FiCheckSquare size={16} /> {activeTab === 'active' ? 'Deactivate' : 'Activate'} ({selectionCount})
        </Button>
      )}
    </>
  );

  if (loading && !assets.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === 'active' ? false : true;
  const bulkTitle = bulkActivateValue ? 'Activate Assets' : 'Deactivate Assets';
  const bulkDescription = `This action will ${bulkActivateValue ? 'activate' : 'deactivate'} the selected assets.`;

  const showFileUploads = isEditMode && currentAssetId;

  return (
    <div className="assets-menu">
      <GridView
        title="Asset Management"
        tabs={ASSET_TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={assets}
        loading={loading}
        totalCount={totalCount}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onSearch={handleSearch}
        showCheckbox={showCheckbox}
        selectedRows={selectedRowIds}
        onSelectionChange={handleSelectionChange}
        createButtonText="Add Asset"
        ariaLabel="Assets data table"
        extraActions={extraActions}
        filterComponent={
          <Box className="assets-menu__filters">
            <Select
              label="Category"
              value={filterCategoryId}
              onChange={(e) => setFilterCategoryId(e.target.value)}
              options={categoryFilterOptions}
              size="small"
              className="assets-menu__filter-item"
            />
            <Select
              label="Supplier"
              value={filterSupplierId}
              onChange={(e) => setFilterSupplierId(e.target.value)}
              options={supplierFilterOptions}
              size="small"
              className="assets-menu__filter-item"
            />
            {(filterCategoryId || filterSupplierId) && (
              <Button
                variant="text"
                size="small"
                onClick={() => {
                  setFilterCategoryId('');
                  setFilterSupplierId('');
                  setPage(1);
                }}
                className="assets-menu__filter-clear"
              >
                Clear
              </Button>
            )}
          </Box>
        }
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingAsset ? 'Edit Asset' : 'Add Asset'}
        onSubmit={handleFormSubmit(onSubmit)}
        isSubmitting={isSubmitting}
        submitText={editingAsset ? 'Update' : 'Create'}
        size="xl"
      >
        {showFileUploads && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, mb: 3, pb: 3, borderBottom: '1px solid var(--border)' }}>
            <Box sx={{ position: 'relative' }}>
              <Avatar
                src={photoPreview || (editingAsset?.photoProfileUrl) || undefined}
                sx={{
                  width: 100,
                  height: 100,
                  bgcolor: 'var(--primary)',
                  fontSize: 40,
                  fontWeight: 600,
                  border: '3px solid var(--border)',
                }}
              >
                {!photoPreview && !editingAsset?.photoProfileUrl && (formData.assetName?.charAt(0) || 'A')}
              </Avatar>
              <Box sx={{ position: 'absolute', bottom: -4, right: -4 }}>
                <label htmlFor="asset-photo-upload" style={{ cursor: 'pointer' }}>
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
            </Box>
            <Box>
              <Typography variant="subtitle1" fontWeight={600}>
                Update Photo
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Click the camera icon to upload or change photo. Max 5MB.
              </Typography>
            </Box>
          </Box>
        )}

        <FormSection title="Basic Information" description="Asset code, name, and category">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input
                label="Asset Code"
                {...register('assetCode')}
                value={formData.assetCode || ''}
                onChange={(e) => setFormField('assetCode')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'assetCode')}
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
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Technical Details" description="Brand, model, serial numbers, and network information">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input
                label="Brand"
                {...register('brand')}
                value={formData.brand || ''}
                onChange={(e) => setFormField('brand')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'brand')}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Model"
                {...register('model')}
                value={formData.model || ''}
                onChange={(e) => setFormField('model')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'model')}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input
                label="Serial Number"
                {...register('serialNumber')}
                value={formData.serialNumber || ''}
                onChange={(e) => setFormField('serialNumber')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'serialNumber')}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input
                label="IMEI"
                {...register('imei')}
                value={formData.imei || ''}
                onChange={(e) => setFormField('imei')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'imei')}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input
                label="MAC Address"
                {...register('macAddress')}
                value={formData.macAddress || ''}
                onChange={(e) => setFormField('macAddress')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'macAddress')}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input
                label="Hostname"
                {...register('hostname')}
                value={formData.hostname || ''}
                onChange={(e) => setFormField('hostname')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'hostname')}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input
                label="IP Address"
                {...register('ipAddress')}
                value={formData.ipAddress || ''}
                onChange={(e) => setFormField('ipAddress')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'ipAddress')}
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input
                label="Invoice Number"
                {...register('invoiceNumber')}
                value={formData.invoiceNumber || ''}
                onChange={(e) => setFormField('invoiceNumber')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'invoiceNumber')}
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Purchase Information" description="Purchase date, price, warranty, and condition">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={4}>
              <DatePickerInput
                label="Purchase Date"
                {...register('purchaseDate')}
                value={formData.purchaseDate || ''}
                onChange={(e) => setFormField('purchaseDate')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'purchaseDate')}
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
              />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Select
                label="Purchase Condition"
                {...register('assetConditionPurchase')}
                value={formData.assetConditionPurchase || ''}
                onChange={(e) => setFormField('assetConditionPurchase')(e.target.value)}
                options={conditionPurchaseOptions}
                {...getFieldErrorProps(formErrors, 'assetConditionPurchase')}
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Notes & Attachments" description="Additional information and file attachments">
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
          </Grid>
        </FormSection>
      </CrudModal>

      <ImportModal
        isOpen={showImportModal}
        onClose={() => {
          setShowImportModal(false);
          setImportResult(null);
        }}
        onImport={handleImport}
        onDownloadTemplate={handleDownloadTemplate}
        isImporting={isImporting}
        importResult={importResult}
        title="Import Assets"
        description="Upload Excel file with asset data. Assets will be imported as ACTIVE."
      />

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={getSelectedIds(assets)}
        itemName="assets"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default Assets;