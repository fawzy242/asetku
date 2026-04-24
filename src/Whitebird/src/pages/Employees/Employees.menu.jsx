import React, { useState, useEffect, useMemo, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box } from "@mui/material";
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
import { useGridData } from "../../hooks/useGridData";
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

const EmployeesMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showFilters, setShowFilters] = useState(false);
  const [statusFilter, setStatusFilter] = useState("");
  const [departmentFilter, setDepartmentFilter] = useState("");
  const [showModal, setShowModal] = useState(false);
  const [editingEmployee, setEditingEmployee] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [departments, setDepartments] = useState([]);
  const [formData, setFormData] = useState({
    fullName: "", department: "", position: "", division: "", branch: "",
    costCenter: "", phoneNumber: "", email: "", officeLocation: "",
    employmentStatus: "Active", joinDate: ""
  });

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

  const { data: employees, totalCount, loading, page, setPage, pageSize, setPageSize, updateFilters, reload } = useGridData(fetchGridData);

  useEffect(() => {
    employeesData.fetchDepartments().then(r => {
      if (r.success && Array.isArray(r.data)) {
        setDepartments(r.data.filter(Boolean));
      }
    });
  }, []);

  const handleSearch = (search) => updateFilters({ search });

  const resetForm = () => ({
    fullName: "", department: "", position: "", division: "", branch: "",
    costCenter: "", phoneNumber: "", email: "", officeLocation: "",
    employmentStatus: "Active", joinDate: ""
  });

  const handleCreate = () => {
    setEditingEmployee(null);
    setFormData(resetForm());
    setShowModal(true);
  };

  const handleEdit = async (emp) => {
    const r = await employeesData.fetchById(emp.employeeId);
    if (r.success && r.data) {
      const d = r.data;
      setEditingEmployee(d);
      setFormData({
        fullName: d.fullName || "",
        department: d.department || "",
        position: d.position || "",
        division: d.division || "",
        branch: d.branch || "",
        costCenter: d.costCenter || "",
        phoneNumber: d.phoneNumber || "",
        email: d.email || "",
        officeLocation: d.officeLocation || "",
        employmentStatus: d.employmentStatus || "Active",
        joinDate: d.joinDate ? d.joinDate.split("T")[0] : ""
      });
      setShowModal(true);
    }
  };

  const handleDelete = async (emp) => {
    const r = await employeesData.delete(emp.employeeId);
    if (r.success) reload();
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (isSubmitting) return;
    setIsSubmitting(true);
    const data = { ...formData, isActive: formData.employmentStatus === "Active" };
    const r = editingEmployee
      ? await employeesData.update(editingEmployee.employeeId, data)
      : await employeesData.create(data);
    setIsSubmitting(false);
    if (r.success) { setShowModal(false); reload(); }
  };

  const columns = useMemo(() => [
    { field: "employeeCode", headerName: "Code", width: 120 },
    { field: "fullName", headerName: "Full Name", width: 200 },
    { field: "department", headerName: "Department", width: 150 },
    { field: "position", headerName: "Position", width: 150 },
    { field: "email", headerName: "Email", width: 200 },
    {
      field: "employmentStatus", headerName: "Status", width: 120,
      renderCell: (p) => (
        <span className={`status-badge ${(p.value || '') === 'Active' ? 'status-badge--success' : 'status-badge--secondary'}`}>
          {p.value || '-'}
        </span>
      )
    },
    {
      field: "actions", headerName: "Actions", width: 100, sortable: false,
      renderCell: (p) => (
        <div className="table-actions">
          <button className="icon-btn icon-btn--lg" onClick={() => handleEdit(p.row)} title="Edit"><FiEdit2 size={18} /></button>
          <button className="icon-btn icon-btn--danger icon-btn--lg" onClick={() => handleDelete(p.row)} title="Delete"><FiTrash2 size={18} /></button>
        </div>
      )
    },
  ], []);

  const tabs = [
    { id: "all", label: "All" },
    { id: "active", label: "Active" },
    { id: "inactive", label: "Inactive" }
  ];

  if (loading && !employees.length) return <div className="employees-loading"><Spinner size="lg" /></div>;

  return (
    <div className="employees-menu">
      <PageHeader title="Employee Management" buttonText="Add Employee" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      <Tabs tabs={tabs} activeTab={activeTab} onTabChange={(tab) => { setActiveTab(tab); setPage(1); }} />
      <SearchToolbar onSearch={handleSearch} onFilterToggle={() => setShowFilters(!showFilters)} showFilters={showFilters} placeholder="Search by name, email..." />

      {showFilters && (
        <Box sx={{ display: 'flex', gap: 2, mb: 3, p: 2, bgcolor: 'var(--surface)', borderRadius: 2 }}>
          <Select label="Status" value={statusFilter || ""} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} options={STATUS_OPTIONS} />
          <Select label="Department" value={departmentFilter || ""} onChange={(e) => { setDepartmentFilter(e.target.value); setPage(1); }} options={departmentOptions} />
        </Box>
      )}

      <div className="employees-menu__table">
        <DataTable rows={employees} columns={columns} loading={loading} pageSize={pageSize} getRowId={(row) => row.employeeId} hideFooter={true} />
      </div>

      <Pagination currentPage={page} totalPages={Math.ceil(totalCount / pageSize)} pageSize={pageSize} totalItems={totalCount} onPageChange={setPage} onPageSizeChange={setPageSize} />

      <Modal isOpen={showModal} onClose={() => !isSubmitting && setShowModal(false)} title={editingEmployee ? "Edit Employee" : "Add Employee"} size="lg">
        <form onSubmit={handleSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}><Input label="Full Name" value={formData.fullName} onChange={e => setFormData({...formData, fullName: e.target.value})} required /></Grid>
            <Grid item xs={12} sm={6}><Input label="Department" value={formData.department} onChange={e => setFormData({...formData, department: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Position" value={formData.position} onChange={e => setFormData({...formData, position: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Division" value={formData.division} onChange={e => setFormData({...formData, division: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Branch" value={formData.branch} onChange={e => setFormData({...formData, branch: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Cost Center" value={formData.costCenter} onChange={e => setFormData({...formData, costCenter: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Phone" value={formData.phoneNumber} onChange={e => setFormData({...formData, phoneNumber: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Email" type="email" value={formData.email} onChange={e => setFormData({...formData, email: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Office Location" value={formData.officeLocation} onChange={e => setFormData({...formData, officeLocation: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Employment Status" value={formData.employmentStatus || "Active"} onChange={e => setFormData({...formData, employmentStatus: e.target.value || "Active"})} options={EMPLOYMENT_OPTIONS} />
            </Grid>
            <Grid item xs={12} sm={6}><Input label="Join Date" type="date" value={formData.joinDate} onChange={e => setFormData({...formData, joinDate: e.target.value})} /></Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={() => setShowModal(false)} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>{editingEmployee ? "Update" : "Create"}</Button>
          </Box>
        </form>
      </Modal>
    </div>
  );
};

export default EmployeesMenu;