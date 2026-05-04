import React, { useState, useMemo, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box, Chip } from "@mui/material";
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
import FilterPanel from "../../components/molecules/FilterPanel/FilterPanel";
import IconButton from "../../components/atoms/IconButton/IconButton";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudForm } from "../../hooks/useCrudForm";
import utilsHelper from "../../core/utils/utils.helper";
import "./Assets.scss";

const assetsData = new AssetsData();

const INITIAL_FORM_DATA = {
  assetName: "", categoryId: "", subCategory: "", brand: "", model: "",
  serialNumber: "", purchaseDate: "", purchasePrice: "", condition: "Good",
  status: "Available", location: "", currentHolderId: "", supplierId: "",
};

const transformAssetFormData = (data) => ({
  ...data,
  purchasePrice: data.purchasePrice ? parseFloat(data.purchasePrice) : null,
});

const CRUD_OPTIONS = {
  idField: 'assetId',
  transformFormData: transformAssetFormData,
};

const TABS = [
  { id: "all", label: "All Assets" },
  { id: "available", label: "Available" },
  { id: "assigned", label: "Assigned" },
  { id: "maintenance", label: "Under Repair" },
];

const STATUS_CHIP_COLORS = {
  'Available': { bg: 'rgba(16, 185, 129, 0.1)', color: '#10b981' },
  'Assigned': { bg: 'rgba(59, 130, 246, 0.1)', color: '#3b82f6' },
  'Under Repair': { bg: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b' },
  'Retired': { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280' },
  'Disposed': { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280' },
};

const CONDITION_OPTIONS = [
  { value: "Good", label: "Good" },
  { value: "Fair", label: "Fair" },
  { value: "Poor", label: "Poor" },
];

const STATUS_OPTIONS = [
  { value: "Available", label: "Available" },
  { value: "Assigned", label: "Assigned" },
  { value: "Under Repair", label: "Under Repair" },
];

const FILTER_STATUS_OPTIONS = [
  { value: "", label: "All" },
  ...STATUS_OPTIONS,
];

const AssetsMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showFilters, setShowFilters] = useState(false);
  const [statusFilter, setStatusFilter] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");

  const { categories, suppliers, employees } = useReferenceData();

  const {
    showModal,
    editingRecord: editingAsset,
    isSubmitting,
    formData,
    setFormData,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudForm(INITIAL_FORM_DATA, assetsData, CRUD_OPTIONS);

  const fetchGridData = useCallback(async (params) => {
    const filters = { ...params };
    if (activeTab === "available") filters.status = "Available";
    else if (activeTab === "assigned") filters.status = "Assigned";
    else if (activeTab === "maintenance") filters.status = "Under Repair";
    if (statusFilter) filters.status = statusFilter;
    if (categoryFilter) filters.categoryId = categoryFilter;
    return assetsData.fetchGridData(filters);
  }, [activeTab, statusFilter, categoryFilter]);

  const {
    data: assets,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    updateFilters,
    reload
  } = useGridData(['assets', activeTab, statusFilter, categoryFilter], fetchGridData);

  const handleSearch = useCallback((search) => {
    updateFilters({ search });
  }, [updateFilters]);

  const handleDelete = useCallback(async (asset) => {
    const r = await assetsData.delete(asset.assetId);
    if (r.success) reload();
  }, [reload]);

  const onSubmit = useCallback(async (e) => {
    e.preventDefault();
    const success = await crudHandleSubmit();
    if (success) reload();
  }, [crudHandleSubmit, reload]);

  const columns = useMemo(() => [
    { field: "assetCode", headerName: "Code", width: 120 },
    { field: "assetName", headerName: "Name", flex: 1, minWidth: 180 },
    { field: "categoryName", headerName: "Category", width: 150 },
    { field: "brand", headerName: "Brand", width: 120 },
    { field: "model", headerName: "Model", width: 120 },
    {
      field: "status",
      headerName: "Status",
      width: 130,
      renderCell: (p) => {
        const colors = STATUS_CHIP_COLORS[p.value] || STATUS_CHIP_COLORS['Available'];
        return (
          <Chip
            label={p.value || '-'}
            size="small"
            sx={{
              bgcolor: colors.bg,
              color: colors.color,
              fontWeight: 500,
              fontSize: '0.75rem',
              height: 24,
              borderRadius: '4px',
            }}
          />
        );
      },
    },
    { field: "condition", headerName: "Condition", width: 100 },
    {
      field: "purchasePrice",
      headerName: "Price",
      width: 130,
      valueFormatter: (p) => utilsHelper.formatCurrency(p.value),
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 100,
      sortable: false,
      renderCell: (p) => (
        <div className="table-actions">
          <IconButton onClick={() => handleEdit(p.row)} title="Edit asset" size="lg">
            <FiEdit2 size={18} />
          </IconButton>
          <IconButton onClick={() => handleDelete(p.row)} title="Delete asset" variant="danger" size="lg">
            <FiTrash2 size={18} />
          </IconButton>
        </div>
      )
    },
  ], [handleEdit, handleDelete]);

  const categoryOptions = useMemo(() => [
    { value: "", label: "All" },
    ...categories.map(c => ({ value: c.value, label: c.label }))
  ], [categories]);

  const employeeOptions = useMemo(() => [
    { value: "", label: "None" },
    ...employees.map(e => ({ value: e.value, label: e.label }))
  ], [employees]);

  const supplierOptions = useMemo(() => [
    { value: "", label: "None" },
    ...suppliers.map(s => ({ value: s.value, label: s.label }))
  ], [suppliers]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
  }, [setPage]);

  if (loading && !assets.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="assets-menu">
      <PageHeader title="Asset Management" buttonText="Add Asset" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} onFilterToggle={() => setShowFilters(!showFilters)} showFilters={showFilters} placeholder="Search by code, name..." />

      <FilterPanel visible={showFilters}>
        <Select label="Status" value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} options={FILTER_STATUS_OPTIONS} />
        <Select label="Category" value={categoryFilter} onChange={(e) => { setCategoryFilter(e.target.value); setPage(1); }} options={categoryOptions} />
      </FilterPanel>

      <div className="assets-menu__table">
        <DataTable
          rows={assets}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.assetId}
          hideFooter={true}
          ariaLabel="Assets data table"
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

      <Modal isOpen={showModal} onClose={handleClose} title={editingAsset ? "Edit Asset" : "Add New Asset"} size="lg">
        <form onSubmit={onSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input label="Asset Name" value={formData.assetName} onChange={e => setFormData({ ...formData, assetName: e.target.value })} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Category" value={formData.categoryId} onChange={e => setFormData({ ...formData, categoryId: e.target.value })} options={categories.map(c => ({ value: c.value, label: c.label }))} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Sub Category" value={formData.subCategory} onChange={e => setFormData({ ...formData, subCategory: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Brand" value={formData.brand} onChange={e => setFormData({ ...formData, brand: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Model" value={formData.model} onChange={e => setFormData({ ...formData, model: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Serial Number" value={formData.serialNumber} onChange={e => setFormData({ ...formData, serialNumber: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Purchase Date" type="date" value={formData.purchaseDate} onChange={e => setFormData({ ...formData, purchaseDate: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Purchase Price" type="number" value={formData.purchasePrice} onChange={e => setFormData({ ...formData, purchasePrice: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Condition" value={formData.condition} onChange={e => setFormData({ ...formData, condition: e.target.value })} options={CONDITION_OPTIONS} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Status" value={formData.status} onChange={e => setFormData({ ...formData, status: e.target.value })} options={STATUS_OPTIONS} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Location" value={formData.location} onChange={e => setFormData({ ...formData, location: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Current Holder" value={formData.currentHolderId} onChange={e => setFormData({ ...formData, currentHolderId: e.target.value })} options={employeeOptions} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Supplier" value={formData.supplierId} onChange={e => setFormData({ ...formData, supplierId: e.target.value })} options={supplierOptions} />
            </Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={handleClose} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>
              {editingAsset ? "Update" : "Create"}
            </Button>
          </Box>
        </form>
      </Modal>
    </div>
  );
};

export default AssetsMenu;