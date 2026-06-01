import React, { useState, useMemo, useCallback, useEffect, useRef } from "react";
import { Grid, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import EmployeesData from "./Employees.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import GridFilterPanel from "../../components/molecules/GridFilterPanel/GridFilterPanel";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import DatePickerInput from "../../components/atoms/Input/DatePickerInput";
import Spinner from "../../components/atoms/Spinner/Spinner";
import IconButton from "../../components/atoms/IconButton/IconButton";
import Button from "../../components/atoms/Button/Button";
import FileUploader from "../../components/molecules/FileUploader/FileUploader";
import ImportModal from "../../components/molecules/ImportModal/ImportModal";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import { FiEdit2, FiTrash2, FiUpload, FiCheckSquare } from "react-icons/fi";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudFormBase } from "../../hooks/useCrudFormBase";
import { cleanEmployeeFormData } from "../../core/utils/formHelpers";
import "./Employees.scss";

const employeesData = new EmployeesData();
employeesData.transformFormData = cleanEmployeeFormData;

const INITIAL_FORM_DATA = {
  employeeCode: "", fullName: "", address: "", departmentId: "",
  position: "", employmentStatus: "", phoneNumber: "", email: "",
  officeId: "", joinDate: "", resignDate: "",
};

const EMPLOYEE_TABS = [
  { id: "all", label: "All" },
  { id: "Active", label: "Active" },
  { id: "Inactive", label: "Inactive" },
];

const EmployeesMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showFilters, setShowFilters] = useState(false);
  const [statusFilter, setStatusFilter] = useState("");
  const [departmentFilter, setDepartmentFilter] = useState("");
  const [selectedRows, setSelectedRows] = useState([]);
  const [showImportModal, setShowImportModal] = useState(false);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);
  const [searchTerm, setSearchTerm] = useState("");
  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();

  const { departments, offices, employeePositions, employeeStatuses } = useReferenceData();

  const {
    showModal,
    editingRecord: editingEmployee,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, employeesData, {
    idField: 'employeeId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
    },
  });

  const showCheckbox = useMemo(() => {
    return activeTab === "Active" || activeTab === "Inactive";
  }, [activeTab]);

  const fetchGridData = useCallback(async (params) => {
    const filters = { 
      page: params.page || 1,
      pageSize: params.pageSize || 10,
      search: params.search || searchTerm,
      ...params 
    };
    
    if (activeTab === "Active") {
      filters.isActive = true;
    } else if (activeTab === "Inactive") {
      filters.isActive = false;
    }
    
    if (statusFilter === "active") {
      filters.isActive = true;
    } else if (statusFilter === "inactive") {
      filters.isActive = false;
    }
    
    if (departmentFilter) {
      filters.departmentId = departmentFilter;
    }
    
    const result = await employeesData.fetchGridData(filters);
    return result;
  }, [activeTab, statusFilter, departmentFilter, searchTerm]);

  const {
    data: employees,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    reload
  } = useGridData(['employees', activeTab, statusFilter, departmentFilter, searchTerm], fetchGridData);

  useEffect(() => {
    isMountedRef.current = true;
    setPage(1);
    setSelectedRows([]);
    reload();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, setPage]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
  }, [setPage]);

  const handleDelete = useCallback(async (emp) => {
    const r = await employeesData.delete(emp.employeeId);
    if (r.success && isMountedRef.current) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
      reload();
    }
  }, [reload, queryClient]);

  const onSubmit = useCallback(async () => {
    const success = await crudHandleSubmit();
    if (success && isMountedRef.current) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
      reload();
    }
    return success;
  }, [crudHandleSubmit, reload, queryClient]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
    setStatusFilter("");
    setDepartmentFilter("");
    setSearchTerm("");
    setSelectedRows([]);
    setShowFilters(false);
  }, [setPage]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
    const r = await employeesData.bulkActivate(ids, activate);
    if (r.success && isMountedRef.current) {
      setSelectedRows([]);
      queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
      reload();
    }
  }, [reload, queryClient]);

  const handleImport = useCallback(async (file) => {
    setIsImporting(true);
    const r = await employeesData.importEmployees(file);
    setIsImporting(false);
    if (r.success && isMountedRef.current) {
      setImportResult(r.data);
      queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
      reload();
    }
  }, [reload, queryClient]);

  const handleDownloadTemplate = useCallback(async () => {
    await employeesData.downloadTemplate();
  }, []);

  const handleResetFilters = useCallback(() => {
    setStatusFilter("");
    setDepartmentFilter("");
    setSearchTerm("");
    setPage(1);
  }, [setPage]);

  const departmentOptions = useMemo(() => [
    { value: "", label: "All Departments" },
    ...departments.map(d => ({ value: d.value, label: d.label }))
  ], [departments]);

  const positionOptions = useMemo(() => [
    { value: "", label: "Select Position" },
    ...employeePositions.map(p => ({ value: p.value, label: p.label }))
  ], [employeePositions]);

  const statusOptions = useMemo(() => [
    { value: "", label: "Select Status" },
    ...employeeStatuses.map(s => ({ value: s.value, label: s.label }))
  ], [employeeStatuses]);

  const officeOptions = useMemo(() => [
    { value: "", label: "None" },
    ...offices.map(o => ({ value: o.value, label: o.label }))
  ], [offices]);

  const filterStatusOptions = useMemo(() => [
    { value: "", label: "All Status" },
    { value: "active", label: "Active" },
    { value: "inactive", label: "Inactive" },
  ], []);

  const columns = useMemo(() => [
    { field: "employeeCode", headerName: "Code", width: 120 },
    { field: "fullName", headerName: "Full Name", flex: 1, minWidth: 180 },
    { field: "departmentName", headerName: "Department", width: 150 },
    { field: "positionName", headerName: "Position", width: 150 },
    { field: "email", headerName: "Email", width: 200 },
    { field: "officeName", headerName: "Office", width: 150 },
    { 
      field: "employmentStatusName", 
      headerName: "Status", 
      width: 120, 
      renderCell: (p) => <Chip label={p?.value || '-'} size="small" sx={getStatusChipStyles(p?.value)} /> 
    },
    { 
      field: "actions", 
      headerName: "Actions", 
      width: 100, 
      sortable: false, 
      renderCell: (p) => (
        <div className="table-actions">
          <IconButton onClick={() => handleEdit(p?.row)} title="Edit employee" size="lg"><FiEdit2 size={18} /></IconButton>
          <IconButton onClick={() => handleDelete(p?.row)} title="Delete employee" variant="danger" size="lg"><FiTrash2 size={18} /></IconButton>
        </div>
      )
    },
  ], [handleEdit, handleDelete]);

  const filterContent = (
    <>
      <Select 
        label="Status" 
        value={statusFilter} 
        onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} 
        options={filterStatusOptions} 
      />
      <Select 
        label="Department" 
        value={departmentFilter} 
        onChange={(e) => { setDepartmentFilter(e.target.value); setPage(1); }} 
        options={departmentOptions} 
      />
    </>
  );

  const extraActions = (
    <>
      <Button variant="outline" onClick={() => setShowImportModal(true)} startIcon={<FiUpload />}>
        Import
      </Button>
      {selectedRows.length > 0 && showCheckbox && (
        <Button variant="primary" onClick={() => setShowBulkActivateModal(true)} startIcon={<FiCheckSquare />}>
          {activeTab === "Active" ? "Deactivate" : "Activate"} ({selectedRows.length})
        </Button>
      )}
    </>
  );

  if (loading && !employees.length) return <div className="page-loading"><Spinner size="lg" /></div>;

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
        page={page}
        totalPages={Math.ceil(totalCount / pageSize) || 1}
        pageSize={pageSize}
        totalItems={totalCount}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onSearch={handleSearch}
        showCheckbox={showCheckbox}
        selectedRows={selectedRows}
        onSelectionChange={setSelectedRows}
        createButtonText="Add Employee"
        ariaLabel="Employees data table"
        extraActions={extraActions}
        filterChildren={filterContent}
        showFilters={showFilters}
        onFilterToggle={() => setShowFilters(!showFilters)}
        onResetFilters={handleResetFilters}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingEmployee ? "Edit Employee" : "Add Employee"}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingEmployee ? "Update" : "Create"}
        size="lg"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Input label="Employee Code" value={formData.employeeCode || ""} onChange={(e) => setFormField('employeeCode')(e.target.value)} required />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input label="Full Name" value={formData.fullName || ""} onChange={(e) => setFormField('fullName')(e.target.value)} required />
          </Grid>
          <Grid item xs={12}>
            <Input label="Address" value={formData.address || ""} onChange={(e) => setFormField('address')(e.target.value)} multiline rows={2} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select label="Department" value={formData.departmentId || ""} onChange={(e) => setFormField('departmentId')(e.target.value)} options={departmentOptions} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select label="Position" value={formData.position || ""} onChange={(e) => setFormField('position')(e.target.value)} options={positionOptions} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select label="Employment Status" value={formData.employmentStatus || ""} onChange={(e) => setFormField('employmentStatus')(e.target.value)} options={statusOptions} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select label="Office" value={formData.officeId || ""} onChange={(e) => setFormField('officeId')(e.target.value)} options={officeOptions} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input label="Phone Number" value={formData.phoneNumber || ""} onChange={(e) => setFormField('phoneNumber')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input label="Email" type="email" value={formData.email || ""} onChange={(e) => setFormField('email')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <DatePickerInput label="Join Date" value={formData.joinDate || ""} onChange={(e) => setFormField('joinDate')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <DatePickerInput label="Resign Date" value={formData.resignDate || ""} onChange={(e) => setFormField('resignDate')(e.target.value)} />
          </Grid>
          <Grid item xs={12}>
            <FileUploader 
              referenceTable="Employee"
              referenceId={editingEmployee?.employeeId}
              onUploadComplete={reload}
            />
          </Grid>
        </Grid>
      </CrudModal>

      <ImportModal
        isOpen={showImportModal}
        onClose={() => { setShowImportModal(false); setImportResult(null); }}
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
        onConfirm={(ids) => handleBulkActivate(ids, activeTab === "Active" ? false : true)}
        selectedIds={selectedRows}
        itemName="employees"
        title={activeTab === "Active" ? "Deactivate Employees" : "Activate Employees"}
        description={`This action will ${activeTab === "Active" ? "deactivate" : "activate"} the selected employees.`}
      />
    </div>
  );
};

export default EmployeesMenu;