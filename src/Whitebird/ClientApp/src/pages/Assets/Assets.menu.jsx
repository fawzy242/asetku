import React, { useState, useMemo, useCallback, useEffect, useRef } from "react";
import { Grid, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import AssetsData from "./Assets.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import FormSection from "../../components/atoms/FormSection/FormSection";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import DatePickerInput from "../../components/atoms/Input/DatePickerInput";
import NumberInput from "../../components/atoms/Input/NumberInput";
import Spinner from "../../components/atoms/Spinner/Spinner";
import FileUploader from "../../components/molecules/FileUploader/FileUploader";
import ImportModal from "../../components/molecules/ImportModal/ImportModal";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import { FiUpload, FiCheckSquare } from "react-icons/fi";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { ASSET_GRID_TABS } from "../../core/constants/assetStatuses";
import { ACTION_TYPES, useGridActions } from "../../hooks/useGridActions";
import { useBulkSelection } from "../../hooks/useBulkSelection";
import { useSweetAlert } from "../../hooks/useSweetAlert";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudFormBase } from "../../hooks/useCrudFormBase";
import { cleanAssetFormData } from "../../core/utils/formHelpers";
import utilsHelper from "../../core/utils/utils.helper";
import "./Assets.scss";

const assetsData = new AssetsData();
assetsData.transformFormData = cleanAssetFormData;

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
  const [searchTerm, setSearchTerm] = useState("");
  const [showImportModal, setShowImportModal] = useState(false);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);
  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirmDelete, confirm } = useSweetAlert();

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

  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } = useBulkSelection({ idField: 'assetId' });

  const showCheckbox = useMemo(() => {
    return activeTab === "Active" || activeTab === "Inactive";
  }, [activeTab]);

  // Check if Operational Office is Yes - show/hide office field
  const showOfficeField = formData.operasionalOffice === true;

  const buildFilters = useCallback(() => {
    const filters = {};
    if (activeTab !== 'all') {
      filters.status = activeTab;
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
  } = useGridData(['assets', activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    isMountedRef.current = true;
    setPage(1);
    clearSelection();
    reload();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, setPage, clearSelection]);

  const assets = useMemo(() => {
    return (rawAssets || []).map(asset => ({
      ...asset,
      id: asset.assetId,
      displayStatus: asset.currentStatus || asset.status || 'Available',
      displayCondition: asset.assetConditionName || '-',
    }));
  }, [rawAssets]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
    clearSelection();
  }, [setPage, clearSelection]);

  const handleDelete = useCallback(async (asset) => {
    const confirmed = await confirmDelete('Delete Asset', `Are you sure you want to delete "${asset.assetName}"?`);
    if (!confirmed) return;
    const r = await assetsData.delete(asset.assetId);
    if (r.success && isMountedRef.current) {
      toast.success('Asset deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      reload();
    }
  }, [reload, queryClient, toast, confirmDelete]);

  const onSubmit = useCallback(async () => {
    // Validate Operational Office rule
    if (formData.operasionalOffice === true && !formData.officeId) {
      toast.error('Office is required when Operational Office is enabled');
      return false;
    }
    
    const success = await crudHandleSubmit();
    if (success && isMountedRef.current) {
      toast.success(editingAsset ? 'Asset updated successfully' : 'Asset created successfully');
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      reload();
    }
    return success;
  }, [crudHandleSubmit, reload, queryClient, toast, editingAsset, formData]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
    setSearchTerm("");
    clearSelection();
  }, [setPage, clearSelection]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
    const actionText = activate ? 'activate' : 'deactivate';
    const confirmed = await confirm({
      title: activate ? 'Activate Assets' : 'Deactivate Assets',
      text: `Are you sure you want to ${actionText} ${ids.length} asset(s)?`,
      confirmButtonText: activate ? 'Yes, Activate' : 'Yes, Deactivate',
    });
    if (!confirmed) return;
    const r = await assetsData.bulkActivate(ids, activate);
    if (r.success && isMountedRef.current) {
      toast.success(`${r.data || ids.length} asset(s) ${actionText}d successfully`);
      clearSelection();
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      reload();
    }
  }, [reload, queryClient, toast, confirm, clearSelection]);

  const handleImport = useCallback(async (file) => {
    setIsImporting(true);
    const r = await assetsData.importAssets(file);
    setIsImporting(false);
    if (r.success && isMountedRef.current) {
      setImportResult(r.data);
      if (r.data.errorCount === 0) {
        toast.success(`Import completed: ${r.data.successCount} assets imported`);
      } else {
        toast.warning(`Import completed: ${r.data.successCount} success, ${r.data.errorCount} errors`);
      }
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      reload();
    }
  }, [reload, queryClient, toast]);

  const handleDownloadTemplate = useCallback(async () => {
    await assetsData.downloadTemplate();
  }, []);

  const handleGridAction = useCallback((actionType, row) => {
    switch (actionType) {
      case ACTION_TYPES.EDIT:
        handleEdit(row);
        break;
      case ACTION_TYPES.DELETE:
        handleDelete(row);
        break;
      default:
        break;
    }
  }, [handleEdit, handleDelete]);

  const getConditionalActions = useCallback(() => {
    return [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE];
  }, []);

  const { actionColumn } = useGridActions({
    actions: [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleGridAction,
    getConditionalActions,
    rowIdField: 'assetId',
  });

  const categoryOptions = useMemo(() => [
    { value: "", label: "Select Category" },
    ...categories.map(c => ({ value: c.value, label: c.label }))
  ], [categories]);

  const supplierOptions = useMemo(() => [
    { value: "", label: "Select Supplier" },
    ...suppliers.map(s => ({ value: s.value, label: s.label }))
  ], [suppliers]);

  const officeOptions = useMemo(() => [
    { value: "", label: "Select Office" },
    ...offices.map(o => ({ value: o.value, label: o.label }))
  ], [offices]);

  const conditionOptions = useMemo(() => [
    { value: "", label: "Select Condition" },
    ...assetConditions.map(c => ({ value: c.value, label: c.label }))
  ], [assetConditions]);

  const conditionPurchaseOptions = [
    { value: "", label: "Select Purchase Condition" },
    { value: "1", label: "New" },
    { value: "2", label: "Second Hand" },
  ];

  const booleanOptions = [
    { value: "false", label: "No" },
    { value: "true", label: "Yes" },
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
    actionColumn,
  ], [actionColumn]);

  const extraActions = (
    <>
      <button className="btn btn--outline btn--sm" onClick={() => setShowImportModal(true)} style={{ display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
        <FiUpload size={16} /> Import
      </button>
      {hasSelection && showCheckbox && (
        <button className="btn btn--primary btn--sm" onClick={() => setShowBulkActivateModal(true)} style={{ display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
          <FiCheckSquare size={16} /> {activeTab === "Active" ? "Deactivate" : "Activate"} ({selectionCount})
        </button>
      )}
    </>
  );

  if (loading && !assets.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === "Active" ? false : true;
  const bulkTitle = bulkActivateValue ? "Activate Assets" : "Deactivate Assets";
  const bulkDescription = `This action will ${bulkActivateValue ? "activate" : "deactivate"} the selected assets.`;

  return (
    <div className="assets-menu">
      <GridView
        title="Asset Management"
        tabs={ASSET_GRID_TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={assets}
        loading={loading}
        totalCount={totalCount}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onSearch={handleSearch}
        showCheckbox={showCheckbox}
        selectedRows={selectedRowIds}
        onSelectionChange={handleSelectionChange}
        createButtonText="Add Asset"
        ariaLabel="Assets data table"
        extraActions={extraActions}
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
        <FormSection title="Basic Information" description="Asset code, name, and category">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input label="Asset Code" value={formData.assetCode || ""} onChange={(e) => setFormField('assetCode')(e.target.value)} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Asset Name" value={formData.assetName || ""} onChange={(e) => setFormField('assetName')(e.target.value)} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Category" value={formData.categoryId || ""} onChange={(e) => setFormField('categoryId')(e.target.value)} options={categoryOptions} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Supplier" value={formData.supplierId || ""} onChange={(e) => setFormField('supplierId')(e.target.value)} options={supplierOptions} />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Operational Office" description="Configure operational office settings">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Operasional Office" 
                value={formData.operasionalOffice ? "true" : "false"} 
                onChange={(e) => {
                  const val = e.target.value === "true";
                  setFormField('operasionalOffice')(val);
                  // If set to No, clear officeId
                  if (!val) {
                    setFormField('officeId')("");
                  }
                }} 
                options={booleanOptions} 
              />
            </Grid>
            {showOfficeField && (
              <Grid item xs={12} sm={6}>
                <Select 
                  label="Office" 
                  value={formData.officeId || ""} 
                  onChange={(e) => setFormField('officeId')(e.target.value)} 
                  options={officeOptions} 
                  required 
                />
              </Grid>
            )}
          </Grid>
        </FormSection>

        <FormSection title="Technical Details" description="Brand, model, serial numbers, and network information">
          <Grid container spacing={2}>
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
          </Grid>
        </FormSection>

        <FormSection title="Purchase Information" description="Purchase date, price, warranty, and condition">
          <Grid container spacing={2}>
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
          </Grid>
        </FormSection>

        <FormSection title="Location & Depreciation" description="Office assignment and financial depreciation">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={4}>
              <Select 
                label="Office" 
                value={formData.officeId || ""} 
                onChange={(e) => setFormField('officeId')(e.target.value)} 
                options={officeOptions} 
                disabled={!showOfficeField}
              />
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
          </Grid>
        </FormSection>

        <FormSection title="Notes & Attachments" description="Additional information and file attachments">
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input label="Notes" value={formData.notes || ""} onChange={(e) => setFormField('notes')(e.target.value)} multiline rows={3} />
            </Grid>
            <Grid item xs={12}>
              <FileUploader 
                referenceTable="Asset"
                referenceId={editingAsset?.assetId}
                onUploadComplete={reload}
              />
            </Grid>
          </Grid>
        </FormSection>
      </CrudModal>

      <ImportModal
        isOpen={showImportModal}
        onClose={() => { setShowImportModal(false); setImportResult(null); }}
        onImport={handleImport}
        onDownloadTemplate={handleDownloadTemplate}
        isImporting={isImporting}
        importResult={importResult}
        title="Import Assets"
        description="Upload Excel file with asset data. Assets will be imported as ACTIVE."
      />

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={getSelectedIds(assets)}
        itemName="assets"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default AssetsMenu;