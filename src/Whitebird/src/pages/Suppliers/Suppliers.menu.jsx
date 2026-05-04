import React, { useState, useMemo, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box, Chip } from "@mui/material";
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
import { useGridData } from "../../hooks/useGridData";
import { useCrudForm } from "../../hooks/useCrudForm";
import "./Suppliers.scss";

const suppliersData = new SuppliersData();

const INITIAL_FORM_DATA = {
  supplierName: "", contactPerson: "", phoneNumber: "", email: "", address: "",
};

const TABS = [
  { id: "all", label: "All" },
  { id: "active", label: "Active" },
  { id: "inactive", label: "Inactive" },
];

const SuppliersMenu = () => {
  const [activeTab, setActiveTab] = useState("all");

  const {
    showModal,
    editingRecord: editingSupplier,
    isSubmitting,
    formData,
    setFormData,
    handleCreate,
    handleEdit,
    handleClose,
  } = useCrudForm(INITIAL_FORM_DATA, suppliersData, {
    idField: 'supplierId',
  });

  const fetchGridData = useCallback(async (params) => {
    const result = await suppliersData.fetchGridData(params);
    if (result.success) {
      const rawData = result.data;
      let dataArray = [];

      if (rawData?.data?.data && Array.isArray(rawData.data.data)) {
        dataArray = rawData.data.data;
      } else if (rawData?.data && Array.isArray(rawData.data)) {
        dataArray = rawData.data;
      } else if (Array.isArray(rawData)) {
        dataArray = rawData;
      }

      if (activeTab === "active") {
        dataArray = dataArray.filter(s => s.isActive === true);
      } else if (activeTab === "inactive") {
        dataArray = dataArray.filter(s => s.isActive === false);
      }

      return { success: true, data: { data: dataArray, totalCount: dataArray.length } };
    }
    return result;
  }, [activeTab]);

  const {
    data: suppliers,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    updateFilters,
    reload
  } = useGridData(['suppliers', activeTab], fetchGridData);

  const handleSearch = useCallback((search) => updateFilters({ search }), [updateFilters]);

  const handleDelete = useCallback(async (sup) => {
    const r = await suppliersData.delete(sup.supplierId);
    if (r.success) reload();
  }, [reload]);

  const onSubmit = useCallback(async (e) => {
    e.preventDefault();
    if (!formData.supplierName.trim()) return;
    if (isSubmitting) return;
    const data = { ...formData, isActive: editingSupplier ? editingSupplier.isActive : true };
    const r = editingSupplier
      ? await suppliersData.update(editingSupplier.supplierId, data)
      : await suppliersData.create(data);
    if (r.success) { handleClose(); reload(); }
  }, [formData, isSubmitting, editingSupplier, handleClose, reload]);

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
      renderCell: (p) => (
        <Chip
          label={p.value ? 'Active' : 'Inactive'}
          size="small"
          sx={{
            bgcolor: p.value ? 'rgba(16, 185, 129, 0.1)' : 'rgba(107, 114, 128, 0.1)',
            color: p.value ? '#10b981' : '#6b7280',
            fontWeight: 500,
            fontSize: '0.75rem',
            height: 24,
            borderRadius: '4px',
          }}
        />
      ),
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 100,
      sortable: false,
      renderCell: (p) => (
        <div className="table-actions">
          <IconButton onClick={() => handleEdit(p.row)} title="Edit supplier" size="lg">
            <FiEdit2 size={18} />
          </IconButton>
          <IconButton onClick={() => handleDelete(p.row)} title="Delete supplier" variant="danger" size="lg">
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

  if (loading && !suppliers.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="suppliers-menu">
      <PageHeader title="Supplier Management" buttonText="Add Supplier" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} placeholder="Search by name, contact..." />
      <div className="suppliers-menu__table">
        <DataTable
          rows={suppliers}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.supplierId}
          hideFooter={true}
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
              <Input label="Supplier Name" value={formData.supplierName} onChange={e => setFormData({ ...formData, supplierName: e.target.value })} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Contact Person" value={formData.contactPerson} onChange={e => setFormData({ ...formData, contactPerson: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Phone Number" value={formData.phoneNumber} onChange={e => setFormData({ ...formData, phoneNumber: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Email" type="email" value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })} />
            </Grid>
            <Grid item xs={12}>
              <Input label="Address" value={formData.address} onChange={e => setFormData({ ...formData, address: e.target.value })} multiline rows={2} />
            </Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={handleClose} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>
              {editingSupplier ? "Update" : "Create"}
            </Button>
          </Box>
        </form>
      </Modal>
    </div>
  );
};

export default SuppliersMenu;