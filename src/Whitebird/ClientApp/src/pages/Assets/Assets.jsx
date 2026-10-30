// src/pages/Assets/Assets.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Box } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import {
  FiUpload,
  FiCheckSquare,
  FiPlus,
  FiTrash2,
} from 'react-icons/fi';
import { AssetsData } from '../../core/services';
import { assetSchema, getAssetDefaultValues } from './schema';
import PageHeader from '../../components/atoms/PageHeader';
import BulkActionBar from '../../components/atoms/BulkActionBar';
import RefreshButton from '../../components/atoms/RefreshButton';
import Button from '../../components/atoms/Button/Button';
import Select from '../../components/atoms/Select/Select';
import Tabs from '../../components/molecules/Tabs/Tabs';
import SearchToolbar from '../../components/molecules/SearchToolbar/SearchToolbar';
import DataTable from '../../components/molecules/DataTable/DataTable';
import ImportModal from '../../components/molecules/ImportModal/ImportModal';
import BulkActivateModal from '../../components/molecules/BulkActivateModal/BulkActivateModal';
import ActionMenu from '../../components/molecules/ActionMenu';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useReferenceData } from '../../hooks/useReferenceData';
import { useOptions } from '../../hooks/useOptions';
import { getAssetColumns } from './columns';
import AssetFormDialog from './components/AssetFormDialog';
import './Assets.scss';

const assetsData = new AssetsData();

// ============================================================
// TABS
// ============================================================
const STATUS_TABS = [
  { id: 'available', label: 'Available' },
  { id: 'assigned', label: 'Assigned' },
  { id: 'onloan', label: 'On Loan' },
  { id: 'inmaintenance', label: 'In Maintenance' },
  { id: 'damaged', label: 'Damaged' },
];

const CRUD_TABS = [
  { id: 'all', label: 'All' },
  { id: 'active', label: 'Active' },
  { id: 'inactive', label: 'Inactive' },
];

// Status tab id -> backend status string (unchanged from existing mapping)
const STATUS_TO_BACKEND = {
  available: 'Available',
  assigned: 'Assigned',
  onloan: 'On Loan',
  inmaintenance: 'In Maintenance',
  damaged: 'Damaged',
};

const Assets = () => {
  // ============================================================
  // SHARED MODAL STATE (used by both grids)
  // ============================================================
  const [showModal, setShowModal] = useState(false);
  const [editingAsset, setEditingAsset] = useState(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [isViewMode, setIsViewMode] = useState(false);
  const [currentAssetId, setCurrentAssetId] = useState(null);
  const [showImportModal, setShowImportModal] = useState(false);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);

  const [photoFile, setPhotoFile] = useState(null);
  const [photoPreview, setPhotoPreview] = useState(null);
  const [attachmentFiles, setAttachmentFiles] = useState([]);
  const [existingAttachments, setExistingAttachments] = useState([]);

  // ============================================================
  // STATUS GRID STATE
  // ============================================================
  const [statusTab, setStatusTab] = useState('available');
  const [statusSearch, setStatusSearch] = useState('');

  // ============================================================
  // CRUD GRID STATE
  // ============================================================
  const [crudTab, setCrudTab] = useState('all');
  const [crudSearch, setCrudSearch] = useState('');
  const [filterCategoryId, setFilterCategoryId] = useState('');
  const [filterSupplierId, setFilterSupplierId] = useState('');

  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirm } = useSweetAlert();

  const { categories, suppliers, assetConditionPurchases } = useReferenceData();

  const {
    selectedRowIds,
    selectionCount,
    hasSelection,
    handleSelectionChange,
    clearSelection,
    getSelectedIds,
  } = useBulkSelection({ idField: 'assetId' });

  // ============================================================
  // FORM (shared Add/Edit modal)
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

  const setFormField = useCallback(
    (field) => (value) => {
      setValue(field, value, { shouldValidate: true });
    },
    [setValue]
  );

  // ============================================================
  // STATUS GRID — DATA
  // ============================================================
  const buildStatusFilters = useCallback(() => {
    const filters = {};
    const status = STATUS_TO_BACKEND[statusTab];
    if (status) filters.status = status;
    if (statusSearch) filters.search = statusSearch;
    return filters;
  }, [statusTab, statusSearch]);

  const fetchStatusGridData = useCallback(
    async (params) => {
      const filters = buildStatusFilters();
      const requestParams = {
        page: params.page || 1,
        pageSize: params.pageSize || 10,
        ...filters,
        ...params,
      };
      return await assetsData.fetchGridData(requestParams);
    },
    [buildStatusFilters]
  );

  const {
    data: rawStatusAssets,
    totalCount: statusTotalCount,
    loading: statusLoading,
    page: statusPage,
    setPage: setStatusPage,
    pageSize: statusPageSize,
    setPageSize: setStatusPageSize,
    sortModel: statusSortModel,
    setSortModel: setStatusSortModel,
    reload: reloadStatus,
  } = useGridData(
    ['assets-status', statusTab, statusSearch],
    fetchStatusGridData
  );

  // ============================================================
  // CRUD GRID — DATA
  // ============================================================
  const buildCrudFilters = useCallback(() => {
    const filters = {};
    if (crudTab === 'active') filters.isActive = true;
    else if (crudTab === 'inactive') filters.isActive = false;
    if (crudSearch) filters.search = crudSearch;
    if (filterCategoryId) filters.categoryId = parseInt(filterCategoryId, 10);
    if (filterSupplierId) filters.supplierId = parseInt(filterSupplierId, 10);
    return filters;
  }, [crudTab, crudSearch, filterCategoryId, filterSupplierId]);

  const fetchCrudGridData = useCallback(
    async (params) => {
      const filters = buildCrudFilters();
      const requestParams = {
        page: params.page || 1,
        pageSize: params.pageSize || 10,
        ...filters,
        ...params,
      };
      return await assetsData.fetchGridData(requestParams);
    },
    [buildCrudFilters]
  );

  const {
    data: rawCrudAssets,
    totalCount: crudTotalCount,
    loading: crudLoading,
    page: crudPage,
    setPage: setCrudPage,
    pageSize: crudPageSize,
    setPageSize: setCrudPageSize,
    sortModel: crudSortModel,
    setSortModel: setCrudSortModel,
    reload: reloadCrud,
  } = useGridData(
    ['assets-crud', crudTab, crudSearch, filterCategoryId, filterSupplierId],
    fetchCrudGridData
  );

  const reloadBoth = useCallback(() => {
    reloadStatus();
    reloadCrud();
  }, [reloadStatus, reloadCrud]);

  // ============================================================
  // EFFECTS
  // ============================================================
  useEffect(() => {
    isMountedRef.current = true;
    return () => {
      isMountedRef.current = false;
    };
  }, []);

  useEffect(() => {
    setStatusPage(1);
  }, [statusTab, statusSearch, setStatusPage]);

  useEffect(() => {
    setCrudPage(1);
    clearSelection();
  }, [
    crudTab,
    crudSearch,
    filterCategoryId,
    filterSupplierId,
    setCrudPage,
    clearSelection,
  ]);

  // ============================================================
  // DATA TRANSFORM
  // ============================================================
  const transformAsset = useCallback(
    (asset) => ({
      ...asset,
      id: asset.assetId,
      displayStatus: asset.currentStatus || asset.status || 'Available',
    }),
    []
  );

  const statusAssets = useMemo(
    () => (rawStatusAssets || []).map(transformAsset),
    [rawStatusAssets, transformAsset]
  );

  const crudAssets = useMemo(
    () => (rawCrudAssets || []).map(transformAsset),
    [rawCrudAssets, transformAsset]
  );

  // ============================================================
  // OPTIONS
  // ============================================================
  const categoryFilterOptions = useOptions(categories, 'All Categories');
  const supplierFilterOptions = useOptions(suppliers, 'All Suppliers');
  const categoryFormOptions = useOptions(categories, 'Select Category');
  const supplierFormOptions = useOptions(suppliers, 'Select Supplier');
  const conditionPurchaseOptions = useOptions(
    assetConditionPurchases,
    'Select Purchase Condition'
  );

  // ============================================================
  // PHOTO
  // ============================================================
  const handlePhotoChange = useCallback(
    (e) => {
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
    },
    [toast]
  );

  const handleRemovePhoto = useCallback(() => {
    setPhotoFile(null);
    setPhotoPreview(null);
  }, []);

  // ============================================================
  // SHARED CRUD OPERATIONS
  // ============================================================
  const handleCreate = useCallback(() => {
    reset(getAssetDefaultValues());
    setIsEditMode(false);
    setIsViewMode(false);
    setCurrentAssetId(null);
    setAttachmentFiles([]);
    setExistingAttachments([]);
    setPhotoFile(null);
    setPhotoPreview(null);
    setEditingAsset(null);
    setShowModal(true);
  }, [reset]);

  const handleEdit = useCallback(
    async (record) => {
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
          Object.keys(defaults).forEach((key) => {
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
    },
    [setValue, toast]
  );

  const handleView = useCallback(
    (row) => {
      setIsViewMode(true);
      handleEdit(row);
    },
    [handleEdit]
  );

  const handleClose = useCallback(() => {
    if (!isSubmitting) {
      setShowModal(false);
      setIsEditMode(false);
      setIsViewMode(false);
      setCurrentAssetId(null);
      setEditingAsset(null);
      reset(getAssetDefaultValues());
      setPhotoFile(null);
      setPhotoPreview(null);
      setAttachmentFiles([]);
      setExistingAttachments([]);
    }
  }, [isSubmitting, reset]);

  const onSubmit = useCallback(
    async (data) => {
      if (isSubmitting || isViewMode) return;

      const submitData = {
        ...data,
        photoFile,
        attachmentFiles,
        attachmentIdsToKeep: isEditMode
          ? existingAttachments.map((a) => a.fileAttachmentId)
          : [],
        replaceAttachments: isEditMode ? true : false,
      };

      try {
        let result;
        if (isEditMode && editingAsset) {
          result = await assetsData.update(editingAsset.assetId, submitData);
        } else {
          result = await assetsData.create(submitData);
        }

        if (result.success) {
          setPhotoFile(null);
          setPhotoPreview(null);
          setAttachmentFiles([]);
          setExistingAttachments([]);
          setIsEditMode(false);
          setIsViewMode(false);
          setCurrentAssetId(null);
          setEditingAsset(null);
          setShowModal(false);
          reset(getAssetDefaultValues());

          queryClient.invalidateQueries({ queryKey: ['assets'] });
          queryClient.invalidateQueries({ queryKey: ['reference', 'assets'] });
          reloadBoth();
          clearSelection();

          const message =
            isEditMode && editingAsset
              ? 'Asset updated successfully'
              : 'Asset created successfully';
          toast.success(message);
        } else {
          const errorMsg =
            result.error ||
            result.errors?.join('\n') ||
            'Failed to save asset';
          toast.error(errorMsg);
        }
      } catch (error) {
        const errorMsg =
          error.userMessage || error.message || 'Failed to save asset';
        toast.error(errorMsg);
      }
    },
    [
      isSubmitting,
      isViewMode,
      isEditMode,
      editingAsset,
      photoFile,
      attachmentFiles,
      existingAttachments,
      queryClient,
      reloadBoth,
      clearSelection,
      toast,
      reset,
    ]
  );

  // ============================================================
  // STATUS GRID HANDLERS
  // ============================================================
  const handleStatusSearch = useCallback((search) => {
    setStatusSearch(search);
  }, []);

  const handleStatusTabChange = useCallback((tab) => {
    setStatusTab(tab);
    setStatusSearch('');
  }, []);

  const handleStatusRefresh = useCallback(() => {
    reloadStatus();
  }, [reloadStatus]);

  // ============================================================
  // CRUD GRID HANDLERS
  // ============================================================
  const handleCrudSearch = useCallback(
    (search) => {
      setCrudSearch(search);
      setCrudPage(1);
      clearSelection();
    },
    [setCrudPage, clearSelection]
  );

  const handleCrudRefresh = useCallback(() => {
    reloadCrud();
    clearSelection();
  }, [reloadCrud, clearSelection]);

  const handleCrudTabChange = useCallback(
    (tab) => {
      setCrudTab(tab);
      setCrudSearch('');
      clearSelection();
    },
    [clearSelection]
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
        queryClient.invalidateQueries({ queryKey: ['reference', 'assets'] });
        reloadBoth();
        clearSelection();
      }
    },
    [reloadBoth, queryClient, confirm, clearSelection, toast]
  );

  const handleBulkDelete = useCallback(
    async (ids) => {
      if (!ids || ids.length === 0) return;
      const confirmed = await confirm({
        title: 'Delete Assets',
        text: `Are you sure you want to delete ${ids.length} asset(s)? This action cannot be undone.`,
        icon: 'warning',
        confirmButtonText: 'Yes, Delete',
        confirmButtonColor: '#ef4444',
      });
      if (!confirmed) return;

      const r = await assetsData.bulkDelete(ids);
      if (!isMountedRef.current) return;

      if (r.success) {
        toast.success(`${r.data.successCount} asset(s) deleted`);
      } else if (r.partial) {
        toast.warning(
          `${r.data.successCount} deleted, ${r.data.failCount} failed`
        );
      } else {
        toast.error(r.error || 'Failed to delete assets');
      }

      clearSelection();
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      queryClient.invalidateQueries({ queryKey: ['reference', 'assets'] });
      reloadBoth();
    },
    [reloadBoth, queryClient, toast, confirm, clearSelection]
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
        queryClient.invalidateQueries({ queryKey: ['reference', 'assets'] });
        reloadBoth();
      }
    },
    [reloadBoth, queryClient, confirm, clearSelection, toast]
  );

  const handleImport = useCallback(
    async (file) => {
      setIsImporting(true);
      const r = await assetsData.importAssets(file);
      setIsImporting(false);
      if (r.success && isMountedRef.current) {
        setImportResult(r.data);
        if (r.data?.errorCount > 0) {
          toast.info(
            `Import completed: ${r.data.successCount} success, ${r.data.errorCount} errors`
          );
        } else {
          toast.success(
            `Import completed: ${r.data?.successCount || 0} assets imported`
          );
        }
        queryClient.invalidateQueries({ queryKey: ['assets'] });
        reloadBoth();
        clearSelection();
      }
    },
    [reloadBoth, queryClient, clearSelection, toast]
  );

  const handleDownloadTemplate = useCallback(async () => {
    const result = await assetsData.downloadTemplate();
    if (!result.success) {
      toast.error(result.error || 'Failed to download template');
    }
  }, [toast]);

  const handleFilesChange = useCallback((files) => {
    setAttachmentFiles(files.filter((f) => f.isNew));
    const existing = files.filter((f) => !f.isNew);
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

  const handleGridAction = useCallback(
    (actionType, row) => {
      if (actionType === ACTION_TYPES.VIEW) {
        handleView(row);
      } else if (actionType === ACTION_TYPES.EDIT) {
        setIsViewMode(false);
        setAttachmentFiles([]);
        setExistingAttachments([]);
        setPhotoFile(null);
        setPhotoPreview(null);
        handleEdit(row);
      } else if (actionType === ACTION_TYPES.DELETE) {
        handleDelete(row);
      }
    },
    [handleView, handleEdit, handleDelete]
  );

  const getConditionalActions = useCallback((row) => {
    const actions = [];
    if (row.canEdit !== false) actions.push(ACTION_TYPES.EDIT);
    if (row.canDelete !== false) actions.push(ACTION_TYPES.DELETE);
    return actions;
  }, []);

  const {
    actionColumn,
    menuAnchorEl,
    menuOpen,
    menuActions,
    selectedRow,
    handleMenuClose,
    handleActionClick,
  } = useGridActions({
    actions: [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleGridAction,
    getConditionalActions,
    rowIdField: 'assetId',
  });

  const statusColumns = useMemo(() => {
    const cols = getAssetColumns(null);
    return cols.filter((c) => c && c.field !== 'actions');
  }, []);

  const crudColumns = useMemo(
    () => getAssetColumns(actionColumn),
    [actionColumn]
  );

  const bulkActivateValue = crudTab === 'active' ? false : true;
  const showActivateToggle = crudTab === 'active' || crudTab === 'inactive';

  const bulkActions = hasSelection
    ? [
        ...(showActivateToggle
          ? [
              {
                key: 'bulk-toggle',
                label: `${bulkActivateValue ? 'Activate' : 'Deactivate'} (${selectionCount})`,
                variant: 'primary',
                icon: <FiCheckSquare size={16} />,
                onClick: () => setShowBulkActivateModal(true),
              },
            ]
          : []),
        {
          key: 'bulk-delete',
          label: `Delete (${selectionCount})`,
          variant: 'danger',
          icon: <FiTrash2 size={16} />,
          onClick: () => handleBulkDelete(getSelectedIds(crudAssets)),
        },
      ]
    : [];

  const bulkTitle = bulkActivateValue
    ? 'Activate Assets'
    : 'Deactivate Assets';
  const bulkDescription = `This action will ${bulkActivateValue ? 'activate' : 'deactivate'} the selected assets.`;

  return (
    <div className="assets-menu">
      <PageHeader
        title="Asset Management"
        description="Manage inventory assets and their lifecycle"
      />

      <section className="assets-menu__section">
        <header className="assets-menu__section-header">
          <h2 className="assets-menu__section-title">Asset Status</h2>
          <div className="assets-menu__section-actions">
            <RefreshButton
              onClick={handleStatusRefresh}
              loading={statusLoading}
            />
          </div>
        </header>

        <Tabs
          tabs={STATUS_TABS}
          activeTab={statusTab}
          onTabChange={handleStatusTabChange}
        />

        <SearchToolbar
          onSearch={handleStatusSearch}
          placeholder="Search assets..."
        />

        <Box className="assets-menu__grid-box">
          <DataTable
            rows={statusAssets}
            columns={statusColumns}
            loading={statusLoading}
            pageSize={statusPageSize}
            page={statusPage}
            totalRowCount={statusTotalCount}
            onPageChange={setStatusPage}
            onPageSizeChange={setStatusPageSize}
            getRowId={(row) => row?.assetId || `row-${Math.random()}`}
            checkboxSelection={false}
            onRowClick={handleView}
            sortModel={statusSortModel}
            onSortModelChange={setStatusSortModel}
            paginationMode="server"
            sortingMode="server"
            ariaLabel="Asset status table"
          />
        </Box>
      </section>

      <section className="assets-menu__section">
        <header className="assets-menu__section-header">
          <h2 className="assets-menu__section-title">Asset Management</h2>
          <div className="assets-menu__section-actions">
            <RefreshButton onClick={handleCrudRefresh} loading={crudLoading} />
            <Button
              variant="outline"
              size="sm"
              onClick={() => setShowImportModal(true)}
              startIcon={<FiUpload size={16} />}
            >
              Import
            </Button>
            <BulkActionBar
              visible={bulkActions.length > 0}
              actions={bulkActions}
            />
            <Button
              variant="primary"
              size="sm"
              onClick={handleCreate}
              startIcon={<FiPlus size={16} />}
            >
              Add Asset
            </Button>
          </div>
        </header>

        <Tabs
          tabs={CRUD_TABS}
          activeTab={crudTab}
          onTabChange={handleCrudTabChange}
        />

        <SearchToolbar
          onSearch={handleCrudSearch}
          placeholder="Search assets..."
        />

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
              size="sm"
              onClick={() => {
                setFilterCategoryId('');
                setFilterSupplierId('');
              }}
              className="assets-menu__filter-clear"
            >
              Clear
            </Button>
          )}
        </Box>

        <Box className="assets-menu__grid-box">
          <DataTable
            rows={crudAssets}
            columns={crudColumns}
            loading={crudLoading}
            pageSize={crudPageSize}
            page={crudPage}
            totalRowCount={crudTotalCount}
            onPageChange={setCrudPage}
            onPageSizeChange={setCrudPageSize}
            getRowId={(row) => row?.assetId || `row-${Math.random()}`}
            checkboxSelection={true}
            onRowSelectionModelChange={handleSelectionChange}
            onRowClick={handleView}
            sortModel={crudSortModel}
            onSortModelChange={setCrudSortModel}
            paginationMode="server"
            sortingMode="server"
            ariaLabel="Assets data table"
          />
        </Box>
      </section>

      <ActionMenu
        anchorEl={menuAnchorEl}
        open={menuOpen}
        onClose={handleMenuClose}
        actions={menuActions}
        row={selectedRow}
        onAction={handleActionClick}
      />

      <AssetFormDialog
        isOpen={showModal}
        onClose={handleClose}
        onSubmit={isViewMode ? () => {} : handleFormSubmit(onSubmit)}
        isSubmitting={isSubmitting}
        isViewMode={isViewMode}
        isEditMode={isEditMode}
        editingAsset={editingAsset}
        currentAssetId={currentAssetId}
        formData={formData}
        setFormField={setFormField}
        register={register}
        formErrors={formErrors}
        photoPreview={photoPreview}
        handlePhotoChange={handlePhotoChange}
        handleRemovePhoto={handleRemovePhoto}
        attachmentFiles={attachmentFiles}
        existingAttachments={existingAttachments}
        handleFilesChange={handleFilesChange}
        handleUploadComplete={handleUploadComplete}
        handleFileDeleted={handleFileDeleted}
        categoryFormOptions={categoryFormOptions}
        supplierFormOptions={supplierFormOptions}
        conditionPurchaseOptions={conditionPurchaseOptions}
      />

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
        description="Upload an .xlsx file with asset data. Assets will be imported as ACTIVE."
      />

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={getSelectedIds(crudAssets)}
        itemName="assets"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default Assets;