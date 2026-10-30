// src/pages/Employees/Employees.jsx
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
import { EmployeesData } from '../../core/services';
import { employeeSchema, getEmployeeDefaultValues } from './schema';
import PageHeader from '../../components/atoms/PageHeader';
import BulkActionBar from '../../components/atoms/BulkActionBar';
import RefreshButton from '../../components/atoms/RefreshButton';
import Button from '../../components/atoms/Button/Button';
import Spinner from '../../components/atoms/Spinner/Spinner';
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
import { getEmployeeColumns } from './columns';
import EmployeeFormDialog from './components/EmployeeFormDialog';
import './Employees.scss';

const employeesData = new EmployeesData();

const EMPLOYEE_TABS = [
  { id: 'all', label: 'All' },
  { id: 'Active', label: 'Active' },
  { id: 'Inactive', label: 'Inactive' },
];

const Employees = () => {
  const [activeTab, setActiveTab] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [showImportModal, setShowImportModal] = useState(false);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [editingEmployee, setEditingEmployee] = useState(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [isViewMode, setIsViewMode] = useState(false);
  const [currentEmployeeId, setCurrentEmployeeId] = useState(null);

  const [photoFile, setPhotoFile] = useState(null);
  const [photoPreview, setPhotoPreview] = useState(null);
  const [attachmentFiles, setAttachmentFiles] = useState([]);
  const [existingAttachments, setExistingAttachments] = useState([]);

  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirmDelete, confirm } = useSweetAlert();

  const { departments, offices, employeePositions, employeeStatuses } =
    useReferenceData();

  const {
    selectedRowIds,
    selectionCount,
    hasSelection,
    handleSelectionChange,
    clearSelection,
    getSelectedIds,
  } = useBulkSelection({ idField: 'employeeId' });

  const showCheckbox = true;
  const showActivateToggle = useMemo(
    () => activeTab === 'Active' || activeTab === 'Inactive',
    [activeTab]
  );

  const {
    register,
    handleSubmit: handleFormSubmit,
    formState: { errors: formErrors, isSubmitting },
    setValue,
    reset,
    watch,
  } = useForm({
    resolver: zodResolver(employeeSchema),
    defaultValues: getEmployeeDefaultValues(),
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
    if (activeTab === 'Active') filters.isActive = true;
    else if (activeTab === 'Inactive') filters.isActive = false;
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
      return await employeesData.fetchGridData(requestParams);
    },
    [buildFilters]
  );

  const {
    data: employees,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    sortModel,
    setSortModel,
    reload,
  } = useGridData(['employees', activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    isMountedRef.current = true;
    setPage(1);
    clearSelection();
    reload();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, setPage, clearSelection]);

  const departmentOptions = useOptions(departments, 'Select Department');
  const positionOptions = useOptions(employeePositions, 'Select Position');
  const statusOptions = useOptions(employeeStatuses, 'Select Status');
  const officeOptions = useOptions(offices, 'Select Office');

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

  const handleCreate = useCallback(() => {
    reset(getEmployeeDefaultValues());
    setIsEditMode(false);
    setIsViewMode(false);
    setCurrentEmployeeId(null);
    setAttachmentFiles([]);
    setExistingAttachments([]);
    setPhotoFile(null);
    setPhotoPreview(null);
    setEditingEmployee(null);
    setShowModal(true);
  }, [reset]);

  const handleEdit = useCallback(
    async (record) => {
      const recordId = record?.employeeId;
      if (!recordId) return;

      setIsEditMode(true);
      setCurrentEmployeeId(recordId);
      setPhotoFile(null);
      setPhotoPreview(null);
      setEditingEmployee(record);

      try {
        const result = await employeesData.fetchById(recordId);
        if (result.success && result.data) {
          const data = result.data;
          const defaults = getEmployeeDefaultValues();
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
          toast.error('Failed to load employee data');
        }
      } catch (error) {
        console.warn('Failed to fetch employee:', error);
        setExistingAttachments([]);
        toast.error('Failed to load employee data');
      }
    },
    [setValue, toast]
  );

  const handleClose = useCallback(() => {
    if (!isSubmitting) {
      setShowModal(false);
      setIsEditMode(false);
      setIsViewMode(false);
      setCurrentEmployeeId(null);
      setEditingEmployee(null);
      reset(getEmployeeDefaultValues());
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
        if (isEditMode && editingEmployee) {
          result = await employeesData.update(editingEmployee.employeeId, submitData);
        } else {
          result = await employeesData.create(submitData);
        }

        if (result.success) {
          setPhotoFile(null);
          setPhotoPreview(null);
          setAttachmentFiles([]);
          setExistingAttachments([]);
          setIsEditMode(false);
          setIsViewMode(false);
          setCurrentEmployeeId(null);
          setEditingEmployee(null);
          setShowModal(false);
          reset(getEmployeeDefaultValues());

          queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
          reload();
          clearSelection();

          const message =
            isEditMode && editingEmployee
              ? 'Employee updated successfully'
              : 'Employee created successfully';
          toast.success(message);
        } else {
          const errorMsg =
            result.error ||
            result.errors?.join('\n') ||
            'Failed to save employee';
          toast.error(errorMsg);
        }
      } catch (error) {
        const errorMsg =
          error.userMessage || error.message || 'Failed to save employee';
        toast.error(errorMsg);
      }
    },
    [
      isSubmitting,
      isViewMode,
      isEditMode,
      editingEmployee,
      photoFile,
      attachmentFiles,
      existingAttachments,
      queryClient,
      reload,
      clearSelection,
      toast,
      reset,
    ]
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
    async (emp) => {
      const confirmed = await confirmDelete(
        'Delete Employee',
        `Are you sure you want to delete "${emp.fullName}"?`
      );
      if (!confirmed) return;
      const r = await employeesData.delete(emp.employeeId);
      if (r.success && isMountedRef.current) {
        toast.success('Employee deleted successfully');
        queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
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
        title: 'Delete Employees',
        text: `Are you sure you want to delete ${ids.length} employee(s)? This action cannot be undone.`,
        icon: 'warning',
        confirmButtonText: 'Yes, Delete',
        confirmButtonColor: '#ef4444',
      });
      if (!confirmed) return;

      const r = await employeesData.bulkDelete(ids);
      if (!isMountedRef.current) return;

      if (r.success) {
        toast.success(`${r.data.successCount} employee(s) deleted`);
      } else if (r.partial) {
        toast.warning(
          `${r.data.successCount} deleted, ${r.data.failCount} failed`
        );
      } else {
        toast.error(r.error || 'Failed to delete employees');
      }

      clearSelection();
      queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
      reload();
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

  const handleBulkActivate = useCallback(
    async (ids, activate) => {
      const actionText = activate ? 'activate' : 'deactivate';
      const confirmed = await confirm({
        title: activate ? 'Activate Employees' : 'Deactivate Employees',
        text: `Are you sure you want to ${actionText} ${ids.length} employee(s)?`,
        confirmButtonText: activate ? 'Yes, Activate' : 'Yes, Deactivate',
      });
      if (!confirmed) return;
      const r = await employeesData.bulkActivate(ids, activate);
      if (r.success && isMountedRef.current) {
        const count = r.data || ids.length;
        toast.success(`${count} employee(s) ${actionText}d successfully`);
        clearSelection();
        queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
        reload();
      }
    },
    [reload, queryClient, toast, confirm, clearSelection]
  );

  const handleImport = useCallback(
    async (file) => {
      setIsImporting(true);
      const r = await employeesData.importEmployees(file);
      setIsImporting(false);
      if (r.success && isMountedRef.current) {
        setImportResult(r.data);
        if (r.data?.errorCount > 0) {
          toast.info(
            `Import completed: ${r.data.successCount} success, ${r.data.errorCount} errors`
          );
        } else {
          toast.success(
            `Import completed: ${r.data?.successCount || 0} employees imported`
          );
        }
        queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
        reload();
        clearSelection();
      }
    },
    [reload, queryClient, toast, clearSelection]
  );

  const handleDownloadTemplate = useCallback(async () => {
    const result = await employeesData.downloadTemplate();
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
        const result = await employeesData.fetchById(currentEmployeeId);
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
  }, [currentEmployeeId]);

  const handleGridAction = useCallback(
    (actionType, row) => {
      if (actionType === ACTION_TYPES.VIEW) {
        setIsViewMode(true);
        handleEdit(row);
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
    rowIdField: 'employeeId',
  });

  const columns = useMemo(() => getEmployeeColumns(actionColumn), [actionColumn]);

  const bulkActivateValue = activeTab === 'Active' ? false : true;

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
          onClick: () => handleBulkDelete(getSelectedIds(employees)),
        },
      ]
    : [];

  const bulkTitle = bulkActivateValue
    ? 'Activate Employees'
    : 'Deactivate Employees';
  const bulkDescription = `This action will ${bulkActivateValue ? 'activate' : 'deactivate'} the selected employees.`;

  if (loading && !employees.length) {
    return (
      <div className="page-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="employees-menu">
      <PageHeader
        title="Employee Management"
        description="Manage employees, their assets, and assignments"
        actions={
          <>
            <RefreshButton onClick={handleRefresh} loading={loading} />
            <Button
              variant="outline"
              onClick={() => setShowImportModal(true)}
              startIcon={<FiUpload />}
            >
              Import
            </Button>
            <BulkActionBar
              visible={bulkActions.length > 0}
              actions={bulkActions}
            />
            <Button variant="primary" onClick={handleCreate} startIcon={<FiPlus />}>
              Add Employee
            </Button>
          </>
        }
      />

      <Tabs
        tabs={EMPLOYEE_TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
      />

      <SearchToolbar onSearch={handleSearch} placeholder="Search employees..." />

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
          rows={employees}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          page={page}
          totalRowCount={totalCount}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
          getRowId={(row) => row?.employeeId || `row-${Math.random()}`}
          checkboxSelection={showCheckbox}
          onRowSelectionModelChange={handleSelectionChange}
          onRowClick={handleRowClick}
          sortModel={sortModel}
          onSortModelChange={setSortModel}
          paginationMode="server"
          sortingMode="server"
          ariaLabel="Employees data table"
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

      <EmployeeFormDialog
        isOpen={showModal}
        onClose={handleClose}
        onSubmit={isViewMode ? () => {} : handleFormSubmit(onSubmit)}
        isSubmitting={isSubmitting}
        isViewMode={isViewMode}
        isEditMode={isEditMode}
        editingEmployee={editingEmployee}
        currentEmployeeId={currentEmployeeId}
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
        departmentOptions={departmentOptions}
        positionOptions={positionOptions}
        statusOptions={statusOptions}
        officeOptions={officeOptions}
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
        title="Import Employees"
        description="Upload an .xlsx file with employee data. Employees will be imported as ACTIVE."
      />

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={getSelectedIds(employees)}
        itemName="employees"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default Employees;