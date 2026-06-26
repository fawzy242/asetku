import React, { useState, useMemo, useCallback, useEffect } from "react";
import { Grid } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import DepartmentsData from "./Departments.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import FormSection from "../../components/atoms/FormSection/FormSection";
import Input from "../../components/atoms/Input/Input";
import Spinner from "../../components/atoms/Spinner/Spinner";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import StatusChip from "../../components/atoms/StatusChip/StatusChip";
import Button from "../../components/atoms/Button/Button";
import { FiCheckSquare } from "react-icons/fi";
import { ACTION_TYPES, useGridActions } from "../../hooks/useGridActions";
import { useBulkSelection } from "../../hooks/useBulkSelection";
import { useSweetAlert } from "../../hooks/useSweetAlert";
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
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const queryClient = useQueryClient();
  const { confirm, modal } = useSweetAlert(); // HAPUS confirmDelete

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
    idField: "departmentId",
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reference", "departments"] });
    },
  });

  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } = useBulkSelection({ idField: "departmentId" });

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
  } = useGridData(["departments", activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    reload();
    clearSelection();
  }, [activeTab, reload, clearSelection]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
    clearSelection();
  }, [setPage, clearSelection]);

  // HAPUS confirmDelete - BaseData yang handle
  const handleDelete = useCallback(async (dept) => {
    const r = await departmentsData.delete(dept.departmentId);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ["reference", "departments"] });
      reload();
      clearSelection();
    }
  }, [reload, queryClient, clearSelection]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
    const actionText = activate ? "activate" : "deactivate";
    const confirmed = await confirm({
      title: activate ? "Activate Departments" : "Deactivate Departments",
      text: `Are you sure you want to ${actionText} ${ids.length} department(s)?`,
      confirmButtonText: activate ? "Yes, Activate" : "Yes, Deactivate",
    });
    if (!confirmed) return;

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
      queryClient.invalidateQueries({ queryKey: ["reference", "departments"] });
      reload();
      clearSelection();
    }
  }, [reload, queryClient, confirm, clearSelection]);

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
      clearSelection();
    }
    return success;
  }, [formData, editingDepartment, crudHandleSubmit, setFormField, reload, clearSelection]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
    setSearchTerm("");
    clearSelection();
  }, [setPage, clearSelection]);

  const handleGridAction = useCallback((actionType, row) => {
    switch (actionType) {
      case ACTION_TYPES.EDIT:
        handleEdit(row);
        break;
      case ACTION_TYPES.DELETE:
        handleDelete(row);
        break;
      default:
        break;
    }
  }, [handleEdit, handleDelete]);

  const getConditionalActions = useCallback(() => {
    return [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE];
  }, []);

  const { actionColumn } = useGridActions({
    actions: [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleGridAction,
    getConditionalActions,
    rowIdField: "departmentId",
  });

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
        const status = p.value ? "Active" : "Inactive";
        return <StatusChip status={status} />;
      },
    },
    actionColumn,
  ], [actionColumn]);

  const extraActions = (
    <>
      {hasSelection && showCheckbox && (
        <Button variant="primary" size="sm" onClick={() => setShowBulkActivateModal(true)} className="u-inline-flex u-btn-gap">
          <FiCheckSquare size={16} /> {activeTab === "active" ? "Deactivate" : "Activate"} ({selectionCount})
        </Button>
      )}
    </>
  );

  if (loading && !departments.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === "active" ? false : true;
  const bulkButtonText = activeTab === "active" ? "Deactivate" : "Activate";
  const bulkTitle = bulkButtonText === "Activate" ? "Activate Departments" : "Deactivate Departments";
  const bulkDescription = `This action will ${bulkButtonText.toLowerCase()} the selected departments.`;

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
        totalCount={totalCount}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onSearch={handleSearch}
        showCheckbox={showCheckbox}
        selectedRows={selectedRowIds}
        onSelectionChange={handleSelectionChange}
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
        <FormSection title="Basic Information" description="Department code and name">
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input
                label="Department Code"
                value={formData.departmentCode || ""}
                onChange={(e) => setFormField("departmentCode")(e.target.value)}
                placeholder="Optional (max 100 chars)"
              />
            </Grid>
            <Grid item xs={12}>
              <Input
                label="Department Name"
                value={formData.departmentName || ""}
                onChange={(e) => setFormField("departmentName")(e.target.value)}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <Input
                label="Description"
                value={formData.description || ""}
                onChange={(e) => setFormField("description")(e.target.value)}
                multiline
                rows={2}
              />
            </Grid>
          </Grid>
        </FormSection>
      </CrudModal>

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={getSelectedIds(departments)}
        itemName="departments"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default DepartmentsMenu;