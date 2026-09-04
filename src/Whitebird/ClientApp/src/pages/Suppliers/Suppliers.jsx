// src/pages/Suppliers/Suppliers.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { Grid } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { SuppliersData } from '../../core/services';
import GridView from '../../components/organisms/GridView/GridView';
import CrudModal from '../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../components/atoms/FormSection/FormSection';
import Input from '../../components/atoms/Input/Input';
import Spinner from '../../components/atoms/Spinner/Spinner';
import BulkActivateModal from '../../components/molecules/BulkActivateModal/BulkActivateModal';
import Button from '../../components/atoms/Button/Button';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import { FiCheckSquare } from 'react-icons/fi';
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
  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirmDelete, confirm } = useSweetAlert();

  // ============================================================
  // BULK SELECTION - HARUS DI ATAS useEffect
  // ============================================================
  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } =
    useBulkSelection({ idField: 'supplierId' });

  const showCheckbox = activeTab === 'active' || activeTab === 'inactive';

  // ============================================================
  // CRUD FORM
  // ============================================================
  const {
    showModal,
    editingRecord: editingSupplier,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, suppliersData, {
    idField: 'supplierId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reference', 'suppliers'] });
      toast.success(editingSupplier ? 'Supplier updated successfully' : 'Supplier created successfully');
      reload();
      clearSelection();
    },
    onError: (error) => {
      toast.error(error || 'Failed to save supplier');
    },
  });

  // ============================================================
  // BUILD FILTERS
  // ============================================================
  const buildFilters = useCallback(() => {
    const filters = {};
    if (activeTab === 'active') filters.isActive = true;
    else if (activeTab === 'inactive') filters.isActive = false;
    if (searchTerm) filters.search = searchTerm;
    return filters;
  }, [activeTab, searchTerm]);

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
      const result = await suppliersData.fetchGridData(requestParams);
      return result;
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
    reload,
  } = useGridData(['suppliers', activeTab, searchTerm], fetchGridData);

  // ============================================================
  // EFFECTS
  // ============================================================
  useEffect(() => {
    isMountedRef.current = true;
    reload();
    clearSelection();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, clearSelection]);

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
    async (sup) => {
      const confirmed = await confirmDelete('Delete Supplier', `Are you sure you want to delete "${sup.supplierName}"?`);
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
      if (actionType === ACTION_TYPES.EDIT) handleEdit(row);
      else if (actionType === ACTION_TYPES.DELETE) handleDelete(row);
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
    rowIdField: 'supplierId',
  });

  const columns = useMemo(
    () => [
      { field: 'supplierName', headerName: 'Name', flex: 1, minWidth: 180 },
      { field: 'contactPerson', headerName: 'Contact', width: 160 },
      { field: 'phoneNumber', headerName: 'Phone', width: 140 },
      { field: 'email', headerName: 'Email', width: 200 },
      { field: 'assetCount', headerName: 'Assets', width: 90 },
      {
        field: 'isActive',
        headerName: 'Status',
        width: 110,
        renderCell: (p) => {
          const status = p?.value ? 'Active' : 'Inactive';
          return <StatusChip status={status} />;
        },
      },
      actionColumn,
    ],
    [actionColumn]
  );

  const extraActions = (
    <>
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

  if (loading && !data.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === 'active' ? false : true;
  const bulkTitle = bulkActivateValue ? 'Activate Suppliers' : 'Deactivate Suppliers';
  const bulkDescription = `This action will ${bulkActivateValue ? 'activate' : 'deactivate'} the selected suppliers.`;

  return (
    <div className="suppliers-menu">
      <GridView
        title="Supplier Management"
        tabs={TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={data}
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
        createButtonText="Add Supplier"
        ariaLabel="Suppliers data table"
        extraActions={extraActions}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingSupplier ? 'Edit Supplier' : 'Add Supplier'}
        onSubmit={crudHandleSubmit}
        isSubmitting={isSubmitting}
        submitText={editingSupplier ? 'Update' : 'Create'}
        size="lg"
      >
        <FormSection title="Basic Information" description="Supplier name and contact person">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input
                label="Supplier Name"
                value={formData.supplierName || ''}
                onChange={(e) => setFormField('supplierName')(e.target.value)}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Contact Person"
                value={formData.contactPerson || ''}
                onChange={(e) => setFormField('contactPerson')(e.target.value)}
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Contact Information" description="Phone, email, and address">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input
                label="Phone Number"
                value={formData.phoneNumber || ''}
                onChange={(e) => setFormField('phoneNumber')(e.target.value)}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Email"
                type="email"
                value={formData.email || ''}
                onChange={(e) => setFormField('email')(e.target.value)}
              />
            </Grid>
            <Grid item xs={12}>
              <Input
                label="Address"
                value={formData.address || ''}
                onChange={(e) => setFormField('address')(e.target.value)}
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