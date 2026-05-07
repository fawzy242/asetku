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
import DatePickerInput from "../../components/atoms/Input/DatePickerInput";
import NumberInput from "../../components/atoms/Input/NumberInput";
import Spinner from "../../components/atoms/Spinner/Spinner";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import SearchToolbar from "../../components/molecules/SearchToolbar/SearchToolbar";
import Tabs from "../../components/molecules/Tabs/Tabs";
import FilterPanel from "../../components/molecules/FilterPanel/FilterPanel";
import IconButton from "../../components/atoms/IconButton/IconButton";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudForm } from "../../hooks/useCrudForm";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { ASSET_STATUS_OPTIONS, ASSET_STATUS_FILTER_OPTIONS, ASSET_STATUS_TABS, CONDITION_OPTIONS } from "../../core/constants/assetStatuses";
import utilsHelper from "../../core/utils/utils.helper";
import "./Assets.scss";

const assetsData = new AssetsData();

const INITIAL_FORM_DATA = {
  assetName: "", categoryId: "", subCategory: "", assetType: "",
  brand: "", model: "", serialNumber: "", imei: "", macAddress: "",
  purchaseDate: "", purchasePrice: "", invoiceNumber: "", supplierId: "",
  warrantyPeriod: "", warrantyExpiryDate: "",
  condition: "Good", status: "Available", location: "",
  currentHolderId: "", responsiblePartyId: "",
  residualValue: "", usefulLife: "", depreciationStartDate: "", notes: "",
};

const NULLABLE_STRING_FIELDS = [
  'subCategory', 'assetType', 'brand', 'model', 'serialNumber',
  'imei', 'macAddress', 'invoiceNumber', 'condition', 'status',
  'location', 'notes', 'purchaseDate', 'warrantyExpiryDate', 'depreciationStartDate'
];

const NULLABLE_INT_FIELDS = [
  'supplierId', 'currentHolderId', 'responsiblePartyId', 'warrantyPeriod', 'usefulLife'
];

const NULLABLE_FLOAT_FIELDS = ['purchasePrice', 'residualValue'];

const transformAssetFormData = (data) => {
  const result = { ...data };
  NULLABLE_STRING_FIELDS.forEach(f => { if (result[f] === '' || result[f] === undefined) result[f] = null; });
  NULLABLE_INT_FIELDS.forEach(f => {
    if (result[f] === '' || result[f] === null || result[f] === undefined) result[f] = null;
    else if (typeof result[f] === 'string' && result[f].trim() !== '' && !isNaN(result[f])) result[f] = parseInt(result[f], 10);
    else if (typeof result[f] === 'number') result[f] = result[f];
  });
  NULLABLE_FLOAT_FIELDS.forEach(f => {
    if (result[f] === '' || result[f] === null || result[f] === undefined) result[f] = null;
    else if (typeof result[f] === 'string' && result[f].trim() !== '' && !isNaN(result[f])) result[f] = parseFloat(result[f]);
    else if (typeof result[f] === 'number') result[f] = result[f];
  });
  if (result.categoryId === '' || result.categoryId === null || result.categoryId === undefined) result.categoryId = 0;
  else if (typeof result.categoryId === 'string') result.categoryId = parseInt(result.categoryId, 10);
  else if (typeof result.categoryId === 'number') result.categoryId = result.categoryId;
  return result;
};

const CRUD_OPTIONS = { idField: 'assetId', transformFormData: transformAssetFormData };

const AssetsMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showFilters, setShowFilters] = useState(false);
  const [statusFilter, setStatusFilter] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");

  const { categories, suppliers, employees } = useReferenceData();

  const {
    showModal, editingRecord: editingAsset, isSubmitting,
    formData, setFormData, handleCreate, handleEdit, handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudForm(INITIAL_FORM_DATA, assetsData, CRUD_OPTIONS);

  const fetchGridData = useCallback(async (params) => {
    const filters = { ...params };
    return assetsData.fetchGridData(filters);
  }, []);

  const {
    data: rawAssets, loading, page, setPage, pageSize, setPageSize, updateFilters, reload
  } = useGridData(['assets', activeTab, statusFilter, categoryFilter], fetchGridData);

  const { assets, totalCount } = useMemo(() => {
    let filtered = [...rawAssets];
    if (activeTab && activeTab !== 'all') filtered = filtered.filter(a => a.status === activeTab);
    if (statusFilter) filtered = filtered.filter(a => a.status === statusFilter);
    return { assets: filtered, totalCount: filtered.length };
  }, [rawAssets, activeTab, statusFilter]);

  const handleSearch = useCallback((search) => updateFilters({ search }), [updateFilters]);
  const handleDelete = useCallback(async (asset) => { const r = await assetsData.delete(asset.assetId); if (r.success) reload(); }, [reload]);
  const onSubmit = useCallback(async (e) => { e.preventDefault(); const success = await crudHandleSubmit(); if (success) reload(); }, [crudHandleSubmit, reload]);

  const columns = useMemo(() => [
    { field: "assetCode", headerName: "Code", width: 120 },
    { field: "assetName", headerName: "Name", flex: 1, minWidth: 180 },
    { field: "categoryName", headerName: "Category", width: 150 },
    { field: "brand", headerName: "Brand", width: 100 },
    { field: "model", headerName: "Model", width: 100 },
    { field: "status", headerName: "Status", width: 140, renderCell: (p) => <Chip label={p?.value || '-'} size="small" sx={getStatusChipStyles(p?.value)} /> },
    { field: "condition", headerName: "Condition", width: 100 },
    { field: "currentHolderName", headerName: "Holder", width: 150 },
    { field: "purchasePrice", headerName: "Price", width: 130, valueFormatter: (p) => p?.value != null ? utilsHelper.formatCurrency(p.value) : '-' },
    { field: "actions", headerName: "Actions", width: 100, sortable: false, renderCell: (p) => (
      <div className="table-actions">
        <IconButton onClick={() => handleEdit(p?.row)} title="Edit asset" size="lg"><FiEdit2 size={18} /></IconButton>
        <IconButton onClick={() => handleDelete(p?.row)} title="Delete asset" variant="danger" size="lg"><FiTrash2 size={18} /></IconButton>
      </div>
    )},
  ], [handleEdit, handleDelete]);

  const categoryOptions = useMemo(() => [{ value: "", label: "All" }, ...categories.map(c => ({ value: c.value, label: c.label }))], [categories]);
  const employeeOptions = useMemo(() => [{ value: "", label: "None" }, ...employees.map(e => ({ value: e.value, label: e.label }))], [employees]);
  const supplierOptions = useMemo(() => [{ value: "", label: "None" }, ...suppliers.map(s => ({ value: s.value, label: s.label }))], [suppliers]);
  const handleTabChange = useCallback((tab) => { setActiveTab(tab); setPage(1); }, [setPage]);

  if (loading && !rawAssets.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="assets-menu">
       <div className="page-header"><h1 className="page-title">Asset</h1><PageHeader title="Asset Management" buttonText="Add Asset" onButtonClick={handleCreate} buttonIcon={<FiPlus />} /></div>

      <Tabs tabs={ASSET_STATUS_TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} onFilterToggle={() => setShowFilters(!showFilters)} showFilters={showFilters} placeholder="Search by code, name, serial..." />
      <FilterPanel visible={showFilters}>
        <Select label="Status" value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} options={ASSET_STATUS_FILTER_OPTIONS} />
        <Select label="Category" value={categoryFilter} onChange={(e) => { setCategoryFilter(e.target.value); setPage(1); }} options={categoryOptions} />
      </FilterPanel>
      <div className="assets-menu__table">
        <DataTable rows={assets} columns={columns} loading={loading} pageSize={pageSize} getRowId={(row) => row.assetId} hideFooter={true} ariaLabel="Assets data table" />
      </div>
      <Pagination currentPage={page} totalPages={Math.ceil(totalCount / pageSize) || 1} pageSize={pageSize} totalItems={totalCount} onPageChange={setPage} onPageSizeChange={setPageSize} />

      <Modal isOpen={showModal} onClose={handleClose} title={editingAsset ? "Edit Asset" : "Add New Asset"} size="lg">
        <form onSubmit={onSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}><Input label="Asset Name" value={formData.assetName || ""} onChange={e => setFormData({ ...formData, assetName: e.target.value })} required /></Grid>
            <Grid item xs={12} sm={6}><Select label="Category" value={formData.categoryId || ""} onChange={e => setFormData({ ...formData, categoryId: e.target.value })} options={categories.map(c => ({ value: c.value, label: c.label }))} required /></Grid>
            <Grid item xs={12} sm={6}><Input label="Asset Type" value={formData.assetType || ""} onChange={e => setFormData({ ...formData, assetType: e.target.value })} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Sub Category" value={formData.subCategory || ""} onChange={e => setFormData({ ...formData, subCategory: e.target.value })} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Brand" value={formData.brand || ""} onChange={e => setFormData({ ...formData, brand: e.target.value })} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Model" value={formData.model || ""} onChange={e => setFormData({ ...formData, model: e.target.value })} /></Grid>
            <Grid item xs={12} sm={4}><Input label="Serial Number" value={formData.serialNumber || ""} onChange={e => setFormData({ ...formData, serialNumber: e.target.value })} /></Grid>
            <Grid item xs={12} sm={4}><Input label="IMEI" value={formData.imei || ""} onChange={e => setFormData({ ...formData, imei: e.target.value })} /></Grid>
            <Grid item xs={12} sm={4}><Input label="MAC Address" value={formData.macAddress || ""} onChange={e => setFormData({ ...formData, macAddress: e.target.value })} /></Grid>
            <Grid item xs={12} sm={4}><Input label="Invoice Number" value={formData.invoiceNumber || ""} onChange={e => setFormData({ ...formData, invoiceNumber: e.target.value })} /></Grid>
            <Grid item xs={12} sm={4}><DatePickerInput label="Purchase Date" value={formData.purchaseDate || ""} onChange={e => setFormData({ ...formData, purchaseDate: e.target.value })} /></Grid>
            <Grid item xs={12} sm={4}>
              <NumberInput label="Purchase Price" value={formData.purchasePrice} onChange={e => setFormData({ ...formData, purchasePrice: e.target.value })} prefix="Rp " thousandSeparator={true} decimalScale={0} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <NumberInput label="Warranty (Months)" value={formData.warrantyPeriod} onChange={e => setFormData({ ...formData, warrantyPeriod: e.target.value })} suffix=" months" decimalScale={0} min={0} />
            </Grid>
            <Grid item xs={12} sm={4}><DatePickerInput label="Warranty Expiry" value={formData.warrantyExpiryDate || ""} onChange={e => setFormData({ ...formData, warrantyExpiryDate: e.target.value })} /></Grid>
            <Grid item xs={12} sm={4}><Select label="Condition" value={formData.condition || "Good"} onChange={e => setFormData({ ...formData, condition: e.target.value })} options={CONDITION_OPTIONS} /></Grid>
            <Grid item xs={12} sm={4}><Select label="Status" value={formData.status || "Available"} onChange={e => setFormData({ ...formData, status: e.target.value })} options={ASSET_STATUS_OPTIONS} /></Grid>
            <Grid item xs={12} sm={4}><Input label="Location" value={formData.location || ""} onChange={e => setFormData({ ...formData, location: e.target.value })} /></Grid>
            <Grid item xs={12} sm={4}><Select label="Current Holder" value={formData.currentHolderId || ""} onChange={e => setFormData({ ...formData, currentHolderId: e.target.value })} options={employeeOptions} /></Grid>
            <Grid item xs={12} sm={4}><Select label="Responsible Party" value={formData.responsiblePartyId || ""} onChange={e => setFormData({ ...formData, responsiblePartyId: e.target.value })} options={employeeOptions} /></Grid>
            <Grid item xs={12} sm={4}><Select label="Supplier" value={formData.supplierId || ""} onChange={e => setFormData({ ...formData, supplierId: e.target.value })} options={supplierOptions} /></Grid>
            <Grid item xs={12} sm={4}>
              <NumberInput label="Residual Value" value={formData.residualValue} onChange={e => setFormData({ ...formData, residualValue: e.target.value })} prefix="Rp " thousandSeparator={true} decimalScale={0} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <NumberInput label="Useful Life (Years)" value={formData.usefulLife} onChange={e => setFormData({ ...formData, usefulLife: e.target.value })} suffix=" years" decimalScale={0} min={1} />
            </Grid>
            <Grid item xs={12} sm={4}><DatePickerInput label="Depreciation Start" value={formData.depreciationStartDate || ""} onChange={e => setFormData({ ...formData, depreciationStartDate: e.target.value })} /></Grid>
            <Grid item xs={12}><Input label="Notes" value={formData.notes || ""} onChange={e => setFormData({ ...formData, notes: e.target.value })} multiline rows={2} /></Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={handleClose} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>{editingAsset ? "Update" : "Create"}</Button>
          </Box>
        </form>
      </Modal>
    </div>
  );
};

export default AssetsMenu;