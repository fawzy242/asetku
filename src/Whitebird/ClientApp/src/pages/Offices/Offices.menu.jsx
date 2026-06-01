import React, { useState, useMemo, useCallback, useEffect } from "react";
import { Grid, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import OfficesData from "./Offices.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import IconButton from "../../components/atoms/IconButton/IconButton";
import Button from "../../components/atoms/Button/Button";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { FiEdit2, FiTrash2, FiCheckSquare } from "react-icons/fi";
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
  const [selectedRows, setSelectedRows] = useState([]);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const queryClient = useQueryClient();
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
    setSelectedRows([]);
  }, [activeTab, reload]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
    setSelectedRows([]);
  }, [setPage]);

  const handleDelete = useCallback(async (office) => {
    const r = await officesData.delete(office.officeId);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'offices'] });
      reload();
    }
  }, [reload, queryClient]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
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
      queryClient.invalidateQueries({ queryKey: ['reference', 'offices'] });
      reload();
      setSelectedRows([]);
    }
    return successCount;
  }, [reload, queryClient]);

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
      reload();
    }
    return success;
  }, [formData, editingOffice, crudHandleSubmit, setFormField, reload]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
    setSearchTerm("");
    setSelectedRows([]);
  }, [setPage]);

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
    {
      field: "actions",
      headerName: "Actions",
      width: 100,
      sortable: false,
      renderCell: (p) => (
        <div className="table-actions">
          <IconButton onClick={() => handleEdit(p.row)} title="Edit office" size="lg">
            <FiEdit2 size={18} />
          </IconButton>
          <IconButton onClick={() => handleDelete(p.row)} title="Delete office" variant="danger" size="lg">
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

  if (loading && !offices.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const parentOfficeValue = formData.parentOfficeId === null ? "" : (formData.parentOfficeId || "");
  const bulkActivateValue = activeTab === "active" ? false : true;
  const bulkButtonText = activeTab === "active" ? "Deactivate" : "Activate";

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
      </CrudModal>

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={selectedRows}
        itemName="offices"
        title={bulkButtonText === "Activate" ? "Activate Offices" : "Deactivate Offices"}
        description={`This action will ${bulkButtonText.toLowerCase()} the selected offices.`}
      />
    </div>
  );
};

export default OfficesMenu;