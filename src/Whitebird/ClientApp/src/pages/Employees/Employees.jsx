// src/pages/Employees/Employees.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Grid, Box, Avatar, Typography, IconButton } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { FiCamera, FiX, FiUpload, FiCheckSquare } from 'react-icons/fi';
import { EmployeesData } from '../../core/services';
import { employeeSchema, getEmployeeDefaultValues } from '../../schemas/employeeSchema';
import { getFieldErrorProps } from '../../core/utils/validation.utils';
import GridView from '../../components/organisms/GridView/GridView';
import CrudModal from '../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../components/atoms/FormSection/FormSection';
import Input from '../../components/atoms/Input/Input';
import Select from '../../components/atoms/Select/Select';
import DatePickerInput from '../../components/atoms/Input/DatePickerInput';
import Spinner from '../../components/atoms/Spinner/Spinner';
import ImportModal from '../../components/molecules/ImportModal/ImportModal';
import BulkActivateModal from '../../components/molecules/BulkActivateModal/BulkActivateModal';
import Button from '../../components/atoms/Button/Button';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import EmploymentStatusChip from '../../components/atoms/EmploymentStatusChip/EmploymentStatusChip';
import FileUploader from '../../components/molecules/FileUploader/FileUploader';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useReferenceData } from '../../hooks/useReferenceData';
import { useOptions } from '../../hooks/useOptions';
import { getEmployeeColumns } from '../../core/constants/columns';
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
  const [currentEmployeeId, setCurrentEmployeeId] = useState(null);
  
  const [photoFile, setPhotoFile] = useState(null);
  const [photoPreview, setPhotoPreview] = useState(null);
  const [attachmentFiles, setAttachmentFiles] = useState([]);
  const [existingAttachments, setExistingAttachments] = useState([]);

  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirmDelete, confirm } = useSweetAlert();

  const { departments, offices, employeePositions, employeeStatuses } = useReferenceData();

  // ============================================================
  // BULK SELECTION - HARUS DI ATAS useEffect
  // ============================================================
  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } =
    useBulkSelection({ idField: 'employeeId' });

  const showCheckbox = useMemo(() => {
    return activeTab === 'Active' || activeTab === 'Inactive';
  }, [activeTab]);

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
    resolver: zodResolver(employeeSchema),
    defaultValues: getEmployeeDefaultValues(),
  });

  const formData = watch();

  const setFormField = useCallback((field) => (value) => {
    setValue(field, value, { shouldValidate: true });
  }, [setValue]);

  // ============================================================
  // BUILD FILTERS
  // ============================================================
  const buildFilters = useCallback(() => {
    const filters = {};
    if (activeTab === 'Active') {
      filters.isActive = true;
    } else if (activeTab === 'Inactive') {
      filters.isActive = false;
    }
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
      const result = await employeesData.fetchGridData(requestParams);
      return result;
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
    reload,
  } = useGridData(['employees', activeTab, searchTerm], fetchGridData);

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
  }, [activeTab, reload, setPage, clearSelection]);

  // ============================================================
  // OPTIONS
  // ============================================================
  const departmentOptions = useOptions(departments, 'Select Department');
  const positionOptions = useOptions(employeePositions, 'Select Position');
  const statusOptions = useOptions(employeeStatuses, 'Select Status');
  const officeOptions = useOptions(offices, 'Select Office');

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
  // CRUD OPERATIONS
  // ============================================================
  const handleCreate = useCallback(() => {
    reset(getEmployeeDefaultValues());
    setIsEditMode(false);
    setCurrentEmployeeId(null);
    setAttachmentFiles([]);
    setExistingAttachments([]);
    setPhotoFile(null);
    setPhotoPreview(null);
    setEditingEmployee(null);
    setShowModal(true);
  }, [reset]);

  const handleEdit = useCallback(async (record) => {
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
        toast.error('Failed to load employee data');
      }
    } catch (error) {
      console.warn('Failed to fetch employee:', error);
      setExistingAttachments([]);
      toast.error('Failed to load employee data');
    }
  }, [setValue, toast]);

  const handleClose = useCallback(() => {
    if (!isSubmitting) {
      setShowModal(false);
      setIsEditMode(false);
      setCurrentEmployeeId(null);
      setEditingEmployee(null);
      reset(getEmployeeDefaultValues());
      setPhotoFile(null);
      setPhotoPreview(null);
      setAttachmentFiles([]);
      setExistingAttachments([]);
    }
  }, [isSubmitting, reset]);

  // ============================================================
  // FORM SUBMIT
  // ============================================================
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
        setCurrentEmployeeId(null);
        setEditingEmployee(null);
        setShowModal(false);
        reset(getEmployeeDefaultValues());
        
        queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
        reload();
        clearSelection();
        
        const message = isEditMode && editingEmployee 
          ? 'Employee updated successfully' 
          : 'Employee created successfully';
        toast.success(message);
      } else {
        const errorMsg = result.error || result.errors?.join('\n') || 'Failed to save employee';
        toast.error(errorMsg);
      }
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to save employee';
      toast.error(errorMsg);
    }
  }, [isSubmitting, isEditMode, editingEmployee, photoFile, attachmentFiles, existingAttachments, queryClient, reload, clearSelection, toast, reset]);

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
          toast.info(`Import completed: ${r.data.successCount} success, ${r.data.errorCount} errors`);
        } else {
          toast.success(`Import completed: ${r.data?.successCount || 0} employees imported`);
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
    rowIdField: 'employeeId',
  });

  // ============================================================
  // COLUMNS
  // ============================================================
  const columns = useMemo(() => {
    return getEmployeeColumns(actionColumn);
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
          <FiCheckSquare size={16} /> {activeTab === 'Active' ? 'Deactivate' : 'Activate'} ({selectionCount})
        </Button>
      )}
    </>
  );

  if (loading && !employees.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === 'Active' ? false : true;
  const bulkTitle = bulkActivateValue ? 'Activate Employees' : 'Deactivate Employees';
  const bulkDescription = `This action will ${bulkActivateValue ? 'activate' : 'deactivate'} the selected employees.`;

  const showFileUploads = isEditMode && currentEmployeeId;

  return (
    <div className="employees-menu">
      <GridView
        title="Employee Management"
        tabs={EMPLOYEE_TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={employees}
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
        createButtonText="Add Employee"
        ariaLabel="Employees data table"
        extraActions={extraActions}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingEmployee ? 'Edit Employee' : 'Add Employee'}
        onSubmit={handleFormSubmit(onSubmit)}
        isSubmitting={isSubmitting}
        submitText={editingEmployee ? 'Update' : 'Create'}
        size="xl"
      >
        {showFileUploads && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, mb: 3, pb: 3, borderBottom: '1px solid var(--border)' }}>
            <Box sx={{ position: 'relative' }}>
              <Avatar
                src={photoPreview || (editingEmployee?.photoProfileUrl) || undefined}
                sx={{
                  width: 100,
                  height: 100,
                  bgcolor: 'var(--primary)',
                  fontSize: 40,
                  fontWeight: 600,
                  border: '3px solid var(--border)',
                }}
              >
                {!photoPreview && !editingEmployee?.photoProfileUrl && (formData.fullName?.charAt(0) || 'E')}
              </Avatar>
              <Box sx={{ position: 'absolute', bottom: -4, right: -4 }}>
                <label htmlFor="employee-photo-upload" style={{ cursor: 'pointer' }}>
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
                    id="employee-photo-upload"
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

        <FormSection title="Basic Information" description="Employee code and full name">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input
                label="Employee Code"
                {...register('employeeCode')}
                value={formData.employeeCode || ''}
                onChange={(e) => setFormField('employeeCode')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'employeeCode')}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Full Name"
                {...register('fullName')}
                value={formData.fullName || ''}
                onChange={(e) => setFormField('fullName')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'fullName')}
                required
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Contact Information" description="Address, phone, and email">
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input
                label="Address"
                {...register('address')}
                value={formData.address || ''}
                onChange={(e) => setFormField('address')(e.target.value)}
                multiline
                rows={2}
                {...getFieldErrorProps(formErrors, 'address')}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Phone Number"
                {...register('phoneNumber')}
                value={formData.phoneNumber || ''}
                onChange={(e) => setFormField('phoneNumber')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'phoneNumber')}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Email"
                type="email"
                {...register('email')}
                value={formData.email || ''}
                onChange={(e) => setFormField('email')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'email')}
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Employment Details" description="Department, position, status, and office">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Select
                label="Department"
                {...register('departmentId')}
                value={formData.departmentId || ''}
                onChange={(e) => setFormField('departmentId')(e.target.value)}
                options={departmentOptions}
                {...getFieldErrorProps(formErrors, 'departmentId')}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Position"
                {...register('position')}
                value={formData.position || ''}
                onChange={(e) => setFormField('position')(e.target.value)}
                options={positionOptions}
                {...getFieldErrorProps(formErrors, 'position')}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Employment Status"
                {...register('employmentStatus')}
                value={formData.employmentStatus || ''}
                onChange={(e) => setFormField('employmentStatus')(e.target.value)}
                options={statusOptions}
                {...getFieldErrorProps(formErrors, 'employmentStatus')}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Office"
                {...register('officeId')}
                value={formData.officeId || ''}
                onChange={(e) => setFormField('officeId')(e.target.value)}
                options={officeOptions}
                {...getFieldErrorProps(formErrors, 'officeId')}
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Dates" description="Join and resignation dates">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <DatePickerInput
                label="Join Date"
                {...register('joinDate')}
                value={formData.joinDate || ''}
                onChange={(e) => setFormField('joinDate')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'joinDate')}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <DatePickerInput
                label="Resign Date"
                {...register('resignDate')}
                value={formData.resignDate || ''}
                onChange={(e) => setFormField('resignDate')(e.target.value)}
                {...getFieldErrorProps(formErrors, 'resignDate')}
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Attachments" description="Supporting documents">
          <Grid container spacing={2}>
            {showFileUploads && (
              <Grid item xs={12}>
                <FileUploader
                  referenceType="Employee"
                  referenceId={currentEmployeeId}
                  entityType="employee"
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
        title="Import Employees"
        description="Upload Excel or TXT file with employee data. Employees will be imported as ACTIVE."
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