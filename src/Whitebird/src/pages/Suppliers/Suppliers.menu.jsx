import React, { useState, useMemo, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box } from "@mui/material";
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
import { useGridData } from "../../hooks/useGridData";
import "./Suppliers.scss";

const suppliersData = new SuppliersData();

const SuppliersMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showModal, setShowModal] = useState(false);
  const [editingSupplier, setEditingSupplier] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formData, setFormData] = useState({ supplierName: "", contactPerson: "", phoneNumber: "", email: "", address: "" });

  const fetchGridData = useCallback(async (params) => {
    const result = await suppliersData.fetchGridData(params);
    if (result.success) { let data = result.data.data || []; if (activeTab === "active") data = data.filter(s => s.isActive); else if (activeTab === "inactive") data = data.filter(s => !s.isActive); return { success: true, data: { data, totalCount: data.length } }; }
    return result;
  }, [activeTab]);

  const { data: suppliers, totalCount, loading, page, setPage, pageSize, setPageSize, updateFilters, reload } = useGridData(fetchGridData);

  const handleSearch = (search) => updateFilters({ search });
  const handleCreate = () => { setEditingSupplier(null); setFormData({ supplierName: "", contactPerson: "", phoneNumber: "", email: "", address: "" }); setShowModal(true); };
  const handleEdit = async (sup) => { const r = await suppliersData.fetchById(sup.supplierId); if (r.success) { setEditingSupplier(r.data); setFormData({ supplierName: r.data.supplierName, contactPerson: r.data.contactPerson || "", phoneNumber: r.data.phoneNumber || "", email: r.data.email || "", address: r.data.address || "" }); setShowModal(true); } };
  const handleDelete = async (sup) => { const r = await suppliersData.delete(sup.supplierId); if (r.success) reload(); };
  const handleSubmit = async (e) => { e.preventDefault(); if (!formData.supplierName.trim()) return; if (isSubmitting) return; setIsSubmitting(true); const r = editingSupplier ? await suppliersData.update(editingSupplier.supplierId, { ...formData, isActive: editingSupplier.isActive }) : await suppliersData.create(formData); setIsSubmitting(false); if (r.success) { setShowModal(false); reload(); } };

  const columns = useMemo(() => [
    { field: "supplierName", headerName: "Name", width: 200 }, { field: "contactPerson", headerName: "Contact", width: 180 }, { field: "phoneNumber", headerName: "Phone", width: 150 }, { field: "email", headerName: "Email", width: 220 }, { field: "assetCount", headerName: "Assets", width: 100 },
    { field: "isActive", headerName: "Status", width: 100, renderCell: (p) => <span className={`status-badge ${p.value ? 'status-badge--success' : 'status-badge--secondary'}`}>{p.value ? 'Active' : 'Inactive'}</span> },
    { field: "actions", headerName: "Actions", width: 100, sortable: false, renderCell: (p) => (<div className="table-actions"><button className="icon-btn icon-btn--lg" onClick={() => handleEdit(p.row)} title="Edit"><FiEdit2 size={18} /></button><button className="icon-btn icon-btn--danger icon-btn--lg" onClick={() => handleDelete(p.row)} title="Delete"><FiTrash2 size={18} /></button></div>) },
  ], []);

  const tabs = [{ id: "all", label: "All" }, { id: "active", label: "Active" }, { id: "inactive", label: "Inactive" }];
  if (loading && !suppliers.length) return <div className="suppliers-loading"><Spinner size="lg" /></div>;

  return (
    <div className="suppliers-menu">
      <PageHeader title="Supplier Management" buttonText="Add Supplier" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      <Tabs tabs={tabs} activeTab={activeTab} onTabChange={(tab) => { setActiveTab(tab); setPage(1); }} />
      <SearchToolbar onSearch={handleSearch} placeholder="Search by name, contact..." />
      <div className="suppliers-menu__table"><DataTable rows={suppliers} columns={columns} loading={loading} pageSize={pageSize} getRowId={(row) => row.supplierId} hideFooter={true} /></div>
      <Pagination currentPage={page} totalPages={Math.ceil(totalCount / pageSize)} pageSize={pageSize} totalItems={totalCount} onPageChange={setPage} onPageSizeChange={setPageSize} />
      <Modal isOpen={showModal} onClose={() => !isSubmitting && setShowModal(false)} title={editingSupplier ? "Edit Supplier" : "Add Supplier"} size="lg">
        <form onSubmit={handleSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}><Input label="Supplier Name" value={formData.supplierName} onChange={e => setFormData({...formData, supplierName: e.target.value})} required /></Grid>
            <Grid item xs={12} sm={6}><Input label="Contact Person" value={formData.contactPerson} onChange={e => setFormData({...formData, contactPerson: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Phone Number" value={formData.phoneNumber} onChange={e => setFormData({...formData, phoneNumber: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Email" type="email" value={formData.email} onChange={e => setFormData({...formData, email: e.target.value})} /></Grid>
            <Grid item xs={12}><Input label="Address" value={formData.address} onChange={e => setFormData({...formData, address: e.target.value})} multiline rows={2} /></Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}><Button variant="outline" onClick={() => setShowModal(false)} type="button">Cancel</Button><Button type="submit" variant="primary" loading={isSubmitting}>{editingSupplier ? "Update" : "Create"}</Button></Box>
        </form>
      </Modal>
    </div>
  );
};

export default SuppliersMenu;