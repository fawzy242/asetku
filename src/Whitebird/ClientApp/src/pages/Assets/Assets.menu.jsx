import React, { useState, useMemo, useCallback, useEffect, useRef } from "react";
import { Grid, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import AssetsData from "./Assets.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import GridFilterPanel from "../../components/molecules/GridFilterPanel/GridFilterPanel";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import DatePickerInput from "../../components/atoms/Input/DatePickerInput";
import NumberInput from "../../components/atoms/Input/NumberInput";
import Spinner from "../../components/atoms/Spinner/Spinner";
import IconButton from "../../components/atoms/IconButton/IconButton";
import Button from "../../components/atoms/Button/Button";
import FileUploader from "../../components/molecules/FileUploader/FileUploader";
import ImportModal from "../../components/molecules/ImportModal/ImportModal";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import { FiEdit2, FiTrash2, FiUpload, FiCheckSquare } from "react-icons/fi";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudFormBase } from "../../hooks/useCrudFormBase";
import { cleanAssetFormData } from "../../core/utils/formHelpers";
import utilsHelper from "../../core/utils/utils.helper";
import "./Assets.scss";

const assetsData = new AssetsData();
assetsData.transformFormData = cleanAssetFormData;

const ASSET_TABS = [
  { id: "all", label: "All Assets" },
  { id: "Active", label: "Active" },
  { id: "Inactive", label: "Inactive" },
  { id: "Available", label: "Available" },
  { id: "Assigned", label: "Assigned" },
  { id: "On Loan", label: "On Loan" },
  { id: "In Maintenance", label: "In Maintenance" },
];

const INITIAL_FORM_DATA = {
  assetCode: "", assetName: "", categoryId: "", brand: "", model: "",
  serialNumber: "", imei: "", macAddress: "", hostname: "", ipAddress: "",
  purchaseDate: "", purchasePrice: "", invoiceNumber: "", supplierId: "",
  warrantyPeriod: "", warrantyExpiryDate: "", assetCondition: "",
  assetConditionPurchase: "", officeId: "", operasionalOffice: false,
  residualValue: "", usefulLife: "", depreciationStartDate: "", notes: "",
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
  const [searchTerm, setSearchTerm] = useState("");
  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();

  const { categories, suppliers, offices, assetConditions } = useReferenceData();

  const {
    showModal,
    editingRecord: editingAsset,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, assetsData, {
    idField: 'assetId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      queryClient.invalidateQueries({ queryKey: ['reference', 'assets'] });
    },
  });

  const showCheckbox = useMemo(() => {
    return activeTab === "Active" || activeTab === "Inactive";
  }, [activeTab]);

  // FIX: Build filters based on activeTab and statusFilter
  const buildFilters = useCallback(() => {
    const filters = {};
    
    if (activeTab !== 'all') {
      filters.status = activeTab;
    }
    
    if (statusFilter && statusFilter !== 'all' && statusFilter !== '') {
      filters.status = statusFilter;
    }
    
    if (categoryFilter) {
      filters.categoryId = categoryFilter;
    }
    
    if (searchTerm) {
      filters.search = searchTerm;
    }
    
    return filters;
  }, [activeTab, statusFilter, categoryFilter, searchTerm]);

  const fetchGridData = useCallback(async (params) => {
    const filters = buildFilters();
    const requestParams = {
      page: params.page || 1,
      pageSize: params.pageSize || 10,
      ...filters,
      ...params,
    };
    
    const result = await assetsData.fetchGridData(requestParams);
    return result;
  }, [buildFilters]);

  const {
    data: rawAssets,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    reload
  } = useGridData(['assets', activeTab, statusFilter, categoryFilter, searchTerm], fetchGridData);

  useEffect(() => {
    isMountedRef.current = true;
    setPage(1);
    setSelectedRows([]);
    reload();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, setPage]);

  const assets = useMemo(() => {
    return (rawAssets || []).map(asset => ({
      ...asset,
      displayStatus: asset.currentStatus || asset.status || 'Available',
      displayCondition: asset.assetConditionName || '-',
    }));
  }, [rawAssets]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
  }, [setPage]);

  const handleDelete = useCallback(async (asset) => {
    const r = await assetsData.delete(asset.assetId);
    if (r.success && isMountedRef.current) {
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      reload();
    }
  }, [reload, queryClient]);

  const onSubmit = useCallback(async () => {
    const success = await crudHandleSubmit();
    if (success && isMountedRef.current) {
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      reload();
    }
    return success;
  }, [crudHandleSubmit, reload, queryClient]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
    setStatusFilter("");
    setCategoryFilter("");
    setSearchTerm("");
    setSelectedRows([]);
    setShowFilters(false);
  }, [setPage]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
    const r = await assetsData.bulkActivate(ids, activate);
    if (r.success && isMountedRef.current) {
      setSelectedRows([]);
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      reload();
    }
  }, [reload, queryClient]);

  const handleImport = useCallback(async (file) => {
    setIsImporting(true);
    const r = await assetsData.importAssets(file);
    setIsImporting(false);
    if (r.success && isMountedRef.current) {
      setImportResult(r.data);
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      reload();
    }
  }, [reload, queryClient]);

  const handleDownloadTemplate = useCallback(async () => {
    await assetsData.downloadTemplate();
  }, []);

  const handleResetFilters = useCallback(() => {
    setStatusFilter("");
    setCategoryFilter("");
    setSearchTerm("");
    setPage(1);
  }, [setPage]);

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

  const statusOptions = [
    { value: "", label: "All Statuses" },
    { value: "Available", label: "Available" },
    { value: "Assigned", label: "Assigned" },
    { value: "On Loan", label: "On Loan" },
    { value: "In Maintenance", label: "In Maintenance" },
    { value: "Active", label: "Active" },
    { value: "Inactive", label: "Inactive" },
  ];

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
    { field: "officeName", headerName: "Office", width: 150 },
    { 
      field: "purchasePrice", 
      headerName: "Price", 
      width: 130, 
      valueFormatter: (p) => p?.value ? utilsHelper.formatCurrency(p.value) : '-'
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

  const filterContent = (
    <>
      <Select 
        label="Status" 
        value={statusFilter} 
        onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} 
        options={statusOptions} 
      />
      <Select 
        label="Category" 
        value={categoryFilter} 
        onChange={(e) => { setCategoryFilter(e.target.value); setPage(1); }} 
        options={categoryOptions} 
      />
    </>
  );

  const extraActions = (
    <>
      <Button variant="outline" onClick={() => setShowImportModal(true)} startIcon={<FiUpload />}>
        Import
      </Button>
      {selectedRows.length > 0 && showCheckbox && (
        <Button variant="primary" onClick={() => setShowBulkActivateModal(true)} startIcon={<FiCheckSquare />}>
          {activeTab === "Active" ? "Deactivate" : "Activate"} ({selectedRows.length})
        </Button>
      )}
    </>
  );

  if (loading && !assets.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === "Active" ? false : true;

  return (
    <div className="assets-menu">
      <GridView
        title="Asset Management"
        tabs={ASSET_TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={assets}
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
        createButtonText="Add Asset"
        ariaLabel="Assets data table"
        extraActions={extraActions}
        filterChildren={filterContent}
        showFilters={showFilters}
        onFilterToggle={() => setShowFilters(!showFilters)}
        onResetFilters={handleResetFilters}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingAsset ? "Edit Asset" : "Add Asset"}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingAsset ? "Update" : "Create"}
        size="lg"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Input label="Asset Code" value={formData.assetCode || ""} onChange={(e) => setFormField('assetCode')(e.target.value)} required />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input label="Asset Name" value={formData.assetName || ""} onChange={(e) => setFormField('assetName')(e.target.value)} required />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select label="Category" value={formData.categoryId || ""} onChange={(e) => setFormField('categoryId')(e.target.value)} options={categoryOptions.filter(opt => opt.value !== "")} required />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select label="Supplier" value={formData.supplierId || ""} onChange={(e) => setFormField('supplierId')(e.target.value)} options={supplierOptions} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input label="Brand" value={formData.brand || ""} onChange={(e) => setFormField('brand')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Input label="Model" value={formData.model || ""} onChange={(e) => setFormField('model')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input label="Serial Number" value={formData.serialNumber || ""} onChange={(e) => setFormField('serialNumber')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input label="IMEI" value={formData.imei || ""} onChange={(e) => setFormField('imei')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input label="MAC Address" value={formData.macAddress || ""} onChange={(e) => setFormField('macAddress')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input label="Hostname" value={formData.hostname || ""} onChange={(e) => setFormField('hostname')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input label="IP Address" value={formData.ipAddress || ""} onChange={(e) => setFormField('ipAddress')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Input label="Invoice Number" value={formData.invoiceNumber || ""} onChange={(e) => setFormField('invoiceNumber')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <DatePickerInput label="Purchase Date" value={formData.purchaseDate || ""} onChange={(e) => setFormField('purchaseDate')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <NumberInput label="Purchase Price" value={formData.purchasePrice} onChange={(e) => setFormField('purchasePrice')(e.target.value)} prefix="Rp " thousandSeparator={true} decimalScale={0} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <NumberInput label="Warranty (Months)" value={formData.warrantyPeriod} onChange={(e) => setFormField('warrantyPeriod')(e.target.value)} suffix=" months" decimalScale={0} min={0} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <DatePickerInput label="Warranty Expiry" value={formData.warrantyExpiryDate || ""} onChange={(e) => setFormField('warrantyExpiryDate')(e.target.value)} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Select label="Condition" value={formData.assetCondition || ""} onChange={(e) => setFormField('assetCondition')(e.target.value)} options={conditionOptions} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Select label="Purchase Condition" value={formData.assetConditionPurchase || ""} onChange={(e) => setFormField('assetConditionPurchase')(e.target.value)} options={conditionPurchaseOptions} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Select label="Office" value={formData.officeId || ""} onChange={(e) => setFormField('officeId')(e.target.value)} options={officeOptions} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <Select label="Operasional Office" value={formData.operasionalOffice ? "true" : "false"} onChange={(e) => setFormField('operasionalOffice')(e.target.value === "true")} options={[{ value: "false", label: "No" }, { value: "true", label: "Yes" }]} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <NumberInput label="Residual Value" value={formData.residualValue} onChange={(e) => setFormField('residualValue')(e.target.value)} prefix="Rp " thousandSeparator={true} decimalScale={0} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <NumberInput label="Useful Life (Years)" value={formData.usefulLife} onChange={(e) => setFormField('usefulLife')(e.target.value)} suffix=" years" decimalScale={0} min={1} />
          </Grid>
          <Grid item xs={12} sm={4}>
            <DatePickerInput label="Depreciation Start" value={formData.depreciationStartDate || ""} onChange={(e) => setFormField('depreciationStartDate')(e.target.value)} />
          </Grid>
          <Grid item xs={12}>
            <Input label="Notes" value={formData.notes || ""} onChange={(e) => setFormField('notes')(e.target.value)} multiline rows={2} />
          </Grid>
          <Grid item xs={12}>
            <FileUploader 
              referenceTable="Asset"
              referenceId={editingAsset?.assetId}
              onUploadComplete={reload}
            />
          </Grid>
        </Grid>
      </CrudModal>

      <ImportModal
        isOpen={showImportModal}
        onClose={() => { setShowImportModal(false); setImportResult(null); }}
        onImport={handleImport}
        onDownloadTemplate={handleDownloadTemplate}
        isImporting={isImporting}
        importResult={importResult}
        title="Import Assets"
        description="Upload Excel or TXT file with asset data. Assets will be imported as ACTIVE."
      />

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={selectedRows}
        itemName="assets"
        title={bulkActivateValue ? "Activate Assets" : "Deactivate Assets"}
        description={`This action will ${bulkActivateValue ? "activate" : "deactivate"} the selected assets.`}
      />
    </div>
  );
};

export default AssetsMenu;