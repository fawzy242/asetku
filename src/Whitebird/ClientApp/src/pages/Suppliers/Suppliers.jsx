// src/pages/Suppliers/Suppliers.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { Grid, Box } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { FiCheckSquare, FiPlus, FiTrash2 } from 'react-icons/fi';
import { SuppliersData } from '../../core/services';
import PageHeader from '../../components/atoms/PageHeader';
import BulkActionBar from '../../components/atoms/BulkActionBar';
import RefreshButton from '../../components/atoms/RefreshButton';
import Button from '../../components/atoms/Button/Button';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import Spinner from '../../components/atoms/Spinner/Spinner';
import Tabs from '../../components/molecules/Tabs/Tabs';
import SearchToolbar from '../../components/molecules/SearchToolbar/SearchToolbar';
import DataTable from '../../components/molecules/DataTable/DataTable';
import CrudModal from '../../components/molecules/CrudModal/CrudModal';
import BulkActivateModal from '../../components/molecules/BulkActivateModal/BulkActivateModal';
import ActionMenu from '../../components/molecules/ActionMenu';
import FormSection from '../../components/atoms/FormSection/FormSection';
import Input from '../../components/atoms/Input/Input';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useCrudFormBase } from '../../hooks/useCrudFormBase';
import './Suppliers.scss';

const suppliersData = new SuppliersData();

const INITIAL_FORM_DATA = {
  supplierName: '',
  contactPerson: '',
  phoneNumber: '',
  email: '',
  address: '',
};

const TABS = [
  { id: 'all', label: 'All' },
  { id: 'active', label: 'Active' },
  { id: 'inactive', label: 'Inactive' },
];

const Suppliers = () => {
  const [activeTab, setActiveTab] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [isViewMode, setIsViewMode] = useState(false);
  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirmDelete, confirm } = useSweetAlert();

  const {
    selectedRowIds,
    selectionCount,
    hasSelection,
    handleSelectionChange,
    clearSelection,
    getSelectedIds,
  } = useBulkSelection({ idField: 'supplierId' });

  const showCheckbox = true;
  const showActivateToggle =
    activeTab === 'active' || activeTab === 'inactive';

  const {
    showModal,
    editingRecord: editingSupplier,
    isSubmitting,
    formData,
    setFormField,
    handleCreate: baseHandleCreate,
    handleEdit,
    handleClose: baseHandleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, suppliersData, {
    idField: 'supplierId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reference', 'suppliers'] });
      toast.success(
        editingSupplier
          ? 'Supplier updated successfully'
          : 'Supplier created successfully'
      );
      reload();
      clearSelection();
    },
    onError: (error) => {
      toast.error(error || 'Failed to save supplier');
    },
  });

  const handleCreate = useCallback(() => {
    setIsViewMode(false);
    baseHandleCreate();
  }, [baseHandleCreate]);

  const handleClose = useCallback(() => {
    setIsViewMode(false);
    baseHandleClose();
  }, [baseHandleClose]);

  const buildFilters = useCallback(() => {
    const filters = {};
    if (activeTab === 'active') filters.isActive = true;
    else if (activeTab === 'inactive') filters.isActive = false;
    if (searchTerm) filters.search = searchTerm;
    return filters;
  }, [activeTab, searchTerm]);

  const fetchGridData = useCallback(
    async (params) => {
      const filters = buildFilters();
      const requestParams = {
        page: params.page || 1,
        pageSize: params.pageSize || 10,
        ...filters,
        ...params,
      };
      return await suppliersData.fetchGridData(requestParams);
    },
    [buildFilters]
  );

  const {
    data,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    sortModel,
    setSortModel,
    reload,
  } = useGridData(['suppliers', activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    isMountedRef.current = true;
    reload();
    clearSelection();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, clearSelection]);

  const handleSearch = useCallback(
    (search) => {
      setSearchTerm(search);
      setPage(1);
      clearSelection();
    },
    [setPage, clearSelection]
  );

  const handleRefresh = useCallback(() => {
    reload();
    clearSelection();
  }, [reload, clearSelection]);

  const handleDelete = useCallback(
    async (sup) => {
      const confirmed = await confirmDelete(
        'Delete Supplier',
        `Are you sure you want to delete "${sup.supplierName}"?`
      );
      if (!confirmed) return;
      const r = await suppliersData.delete(sup.supplierId);
      if (r.success && isMountedRef.current) {
        toast.success('Supplier deleted successfully');
        queryClient.invalidateQueries({ queryKey: ['reference', 'suppliers'] });
        reload();
        clearSelection();
      }
    },
    [reload, queryClient, toast, confirmDelete, clearSelection]
  );

  const handleBulkDelete = useCallback(
    async (ids) => {
      if (!ids || ids.length === 0) return;
      const confirmed = await confirm({
        title: 'Delete Suppliers',
        text: `Are you sure you want to delete ${ids.length} supplier(s)? This action cannot be undone.`,
        icon: 'warning',
        confirmButtonText: 'Yes, Delete',
        confirmButtonColor: '#ef4444',
      });
      if (!confirmed) return;

      const r = await suppliersData.bulkDelete(ids);
      if (!isMountedRef.current) return;

      if (r.success) {
        toast.success(`${r.data.successCount} supplier(s) deleted`);
      } else if (r.partial) {
        toast.warning(
          `${r.data.successCount} deleted, ${r.data.failCount} failed`
        );
      } else {
        toast.error(r.error || 'Failed to delete suppliers');
      }

      clearSelection();
      queryClient.invalidateQueries({ queryKey: ['reference', 'suppliers'] });
      reload();
    },
    [reload, queryClient, toast, confirm, clearSelection]
  );

  const handleBulkActivate = useCallback(
    async (ids, activate) => {
      const actionText = activate ? 'activate' : 'deactivate';
      const confirmed = await confirm({
        title: activate ? 'Activate Suppliers' : 'Deactivate Suppliers',
        text: `Are you sure you want to ${actionText} ${ids.length} supplier(s)?`,
        confirmButtonText: activate ? 'Yes, Activate' : 'Yes, Deactivate',
      });
      if (!confirmed) return;
      const r = await suppliersData.bulkActivate(ids, activate);
      if (r.success && isMountedRef.current) {
        const count = r.data || ids.length;
        toast.success(`${count} supplier(s) ${actionText}d successfully`);
        clearSelection();
        queryClient.invalidateQueries({ queryKey: ['reference', 'suppliers'] });
        reload();
      }
    },
    [reload, queryClient, toast, confirm, clearSelection]
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

  const handleGridAction = useCallback(
    (actionType, row) => {
      if (actionType === ACTION_TYPES.VIEW) {
        setIsViewMode(true);
        handleEdit(row);
      } else if (actionType === ACTION_TYPES.EDIT) {
        setIsViewMode(false);
        handleEdit(row);
      } else if (actionType === ACTION_TYPES.DELETE) {
        handleDelete(row);
      }
    },
    [handleEdit, handleDelete]
  );

  const handleRowClick = useCallback(
    (row) => {
      setIsViewMode(true);
      handleEdit(row);
    },
    [handleEdit]
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
    rowIdField: 'supplierId',
  });

  const columns = useMemo(
    () => [
      { field: 'supplierName', headerName: 'Name', flex: 1, minWidth: 180 },
      { field: 'contactPerson', headerName: 'Contact', width: 160 },
      { field: 'phoneNumber', headerName: 'Phone', width: 140 },
      { field: 'email', headerName: 'Email', width: 200 },
      { field: 'assetCount', headerName: 'Assets', width: 90, sortable: false },
      {
        field: 'isActive',
        headerName: 'Status',
        width: 110,
        sortable: false,
        renderCell: (p) => {
          const status = p?.value ? 'Active' : 'Inactive';
          return <StatusChip status={status} />;
        },
      },
      actionColumn,
    ],
    [actionColumn]
  );

  const bulkActivateValue = activeTab === 'active' ? false : true;

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
          onClick: () => handleBulkDelete(getSelectedIds(data)),
        },
      ]
    : [];

  const bulkTitle = bulkActivateValue
    ? 'Activate Suppliers'
    : 'Deactivate Suppliers';
  const bulkDescription = `This action will ${bulkActivateValue ? 'activate' : 'deactivate'} the selected suppliers.`;

  if (loading && !data.length) {
    return (
      <div className="page-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="suppliers-menu">
      <PageHeader
        title="Supplier Management"
        description="Manage suppliers for asset procurement"
        actions={
          <>
            <RefreshButton onClick={handleRefresh} loading={loading} />
            <BulkActionBar visible={bulkActions.length > 0} actions={bulkActions} />
            <Button variant="primary" onClick={handleCreate} startIcon={<FiPlus />}>
              Add Supplier
            </Button>
          </>
        }
      />

      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />

      <SearchToolbar onSearch={handleSearch} placeholder="Search suppliers..." />

      <Box
        sx={{
          backgroundColor: 'var(--card-bg)',
          borderRadius: '12px',
          overflow: 'hidden',
          mb: 2,
          border: '1px solid var(--border)',
          width: '100%',
          minWidth: 0,
        }}
      >
        <DataTable
          rows={data}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          page={page}
          totalRowCount={totalCount}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
          getRowId={(row) => row?.supplierId || `row-${Math.random()}`}
          checkboxSelection={showCheckbox}
          onRowSelectionModelChange={handleSelectionChange}
          onRowClick={handleRowClick}
          sortModel={sortModel}
          onSortModelChange={setSortModel}
          paginationMode="server"
          sortingMode="server"
          ariaLabel="Suppliers data table"
        />
      </Box>

      <ActionMenu
        anchorEl={menuAnchorEl}
        open={menuOpen}
        onClose={handleMenuClose}
        actions={menuActions}
        row={selectedRow}
        onAction={handleActionClick}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={
          isViewMode
            ? 'View Supplier'
            : editingSupplier
              ? 'Edit Supplier'
              : 'Add Supplier'
        }
        onSubmit={isViewMode ? () => {} : crudHandleSubmit}
        isSubmitting={isSubmitting}
        submitText={isViewMode ? 'Close' : editingSupplier ? 'Update' : 'Create'}
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
                disabled={isViewMode}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Contact Person"
                value={formData.contactPerson || ''}
                onChange={(e) => setFormField('contactPerson')(e.target.value)}
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
                disabled={isViewMode}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Email"
                type="email"
                value={formData.email || ''}
                onChange={(e) => setFormField('email')(e.target.value)}
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

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={getSelectedIds(data)}
        itemName="suppliers"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default Suppliers;