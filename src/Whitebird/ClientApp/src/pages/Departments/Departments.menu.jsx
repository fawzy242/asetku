import React, { useState, useMemo, useCallback, useEffect } from "react";
import { Grid, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import DepartmentsData from "./Departments.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import Input from "../../components/atoms/Input/Input";
import Spinner from "../../components/atoms/Spinner/Spinner";
import IconButton from "../../components/atoms/IconButton/IconButton";
import Button from "../../components/atoms/Button/Button";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { FiEdit2, FiTrash2, FiCheckSquare } from "react-icons/fi";
import { useGridData } from "../../hooks/useGridData";
import { useCrudFormBase } from "../../hooks/useCrudFormBase";
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

const DepartmentsMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedRows, setSelectedRows] = useState([]);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const queryClient = useQueryClient();

  const {
    showModal,
    editingRecord: editingDepartment,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, departmentsData, {
    idField: 'departmentId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reference', 'departments'] });
    },
  });

  const showCheckbox = activeTab === "active" || activeTab === "inactive";

  const buildFilters = useCallback(() => {
    const filters = {};
    if (activeTab === "active") {
      filters.isActive = true;
    } else if (activeTab === "inactive") {
      filters.isActive = false;
    }
    if (searchTerm) {
      filters.search = searchTerm;
    }
    return filters;
  }, [activeTab, searchTerm]);

  const fetchGridData = useCallback(async (params) => {
    const filters = buildFilters();
    const requestParams = {
      page: params.page || 1,
      pageSize: params.pageSize || 10,
      ...filters,
      ...params,
    };
    
    const result = await departmentsData.fetchGridData(requestParams);
    return result;
  }, [buildFilters]);

  const {
    data: departments,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    reload
  } = useGridData(['departments', activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    reload();
    setSelectedRows([]);
  }, [activeTab, reload]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
    setSelectedRows([]);
  }, [setPage]);

  const handleDelete = useCallback(async (dept) => {
    const r = await departmentsData.delete(dept.departmentId);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'departments'] });
      reload();
    }
  }, [reload, queryClient]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
    let successCount = 0;
    for (const id of ids) {
      const result = await departmentsData.fetchById(id);
      if (result.success && result.data) {
        const updateData = { ...result.data, isActive: activate };
        const updateResult = await departmentsData.update(id, updateData);
        if (updateResult.success) successCount++;
      }
    }
    if (successCount > 0) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'departments'] });
      reload();
      setSelectedRows([]);
    }
    return successCount;
  }, [reload, queryClient]);

  const onSubmit = useCallback(async () => {
    const submitData = { 
      ...formData, 
      isActive: editingDepartment ? editingDepartment.isActive : true 
    };
    
    Object.keys(submitData).forEach(key => {
      setFormField(key)(submitData[key]);
    });
    
    const success = await crudHandleSubmit();
    if (success) {
      reload();
    }
    return success;
  }, [formData, editingDepartment, crudHandleSubmit, setFormField, reload]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
    setSearchTerm("");
    setSelectedRows([]);
  }, [setPage]);

  const columns = useMemo(() => [
    { field: "departmentCode", headerName: "Code", width: 150 },
    { field: "departmentName", headerName: "Name", flex: 1, minWidth: 200 },
    { field: "description", headerName: "Description", flex: 1, minWidth: 200 },
    { field: "employeeCount", headerName: "Employees", width: 130 },
    {
      field: "isActive",
      headerName: "Status",
      width: 110,
      renderCell: (p) => {
        const status = p.value ? 'Active' : 'Inactive';
        return <Chip label={status} size="small" sx={getStatusChipStyles(status)} />;
      },
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

  const extraActions = (
    <>
      {selectedRows.length > 0 && showCheckbox && (
        <Button variant="primary" onClick={() => setShowBulkActivateModal(true)} startIcon={<FiCheckSquare />}>
          {activeTab === "active" ? "Deactivate" : "Activate"} ({selectedRows.length})
        </Button>
      )}
    </>
  );

  if (loading && !departments.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === "active" ? false : true;
  const bulkButtonText = activeTab === "active" ? "Deactivate" : "Activate";

  return (
    <div className="departments-menu">
      <GridView
        title="Department Management"
        tabs={TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={departments}
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
        createButtonText="Add Department"
        ariaLabel="Departments data table"
        extraActions={extraActions}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingDepartment ? "Edit Department" : "Add Department"}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingDepartment ? "Update" : "Create"}
        size="md"
      >
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Input 
              label="Department Code" 
              value={formData.departmentCode || ""} 
              onChange={(e) => setFormField('departmentCode')(e.target.value)} 
              placeholder="Optional (max 100 chars)"
            />
          </Grid>
          <Grid item xs={12}>
            <Input 
              label="Department Name" 
              value={formData.departmentName || ""} 
              onChange={(e) => setFormField('departmentName')(e.target.value)} 
              required 
            />
          </Grid>
          <Grid item xs={12}>
            <Input 
              label="Description" 
              value={formData.description || ""} 
              onChange={(e) => setFormField('description')(e.target.value)} 
              multiline 
              rows={2} 
            />
          </Grid>
        </Grid>
      </CrudModal>

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={selectedRows}
        itemName="departments"
        title={bulkButtonText === "Activate" ? "Activate Departments" : "Deactivate Departments"}
        description={`This action will ${bulkButtonText.toLowerCase()} the selected departments.`}
      />
    </div>
  );
};

export default DepartmentsMenu;