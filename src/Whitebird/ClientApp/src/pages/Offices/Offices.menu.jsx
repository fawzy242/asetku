import React, { useState, useMemo, useCallback, useEffect } from "react";
import { Grid, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import OfficesData from "./Offices.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import FormSection from "../../components/atoms/FormSection/FormSection";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { ACTION_TYPES, useGridActions } from "../../hooks/useGridActions";
import { useBulkSelection } from "../../hooks/useBulkSelection";
import { useSweetAlert } from "../../hooks/useSweetAlert";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudFormBase } from "../../hooks/useCrudFormBase";
import { cleanOfficeFormData } from "../../core/utils/formHelpers";
import "./Offices.scss";

const officesData = new OfficesData();
officesData.transformFormData = cleanOfficeFormData;

const OFFICE_TYPE_OPTIONS = [
  { value: "", label: "Select Type" },
  { value: "1", label: "Head Office" },
  { value: "2", label: "Branch Office" },
];

const INITIAL_FORM_DATA = {
  officeName: "",
  officeCode: "",
  officeType: "",
  city: "",
  address: "",
  phone: "",
  parentOfficeId: null,
};

const TABS = [
  { id: "all", label: "All" },
  { id: "active", label: "Active" },
  { id: "inactive", label: "Inactive" },
];

const OfficesMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const queryClient = useQueryClient();
  const { toast, confirmDelete, confirm } = useSweetAlert();
  const { offices: parentOffices } = useReferenceData();

  const {
    showModal,
    editingRecord: editingOffice,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, officesData, {
    idField: 'officeId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reference', 'offices'] });
    },
  });

  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } = useBulkSelection({ idField: 'officeId' });

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
    const result = await officesData.fetchGridData(requestParams);
    return result;
  }, [buildFilters]);

  const {
    data: offices,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    reload
  } = useGridData(['offices', activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    reload();
    clearSelection();
  }, [activeTab, reload, clearSelection]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
    clearSelection();
  }, [setPage, clearSelection]);

  const handleDelete = useCallback(async (office) => {
    const confirmed = await confirmDelete('Delete Office', `Are you sure you want to delete "${office.officeName}"? This may affect assets and employees assigned to it.`);
    if (!confirmed) return;
    const r = await officesData.delete(office.officeId);
    if (r.success) {
      toast.success('Office deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['reference', 'offices'] });
      reload();
      clearSelection();
    }
  }, [reload, queryClient, toast, confirmDelete, clearSelection]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
    const actionText = activate ? 'activate' : 'deactivate';
    const confirmed = await confirm({
      title: activate ? 'Activate Offices' : 'Deactivate Offices',
      text: `Are you sure you want to ${actionText} ${ids.length} office(s)?`,
      confirmButtonText: activate ? 'Yes, Activate' : 'Yes, Deactivate',
    });
    if (!confirmed) return;
    
    let successCount = 0;
    for (const id of ids) {
      const result = await officesData.fetchById(id);
      if (result.success && result.data) {
        const updateData = { ...result.data, isActive: activate };
        const updateResult = await officesData.update(id, updateData);
        if (updateResult.success) successCount++;
      }
    }
    if (successCount > 0) {
      toast.success(`${successCount} office(s) ${actionText}d successfully`);
      queryClient.invalidateQueries({ queryKey: ['reference', 'offices'] });
      reload();
      clearSelection();
    }
  }, [reload, queryClient, toast, confirm, clearSelection]);

  const onSubmit = useCallback(async () => {
    const submitData = { 
      ...formData, 
      isActive: editingOffice ? editingOffice.isActive : true 
    };
    if (submitData.parentOfficeId === "" || submitData.parentOfficeId === undefined || submitData.parentOfficeId === null) {
      submitData.parentOfficeId = null;
    }
    Object.keys(submitData).forEach(key => {
      setFormField(key)(submitData[key]);
    });
    const success = await crudHandleSubmit();
    if (success) {
      toast.success(editingOffice ? 'Office updated successfully' : 'Office created successfully');
      reload();
      clearSelection();
    }
    return success;
  }, [formData, editingOffice, crudHandleSubmit, setFormField, reload, toast, clearSelection]);

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
    rowIdField: 'officeId',
  });

  const parentOfficeOptions = useMemo(() => [
    { value: "", label: "None (Top Level)" },
    ...parentOffices
      .filter(o => !editingOffice || o.value !== editingOffice.officeId)
      .map(o => ({ value: o.value, label: o.label }))
  ], [parentOffices, editingOffice]);

  const columns = useMemo(() => [
    { field: "officeCode", headerName: "Code", width: 120 },
    { field: "officeName", headerName: "Name", flex: 1, minWidth: 180 },
    { field: "officeTypeName", headerName: "Type", width: 130 },
    { field: "city", headerName: "City", width: 140 },
    { field: "parentOfficeName", headerName: "Parent", width: 170 },
    { field: "childCount", headerName: "Sub-offices", width: 130 },
    {
      field: "isActive",
      headerName: "Status",
      width: 110,
      renderCell: (p) => {
        const status = p.value ? 'Active' : 'Inactive';
        return <Chip label={status} size="small" sx={getStatusChipStyles(status)} />;
      },
    },
    actionColumn,
  ], [actionColumn]);

  const extraActions = (
    <>
      {hasSelection && showCheckbox && (
        <button className="btn btn--primary btn--sm" onClick={() => setShowBulkActivateModal(true)} style={{ display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
          {activeTab === "active" ? "Deactivate" : "Activate"} ({selectionCount})
        </button>
      )}
    </>
  );

  if (loading && !offices.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const parentOfficeValue = formData.parentOfficeId === null ? "" : (formData.parentOfficeId || "");
  const bulkActivateValue = activeTab === "active" ? false : true;
  const bulkButtonText = activeTab === "active" ? "Deactivate" : "Activate";
  const bulkTitle = bulkButtonText === "Activate" ? "Activate Offices" : "Deactivate Offices";
  const bulkDescription = `This action will ${bulkButtonText.toLowerCase()} the selected offices.`;

  return (
    <div className="offices-menu">
      <GridView
        title="Office Management"
        tabs={TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={offices}
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
        createButtonText="Add Office"
        ariaLabel="Offices data table"
        extraActions={extraActions}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingOffice ? "Edit Office" : "Add Office"}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingOffice ? "Update" : "Create"}
        size="lg"
      >
        <FormSection title="Basic Information" description="Office name, code, and type">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input 
                label="Office Name" 
                value={formData.officeName || ""} 
                onChange={(e) => setFormField('officeName')(e.target.value)} 
                required 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input 
                label="Office Code" 
                value={formData.officeCode || ""} 
                onChange={(e) => setFormField('officeCode')(e.target.value)} 
                placeholder="Optional"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Office Type" 
                value={formData.officeType || ""} 
                onChange={(e) => setFormField('officeType')(e.target.value)} 
                options={OFFICE_TYPE_OPTIONS} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Parent Office" 
                value={parentOfficeValue}
                onChange={(e) => setFormField('parentOfficeId')(e.target.value || null)} 
                options={parentOfficeOptions} 
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Location Details" description="City, address, and phone">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input 
                label="City" 
                value={formData.city || ""} 
                onChange={(e) => setFormField('city')(e.target.value)} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input 
                label="Phone" 
                value={formData.phone || ""} 
                onChange={(e) => setFormField('phone')(e.target.value)} 
              />
            </Grid>
            <Grid item xs={12}>
              <Input 
                label="Address" 
                value={formData.address || ""} 
                onChange={(e) => setFormField('address')(e.target.value)} 
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
        selectedIds={getSelectedIds(offices)}
        itemName="offices"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default OfficesMenu;