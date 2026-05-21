import React, { useState, useMemo, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus, FiUpload, FiCheckSquare, FiDownload } from "react-icons/fi";
import { Grid, Box, Chip, Button as MuiButton } from "@mui/material";
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
import ImportModal from "../../components/molecules/ImportModal/ImportModal";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudForm } from "../../hooks/useCrudForm";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { ASSET_STATUS_OPTIONS, CONDITION_OPTIONS } from "../../core/constants/assetStatuses";
import { ASSET_CONDITION_MAP, getAssetConditionName } from "../../core/utils/mappingHelpers";
import utilsHelper from "../../core/utils/utils.helper";
import FileUploader from '../../components/molecules/FileUploader/FileUploader';
import "./Assets.scss";

const assetsData = new AssetsData();

// UPDATED: Removed obsolete fields, added new fields
const INITIAL_FORM_DATA = {
  assetCode: "",
  assetName: "",
  categoryId: "",
  brand: "",
  model: "",
  serialNumber: "",
  imei: "",
  macAddress: "",
  hostname: "",
  ipAddress: "",
  purchaseDate: "",
  purchasePrice: "",
  invoiceNumber: "",
  supplierId: "",
  warrantyPeriod: "",
  warrantyExpiryDate: "",
  assetCondition: "",
  assetConditionPurchase: "",
  officeId: "",
  operasionalOffice: false,
  residualValue: "",
  usefulLife: "",
  depreciationStartDate: "",
  notes: "",
};

// STATUS TABS (derived, not from field)
const ASSET_TABS = [
  { id: "all", label: "All Assets" },
  { id: "Available", label: "Available" },
  { id: "Assigned", label: "Assigned" },
  { id: "On Loan", label: "On Loan" },
  { id: "In Maintenance", label: "In Maintenance" },
];

const NULLABLE_STRING_FIELDS = [
  'brand', 'model', 'serialNumber', 'imei', 'macAddress',
  'hostname', 'ipAddress', 'invoiceNumber', 'notes'
];

const CRUD_OPTIONS = { 
  idField: 'assetId', 
  transformFormData: (data) => assetsData.transformFormData ? assetsData.transformFormData(data) : data 
};

// Override transform for AssetsData
assetsData.transformFormData = (data) => {
  const result = { ...data };
  NULLABLE_STRING_FIELDS.forEach(f => {
    if (result[f] === '' || result[f] === undefined) result[f] = null;
  });
  
  const intFields = ['categoryId', 'supplierId', 'officeId', 'warrantyPeriod', 'usefulLife', 'assetCondition', 'assetConditionPurchase'];
  intFields.forEach(f => {
    if (result[f] === '' || result[f] === null || result[f] === undefined) result[f] = null;
    else if (typeof result[f] === 'string') result[f] = parseInt(result[f], 10);
  });
  
  const floatFields = ['purchasePrice', 'residualValue'];
  floatFields.forEach(f => {
    if (result[f] === '' || result[f] === null || result[f] === undefined) result[f] = null;
    else if (typeof result[f] === 'string') result[f] = parseFloat(result[f]);
  });
  
  if (result.operasionalOffice === 'true' || result.operasionalOffice === true) result.operasionalOffice = true;
  else if (result.operasionalOffice === 'false' || result.operasionalOffice === false) result.operasionalOffice = false;
  else result.operasionalOffice = null;
  
  return result;
};

const AssetsMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showFilters, setShowFilters] = useState(false);
  const [statusFilter, setStatusFilter] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [selectedRows, setSelectedRows] = useState([]);
  const [showImportModal, setShowImportModal] = useState(false);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);

  const { categories, suppliers, offices, assetConditions } = useReferenceData();

  const {
    showModal, editingRecord: editingAsset, isSubmitting,
    formData, setFormData, handleCreate, handleEdit, handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudForm(INITIAL_FORM_DATA, assetsData, CRUD_OPTIONS);

  const fetchGridData = useCallback(async (params) => {
    const filters = { ...params };
    if (activeTab !== 'all') filters.status = activeTab;
    if (statusFilter) filters.status = statusFilter;
    if (categoryFilter) filters.categoryId = categoryFilter;
    return assetsData.fetchGridData(filters);
  }, [activeTab, statusFilter, categoryFilter]);

  const {
    data: rawAssets, loading, page, setPage, pageSize, setPageSize, updateFilters, reload
  } = useGridData(['assets', activeTab, statusFilter, categoryFilter], fetchGridData);

  // Derive status for each asset (from transaction, not from field)
  const assets = useMemo(() => {
    return (rawAssets || []).map(asset => ({
      ...asset,
      // Status will be derived from active transaction by backend
      // Frontend just displays what backend returns
      displayStatus: asset.currentStatus || asset.status || 'Available',
      displayCondition: asset.assetConditionName || getAssetConditionName(asset.assetCondition) || '-',
    }));
  }, [rawAssets]);

  const totalCount = assets.length;

  const handleSearch = useCallback((search) => updateFilters({ search }), [updateFilters]);
  
  const handleDelete = useCallback(async (asset) => {
    const r = await assetsData.delete(asset.assetId);
    if (r.success) reload();
  }, [reload]);
  
  const onSubmit = useCallback(async (e) => {
    e.preventDefault();
    const success = await crudHandleSubmit();
    if (success) reload();
  }, [crudHandleSubmit, reload]);
  
  const handleBulkActivate = useCallback(async (ids, activate) => {
    const r = await assetsData.bulkActivate(ids, activate);
    if (r.success) {
      setSelectedRows([]);
      reload();
    }
  }, [reload]);
  
  const handleImport = useCallback(async (file) => {
    setIsImporting(true);
    const r = await assetsData.importAssets(file);
    setIsImporting(false);
    if (r.success) {
      setImportResult(r.data);
      reload();
    }
  }, [reload]);
  
  const handleDownloadTemplate = useCallback(async () => {
    await assetsData.downloadTemplate();
  }, []);

  const columns = useMemo(() => [
    { field: "assetCode", headerName: "Code", width: 120 },
    { field: "assetName", headerName: "Name", flex: 1, minWidth: 180 },
    { field: "categoryName", headerName: "Category", width: 150 },
    { field: "brand", headerName: "Brand", width: 100 },
    { field: "model", headerName: "Model", width: 100 },
    { 
      field: "displayStatus", 
      headerName: "Status", 
      width: 140, 
      renderCell: (p) => <Chip label={p?.value || '-'} size="small" sx={getStatusChipStyles(p?.value)} /> 
    },
    { field: "displayCondition", headerName: "Condition", width: 100 },
    { field: "currentHolderName", headerName: "Holder", width: 150 },
    { field: "officeName", headerName: "Office", width: 150 },
    { 
      field: "purchasePrice", 
      headerName: "Price", 
      width: 130, 
      valueFormatter: (p) => p?.value != null ? utilsHelper.formatCurrency(p.value) : '-' 
    },
    { 
      field: "actions", 
      headerName: "Actions", 
      width: 100, 
      sortable: false, 
      renderCell: (p) => (
        <div className="table-actions">
          <IconButton onClick={() => handleEdit(p?.row)} title="Edit asset" size="lg"><FiEdit2 size={18} /></IconButton>
          <IconButton onClick={() => handleDelete(p?.row)} title="Delete asset" variant="danger" size="lg"><FiTrash2 size={18} /></IconButton>
        </div>
      )
    },
  ], [handleEdit, handleDelete]);

  const categoryOptions = useMemo(() => [
    { value: "", label: "All Categories" },
    ...categories.map(c => ({ value: c.value, label: c.label }))
  ], [categories]);
  
  const supplierOptions = useMemo(() => [
    { value: "", label: "None" },
    ...suppliers.map(s => ({ value: s.value, label: s.label }))
  ], [suppliers]);
  
  const officeOptions = useMemo(() => [
    { value: "", label: "None" },
    ...offices.map(o => ({ value: o.value, label: o.label }))
  ], [offices]);
  
  const conditionOptions = useMemo(() => [
    { value: "", label: "Select Condition" },
    ...assetConditions.map(c => ({ value: c.value, label: c.label }))
  ], [assetConditions]);
  
  const conditionPurchaseOptions = [
    { value: "", label: "Select" },
    { value: "1", label: "New" },
    { value: "2", label: "Second Hand" },
  ];

  const handleTabChange = useCallback((tab) => { 
    setActiveTab(tab); 
    setPage(1); 
  }, [setPage]);

  if (loading && !rawAssets.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="assets-menu">
      <div className="page-header">
        <h1 className="page-title">Asset</h1>
        <div style={{ display: 'flex', gap: '12px' }}>
          <Button variant="outline" onClick={() => setShowImportModal(true)} startIcon={<FiUpload />}>
            Import
          </Button>
          {selectedRows.length > 0 && (
            <Button variant="primary" onClick={() => setShowBulkActivateModal(true)} startIcon={<FiCheckSquare />}>
              Activate ({selectedRows.length})
            </Button>
          )}
          <PageHeader title="Asset Management" buttonText="Add Asset" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
        </div>
      </div>

      <Tabs tabs={ASSET_TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} onFilterToggle={() => setShowFilters(!showFilters)} showFilters={showFilters} placeholder="Search by code, name, serial..." />
      <FilterPanel visible={showFilters}>
        <Select label="Status" value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} options={[{ value: "", label: "All Statuses" }, ...ASSET_STATUS_OPTIONS]} />
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
          checkboxSelection={true}
          onSelectionChange={(newSelection) => setSelectedRows(newSelection)}
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

      {/* Create/Edit Modal */}
      <Modal isOpen={showModal} onClose={handleClose} title={editingAsset ? "Edit Asset" : "Add New Asset"} size="lg">
        <form onSubmit={onSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input label="Asset Code" value={formData.assetCode || ""} onChange={e => setFormData({ ...formData, assetCode: e.target.value })} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Asset Name" value={formData.assetName || ""} onChange={e => setFormData({ ...formData, assetName: e.target.value })} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Category" value={formData.categoryId || ""} onChange={e => setFormData({ ...formData, categoryId: e.target.value })} options={categories.map(c => ({ value: c.value, label: c.label }))} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Supplier" value={formData.supplierId || ""} onChange={e => setFormData({ ...formData, supplierId: e.target.value })} options={supplierOptions} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Brand" value={formData.brand || ""} onChange={e => setFormData({ ...formData, brand: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Model" value={formData.model || ""} onChange={e => setFormData({ ...formData, model: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input label="Serial Number" value={formData.serialNumber || ""} onChange={e => setFormData({ ...formData, serialNumber: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input label="IMEI" value={formData.imei || ""} onChange={e => setFormData({ ...formData, imei: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input label="MAC Address" value={formData.macAddress || ""} onChange={e => setFormData({ ...formData, macAddress: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input label="Hostname" value={formData.hostname || ""} onChange={e => setFormData({ ...formData, hostname: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input label="IP Address" value={formData.ipAddress || ""} onChange={e => setFormData({ ...formData, ipAddress: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Input label="Invoice Number" value={formData.invoiceNumber || ""} onChange={e => setFormData({ ...formData, invoiceNumber: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <DatePickerInput label="Purchase Date" value={formData.purchaseDate || ""} onChange={e => setFormData({ ...formData, purchaseDate: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <NumberInput label="Purchase Price" value={formData.purchasePrice} onChange={e => setFormData({ ...formData, purchasePrice: e.target.value })} prefix="Rp " thousandSeparator={true} decimalScale={0} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <NumberInput label="Warranty (Months)" value={formData.warrantyPeriod} onChange={e => setFormData({ ...formData, warrantyPeriod: e.target.value })} suffix=" months" decimalScale={0} min={0} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <DatePickerInput label="Warranty Expiry" value={formData.warrantyExpiryDate || ""} onChange={e => setFormData({ ...formData, warrantyExpiryDate: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Select label="Condition" value={formData.assetCondition || ""} onChange={e => setFormData({ ...formData, assetCondition: e.target.value })} options={conditionOptions} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Select label="Purchase Condition" value={formData.assetConditionPurchase || ""} onChange={e => setFormData({ ...formData, assetConditionPurchase: e.target.value })} options={conditionPurchaseOptions} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Select label="Office" value={formData.officeId || ""} onChange={e => setFormData({ ...formData, officeId: e.target.value })} options={officeOptions} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <Select label="Operasional Office" value={formData.operasionalOffice ? "true" : "false"} onChange={e => setFormData({ ...formData, operasionalOffice: e.target.value === "true" })} options={[{ value: "false", label: "No" }, { value: "true", label: "Yes" }]} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <NumberInput label="Residual Value" value={formData.residualValue} onChange={e => setFormData({ ...formData, residualValue: e.target.value })} prefix="Rp " thousandSeparator={true} decimalScale={0} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <NumberInput label="Useful Life (Years)" value={formData.usefulLife} onChange={e => setFormData({ ...formData, usefulLife: e.target.value })} suffix=" years" decimalScale={0} min={1} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <DatePickerInput label="Depreciation Start" value={formData.depreciationStartDate || ""} onChange={e => setFormData({ ...formData, depreciationStartDate: e.target.value })} />
            </Grid>
            <Grid item xs={12}>
              <Input label="Notes" value={formData.notes || ""} onChange={e => setFormData({ ...formData, notes: e.target.value })} multiline rows={2} />
            </Grid>
            <Grid item xs={12}>
  <FileUploader 
    referenceTable="Asset"
    referenceId={editingAsset?.assetId}
    onUploadComplete={reload}
  />
</Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={handleClose} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>{editingAsset ? "Update" : "Create"}</Button>
          </Box>
        </form>
      </Modal>

      {/* Import Modal */}
      <ImportModal
        isOpen={showImportModal}
        onClose={() => { setShowImportModal(false); setImportResult(null); }}
        onImport={handleImport}
        onDownloadTemplate={handleDownloadTemplate}
        isImporting={isImporting}
        importResult={importResult}
        title="Import Assets"
        description="Upload Excel or TXT file with asset data. Assets will be imported as INACTIVE and need activation."
      />

      {/* Bulk Activate Modal */}
      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={handleBulkActivate}
        selectedIds={selectedRows}
        itemName="assets"
      />
    </div>
  );
};

export default AssetsMenu;