import React, { useState, useMemo, useCallback, useEffect, useRef } from "react";
import { Grid } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import EmployeesData from "./Employees.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import FormSection from "../../components/atoms/FormSection/FormSection";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import DatePickerInput from "../../components/atoms/Input/DatePickerInput";
import Spinner from "../../components/atoms/Spinner/Spinner";
import FileUploader from "../../components/molecules/FileUploader/FileUploader";
import ImportModal from "../../components/molecules/ImportModal/ImportModal";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import StatusChip from "../../components/atoms/StatusChip/StatusChip";
import EmploymentStatusChip from "../../components/atoms/EmploymentStatusChip/EmploymentStatusChip";
import Button from "../../components/atoms/Button/Button"; // ADD THIS IMPORT
import { FiUpload, FiCheckSquare } from "react-icons/fi";
import { ACTION_TYPES, useGridActions } from "../../hooks/useGridActions";
import { useBulkSelection } from "../../hooks/useBulkSelection";
import { useSweetAlert } from "../../hooks/useSweetAlert";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudFormBase } from "../../hooks/useCrudFormBase";
import { useOptions } from "../../hooks/useOptions";
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
  const [searchTerm, setSearchTerm] = useState("");
  const [showImportModal, setShowImportModal] = useState(false);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);
  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirmDelete, confirm } = useSweetAlert();

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
    idField: "employeeId",
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reference", "employees"] });
    },
  });

  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } = useBulkSelection({ idField: "employeeId" });

  const showCheckbox = useMemo(() => {
    return activeTab === "Active" || activeTab === "Inactive";
  }, [activeTab]);

  const buildFilters = useCallback(() => {
    const filters = {};
    if (activeTab === "Active") {
      filters.isActive = true;
    } else if (activeTab === "Inactive") {
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
    const result = await employeesData.fetchGridData(requestParams);
    return result;
  }, [buildFilters]);

  const {
    data: employees,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    reload
  } = useGridData(["employees", activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    isMountedRef.current = true;
    setPage(1);
    clearSelection();
    reload();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, setPage, clearSelection]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
    clearSelection();
  }, [setPage, clearSelection]);

  const handleDelete = useCallback(async (emp) => {
    const confirmed = await confirmDelete("Delete Employee", `Are you sure you want to delete "${emp.fullName}"?`);
    if (!confirmed) return;
    const r = await employeesData.delete(emp.employeeId);
    if (r.success && isMountedRef.current) {
      toast.success("Employee deleted successfully");
      queryClient.invalidateQueries({ queryKey: ["reference", "employees"] });
      reload();
      clearSelection();
    }
  }, [reload, queryClient, toast, confirmDelete, clearSelection]);

  const onSubmit = useCallback(async () => {
    const success = await crudHandleSubmit();
    if (success && isMountedRef.current) {
      toast.success(editingEmployee ? "Employee updated successfully" : "Employee created successfully");
      queryClient.invalidateQueries({ queryKey: ["reference", "employees"] });
      reload();
      clearSelection();
    }
    return success;
  }, [crudHandleSubmit, reload, queryClient, toast, editingEmployee, clearSelection]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
    setSearchTerm("");
    clearSelection();
  }, [setPage, clearSelection]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
    const actionText = activate ? "activate" : "deactivate";
    const confirmed = await confirm({
      title: activate ? "Activate Employees" : "Deactivate Employees",
      text: `Are you sure you want to ${actionText} ${ids.length} employee(s)?`,
      confirmButtonText: activate ? "Yes, Activate" : "Yes, Deactivate",
    });
    if (!confirmed) return;
    const r = await employeesData.bulkActivate(ids, activate);
    if (r.success && isMountedRef.current) {
      toast.success(`${r.data || ids.length} employee(s) ${actionText}d successfully`);
      clearSelection();
      queryClient.invalidateQueries({ queryKey: ["reference", "employees"] });
      reload();
    }
  }, [reload, queryClient, toast, confirm, clearSelection]);

  const handleImport = useCallback(async (file) => {
    setIsImporting(true);
    const r = await employeesData.importEmployees(file);
    setIsImporting(false);
    if (r.success && isMountedRef.current) {
      setImportResult(r.data);
      if (r.data.errorCount === 0) {
        toast.success(`Import completed: ${r.data.successCount} employees imported`);
      } else {
        toast.warning(`Import completed: ${r.data.successCount} success, ${r.data.errorCount} errors`);
      }
      queryClient.invalidateQueries({ queryKey: ["reference", "employees"] });
      reload();
      clearSelection();
    }
  }, [reload, queryClient, toast, clearSelection]);

  const handleDownloadTemplate = useCallback(async () => {
    await employeesData.downloadTemplate();
  }, []);

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
    rowIdField: "employeeId",
  });

  const departmentOptions = useOptions(departments, "Select Department");
  const positionOptions = useOptions(employeePositions, "Select Position");
  const statusOptions = useOptions(employeeStatuses, "Select Status");
  const officeOptions = useOptions(offices, "Select Office");

  const columns = useMemo(() => [
    { field: "employeeCode", headerName: "Code", width: 120 },
    { 
      field: "fullName", 
      headerName: "Full Name", 
      flex: 1, 
      minWidth: 180 
    },
    { 
      field: "employmentStatusName", 
      headerName: "Employment Status", 
      width: 160,
      renderCell: (p) => {
        const status = p?.value || "Unknown";
        return <EmploymentStatusChip status={status} />;
      }
    },
    { field: "departmentName", headerName: "Department", width: 150 },
    { field: "positionName", headerName: "Position", width: 150 },
    { field: "email", headerName: "Email", width: 200 },
    { field: "officeName", headerName: "Office", width: 150 },
    { 
      field: "isActive", 
      headerName: "Status", 
      width: 120,
      renderCell: (p) => {
        const status = p?.value ? "Active" : "Inactive";
        return <StatusChip status={status} />;
      }
    },
    actionColumn,
  ], [actionColumn]);

  const extraActions = (
    <>
      <Button variant="outline" size="sm" onClick={() => setShowImportModal(true)} className="u-inline-flex u-btn-gap">
        <FiUpload size={16} /> Import
      </Button>
      {hasSelection && showCheckbox && (
        <Button variant="primary" size="sm" onClick={() => setShowBulkActivateModal(true)} className="u-inline-flex u-btn-gap">
          <FiCheckSquare size={16} /> {activeTab === "Active" ? "Deactivate" : "Activate"} ({selectionCount})
        </Button>
      )}
    </>
  );

  if (loading && !employees.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === "Active" ? false : true;
  const bulkTitle = bulkActivateValue ? "Activate Employees" : "Deactivate Employees";
  const bulkDescription = `This action will ${bulkActivateValue ? "activate" : "deactivate"} the selected employees.`;

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
        title={editingEmployee ? "Edit Employee" : "Add Employee"}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingEmployee ? "Update" : "Create"}
        size="xl"
      >
        <FormSection title="Basic Information" description="Employee code and full name">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input label="Employee Code" value={formData.employeeCode || ""} onChange={(e) => setFormField("employeeCode")(e.target.value)} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Full Name" value={formData.fullName || ""} onChange={(e) => setFormField("fullName")(e.target.value)} required />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Contact Information" description="Address, phone, and email">
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input label="Address" value={formData.address || ""} onChange={(e) => setFormField("address")(e.target.value)} multiline rows={2} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Phone Number" value={formData.phoneNumber || ""} onChange={(e) => setFormField("phoneNumber")(e.target.value)} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Email" type="email" value={formData.email || ""} onChange={(e) => setFormField("email")(e.target.value)} />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Employment Details" description="Department, position, status, and office">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Select label="Department" value={formData.departmentId || ""} onChange={(e) => setFormField("departmentId")(e.target.value)} options={departmentOptions} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Position" value={formData.position || ""} onChange={(e) => setFormField("position")(e.target.value)} options={positionOptions} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Employment Status" value={formData.employmentStatus || ""} onChange={(e) => setFormField("employmentStatus")(e.target.value)} options={statusOptions} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Office" value={formData.officeId || ""} onChange={(e) => setFormField("officeId")(e.target.value)} options={officeOptions} />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Dates" description="Join and resignation dates">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <DatePickerInput label="Join Date" value={formData.joinDate || ""} onChange={(e) => setFormField("joinDate")(e.target.value)} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <DatePickerInput label="Resign Date" value={formData.resignDate || ""} onChange={(e) => setFormField("resignDate")(e.target.value)} />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Attachments" description="Supporting documents">
          <FileUploader
            referenceTable="Employee"
            referenceId={editingEmployee?.employeeId}
            onUploadComplete={reload}
          />
        </FormSection>
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
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={getSelectedIds(employees)}
        itemName="employees"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default EmployeesMenu;