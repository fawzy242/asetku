import React, { useState, useMemo, useCallback, useEffect } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import DepartmentsData from "./Departments.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Modal from "../../components/molecules/Modal/Modal";
import Input from "../../components/atoms/Input/Input";
import Spinner from "../../components/atoms/Spinner/Spinner";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import SearchToolbar from "../../components/molecules/SearchToolbar/SearchToolbar";
import Tabs from "../../components/molecules/Tabs/Tabs";
import IconButton from "../../components/atoms/IconButton/IconButton";
import StatusBadge from "../../components/atoms/StatusBadge/StatusBadge";
import { useGridData } from "../../hooks/useGridData";
import { useCrudForm } from "../../hooks/useCrudForm";
import { cleanDepartmentFormData } from "../../core/utils/formHelpers";
import "./Departments.scss";

const departmentsData = new DepartmentsData();
departmentsData.transformFormData = cleanDepartmentFormData;

const INITIAL_FORM_DATA = {
  departmentName: "",
  description: "",
  departmentCode: "",
};

const TABS = [
  { id: "all", label: "All" },
  { id: "active", label: "Active" },
  { id: "inactive", label: "Inactive" },
];

const CRUD_OPTIONS = { 
  idField: 'departmentId',
};

const DepartmentsMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");
  const queryClient = useQueryClient();

  const {
    showModal,
    editingRecord: editingDepartment,
    isSubmitting,
    formData,
    setFormData,
    handleCreate,
    handleEdit,
    handleClose,
  } = useCrudForm(INITIAL_FORM_DATA, departmentsData, CRUD_OPTIONS);

  const fetchGridData = useCallback(async (params) => {
    const filters = { 
      page: params.page || 1,
      pageSize: params.pageSize || 10,
      search: params.search || searchTerm,
      ...params 
    };
    
    // Apply tab filter
    if (activeTab === "active") {
      filters.isActive = true;
    } else if (activeTab === "inactive") {
      filters.isActive = false;
    }
    
    const result = await departmentsData.fetchGridData(filters);
    return result;
  }, [activeTab, searchTerm]);

  const {
    data: departments,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    updateFilters,
    reload
  } = useGridData(['departments', activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    reload();
  }, [activeTab, reload]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    updateFilters({ search });
    setPage(1);
  }, [updateFilters, setPage]);

  const handleDelete = useCallback(async (dept) => {
    const r = await departmentsData.delete(dept.departmentId);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'departments'] });
      reload();
    }
  }, [reload, queryClient]);

  const onSubmit = useCallback(async (e) => {
    e.preventDefault();
    if (!formData.departmentName?.trim()) return;
    if (isSubmitting) return;
    
    const data = { 
      ...formData, 
      isActive: editingDepartment ? editingDepartment.isActive : true 
    };
    
    const r = editingDepartment
      ? await departmentsData.update(editingDepartment.departmentId, data)
      : await departmentsData.create(data);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'departments'] });
      handleClose();
      reload();
    }
  }, [formData, isSubmitting, editingDepartment, handleClose, reload, queryClient]);

  const columns = useMemo(() => [
    { field: "departmentCode", headerName: "Code", width: 150 },
    { field: "departmentName", headerName: "Name", flex: 1, minWidth: 200 },
    { field: "description", headerName: "Description", flex: 1, minWidth: 200 },
    { field: "employeeCount", headerName: "Employees", width: 130 },
    {
      field: "isActive",
      headerName: "Status",
      width: 110,
      renderCell: (p) => <StatusBadge status={p.value ? 'Active' : 'Inactive'} />,
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 100,
      sortable: false,
      renderCell: (p) => (
        <div className="table-actions">
          <IconButton onClick={() => handleEdit(p.row)} title="Edit department" size="lg">
            <FiEdit2 size={18} />
          </IconButton>
          <IconButton onClick={() => handleDelete(p.row)} title="Delete department" variant="danger" size="lg">
            <FiTrash2 size={18} />
          </IconButton>
        </div>
      )
    },
  ], [handleEdit, handleDelete]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
    setSearchTerm("");
  }, [setPage]);

  if (loading && !departments.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="departments-menu">
      <PageHeader 
        title="Department Management"
        actions={
          <Button variant="primary" onClick={handleCreate} startIcon={<FiPlus />}>
            Add Department
          </Button>
        }
      />

      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} placeholder="Search by name, code..." />
      
      <div className="departments-menu__table" style={{ width: '100%', minWidth: 0 }}>
        <DataTable
          rows={departments}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.departmentId}
          hideFooter={true}
          autoHeight={true}
          ariaLabel="Departments data table"
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

      <Modal isOpen={showModal} onClose={handleClose} title={editingDepartment ? "Edit Department" : "Add Department"} size="md">
        <form onSubmit={onSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input 
                label="Department Code" 
                value={formData.departmentCode || ""} 
                onChange={e => setFormData({ ...formData, departmentCode: e.target.value })} 
                placeholder="Optional (max 100 chars)"
              />
            </Grid>
            <Grid item xs={12}>
              <Input 
                label="Department Name" 
                value={formData.departmentName || ""} 
                onChange={e => setFormData({ ...formData, departmentName: e.target.value })} 
                required 
              />
            </Grid>
            <Grid item xs={12}>
              <Input 
                label="Description" 
                value={formData.description || ""} 
                onChange={e => setFormData({ ...formData, description: e.target.value })} 
                multiline 
                rows={2} 
              />
            </Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={handleClose} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>
              {editingDepartment ? "Update" : "Create"}
            </Button>
          </Box>
        </form>
      </Modal>
    </div>
  );
};

export default DepartmentsMenu;