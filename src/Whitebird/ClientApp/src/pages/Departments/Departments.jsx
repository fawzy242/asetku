// src/pages/Departments/Departments.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { Box } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { FiCheckSquare, FiPlus, FiTrash2 } from 'react-icons/fi';
import { DepartmentsData } from '../../core/services';
import PageHeader from '../../components/atoms/PageHeader';
import BulkActionBar from '../../components/atoms/BulkActionBar';
import RefreshButton from '../../components/atoms/RefreshButton';
import Button from '../../components/atoms/Button/Button';
import Spinner from '../../components/atoms/Spinner/Spinner';
import Tabs from '../../components/molecules/Tabs/Tabs';
import SearchToolbar from '../../components/molecules/SearchToolbar/SearchToolbar';
import DataTable from '../../components/molecules/DataTable/DataTable';
import BulkActivateModal from '../../components/molecules/BulkActivateModal/BulkActivateModal';
import ActionMenu from '../../components/molecules/ActionMenu';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useCrudFormBase } from '../../hooks/useCrudFormBase';
import { getDepartmentColumns } from './columns';
import DepartmentFormDialog from './components/DepartmentFormDialog';
import './Departments.scss';

const departmentsData = new DepartmentsData();

const INITIAL_FORM_DATA = {
  departmentCode: '',
  departmentName: '',
  description: '',
};

const TABS = [
  { id: 'all', label: 'All' },
  { id: 'active', label: 'Active' },
  { id: 'inactive', label: 'Inactive' },
];

const Departments = () => {
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
  } = useBulkSelection({ idField: 'departmentId' });

  const showCheckbox = true;
  const showActivateToggle =
    activeTab === 'active' || activeTab === 'inactive';

  const {
    showModal,
    editingRecord: editingDepartment,
    isSubmitting,
    formData,
    setFormField,
    handleCreate: baseHandleCreate,
    handleEdit,
    handleClose: baseHandleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, departmentsData, {
    idField: 'departmentId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reference', 'departments'] });
      toast.success(
        editingDepartment
          ? 'Department updated successfully'
          : 'Department created successfully'
      );
      reload();
      clearSelection();
    },
    onError: (error) => {
      toast.error(error || 'Failed to save department');
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
      return await departmentsData.fetchGridData(requestParams);
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
  } = useGridData(['departments', activeTab, searchTerm], fetchGridData);

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
    async (dept) => {
      const confirmed = await confirmDelete(
        'Delete Department',
        `Are you sure you want to delete "${dept.departmentName}"? This may affect employees assigned to it.`
      );
      if (!confirmed) return;
      const r = await departmentsData.delete(dept.departmentId);
      if (r.success && isMountedRef.current) {
        toast.success('Department deleted successfully');
        queryClient.invalidateQueries({ queryKey: ['reference', 'departments'] });
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
        title: 'Delete Departments',
        text: `Are you sure you want to delete ${ids.length} department(s)? This action cannot be undone.`,
        icon: 'warning',
        confirmButtonText: 'Yes, Delete',
        confirmButtonColor: '#ef4444',
      });
      if (!confirmed) return;

      const r = await departmentsData.bulkDelete(ids);
      if (!isMountedRef.current) return;

      if (r.success) {
        toast.success(`${r.data.successCount} department(s) deleted`);
      } else if (r.partial) {
        toast.warning(
          `${r.data.successCount} deleted, ${r.data.failCount} failed`
        );
      } else {
        toast.error(r.error || 'Failed to delete departments');
      }

      clearSelection();
      queryClient.invalidateQueries({ queryKey: ['reference', 'departments'] });
      reload();
    },
    [reload, queryClient, toast, confirm, clearSelection]
  );

  const handleBulkActivate = useCallback(
    async (ids, activate) => {
      const actionText = activate ? 'activate' : 'deactivate';
      const confirmed = await confirm({
        title: activate ? 'Activate Departments' : 'Deactivate Departments',
        text: `Are you sure you want to ${actionText} ${ids.length} department(s)?`,
        confirmButtonText: activate ? 'Yes, Activate' : 'Yes, Deactivate',
      });
      if (!confirmed) return;
      const r = await departmentsData.bulkActivate(ids, activate);
      if (r.success && isMountedRef.current) {
        const count = r.data || ids.length;
        toast.success(`${count} department(s) ${actionText}d successfully`);
        clearSelection();
        queryClient.invalidateQueries({ queryKey: ['reference', 'departments'] });
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
    rowIdField: 'departmentId',
  });

  const columns = useMemo(
    () => getDepartmentColumns(actionColumn),
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
    ? 'Activate Departments'
    : 'Deactivate Departments';
  const bulkDescription = `This action will ${bulkActivateValue ? 'activate' : 'deactivate'} the selected departments.`;

  if (loading && !data.length) {
    return (
      <div className="page-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="departments-menu">
      <PageHeader
        title="Department Management"
        description="Manage departments used across the organization"
        actions={
          <>
            <RefreshButton onClick={handleRefresh} loading={loading} />
            <BulkActionBar visible={bulkActions.length > 0} actions={bulkActions} />
            <Button variant="primary" onClick={handleCreate} startIcon={<FiPlus />}>
              Add Department
            </Button>
          </>
        }
      />

      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />

      <SearchToolbar onSearch={handleSearch} placeholder="Search departments..." />

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
          getRowId={(row) => row?.departmentId || `row-${Math.random()}`}
          checkboxSelection={showCheckbox}
          onRowSelectionModelChange={handleSelectionChange}
          onRowClick={handleRowClick}
          sortModel={sortModel}
          onSortModelChange={setSortModel}
          paginationMode="server"
          sortingMode="server"
          ariaLabel="Departments data table"
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

      <DepartmentFormDialog
        isOpen={showModal}
        onClose={handleClose}
        onSubmit={isViewMode ? () => {} : crudHandleSubmit}
        isSubmitting={isSubmitting}
        isViewMode={isViewMode}
        editingRecord={editingDepartment}
        formData={formData}
        setFormField={setFormField}
      />

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={getSelectedIds(data)}
        itemName="departments"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default Departments;