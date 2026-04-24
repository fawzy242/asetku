import React, { useState, useEffect, useMemo, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box } from "@mui/material";
import AssetsData from "./Assets.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Modal from "../../components/molecules/Modal/Modal";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import SearchToolbar from "../../components/molecules/SearchToolbar/SearchToolbar";
import Tabs from "../../components/molecules/Tabs/Tabs";
import { useGridData } from "../../hooks/useGridData";
import utilsHelper from "../../core/utils/utils.helper";
import "./Assets.scss";

const assetsData = new AssetsData();

const AssetsMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showFilters, setShowFilters] = useState(false);
  const [statusFilter, setStatusFilter] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [showModal, setShowModal] = useState(false);
  const [editingAsset, setEditingAsset] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [dropdownData, setDropdownData] = useState({ categories: [], suppliers: [], employees: [] });
  const [formData, setFormData] = useState({
    assetName: "", categoryId: "", subCategory: "", brand: "", model: "",
    serialNumber: "", purchaseDate: "", purchasePrice: "", condition: "Good",
    status: "Available", location: "", currentHolderId: "", supplierId: "",
  });

  const fetchGridData = useCallback(async (params) => {
    const filters = { ...params };
    if (activeTab === "available") filters.status = "Available";
    else if (activeTab === "assigned") filters.status = "Assigned";
    else if (activeTab === "maintenance") filters.status = "Under Repair";
    if (statusFilter) filters.status = statusFilter;
    if (categoryFilter) filters.categoryId = categoryFilter;
    return assetsData.fetchGridData(filters);
  }, [activeTab, statusFilter, categoryFilter]);

  const { data: assets, totalCount, loading, page, setPage, pageSize, setPageSize, updateFilters, reload } = useGridData(fetchGridData);

  useEffect(() => { assetsData.fetchDropdownData().then(r => { if (r.success) setDropdownData(r.data); }); }, []);

  const handleSearch = (search) => { updateFilters({ search }); };
  const handleCreate = () => { setEditingAsset(null); setFormData({ assetName: "", categoryId: "", subCategory: "", brand: "", model: "", serialNumber: "", purchaseDate: "", purchasePrice: "", condition: "Good", status: "Available", location: "", currentHolderId: "", supplierId: "" }); setShowModal(true); };
  const handleEdit = async (asset) => { const r = await assetsData.fetchById(asset.assetId); if (r.success) { setEditingAsset(r.data); setFormData({ ...r.data, purchaseDate: r.data.purchaseDate?.split("T")[0] || "", purchasePrice: r.data.purchasePrice?.toString() || "" }); setShowModal(true); } };
  const handleDelete = async (asset) => { const r = await assetsData.delete(asset.assetId); if (r.success) reload(); };
  const handleSubmit = async (e) => { e.preventDefault(); if (isSubmitting) return; setIsSubmitting(true); const submitData = { ...formData, purchasePrice: formData.purchasePrice ? parseFloat(formData.purchasePrice) : null }; const r = editingAsset ? await assetsData.update(editingAsset.assetId, submitData) : await assetsData.create(submitData); setIsSubmitting(false); if (r.success) { setShowModal(false); reload(); } };

  const columns = useMemo(() => [
    { field: "assetCode", headerName: "Code", width: 120 },
    { field: "assetName", headerName: "Name", width: 200 },
    { field: "categoryName", headerName: "Category", width: 150 },
    { field: "brand", headerName: "Brand", width: 120 },
    { field: "model", headerName: "Model", width: 120 },
    { field: "status", headerName: "Status", width: 130, renderCell: (p) => <span className={`status-badge status-badge--${utilsHelper.getStatusColor(p.value)}`}>{p.value}</span> },
    { field: "condition", headerName: "Condition", width: 100 },
    { field: "purchasePrice", headerName: "Price", width: 130, valueFormatter: (p) => utilsHelper.formatCurrency(p.value) },
    { field: "actions", headerName: "Actions", width: 100, sortable: false, renderCell: (p) => (<div className="table-actions"><button className="icon-btn icon-btn--lg" onClick={() => handleEdit(p.row)} title="Edit"><FiEdit2 size={18} /></button><button className="icon-btn icon-btn--danger icon-btn--lg" onClick={() => handleDelete(p.row)} title="Delete"><FiTrash2 size={18} /></button></div>) },
  ], []);

  const tabs = [
    { id: "all", label: "All Assets" },
    { id: "available", label: "Available" },
    { id: "assigned", label: "Assigned" },
    { id: "maintenance", label: "Under Repair" },
  ];

  if (loading && !assets.length) return <div className="assets-loading"><Spinner size="lg" /></div>;

  return (
    <div className="assets-menu">
      <PageHeader title="Asset Management" buttonText="Add Asset" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      <Tabs tabs={tabs} activeTab={activeTab} onTabChange={(tab) => { setActiveTab(tab); setPage(1); }} />
      <SearchToolbar onSearch={handleSearch} onFilterToggle={() => setShowFilters(!showFilters)} showFilters={showFilters} placeholder="Search by code, name..." />

      {showFilters && (
        <Box sx={{ display: 'flex', gap: 2, mb: 3, p: 2, bgcolor: 'var(--surface)', borderRadius: 2 }}>
          <Select label="Status" value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} options={[{ value: "", label: "All" }, { value: "Available", label: "Available" }, { value: "Assigned", label: "Assigned" }, { value: "Under Repair", label: "Under Repair" }]} />
          <Select label="Category" value={categoryFilter} onChange={(e) => { setCategoryFilter(e.target.value); setPage(1); }} options={[{ value: "", label: "All" }, ...dropdownData.categories.map(c => ({ value: c.categoryId, label: c.categoryName }))]} />
        </Box>
      )}

      <div className="assets-menu__table">
        <DataTable rows={assets} columns={columns} loading={loading} pageSize={pageSize} getRowId={(row) => row.assetId} hideFooter={true} />
      </div>

      <Pagination currentPage={page} totalPages={Math.ceil(totalCount / pageSize)} pageSize={pageSize} totalItems={totalCount} onPageChange={setPage} onPageSizeChange={setPageSize} />

      <Modal isOpen={showModal} onClose={() => !isSubmitting && setShowModal(false)} title={editingAsset ? "Edit Asset" : "Add New Asset"} size="lg">
        <form onSubmit={handleSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}><Input label="Asset Name" value={formData.assetName} onChange={e => setFormData({...formData, assetName: e.target.value})} required /></Grid>
            <Grid item xs={12} sm={6}><Select label="Category" value={formData.categoryId} onChange={e => setFormData({...formData, categoryId: e.target.value})} options={dropdownData.categories.map(c => ({value: c.categoryId, label: c.categoryName}))} required /></Grid>
            <Grid item xs={12} sm={6}><Input label="Sub Category" value={formData.subCategory} onChange={e => setFormData({...formData, subCategory: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Brand" value={formData.brand} onChange={e => setFormData({...formData, brand: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Model" value={formData.model} onChange={e => setFormData({...formData, model: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Serial Number" value={formData.serialNumber} onChange={e => setFormData({...formData, serialNumber: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Purchase Date" type="date" value={formData.purchaseDate} onChange={e => setFormData({...formData, purchaseDate: e.target.value})} InputLabelProps={{ shrink: true }} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Purchase Price" type="number" value={formData.purchasePrice} onChange={e => setFormData({...formData, purchasePrice: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Select label="Condition" value={formData.condition} onChange={e => setFormData({...formData, condition: e.target.value})} options={[{value: "Good", label: "Good"}, {value: "Fair", label: "Fair"}, {value: "Poor", label: "Poor"}]} /></Grid>
            <Grid item xs={12} sm={6}><Select label="Status" value={formData.status} onChange={e => setFormData({...formData, status: e.target.value})} options={[{value: "Available", label: "Available"}, {value: "Assigned", label: "Assigned"}, {value: "Under Repair", label: "Under Repair"}]} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Location" value={formData.location} onChange={e => setFormData({...formData, location: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Select label="Current Holder" value={formData.currentHolderId} onChange={e => setFormData({...formData, currentHolderId: e.target.value})} options={[{value: "", label: "None"}, ...dropdownData.employees.map(e => ({value: e.employeeId, label: e.fullName}))]} /></Grid>
            <Grid item xs={12} sm={6}><Select label="Supplier" value={formData.supplierId} onChange={e => setFormData({...formData, supplierId: e.target.value})} options={[{value: "", label: "None"}, ...dropdownData.suppliers.map(s => ({value: s.supplierId, label: s.supplierName}))]} /></Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={() => setShowModal(false)} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>{editingAsset ? "Update" : "Create"}</Button>
          </Box>
        </form>
      </Modal>
    </div>
  );
};

export default AssetsMenu;