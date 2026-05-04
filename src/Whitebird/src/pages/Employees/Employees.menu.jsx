import React, { useState, useMemo, useCallback, useEffect } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box, Chip } from "@mui/material";
import EmployeesData from "./Employees.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Modal from "../../components/molecules/Modal/Modal";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import SearchToolbar from "../../components/molecules/SearchToolbar/SearchToolbar";
import Tabs from "../../components/molecules/Tabs/Tabs";
import FilterPanel from "../../components/molecules/FilterPanel/FilterPanel";
import IconButton from "../../components/atoms/IconButton/IconButton";
import { useGridData } from "../../hooks/useGridData";
import { useCrudForm } from "../../hooks/useCrudForm";
import "./Employees.scss";

const employeesData = new EmployeesData();

const STATUS_OPTIONS = [
  { value: "", label: "All Status" },
  { value: "Active", label: "Active" },
  { value: "Resigned", label: "Resigned" },
  { value: "On Leave", label: "On Leave" },
];

const EMPLOYMENT_OPTIONS = [
  { value: "Active", label: "Active" },
  { value: "Resigned", label: "Resigned" },
  { value: "On Leave", label: "On Leave" },
];

const STATUS_CHIP_COLORS = {
  'Active': { bg: 'rgba(16, 185, 129, 0.1)', color: '#10b981' },
  'Resigned': { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280' },
  'On Leave': { bg: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b' },
};

const INITIAL_FORM_DATA = {
  fullName: "", department: "", position: "", division: "", branch: "",
  costCenter: "", phoneNumber: "", email: "", officeLocation: "",
  employmentStatus: "Active", joinDate: "",
};

const transformEmployeeFormData = (data) => ({
  ...data,
  isActive: data.employmentStatus === "Active",
});

const CRUD_OPTIONS = {
  idField: 'employeeId',
  transformFormData: transformEmployeeFormData,
};

const TABS = [
  { id: "all", label: "All" },
  { id: "active", label: "Active" },
  { id: "inactive", label: "Inactive" },
];

const EmployeesMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showFilters, setShowFilters] = useState(false);
  const [statusFilter, setStatusFilter] = useState("");
  const [departmentFilter, setDepartmentFilter] = useState("");
  const [departments, setDepartments] = useState([]);

  const {
    showModal,
    editingRecord: editingEmployee,
    isSubmitting,
    formData,
    setFormData,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudForm(INITIAL_FORM_DATA, employeesData, CRUD_OPTIONS);

  const departmentOptions = useMemo(() => {
    const base = [{ value: "", label: "All Departments" }];
    if (Array.isArray(departments)) {
      departments.forEach(d => { if (d) base.push({ value: d, label: d }); });
    }
    return base;
  }, [departments]);

  const fetchGridData = useCallback(async (params) => {
    let apiStatus = "";
    if (activeTab === "active") apiStatus = "Active";
    else if (activeTab === "inactive") apiStatus = "Resigned";
    const filters = { ...params };
    if (apiStatus) filters.status = apiStatus;
    if (statusFilter) filters.status = statusFilter;
    if (departmentFilter) filters.department = departmentFilter;
    return employeesData.fetchGridData(filters);
  }, [activeTab, statusFilter, departmentFilter]);

  const {
    data: employees,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    updateFilters,
    reload
  } = useGridData(['employees', activeTab, statusFilter, departmentFilter], fetchGridData);

  useEffect(() => {
    employeesData.fetchDepartments().then(r => {
      if (r.success && Array.isArray(r.data)) {
        setDepartments(r.data.filter(Boolean));
      }
    });
  }, []);

  const handleSearch = useCallback((search) => updateFilters({ search }), [updateFilters]);

  const handleDelete = useCallback(async (emp) => {
    const r = await employeesData.delete(emp.employeeId);
    if (r.success) reload();
  }, [reload]);

  const onSubmit = useCallback(async (e) => {
    e.preventDefault();
    const success = await crudHandleSubmit();
    if (success) reload();
  }, [crudHandleSubmit, reload]);

  const columns = useMemo(() => [
    { field: "employeeCode", headerName: "Code", width: 120 },
    { field: "fullName", headerName: "Full Name", flex: 1, minWidth: 180 },
    { field: "department", headerName: "Department", width: 150 },
    { field: "position", headerName: "Position", width: 150 },
    { field: "email", headerName: "Email", width: 200 },
    {
      field: "employmentStatus",
      headerName: "Status",
      width: 120,
      renderCell: (p) => {
        const colors = STATUS_CHIP_COLORS[p.value] || STATUS_CHIP_COLORS['Active'];
        return (
          <Chip
            label={p.value || '-'}
            size="small"
            sx={{
              bgcolor: colors.bg,
              color: colors.color,
              fontWeight: 500,
              fontSize: '0.75rem',
              height: 24,
              borderRadius: '4px',
            }}
          />
        );
      },
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 100,
      sortable: false,
      renderCell: (p) => (
        <div className="table-actions">
          <IconButton onClick={() => handleEdit(p.row)} title="Edit employee" size="lg">
            <FiEdit2 size={18} />
          </IconButton>
          <IconButton onClick={() => handleDelete(p.row)} title="Delete employee" variant="danger" size="lg">
            <FiTrash2 size={18} />
          </IconButton>
        </div>
      )
    },
  ], [handleEdit, handleDelete]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
  }, [setPage]);

  if (loading && !employees.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="employees-menu">
      <PageHeader title="Employee Management" buttonText="Add Employee" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} onFilterToggle={() => setShowFilters(!showFilters)} showFilters={showFilters} placeholder="Search by name, email..." />

      <FilterPanel visible={showFilters}>
        <Select label="Status" value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} options={STATUS_OPTIONS} />
        <Select label="Department" value={departmentFilter} onChange={(e) => { setDepartmentFilter(e.target.value); setPage(1); }} options={departmentOptions} />
      </FilterPanel>

      <div className="employees-menu__table">
        <DataTable
          rows={employees}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.employeeId}
          hideFooter={true}
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

      <Modal isOpen={showModal} onClose={handleClose} title={editingEmployee ? "Edit Employee" : "Add Employee"} size="lg">
        <form onSubmit={onSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input label="Full Name" value={formData.fullName} onChange={e => setFormData({ ...formData, fullName: e.target.value })} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Department" value={formData.department} onChange={e => setFormData({ ...formData, department: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Position" value={formData.position} onChange={e => setFormData({ ...formData, position: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Division" value={formData.division} onChange={e => setFormData({ ...formData, division: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Branch" value={formData.branch} onChange={e => setFormData({ ...formData, branch: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Cost Center" value={formData.costCenter} onChange={e => setFormData({ ...formData, costCenter: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Phone" value={formData.phoneNumber} onChange={e => setFormData({ ...formData, phoneNumber: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Email" type="email" value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Office Location" value={formData.officeLocation} onChange={e => setFormData({ ...formData, officeLocation: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Employment Status" value={formData.employmentStatus || "Active"}
                onChange={e => setFormData({ ...formData, employmentStatus: e.target.value || "Active" })}
                options={EMPLOYMENT_OPTIONS} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Join Date" type="date" value={formData.joinDate} onChange={e => setFormData({ ...formData, joinDate: e.target.value })} />
            </Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={handleClose} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>
              {editingEmployee ? "Update" : "Create"}
            </Button>
          </Box>
        </form>
      </Modal>
    </div>
  );
};

export default EmployeesMenu;