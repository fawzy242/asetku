// src/pages/Offices/Offices.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { Grid } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { OfficesData } from '../../core/services';
import GridView from '../../components/organisms/GridView/GridView';
import CrudModal from '../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../components/atoms/FormSection/FormSection';
import Input from '../../components/atoms/Input/Input';
import Select from '../../components/atoms/Select/Select';
import Spinner from '../../components/atoms/Spinner/Spinner';
import BulkActivateModal from '../../components/molecules/BulkActivateModal/BulkActivateModal';
import Button from '../../components/atoms/Button/Button';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import { FiCheckSquare } from 'react-icons/fi';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useReferenceData } from '../../hooks/useReferenceData';
import { useCrudFormBase } from '../../hooks/useCrudFormBase';
import { useOptions } from '../../hooks/useOptions';
import './Offices.scss';

const officesData = new OfficesData();

const INITIAL_FORM_DATA = {
  officeName: '',
  officeCode: '',
  officeType: '',
  city: '',
  address: '',
  phone: '',
  parentOfficeId: null,
};

const TABS = [
  { id: 'all', label: 'All' },
  { id: 'active', label: 'Active' },
  { id: 'inactive', label: 'Inactive' },
];

const Offices = () => {
  const [activeTab, setActiveTab] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirmDelete, confirm } = useSweetAlert();
  const { offices: parentOffices, officeTypes } = useReferenceData();

  const {
    showModal,
    editingRecord: editingOffice,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, officesData, {
    idField: 'officeId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reference', 'offices'] });
      toast.success(editingOffice ? 'Office updated successfully' : 'Office created successfully');
      reload();
      clearSelection();
    },
    onError: (error) => {
      toast.error(error || 'Failed to save office');
    },
  });

  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } =
    useBulkSelection({ idField: 'officeId' });

  const showCheckbox = activeTab === 'active' || activeTab === 'inactive';

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
      const result = await officesData.fetchGridData(requestParams);
      return result;
    },
    [buildFilters]
  );

  const { data, totalCount, loading, page, setPage, pageSize, setPageSize, reload } = useGridData(
    ['offices', activeTab, searchTerm],
    fetchGridData
  );

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

  const handleDelete = useCallback(
    async (office) => {
      const confirmed = await confirmDelete(
        'Delete Office',
        `Are you sure you want to delete "${office.officeName}"? This may affect assets and employees assigned to it.`
      );
      if (!confirmed) return;
      const r = await officesData.delete(office.officeId);
      if (r.success && isMountedRef.current) {
        toast.success('Office deleted successfully');
        queryClient.invalidateQueries({ queryKey: ['reference', 'offices'] });
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
        title: activate ? 'Activate Offices' : 'Deactivate Offices',
        text: `Are you sure you want to ${actionText} ${ids.length} office(s)?`,
        confirmButtonText: activate ? 'Yes, Activate' : 'Yes, Deactivate',
      });
      if (!confirmed) return;
      const r = await officesData.bulkActivate(ids, activate);
      if (r.success && isMountedRef.current) {
        const count = r.data || ids.length;
        toast.success(`${count} office(s) ${actionText}d successfully`);
        clearSelection();
        queryClient.invalidateQueries({ queryKey: ['reference', 'offices'] });
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
    rowIdField: 'officeId',
  });

  const parentOfficeOptions = useOptions(
    parentOffices.filter((o) => !editingOffice || o.value !== editingOffice.officeId),
    'None (Top Level)'
  );

  const officeTypeOptions = useOptions(officeTypes, 'Select Type');

  const columns = useMemo(
    () => [
      { field: 'officeCode', headerName: 'Code', width: 120 },
      { field: 'officeName', headerName: 'Name', flex: 1, minWidth: 180 },
      { field: 'officeTypeName', headerName: 'Type', width: 130 },
      { field: 'city', headerName: 'City', width: 140 },
      { field: 'parentOfficeName', headerName: 'Parent', width: 170 },
      { field: 'childCount', headerName: 'Sub-offices', width: 130 },
      {
        field: 'isActive',
        headerName: 'Status',
        width: 110,
        renderCell: (p) => {
          const status = p.value ? 'Active' : 'Inactive';
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
  const bulkTitle = bulkActivateValue ? 'Activate Offices' : 'Deactivate Offices';
  const bulkDescription = `This action will ${bulkActivateValue ? 'activate' : 'deactivate'} the selected offices.`;

  return (
    <div className="offices-menu">
      <GridView
        title="Office Management"
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
        createButtonText="Add Office"
        ariaLabel="Offices data table"
        extraActions={extraActions}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingOffice ? 'Edit Office' : 'Add Office'}
        onSubmit={crudHandleSubmit}
        isSubmitting={isSubmitting}
        submitText={editingOffice ? 'Update' : 'Create'}
        size="lg"
      >
        <FormSection title="Basic Information" description="Office name, code, and type">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input
                label="Office Name"
                value={formData.officeName || ''}
                onChange={(e) => setFormField('officeName')(e.target.value)}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Office Code"
                value={formData.officeCode || ''}
                onChange={(e) => setFormField('officeCode')(e.target.value)}
                placeholder="Optional"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Office Type"
                value={formData.officeType || ''}
                onChange={(e) => setFormField('officeType')(e.target.value)}
                options={officeTypeOptions}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Parent Office"
                value={formData.parentOfficeId || ''}
                onChange={(e) => setFormField('parentOfficeId')(e.target.value || null)}
                options={parentOfficeOptions}
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Location Details" description="City, address, and phone">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input
                label="City"
                value={formData.city || ''}
                onChange={(e) => setFormField('city')(e.target.value)}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Phone"
                value={formData.phone || ''}
                onChange={(e) => setFormField('phone')(e.target.value)}
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
        itemName="offices"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default Offices;