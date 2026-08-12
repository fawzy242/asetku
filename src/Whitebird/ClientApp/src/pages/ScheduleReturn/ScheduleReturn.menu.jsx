// src/pages/ScheduleReturn/ScheduleReturn.menu.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { Grid } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import ScheduleReturnData from './ScheduleReturn.data';
import GridView from '../../components/organisms/GridView/GridView';
import CrudModal from '../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../components/atoms/FormSection/FormSection';
import Input from '../../components/atoms/Input/Input';
import Select from '../../components/atoms/Select/Select';
import DatePickerInput from '../../components/atoms/Input/DatePickerInput';
import Spinner from '../../components/atoms/Spinner/Spinner';
import Button from '../../components/atoms/Button/Button';
import Modal from '../../components/molecules/Modal/Modal';
import BulkActivateModal from '../../components/molecules/BulkActivateModal/BulkActivateModal';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import { FiCalendar, FiCheckSquare } from 'react-icons/fi';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useReferenceData } from '../../hooks/useReferenceData';
import { useCrudFormBase } from '../../hooks/useCrudFormBase';
import { useOptions } from '../../hooks/useOptions';
import utilsHelper from '../../core/utils/utils.helper';
import './ScheduleReturn.scss';

const scheduleData = new ScheduleReturnData();

const INITIAL_FORM_DATA = {
  scheduleDate: '',
  assetId: '',
  employeeId: '',
};

const TABS = [
  { id: 'all', label: 'All' },
  { id: 'active', label: 'Active' },
  { id: 'inactive', label: 'Inactive' },
];

const ScheduleReturnMenu = () => {
  const [activeTab, setActiveTab] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [showGenerateModal, setShowGenerateModal] = useState(false);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [generateData, setGenerateData] = useState({
    dateFrom: '',
    dateTo: '',
    assetId: '',
    employeeId: '',
  });
  const [isGenerating, setIsGenerating] = useState(false);
  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirmDelete, confirm } = useSweetAlert();

  const { assets, employees } = useReferenceData();

  const {
    showModal,
    editingRecord,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, scheduleData, {
    idField: 'scheduleReturnId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schedule-return'] });
    },
  });

  const {
    selectedRowIds,
    selectionCount,
    hasSelection,
    handleSelectionChange,
    clearSelection,
    getSelectedIds,
  } = useBulkSelection({ idField: 'scheduleReturnId' });

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
      const result = await scheduleData.fetchGridData(requestParams);
      return result;
    },
    [buildFilters]
  );

  const { data, totalCount, loading, page, setPage, pageSize, setPageSize, reload } = useGridData(
    ['schedule-return', activeTab, searchTerm],
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
    async (record) => {
      const confirmed = await confirmDelete('Delete Schedule', `Are you sure you want to delete this schedule?`);
      if (!confirmed) return;
      const r = await scheduleData.delete(record.scheduleReturnId);
      if (r.success && isMountedRef.current) {
        toast.success('Schedule deleted');
        queryClient.invalidateQueries({ queryKey: ['schedule-return'] });
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
        title: activate ? 'Activate Schedules' : 'Deactivate Schedules',
        text: `Are you sure you want to ${actionText} ${ids.length} schedule(s)?`,
        confirmButtonText: activate ? 'Yes, Activate' : 'Yes, Deactivate',
      });
      if (!confirmed) return;
      const r = await scheduleData.bulkActivate(ids, activate);
      if (r.success && isMountedRef.current) {
        const count = r.data || ids.length;
        toast.success(`${count} schedule(s) ${actionText}d`);
        clearSelection();
        queryClient.invalidateQueries({ queryKey: ['schedule-return'] });
        reload();
      }
    },
    [reload, queryClient, toast, confirm, clearSelection]
  );

  const onSubmit = useCallback(async () => {
    const success = await crudHandleSubmit();
    if (success && isMountedRef.current) {
      toast.success(editingRecord ? 'Schedule updated' : 'Schedule created');
      reload();
      clearSelection();
    }
    return success;
  }, [crudHandleSubmit, reload, toast, editingRecord, clearSelection]);

  const handleTabChange = useCallback(
    (tab) => {
      setActiveTab(tab);
      setPage(1);
      setSearchTerm('');
      clearSelection();
    },
    [setPage, clearSelection]
  );

  const handleGenerate = useCallback(async () => {
    if (!generateData.dateFrom || !generateData.dateTo || !generateData.assetId || !generateData.employeeId) {
      toast.error('All fields are required');
      return;
    }
    setIsGenerating(true);
    const r = await scheduleData.generate(generateData);
    setIsGenerating(false);
    if (r.success && isMountedRef.current) {
      setShowGenerateModal(false);
      setGenerateData({ dateFrom: '', dateTo: '', assetId: '', employeeId: '' });
      queryClient.invalidateQueries({ queryKey: ['schedule-return'] });
      reload();
      clearSelection();
    }
  }, [generateData, reload, queryClient, toast, clearSelection]);

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
    rowIdField: 'scheduleReturnId',
  });

  const assetOptions = useOptions(assets, 'Select Asset');
  const employeeOptions = useOptions(employees, 'Select Employee');

  const columns = useMemo(
    () => [
      {
        field: 'scheduleDate',
        headerName: 'Schedule Date',
        width: 180,
        renderCell: (p) => utilsHelper.formatDateTime(p?.value),
      },
      { field: 'assetCode', headerName: 'Asset Code', width: 120 },
      { field: 'assetName', headerName: 'Asset Name', flex: 1, minWidth: 180 },
      { field: 'employeeCode', headerName: 'Employee Code', width: 120 },
      { field: 'employeeName', headerName: 'Employee Name', width: 180 },
      {
        field: 'isActive',
        headerName: 'Status',
        width: 110,
        renderCell: (p) => <StatusChip status={p.value ? 'Active' : 'Inactive'} />,
      },
      actionColumn,
    ],
    [actionColumn]
  );

  const extraActions = (
    <>
      <Button variant="outline" size="sm" onClick={() => setShowGenerateModal(true)} className="u-inline-flex u-btn-gap">
        <FiCalendar size={16} /> Generate
      </Button>
      {hasSelection && showCheckbox && (
        <Button variant="primary" size="sm" onClick={() => setShowBulkActivateModal(true)} className="u-inline-flex u-btn-gap">
          <FiCheckSquare size={16} /> {activeTab === 'active' ? 'Deactivate' : 'Activate'} ({selectionCount})
        </Button>
      )}
    </>
  );

  if (loading && !data.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === 'active' ? false : true;
  const bulkTitle = bulkActivateValue ? 'Activate Schedules' : 'Deactivate Schedules';
  const bulkDescription = `This action will ${bulkActivateValue ? 'activate' : 'deactivate'} the selected schedules.`;

  return (
    <div className="schedule-return">
      <GridView
        title="Schedule Return"
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
        createButtonText="Add Schedule"
        ariaLabel="Schedule Return"
        extraActions={extraActions}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingRecord ? 'Edit Schedule' : 'Add Schedule'}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingRecord ? 'Update' : 'Create'}
        size="md"
      >
        <FormSection title="Schedule Details">
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <DatePickerInput
                label="Schedule Date"
                value={formData.scheduleDate || ''}
                onChange={(e) => setFormField('scheduleDate')(e.target.value)}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Asset"
                value={formData.assetId || ''}
                onChange={(e) => setFormField('assetId')(e.target.value)}
                options={assetOptions}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Employee"
                value={formData.employeeId || ''}
                onChange={(e) => setFormField('employeeId')(e.target.value)}
                options={employeeOptions}
                required
              />
            </Grid>
          </Grid>
        </FormSection>
      </CrudModal>

      <Modal
        isOpen={showGenerateModal}
        onClose={() => setShowGenerateModal(false)}
        title="Generate Schedule Return"
        size="md"
      >
        <div style={{ padding: '16px 0' }}>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <DatePickerInput
                label="Date From"
                value={generateData.dateFrom || ''}
                onChange={(e) => setGenerateData({ ...generateData, dateFrom: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <DatePickerInput
                label="Date To"
                value={generateData.dateTo || ''}
                onChange={(e) => setGenerateData({ ...generateData, dateTo: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Asset"
                value={generateData.assetId || ''}
                onChange={(e) => setGenerateData({ ...generateData, assetId: e.target.value })}
                options={assetOptions}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Employee"
                value={generateData.employeeId || ''}
                onChange={(e) => setGenerateData({ ...generateData, employeeId: e.target.value })}
                options={employeeOptions}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <div style={{ display: 'flex', gap: '16px', justifyContent: 'flex-end', marginTop: '16px' }}>
                <Button variant="outline" onClick={() => setShowGenerateModal(false)}>
                  Cancel
                </Button>
                <Button variant="primary" onClick={handleGenerate} loading={isGenerating}>
                  Generate
                </Button>
              </div>
            </Grid>
          </Grid>
        </div>
      </Modal>

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={getSelectedIds(data)}
        itemName="schedules"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default ScheduleReturnMenu;