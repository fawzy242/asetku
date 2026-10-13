// src/pages/ScheduleReturn/ScheduleReturn.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Grid, Box } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';
import { FiCalendar, FiCheckSquare, FiPlus, FiTrash2 } from 'react-icons/fi';
import { ScheduleReturnData } from '../../core/services';
import { getFieldErrorProps } from '../../core/utils/validation.utils';
import PageHeader from '../../components/atoms/PageHeader';
import BulkActionBar from '../../components/atoms/BulkActionBar';
import RefreshButton from '../../components/atoms/RefreshButton';
import Button from '../../components/atoms/Button/Button';
import Spinner from '../../components/atoms/Spinner/Spinner';
import FormSection from '../../components/atoms/FormSection/FormSection';
import Select from '../../components/atoms/Select/Select';
import DatePickerInput from '../../components/atoms/Input/DatePickerInput';
import Tabs from '../../components/molecules/Tabs/Tabs';
import SearchToolbar from '../../components/molecules/SearchToolbar/SearchToolbar';
import DataTable from '../../components/molecules/DataTable/DataTable';
import CrudModal from '../../components/molecules/CrudModal/CrudModal';
import Modal from '../../components/molecules/Modal/Modal';
import BulkActivateModal from '../../components/molecules/BulkActivateModal/BulkActivateModal';
import ActionMenu from '../../components/molecules/ActionMenu';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useReferenceData } from '../../hooks/useReferenceData';
import { useOptions } from '../../hooks/useOptions';
import { getScheduleColumns } from '../../core/constants/columns';
import './ScheduleReturn.scss';

const scheduleData = new ScheduleReturnData();

const scheduleReturnSchema = z.object({
  scheduleDate: z.string().min(1, 'Schedule date is required'),
  assetId: z.coerce.number().min(1, 'Asset is required'),
  employeeId: z.coerce.number().min(1, 'Employee is required'),
});

const getDefaultValues = () => ({
  scheduleDate: '',
  assetId: '',
  employeeId: '',
});

const TABS = [
  { id: 'all', label: 'All' },
  { id: 'active', label: 'Active' },
  { id: 'inactive', label: 'Inactive' },
];

const ScheduleReturn = () => {
  const [activeTab, setActiveTab] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [showGenerateModal, setShowGenerateModal] = useState(false);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editingRecord, setEditingRecord] = useState(null);
  const [isViewMode, setIsViewMode] = useState(false);
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
    selectedRowIds,
    selectionCount,
    hasSelection,
    handleSelectionChange,
    clearSelection,
    getSelectedIds,
  } = useBulkSelection({ idField: 'scheduleReturnId' });

  const showCheckbox = true;
  const showActivateToggle =
    activeTab === 'active' || activeTab === 'inactive';

  const {
    register,
    handleSubmit: handleFormSubmit,
    formState: { errors: formErrors, isSubmitting },
    setValue,
    reset,
    watch,
  } = useForm({
    resolver: zodResolver(scheduleReturnSchema),
    defaultValues: getDefaultValues(),
  });

  const formData = watch();

  const setFormField = useCallback(
    (field) => (value) => {
      setValue(field, value, { shouldValidate: true });
    },
    [setValue]
  );

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
      return await scheduleData.fetchGridData(requestParams);
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
  } = useGridData(['schedule-return', activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    isMountedRef.current = true;
    reload();
    clearSelection();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, clearSelection]);

  const assetOptions = useOptions(assets, 'Select Asset');
  const employeeOptions = useOptions(employees, 'Select Employee');

  const handleCreate = useCallback(() => {
    reset(getDefaultValues());
    setEditingRecord(null);
    setIsViewMode(false);
    setShowModal(true);
  }, [reset]);

  const handleEdit = useCallback(
    async (record) => {
      const recordId = record?.scheduleReturnId;
      if (!recordId) return;

      setEditingRecord(record);

      try {
        const result = await scheduleData.fetchById(recordId);
        if (result.success && result.data) {
          const data = result.data;
          const defaults = getDefaultValues();
          Object.keys(defaults).forEach((key) => {
            if (data[key] !== undefined && data[key] !== null) {
              setValue(key, data[key]);
            } else {
              setValue(key, '');
            }
          });
          setShowModal(true);
        } else {
          toast.error('Failed to load schedule data');
        }
      } catch (error) {
        console.warn('Failed to fetch schedule:', error);
        toast.error('Failed to load schedule data');
      }
    },
    [setValue, toast]
  );

  const handleClose = useCallback(() => {
    if (!isSubmitting) {
      setShowModal(false);
      setEditingRecord(null);
      setIsViewMode(false);
      reset(getDefaultValues());
    }
  }, [isSubmitting, reset]);

  const onSubmit = useCallback(
    async (formValues) => {
      if (isSubmitting || isViewMode) return;

      try {
        const result = editingRecord
          ? await scheduleData.update(editingRecord.scheduleReturnId, {
              ...formValues,
              isActive: editingRecord.isActive,
            })
          : await scheduleData.create(formValues);

        if (result.success) {
          setShowModal(false);
          setEditingRecord(null);
          setIsViewMode(false);
          reset(getDefaultValues());

          queryClient.invalidateQueries({ queryKey: ['schedule-return'] });
          reload();
          clearSelection();

          const message = editingRecord
            ? 'Schedule updated successfully'
            : 'Schedule created successfully';
          toast.success(message);
        } else {
          const errorMsg =
            result.error ||
            result.errors?.join('\n') ||
            'Failed to save schedule';
          toast.error(errorMsg);
        }
      } catch (error) {
        const errorMsg =
          error.userMessage || error.message || 'Failed to save schedule';
        toast.error(errorMsg);
      }
    },
    [isSubmitting, isViewMode, editingRecord, queryClient, reload, clearSelection, toast, reset]
  );

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
    async (record) => {
      const confirmed = await confirmDelete(
        'Delete Schedule',
        `Are you sure you want to delete this schedule?`
      );
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

  const handleBulkDelete = useCallback(
    async (ids) => {
      if (!ids || ids.length === 0) return;
      const confirmed = await confirm({
        title: 'Delete Schedules',
        text: `Are you sure you want to delete ${ids.length} schedule(s)? This action cannot be undone.`,
        icon: 'warning',
        confirmButtonText: 'Yes, Delete',
        confirmButtonColor: '#ef4444',
      });
      if (!confirmed) return;

      const r = await scheduleData.bulkDelete(ids);
      if (!isMountedRef.current) return;

      if (r.success) {
        toast.success(`${r.data.successCount} schedule(s) deleted`);
      } else if (r.partial) {
        toast.warning(
          `${r.data.successCount} deleted, ${r.data.failCount} failed`
        );
      } else {
        toast.error(r.error || 'Failed to delete schedules');
      }

      clearSelection();
      queryClient.invalidateQueries({ queryKey: ['schedule-return'] });
      reload();
    },
    [reload, queryClient, toast, confirm, clearSelection]
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
    if (
      !generateData.dateFrom ||
      !generateData.dateTo ||
      !generateData.assetId ||
      !generateData.employeeId
    ) {
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
      toast.success('Schedules generated successfully');
    } else {
      toast.error(r.error || 'Failed to generate schedules');
    }
  }, [generateData, reload, queryClient, toast, clearSelection]);

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
    rowIdField: 'scheduleReturnId',
  });

  const columns = useMemo(
    () => getScheduleColumns(actionColumn, 'return'),
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
    ? 'Activate Schedules'
    : 'Deactivate Schedules';
  const bulkDescription = `This action will ${bulkActivateValue ? 'activate' : 'deactivate'} the selected schedules.`;

  if (loading && !data.length) {
    return (
      <div className="page-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="schedule-return">
      <PageHeader
        title="Schedule Return"
        description="Plan and track scheduled asset returns"
        actions={
          <>
            <RefreshButton onClick={handleRefresh} loading={loading} />
            <Button
              variant="outline"
              onClick={() => setShowGenerateModal(true)}
              startIcon={<FiCalendar />}
            >
              Generate
            </Button>
            <BulkActionBar
              visible={bulkActions.length > 0}
              actions={bulkActions}
            />
            <Button variant="primary" onClick={handleCreate} startIcon={<FiPlus />}>
              Add Schedule
            </Button>
          </>
        }
      />

      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />

      <SearchToolbar onSearch={handleSearch} placeholder="Search schedules..." />

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
          getRowId={(row) => row?.scheduleReturnId || `row-${Math.random()}`}
          checkboxSelection={showCheckbox}
          onRowSelectionModelChange={handleSelectionChange}
          onRowClick={handleRowClick}
          sortModel={sortModel}
          onSortModelChange={setSortModel}
          paginationMode="server"
          sortingMode="server"
          ariaLabel="Schedule Return"
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
            ? 'View Schedule'
            : editingRecord
              ? 'Edit Schedule'
              : 'Add Schedule'
        }
        onSubmit={isViewMode ? () => {} : handleFormSubmit(onSubmit)}
        isSubmitting={isSubmitting}
        submitText={isViewMode ? 'Close' : editingRecord ? 'Update' : 'Create'}
        cancelText={isViewMode ? 'Close' : 'Cancel'}
        disableSubmit={isViewMode}
        size="md"
      >
        <FormSection title="Schedule Details">
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <DatePickerInput
                label="Schedule Date"
                {...register('scheduleDate')}
                value={formData.scheduleDate || ''}
                onChange={(e) => setFormField('scheduleDate')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'scheduleDate')}
                disabled={isViewMode}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Asset"
                {...register('assetId')}
                value={formData.assetId || ''}
                onChange={(e) => setFormField('assetId')(e.target.value)}
                options={assetOptions}
                {...getFieldErrorProps(formErrors, 'assetId')}
                disabled={isViewMode}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Employee"
                {...register('employeeId')}
                value={formData.employeeId || ''}
                onChange={(e) => setFormField('employeeId')(e.target.value)}
                options={employeeOptions}
                {...getFieldErrorProps(formErrors, 'employeeId')}
                disabled={isViewMode}
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
                onChange={(e) =>
                  setGenerateData({ ...generateData, dateFrom: e.target.value })
                }
                required
              />
            </Grid>
            <Grid item xs={12}>
              <DatePickerInput
                label="Date To"
                value={generateData.dateTo || ''}
                onChange={(e) =>
                  setGenerateData({ ...generateData, dateTo: e.target.value })
                }
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Asset"
                value={generateData.assetId || ''}
                onChange={(e) =>
                  setGenerateData({ ...generateData, assetId: e.target.value })
                }
                options={assetOptions}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Employee"
                value={generateData.employeeId || ''}
                onChange={(e) =>
                  setGenerateData({
                    ...generateData,
                    employeeId: e.target.value,
                  })
                }
                options={employeeOptions}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <div
                style={{
                  display: 'flex',
                  gap: '16px',
                  justifyContent: 'flex-end',
                  marginTop: '16px',
                }}
              >
                <Button
                  variant="outline"
                  onClick={() => setShowGenerateModal(false)}
                >
                  Cancel
                </Button>
                <Button
                  variant="primary"
                  onClick={handleGenerate}
                  loading={isGenerating}
                >
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

export default ScheduleReturn;