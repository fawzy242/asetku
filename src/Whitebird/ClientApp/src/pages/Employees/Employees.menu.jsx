import React, { useState, useMemo, useCallback, useEffect } from "react";
import { FiEdit2, FiTrash2, FiPlus, FiUpload, FiCheckSquare } from "react-icons/fi";
import { Grid, Box, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import EmployeesData from "./Employees.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Modal from "../../components/molecules/Modal/Modal";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import DatePickerInput from "../../components/atoms/Input/DatePickerInput";
import Spinner from "../../components/atoms/Spinner/Spinner";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import SearchToolbar from "../../components/molecules/SearchToolbar/SearchToolbar";
import Tabs from "../../components/molecules/Tabs/Tabs";
import FilterPanel from "../../components/molecules/FilterPanel/FilterPanel";
import IconButton from "../../components/atoms/IconButton/IconButton";
import ImportModal from "../../components/molecules/ImportModal/ImportModal";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudForm } from "../../hooks/useCrudForm";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import utilsHelper from "../../core/utils/utils.helper";
import "./Employees.scss";

const employeesData = new EmployeesData();

// UPDATED: Removed obsolete fields, added new fields
const INITIAL_FORM_DATA = {
  employeeCode: "",
  fullName: "",
  address: "",
  departmentId: "",
  position: "",
  employmentStatus: "",
  phoneNumber: "",
  email: "",
  officeId: "",
  joinDate: "",
  resignDate: "",
};

const NULLABLE_STRING_FIELDS = ['address', 'phoneNumber', 'email'];
const NULLABLE_INT_FIELDS = ['departmentId', 'position', 'employmentStatus', 'officeId'];

const transformEmployeeFormData = (data) => {
  const result = { ...data };
  NULLABLE_STRING_FIELDS.forEach(f => {
    if (result[f] === '' || result[f] === undefined) result[f] = null;
  });
  NULLABLE_INT_FIELDS.forEach(f => {
    if (result[f] === '' || result[f] === null || result[f] === undefined) result[f] = null;
    else if (typeof result[f] === 'string') result[f] = parseInt(result[f], 10);
  });
  if (result.joinDate === '' || result.joinDate === undefined) result.joinDate = null;
  if (result.resignDate === '' || result.resignDate === undefined) result.resignDate = null;
  return result;
};

employeesData.transformFormData = transformEmployeeFormData;

const CRUD_OPTIONS = { idField: 'employeeId' };

const TABS = [
  { id: "all", label: "All" },
  { id: "active", label: "Active" },
  { id: "inactive", label: "Inactive" },
];

const STATUS_OPTIONS = [
  { value: "", label: "All Status" },
  { value: "active", label: "Active" },
  { value: "inactive", label: "Inactive" },
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
  const queryClient = useQueryClient();

  const { departments, offices, employeePositions, employeeStatuses } = useReferenceData();

  const {
    showModal, editingRecord: editingEmployee, isSubmitting,
    formData, setFormData, handleCreate, handleEdit, handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudForm(INITIAL_FORM_DATA, employeesData, CRUD_OPTIONS);

  const fetchGridData = useCallback(async (params) => {
    const filters = { ...params };
    
    if (activeTab === "active") filters.isActive = true;
    else if (activeTab === "inactive") filters.isActive = false;
    
    if (statusFilter === "active") filters.isActive = true;
    else if (statusFilter === "inactive") filters.isActive = false;
    
    if (departmentFilter) filters.departmentId = departmentFilter;
    
    return employeesData.fetchGridData(filters);
  }, [activeTab, statusFilter, departmentFilter]);

  const {
    data: employees, totalCount, loading, page, setPage,
    pageSize, setPageSize, updateFilters, reload
  } = useGridData(['employees', activeTab, statusFilter, departmentFilter], fetchGridData);

  const handleSearch = useCallback((search) => updateFilters({ search }), [updateFilters]);
  
  const handleDelete = useCallback(async (emp) => {
    const r = await employeesData.delete(emp.employeeId);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
      reload();
    }
  }, [reload, queryClient]);
  
  const onSubmit = useCallback(async (e) => {
    e.preventDefault();
    const success = await crudHandleSubmit();
    if (success) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
      reload();
    }
  }, [crudHandleSubmit, reload, queryClient]);
  
  const handleBulkActivate = useCallback(async (ids, activate) => {
    const r = await employeesData.bulkActivate(ids, activate);
    if (r.success) {
      setSelectedRows([]);
      queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
      reload();
    }
  }, [reload, queryClient]);
  
  const handleImport = useCallback(async (file) => {
    setIsImporting(true);
    const r = await employeesData.importEmployees(file);
    setIsImporting(false);
    if (r.success) {
      setImportResult(r.data);
      queryClient.invalidateQueries({ queryKey: ['reference', 'employees'] });
      reload();
    }
  }, [reload, queryClient]);
  
  const handleDownloadTemplate = useCallback(async () => {
    await employeesData.downloadTemplate();
  }, []);

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

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
  }, [setPage]);
  
  const filterDepartmentOptions = useMemo(() => [
    { value: "", label: "All Departments" },
    ...departments.map(d => ({ value: d.value, label: d.label }))
  ], [departments]);

  if (loading && !employees.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="employees-menu">
      <div className="page-header">
        <h1 className="page-title">Employee</h1>
        <div style={{ display: 'flex', gap: '12px' }}>
          <Button variant="outline" onClick={() => setShowImportModal(true)} startIcon={<FiUpload />}>
            Import
          </Button>
          {selectedRows.length > 0 && (
            <Button variant="primary" onClick={() => setShowBulkActivateModal(true)} startIcon={<FiCheckSquare />}>
              Activate ({selectedRows.length})
            </Button>
          )}
          <PageHeader title="Employee Management" buttonText="Add Employee" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
        </div>
      </div>
     
      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar 
        onSearch={handleSearch} 
        onFilterToggle={() => setShowFilters(!showFilters)} 
        showFilters={showFilters} 
        placeholder="Search by name, code, email..." 
      />
      <FilterPanel visible={showFilters}>
        <Select 
          label="Status" 
          value={statusFilter} 
          onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} 
          options={STATUS_OPTIONS} 
        />
        <Select 
          label="Department" 
          value={departmentFilter} 
          onChange={(e) => { setDepartmentFilter(e.target.value); setPage(1); }} 
          options={filterDepartmentOptions} 
        />
      </FilterPanel>
      
      <div className="employees-menu__table">
        <DataTable 
          rows={employees} 
          columns={columns} 
          loading={loading} 
          pageSize={pageSize} 
          getRowId={(row) => row.employeeId} 
          hideFooter={true} 
          checkboxSelection={true}
          onSelectionChange={(newSelection) => setSelectedRows(newSelection)}
          ariaLabel="Employees data table" 
        />
      </div>
      <Pagination 
        currentPage={page} 
        totalPages={Math.ceil(totalCount / pageSize) || 1} 
        pageSize={pageSize} 
        totalItems={totalCount} 
        onPageChange={setPage} 
        onPageSizeChange={setPageSize} 
      />

      {/* Create/Edit Modal */}
      <Modal isOpen={showModal} onClose={handleClose} title={editingEmployee ? "Edit Employee" : "Add Employee"} size="lg">
        <form onSubmit={onSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input 
                label="Employee Code" 
                value={formData.employeeCode || ""} 
                onChange={e => setFormData({ ...formData, employeeCode: e.target.value })} 
                required 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input 
                label="Full Name" 
                value={formData.fullName || ""} 
                onChange={e => setFormData({ ...formData, fullName: e.target.value })} 
                required 
              />
            </Grid>
            <Grid item xs={12}>
              <Input 
                label="Address" 
                value={formData.address || ""} 
                onChange={e => setFormData({ ...formData, address: e.target.value })} 
                multiline 
                rows={2} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Department" 
                value={formData.departmentId || ""} 
                onChange={e => setFormData({ ...formData, departmentId: e.target.value })} 
                options={departmentOptions} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Position" 
                value={formData.position || ""} 
                onChange={e => setFormData({ ...formData, position: e.target.value })} 
                options={positionOptions} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Employment Status" 
                value={formData.employmentStatus || ""} 
                onChange={e => setFormData({ ...formData, employmentStatus: e.target.value })} 
                options={statusOptions} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Office" 
                value={formData.officeId || ""} 
                onChange={e => setFormData({ ...formData, officeId: e.target.value })} 
                options={officeOptions} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input 
                label="Phone Number" 
                value={formData.phoneNumber || ""} 
                onChange={e => setFormData({ ...formData, phoneNumber: e.target.value })} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input 
                label="Email" 
                type="email" 
                value={formData.email || ""} 
                onChange={e => setFormData({ ...formData, email: e.target.value })} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <DatePickerInput 
                label="Join Date" 
                value={formData.joinDate || ""} 
                onChange={e => setFormData({ ...formData, joinDate: e.target.value })} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <DatePickerInput 
                label="Resign Date" 
                value={formData.resignDate || ""} 
                onChange={e => setFormData({ ...formData, resignDate: e.target.value })} 
              />
            </Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={handleClose} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>{editingEmployee ? "Update" : "Create"}</Button>
          </Box>
        </form>
      </Modal>

      {/* Import Modal */}
      <ImportModal
        isOpen={showImportModal}
        onClose={() => { setShowImportModal(false); setImportResult(null); }}
        onImport={handleImport}
        onDownloadTemplate={handleDownloadTemplate}
        isImporting={isImporting}
        importResult={importResult}
        title="Import Employees"
        description="Upload Excel or TXT file with employee data. Employees will be imported as INACTIVE and need activation."
      />

      {/* Bulk Activate Modal */}
      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={handleBulkActivate}
        selectedIds={selectedRows}
        itemName="employees"
      />
    </div>
  );
};

export default EmployeesMenu;