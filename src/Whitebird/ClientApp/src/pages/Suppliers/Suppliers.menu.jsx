import React, { useState, useMemo, useCallback, useEffect } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import SuppliersData from "./Suppliers.data";
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
import FileUploader from "../../components/molecules/FileUploader/FileUploader";
import ModalActions from "../../components/molecules/ModalActions/ModalActions";
import { useGridData } from "../../hooks/useGridData";
import { useCrudForm } from "../../hooks/useCrudForm";
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
  const queryClient = useQueryClient();
  
  const { showModal, editingRecord: editingSupplier, isSubmitting, formData, setFormData, handleCreate, handleEdit, handleClose } = useCrudForm(INITIAL_FORM_DATA, suppliersData, { idField: 'supplierId' });

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
    
    const result = await suppliersData.fetchGridData(filters);
    return result;
  }, [activeTab, searchTerm]);

  const { data: suppliers, totalCount, loading, page, setPage, pageSize, setPageSize, updateFilters, reload } = useGridData(['suppliers', activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    reload();
  }, [activeTab, reload]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    updateFilters({ search });
    setPage(1);
  }, [updateFilters, setPage]);
  
  const handleDelete = useCallback(async (sup) => { 
    const r = await suppliersData.delete(sup.supplierId); 
    if (r.success) { 
      queryClient.invalidateQueries({ queryKey: ['reference', 'suppliers'] }); 
      reload(); 
    } 
  }, [reload, queryClient]);

  const onSubmit = useCallback(async (e) => {
    e.preventDefault();
    if (!formData.supplierName?.trim()) return;
    if (isSubmitting) return;
    const data = { ...formData, isActive: editingSupplier ? editingSupplier.isActive : true };
    if (data.contactPerson === '') data.contactPerson = null;
    if (data.phoneNumber === '') data.phoneNumber = null;
    if (data.email === '') data.email = null;
    if (data.address === '') data.address = null;
    const r = editingSupplier ? await suppliersData.update(editingSupplier.supplierId, data) : await suppliersData.create(data);
    if (r.success) { 
      queryClient.invalidateQueries({ queryKey: ['reference', 'suppliers'] }); 
      handleClose(); 
      reload(); 
    }
  }, [formData, isSubmitting, editingSupplier, handleClose, reload, queryClient]);

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
      renderCell: (p) => <StatusBadge status={p?.value ? 'Active' : 'Inactive'} />,
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

  const handleTabChange = useCallback((tab) => { 
    setActiveTab(tab); 
    setPage(1);
    setSearchTerm("");
  }, [setPage]);
  
  if (loading && !suppliers.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="suppliers-menu">
      <PageHeader 
        title="Supplier Management"
        actions={
          <Button variant="primary" onClick={handleCreate} startIcon={<FiPlus />}>
            Add Supplier
          </Button>
        }
      />
      
      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} placeholder="Search by name, contact..." />
      
      <div className="suppliers-menu__table" style={{ width: '100%', minWidth: 0 }}>
        <DataTable 
          rows={suppliers} 
          columns={columns} 
          loading={loading} 
          pageSize={pageSize} 
          getRowId={(row) => row.supplierId} 
          hideFooter={true} 
          autoHeight={true}
          ariaLabel="Suppliers data table" 
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
      
      <Modal isOpen={showModal} onClose={handleClose} title={editingSupplier ? "Edit Supplier" : "Add Supplier"} size="lg">
        <form onSubmit={onSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input label="Supplier Name" value={formData.supplierName || ""} onChange={e => setFormData({ ...formData, supplierName: e.target.value })} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Contact Person" value={formData.contactPerson || ""} onChange={e => setFormData({ ...formData, contactPerson: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Phone Number" value={formData.phoneNumber || ""} onChange={e => setFormData({ ...formData, phoneNumber: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Email" type="email" value={formData.email || ""} onChange={e => setFormData({ ...formData, email: e.target.value })} />
            </Grid>
            <Grid item xs={12}>
              <Input label="Address" value={formData.address || ""} onChange={e => setFormData({ ...formData, address: e.target.value })} multiline rows={2} />
            </Grid>
            <Grid item xs={12}>
              <FileUploader 
                referenceTable="Supplier"
                referenceId={editingSupplier?.supplierId}
                onUploadComplete={reload}
              />
            </Grid>
          </Grid>
          <ModalActions 
            onCancel={handleClose} 
            isSubmitting={isSubmitting}
            submitText={editingSupplier ? "Update" : "Create"}
          />
        </form>
      </Modal>
    </div>
  );
};

export default SuppliersMenu;