import React, { useState, useMemo, useCallback, useEffect } from "react";
import { Grid, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import SuppliersData from "./Suppliers.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import Input from "../../components/atoms/Input/Input";
import Spinner from "../../components/atoms/Spinner/Spinner";
import IconButton from "../../components/atoms/IconButton/IconButton";
import Button from "../../components/atoms/Button/Button";
import FileUploader from "../../components/molecules/FileUploader/FileUploader";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { FiEdit2, FiTrash2, FiCheckSquare } from "react-icons/fi";
import { useGridData } from "../../hooks/useGridData";
import { useCrudFormBase } from "../../hooks/useCrudFormBase";
import { cleanSupplierFormData } from "../../core/utils/formHelpers";
import "./Suppliers.scss";

const suppliersData = new SuppliersData();
suppliersData.transformFormData = cleanSupplierFormData;

const INITIAL_FORM_DATA = { 
  supplierName: "", 
  contactPerson: "", 
  phoneNumber: "", 
  email: "", 
  address: "" 
};

const TABS = [
  { id: "all", label: "All" },
  { id: "active", label: "Active" },
  { id: "inactive", label: "Inactive" },
];

const SuppliersMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedRows, setSelectedRows] = useState([]);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const queryClient = useQueryClient();

  const {
    showModal,
    editingRecord: editingSupplier,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, suppliersData, {
    idField: 'supplierId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reference', 'suppliers'] });
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
    
    const result = await suppliersData.fetchGridData(requestParams);
    return result;
  }, [buildFilters]);

  const {
    data: suppliers,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    reload
  } = useGridData(['suppliers', activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    reload();
    setSelectedRows([]);
  }, [activeTab, reload]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
    setSelectedRows([]);
  }, [setPage]);

  const handleDelete = useCallback(async (sup) => { 
    const r = await suppliersData.delete(sup.supplierId); 
    if (r.success) { 
      queryClient.invalidateQueries({ queryKey: ['reference', 'suppliers'] }); 
      reload(); 
    } 
  }, [reload, queryClient]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
    let successCount = 0;
    for (const id of ids) {
      const result = await suppliersData.fetchById(id);
      if (result.success && result.data) {
        const updateData = { ...result.data, isActive: activate };
        const updateResult = await suppliersData.update(id, updateData);
        if (updateResult.success) successCount++;
      }
    }
    if (successCount > 0) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'suppliers'] });
      reload();
      setSelectedRows([]);
    }
    return successCount;
  }, [reload, queryClient]);

  const onSubmit = useCallback(async () => {
    const submitData = { 
      ...formData, 
      isActive: editingSupplier ? editingSupplier.isActive : true 
    };
    
    if (submitData.contactPerson === '') submitData.contactPerson = null;
    if (submitData.phoneNumber === '') submitData.phoneNumber = null;
    if (submitData.email === '') submitData.email = null;
    if (submitData.address === '') submitData.address = null;
    
    Object.keys(submitData).forEach(key => {
      setFormField(key)(submitData[key]);
    });
    
    const success = await crudHandleSubmit();
    if (success) {
      reload();
    }
    return success;
  }, [formData, editingSupplier, crudHandleSubmit, setFormField, reload]);

  const handleTabChange = useCallback((tab) => { 
    setActiveTab(tab); 
    setPage(1);
    setSearchTerm("");
    setSelectedRows([]);
  }, [setPage]);

  const columns = useMemo(() => [
    { field: "supplierName", headerName: "Name", flex: 1, minWidth: 180 },
    { field: "contactPerson", headerName: "Contact", width: 160 },
    { field: "phoneNumber", headerName: "Phone", width: 140 },
    { field: "email", headerName: "Email", width: 200 },
    { field: "assetCount", headerName: "Assets", width: 90 },
    { 
      field: "isActive", 
      headerName: "Status", 
      width: 110, 
      renderCell: (p) => {
        const status = p?.value ? 'Active' : 'Inactive';
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
          <IconButton onClick={() => handleEdit(p?.row)} title="Edit supplier" size="lg"><FiEdit2 size={18} /></IconButton>
          <IconButton onClick={() => handleDelete(p?.row)} title="Delete supplier" variant="danger" size="lg"><FiTrash2 size={18} /></IconButton>
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

  if (loading && !suppliers.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === "active" ? false : true;
  const bulkButtonText = activeTab === "active" ? "Deactivate" : "Activate";

  return (
    <div className="suppliers-menu">
      <GridView
        title="Supplier Management"
        tabs={TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={suppliers}
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
        createButtonText="Add Supplier"
        ariaLabel="Suppliers data table"
        extraActions={extraActions}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingSupplier ? "Edit Supplier" : "Add Supplier"}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingSupplier ? "Update" : "Create"}
        size="lg"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Input 
              label="Supplier Name" 
              value={formData.supplierName || ""} 
              onChange={(e) => setFormField('supplierName')(e.target.value)} 
              required 
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input 
              label="Contact Person" 
              value={formData.contactPerson || ""} 
              onChange={(e) => setFormField('contactPerson')(e.target.value)} 
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input 
              label="Phone Number" 
              value={formData.phoneNumber || ""} 
              onChange={(e) => setFormField('phoneNumber')(e.target.value)} 
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input 
              label="Email" 
              type="email" 
              value={formData.email || ""} 
              onChange={(e) => setFormField('email')(e.target.value)} 
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
          <Grid item xs={12}>
            <FileUploader 
              referenceTable="Supplier"
              referenceId={editingSupplier?.supplierId}
              onUploadComplete={reload}
            />
          </Grid>
        </Grid>
      </CrudModal>

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={selectedRows}
        itemName="suppliers"
        title={bulkButtonText === "Activate" ? "Activate Suppliers" : "Deactivate Suppliers"}
        description={`This action will ${bulkButtonText.toLowerCase()} the selected suppliers.`}
      />
    </div>
  );
};

export default SuppliersMenu;