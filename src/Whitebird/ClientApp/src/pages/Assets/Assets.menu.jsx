import React, { useState, useMemo, useCallback } from "react";
import { FiPlus, FiUpload, FiCheckSquare } from "react-icons/fi";
import { Box } from "@mui/material";
import AssetsData from "./Assets.data";
import { getAssetColumns } from "./Assets.columns";
import { AssetForm } from "./Assets.form";
import { AssetFilters } from "./Assets.filters";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Modal from "../../components/molecules/Modal/Modal";
import ModalActions from "../../components/molecules/ModalActions/ModalActions";
import Spinner from "../../components/atoms/Spinner/Spinner";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import SearchToolbar from "../../components/molecules/SearchToolbar/SearchToolbar";
import Tabs from "../../components/molecules/Tabs/Tabs";
import FilterPanel from "../../components/molecules/FilterPanel/FilterPanel";
import ImportModal from "../../components/molecules/ImportModal/ImportModal";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudForm } from "../../hooks/useCrudForm";
import { cleanAssetFormData } from "../../core/utils/formHelpers";
import { ASSET_STATUS_TABS } from "../../core/constants/tabs";
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

const CRUD_OPTIONS = { idField: 'assetId' };

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

  const assets = useMemo(() => {
    return (rawAssets || []).map(asset => ({
      ...asset,
      displayStatus: asset.currentStatus || asset.status || 'Available',
      displayCondition: asset.assetConditionName || '-',
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

  const columns = useMemo(() => getAssetColumns(handleEdit, handleDelete), [handleEdit, handleDelete]);

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
          <Button variant="outline" onClick={() => setShowImportModal(true)} startIcon={<FiUpload />}>Import</Button>
          {selectedRows.length > 0 && (
            <Button variant="primary" onClick={() => setShowBulkActivateModal(true)} startIcon={<FiCheckSquare />}>
              Activate ({selectedRows.length})
            </Button>
          )}
          <PageHeader title="Asset Management" buttonText="Add Asset" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
        </div>
      </div>

      <Tabs tabs={ASSET_STATUS_TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} onFilterToggle={() => setShowFilters(!showFilters)} showFilters={showFilters} placeholder="Search by code, name, serial..." />
      <FilterPanel visible={showFilters}>
        <AssetFilters 
          statusFilter={statusFilter} setStatusFilter={setStatusFilter}
          categoryFilter={categoryFilter} setCategoryFilter={setCategoryFilter}
          categories={categories} setPage={setPage}
        />
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

      <Modal isOpen={showModal} onClose={handleClose} title={editingAsset ? "Edit Asset" : "Add New Asset"} size="lg">
        <form onSubmit={onSubmit}>
          <AssetForm 
            formData={formData} setFormData={setFormData}
            editingAsset={editingAsset}
            categories={categories} suppliers={suppliers}
            offices={offices} assetConditions={assetConditions}
            reload={reload}
          />
          <ModalActions 
            onCancel={handleClose} 
            isSubmitting={isSubmitting}
            submitText={editingAsset ? "Update" : "Create"}
          />
        </form>
      </Modal>

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