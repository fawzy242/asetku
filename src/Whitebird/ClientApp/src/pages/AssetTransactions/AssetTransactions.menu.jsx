import React, { useState, useMemo, useCallback, useEffect } from "react";
import { FiEdit2, FiCheck, FiX, FiRotateCcw, FiPlus, FiUpload } from "react-icons/fi";
import { Grid, Box, Chip } from "@mui/material";
import AssetTransactionsData from "./AssetTransactions.data";
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
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudForm } from "../../hooks/useCrudForm";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { TRANSACTION_TYPE_OPTIONS, TRANSACTION_TYPE_FILTER_OPTIONS, TRANSACTION_TYPES_REQUIRING_PAIR, TRANSACTION_TYPES_REQUIRING_TO_EMPLOYEE, TRANSACTION_TYPES_REQUIRING_FROM_EMPLOYEE, TRANSACTION_TYPES, getTransactionTypeName } from "../../core/constants/transactionTypes";
import { CONDITION_OPTIONS } from "../../core/constants/assetStatuses";
import { getTransactionTypeCode } from "../../core/utils/mappingHelpers";
import utilsHelper from "../../core/utils/utils.helper";
import "./AssetTransactions.scss";

const transactionsData = new AssetTransactionsData();

// UPDATED: Removed obsolete fields (fromLocationId, vendorName)
// Updated: fromAssetTransactionId instead of pairedTransactionId
const INITIAL_FORM_DATA = {
  assetId: "",
  transactionType: "",
  fromEmployeeId: "",
  toEmployeeId: "",
  toLocationId: "",
  expectedReturnDate: "",
  notes: "",
  conditionBefore: "",
  maintenanceType: "",
  maintenanceCost: "",
  fromAssetTransactionId: "",
};

// TABS for transactions (based on approval status and special views)
const TRANSACTION_TABS = [
  { id: "all", label: "All" },
  { id: "pending", label: "Pending" },
  { id: "approved", label: "Approved" },
  { id: "rejected", label: "Rejected" },
  { id: "active-loans", label: "Active Loans" },
  { id: "overdue-loans", label: "Overdue Loans" },
];

const NULLABLE_STRING_FIELDS = ['expectedReturnDate', 'notes', 'conditionBefore', 'maintenanceType'];
const NULLABLE_INT_FIELDS = ['fromEmployeeId', 'toEmployeeId', 'toLocationId', 'fromAssetTransactionId'];

const transformTransactionFormData = (data) => {
  const result = { ...data };
  NULLABLE_STRING_FIELDS.forEach(f => {
    if (result[f] === '' || result[f] === undefined) result[f] = null;
  });
  NULLABLE_INT_FIELDS.forEach(f => {
    if (result[f] === '' || result[f] === null || result[f] === undefined) result[f] = null;
    else if (typeof result[f] === 'string') result[f] = parseInt(result[f], 10);
  });
  
  if (result.transactionType && typeof result.transactionType === 'string') {
    result.transactionType = getTransactionTypeCode(result.transactionType);
  }
  if (result.assetId === '' || result.assetId === null || result.assetId === undefined) result.assetId = 0;
  else if (typeof result.assetId === 'string') result.assetId = parseInt(result.assetId, 10);
  
  if (result.maintenanceCost === '' || result.maintenanceCost === null || result.maintenanceCost === undefined) result.maintenanceCost = null;
  else if (typeof result.maintenanceCost === 'string') result.maintenanceCost = parseFloat(result.maintenanceCost);
  
  if (!result.transactionDate) result.transactionDate = new Date().toISOString();
  
  // Remove obsolete fields
  delete result.vendorName;
  delete result.fromLocationId;
  
  return result;
};

transactionsData.transformFormData = transformTransactionFormData;

const CRUD_OPTIONS = { idField: 'assetTransactionId' };

const AssetTransactionsMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showFilters, setShowFilters] = useState(false);
  const [approvalFilter, setApprovalFilter] = useState("");
  const [typeFilter, setTypeFilter] = useState("");
  const [showReturnModal, setShowReturnModal] = useState(false);
  const [selectedTransaction, setSelectedTransaction] = useState(null);
  const [isReturnSubmitting, setIsReturnSubmitting] = useState(false);
  const [returnData, setReturnData] = useState({ 
    actualReturnDate: new Date().toISOString().split("T")[0], 
    conditionAfter: "", 
    notes: "" 
  });
  const [pairedTransactionOptions, setPairedTransactionOptions] = useState([]);
  const [loadingPairedOptions, setLoadingPairedOptions] = useState(false);
  const [showImportModal, setShowImportModal] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);

  const { employees, offices, assets: refAssets, transactionTypes, assetConditions } = useReferenceData();

  const { 
    showModal, editingRecord: editingTransaction, isSubmitting, 
    formData, setFormData, handleCreate, handleEdit, handleClose, 
    handleSubmit: crudHandleSubmit 
  } = useCrudForm(INITIAL_FORM_DATA, transactionsData, CRUD_OPTIONS);

  const fetchGridData = useCallback(async (params) => {
    const filters = { ...params };
    
    // Handle tab-based filtering
    if (activeTab === 'pending') {
      return transactionsData.fetchGridData({ ...filters, approved: null });
    } else if (activeTab === 'approved') {
      return transactionsData.fetchGridData({ ...filters, approved: true });
    } else if (activeTab === 'rejected') {
      return transactionsData.fetchGridData({ ...filters, approved: false });
    } else if (activeTab === 'active-loans') {
      try {
        const result = await transactionsData.api.getActiveLoans();
        if (result?.data) {
          const loans = result.data?.data || result.data || [];
          return { success: true, data: { data: loans, totalCount: loans.length } };
        }
      } catch { /* fallback to grid */ }
    } else if (activeTab === 'overdue-loans') {
      try {
        const result = await transactionsData.api.getOverdueLoans();
        if (result?.data) {
          const loans = result.data?.data || result.data || [];
          return { success: true, data: { data: loans, totalCount: loans.length } };
        }
      } catch { /* fallback to grid */ }
    }
    
    if (approvalFilter === 'pending') filters.approved = null;
    else if (approvalFilter === 'approved') filters.approved = true;
    else if (approvalFilter === 'rejected') filters.approved = false;
    
    if (typeFilter) filters.transactionType = typeFilter;
    
    return transactionsData.fetchGridData(filters);
  }, [activeTab, approvalFilter, typeFilter]);

  const { 
    data: transactions, totalCount, loading, page, setPage, 
    pageSize, setPageSize, updateFilters, reload 
  } = useGridData(['transactions', activeTab, approvalFilter, typeFilter], fetchGridData);

  // Load paired transaction options for pairing transactions
  useEffect(() => {
    if (formData.transactionType && TRANSACTION_TYPES_REQUIRING_PAIR.includes(formData.transactionType) && formData.assetId) {
      setLoadingPairedOptions(true);
      (async () => {
        try {
          const result = await transactionsData.api.getByAssetId(formData.assetId);
          const allTxns = result?.data?.data || result?.data || [];
          const pairSourceType = formData.transactionType === TRANSACTION_TYPES.LOAN_RETURN ? TRANSACTION_TYPES.LOAN : TRANSACTION_TYPES.MAINTENANCE;
          const validPairs = allTxns.filter(t => 
            t.transactionType === pairSourceType && 
            t.approved === true && 
            !t.fromAssetTransactionId
          );
          setPairedTransactionOptions(validPairs.map(t => ({ 
            value: t.assetTransactionId, 
            label: `${getTransactionTypeName(t.transactionType)} - ${utilsHelper.formatDate(t.transactionDate)} (${t.assetCode || 'N/A'})` 
          })));
        } catch { 
          setPairedTransactionOptions([]); 
        }
        setLoadingPairedOptions(false);
      })();
    } else {
      setPairedTransactionOptions([]);
      if (!TRANSACTION_TYPES_REQUIRING_PAIR.includes(formData.transactionType)) {
        setFormData(prev => ({ ...prev, fromAssetTransactionId: "" }));
      }
    }
  }, [formData.transactionType, formData.assetId]);

  const handleSearch = useCallback((search) => updateFilters({ search }), [updateFilters]);
  
  const handleApprove = useCallback(async (tx) => { 
    const r = await transactionsData.approve(tx.assetTransactionId, true); 
    if (r.success) reload(); 
  }, [reload]);
  
  const handleReject = useCallback(async (tx) => { 
    const r = await transactionsData.approve(tx.assetTransactionId, false); 
    if (r.success) reload(); 
  }, [reload]);
  
  const handleReturn = useCallback((tx) => { 
    setSelectedTransaction(tx); 
    setReturnData({ 
      actualReturnDate: new Date().toISOString().split("T")[0], 
      conditionAfter: "", 
      notes: "" 
    }); 
    setShowReturnModal(true); 
  }, []);
  
  const handleReturnSubmit = useCallback(async () => { 
    if (!selectedTransaction || isReturnSubmitting) return; 
    setIsReturnSubmitting(true); 
    const r = await transactionsData.returnAsset(
      selectedTransaction.assetTransactionId, 
      returnData.actualReturnDate, 
      returnData.conditionAfter, 
      returnData.notes
    ); 
    setIsReturnSubmitting(false); 
    if (r.success) { 
      setShowReturnModal(false); 
      reload(); 
    } 
  }, [selectedTransaction, isReturnSubmitting, returnData, reload]);
  
  const handleCancel = useCallback(async (tx) => { 
    const r = await transactionsData.cancel(tx.assetTransactionId); 
    if (r.success) reload(); 
  }, [reload]);
  
  const onSubmit = useCallback(async (e) => { 
    e.preventDefault(); 
    const success = await crudHandleSubmit(); 
    if (success) reload(); 
  }, [crudHandleSubmit, reload]);
  
  const handleImport = useCallback(async (file) => {
    setIsImporting(true);
    const r = await transactionsData.importTransactions(file);
    setIsImporting(false);
    if (r.success) {
      setImportResult(r.data);
      reload();
    }
  }, [reload]);
  
  const handleDownloadTemplate = useCallback(async () => {
    await transactionsData.downloadTemplate();
  }, []);

  // Helper to get display status from approved flag
  const getDisplayStatus = (row) => {
    if (row.approved === true) return 'Approved';
    if (row.approved === false) return 'Rejected';
    return 'Pending';
  };

  const columns = useMemo(() => [
    { field: "assetCode", headerName: "Asset Code", width: 120 },
    { field: "assetName", headerName: "Asset Name", flex: 1, minWidth: 160 },
    { field: "transactionTypeName", headerName: "Type", width: 150 },
    { field: "fromEmployeeName", headerName: "From", width: 150 },
    { field: "toEmployeeName", headerName: "To", width: 150 },
    { field: "transactionDate", headerName: "Date", width: 160, valueFormatter: (p) => p?.value ? utilsHelper.formatDateTime(p.value) : '-' },
    { 
      field: "approved", 
      headerName: "Status", 
      width: 120, 
      renderCell: (p) => {
        const status = getDisplayStatus(p?.row);
        return <Chip label={status} size="small" sx={getStatusChipStyles(status)} />;
      }
    },
    { field: "expectedReturnDate", headerName: "Exp. Return", width: 130, valueFormatter: (p) => p?.value ? utilsHelper.formatDate(p.value) : "-" },
    { 
      field: "isOverdue", 
      headerName: "Overdue", 
      width: 90, 
      renderCell: (p) => p?.value ? <Chip label="Overdue" size="small" sx={{ bgcolor: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', fontWeight: 500, fontSize: '0.75rem', height: 24, borderRadius: '4px' }} /> : null 
    },
    { 
      field: "actions", 
      headerName: "Actions", 
      width: 210, 
      sortable: false, 
      renderCell: (p) => {
        const row = p?.row;
        const isPending = row.approved === null;
        const isApproved = row.approved === true;
        const isReturnable = isApproved && !row.actualReturnDate && !row.fromAssetTransactionId && 
                             (row.transactionType === TRANSACTION_TYPES.LOAN || 
                              row.transactionType === TRANSACTION_TYPES.HANDOVER ||
                              row.transactionType === TRANSACTION_TYPES.TRANSFER);
        
        return (
          <div className="table-actions">
            {isPending && (
              <>
                <IconButton onClick={() => handleApprove(row)} title="Approve" variant="success" size="lg"><FiCheck size={18} /></IconButton>
                <IconButton onClick={() => handleReject(row)} title="Reject" variant="danger" size="lg"><FiX size={18} /></IconButton>
                <IconButton onClick={() => handleEdit(row)} title="Edit" size="lg"><FiEdit2 size={18} /></IconButton>
                <IconButton onClick={() => handleCancel(row)} title="Cancel" variant="danger" size="lg"><FiX size={18} /></IconButton>
              </>
            )}
            {isReturnable && (
              <Button variant="primary" size="sm" onClick={() => handleReturn(row)} startIcon={<FiRotateCcw />}>Return</Button>
            )}
          </div>
        );
      }
    },
  ], [handleEdit, handleApprove, handleReject, handleCancel, handleReturn]);

  const handleTabChange = useCallback((tab) => { 
    setActiveTab(tab); 
    setPage(1); 
  }, [setPage]);
  
  const showFromEmployee = TRANSACTION_TYPES_REQUIRING_FROM_EMPLOYEE.includes(formData.transactionType);
  const showToEmployee = TRANSACTION_TYPES_REQUIRING_TO_EMPLOYEE.includes(formData.transactionType);
  const showPairedTransaction = TRANSACTION_TYPES_REQUIRING_PAIR.includes(formData.transactionType);
  const showMaintenanceFields = formData.transactionType === TRANSACTION_TYPES.MAINTENANCE || formData.transactionType === TRANSACTION_TYPES.POST_MAINTENANCE;
  
  // Condition options from MasterData
  const conditionOptions = useMemo(() => [
    { value: "", label: "Select Condition" },
    ...(assetConditions || []).map(c => ({ value: c.value, label: c.label }))
  ], [assetConditions]);

  if (loading && !transactions.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="transactions-menu">
      <div className="page-header">
        <h1 className="page-title">Asset Transactions</h1>
        <div style={{ display: 'flex', gap: '12px' }}>
          <Button variant="outline" onClick={() => setShowImportModal(true)} startIcon={<FiUpload />}>
            Import
          </Button>
          <PageHeader title="Asset Transactions" buttonText="New Transaction" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
        </div>
      </div>
      
      <Tabs tabs={TRANSACTION_TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} onFilterToggle={() => setShowFilters(!showFilters)} showFilters={showFilters} placeholder="Search by asset, employee..." />
      <FilterPanel visible={showFilters}>
        <Select label="Approval Status" value={approvalFilter} onChange={(e) => { setApprovalFilter(e.target.value); setPage(1); }} options={[{ value: "", label: "All" }, { value: "pending", label: "Pending" }, { value: "approved", label: "Approved" }, { value: "rejected", label: "Rejected" }]} />
        <Select label="Type" value={typeFilter} onChange={(e) => { setTypeFilter(e.target.value); setPage(1); }} options={TRANSACTION_TYPE_FILTER_OPTIONS} />
      </FilterPanel>
      
      <div className="transactions-menu__table">
        <DataTable 
          rows={transactions} 
          columns={columns} 
          loading={loading} 
          pageSize={pageSize} 
          getRowId={(row) => row.assetTransactionId} 
          hideFooter={true} 
          ariaLabel="Transactions data table" 
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
      <Modal isOpen={showModal} onClose={handleClose} title={editingTransaction ? "Edit Transaction" : "New Transaction"} size="lg">
        <form onSubmit={onSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Transaction Type" 
                value={formData.transactionType || ""} 
                onChange={e => setFormData({ ...formData, transactionType: e.target.value })} 
                options={TRANSACTION_TYPE_OPTIONS} 
                required 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Asset" 
                value={formData.assetId || ""} 
                onChange={e => setFormData({ ...formData, assetId: e.target.value })} 
                options={[{ value: "", label: "Select Asset" }, ...refAssets.map(a => ({ value: a.value, label: a.label }))]} 
                required 
              />
            </Grid>
            {showFromEmployee && (
              <Grid item xs={12} sm={6}>
                <Select 
                  label="From Employee" 
                  value={formData.fromEmployeeId || ""} 
                  onChange={e => setFormData({ ...formData, fromEmployeeId: e.target.value })} 
                  options={[{ value: "", label: "Select Employee" }, ...employees.map(e => ({ value: e.value, label: e.label }))]} 
                />
              </Grid>
            )}
            {showToEmployee && (
              <Grid item xs={12} sm={6}>
                <Select 
                  label="To Employee" 
                  value={formData.toEmployeeId || ""} 
                  onChange={e => setFormData({ ...formData, toEmployeeId: e.target.value })} 
                  options={[{ value: "", label: "Select Employee" }, ...employees.map(e => ({ value: e.value, label: e.label }))]} 
                  required 
                />
              </Grid>
            )}
            <Grid item xs={12} sm={6}>
              <Select 
                label="To Office" 
                value={formData.toLocationId || ""} 
                onChange={e => setFormData({ ...formData, toLocationId: e.target.value })} 
                options={[{ value: "", label: "None" }, ...offices.map(o => ({ value: o.value, label: o.label }))]} 
              />
            </Grid>
            {showPairedTransaction && (
              <Grid item xs={12} sm={6}>
                <Select 
                  label="Paired Transaction" 
                  value={formData.fromAssetTransactionId || ""} 
                  onChange={e => setFormData({ ...formData, fromAssetTransactionId: e.target.value })} 
                  options={[{ value: "", label: loadingPairedOptions ? "Loading..." : "Select Paired Transaction" }, ...pairedTransactionOptions]} 
                  required 
                  disabled={loadingPairedOptions} 
                />
              </Grid>
            )}
            <Grid item xs={12} sm={6}>
              <DatePickerInput 
                label="Expected Return Date" 
                value={formData.expectedReturnDate || ""} 
                onChange={e => setFormData({ ...formData, expectedReturnDate: e.target.value })} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Condition Before" 
                value={formData.conditionBefore || ""} 
                onChange={e => setFormData({ ...formData, conditionBefore: e.target.value })} 
                options={conditionOptions} 
              />
            </Grid>
            {showMaintenanceFields && (
              <>
                <Grid item xs={12} sm={6}>
                  <Select 
                    label="Maintenance Type" 
                    value={formData.maintenanceType || ""} 
                    onChange={e => setFormData({ ...formData, maintenanceType: e.target.value })} 
                    options={[{ value: "", label: "Select Type" }]} 
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <NumberInput 
                    label="Maintenance Cost" 
                    value={formData.maintenanceCost} 
                    onChange={e => setFormData({ ...formData, maintenanceCost: e.target.value })} 
                    prefix="Rp " 
                    thousandSeparator={true} 
                    decimalScale={0} 
                  />
                </Grid>
              </>
            )}
            <Grid item xs={12}>
              <Input 
                label="Notes" 
                value={formData.notes || ""} 
                onChange={e => setFormData({ ...formData, notes: e.target.value })} 
                multiline 
                rows={2} 
              />
            </Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={handleClose} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>{editingTransaction ? "Update" : "Create"}</Button>
          </Box>
        </form>
      </Modal>

      {/* Return Modal */}
      <Modal isOpen={showReturnModal} onClose={() => !isReturnSubmitting && setShowReturnModal(false)} title="Return Asset" size="md">
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <Box sx={{ p: 2, bgcolor: 'var(--surface)', borderRadius: 2 }}>
            <strong>{selectedTransaction?.assetCode} - {selectedTransaction?.assetName}</strong>
            <p style={{ margin: '4px 0 0', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
              Type: {selectedTransaction?.transactionTypeName} | Holder: {selectedTransaction?.toEmployeeName || 'None'}
            </p>
          </Box>
          <DatePickerInput 
            label="Actual Return Date" 
            value={returnData.actualReturnDate} 
            onChange={e => setReturnData({ ...returnData, actualReturnDate: e.target.value })} 
            required 
          />
          <Select 
            label="Condition After" 
            value={returnData.conditionAfter} 
            onChange={e => setReturnData({ ...returnData, conditionAfter: e.target.value })} 
            options={conditionOptions} 
            required 
          />
          <Input 
            label="Notes" 
            value={returnData.notes || ""} 
            onChange={e => setReturnData({ ...returnData, notes: e.target.value })} 
            multiline 
            rows={2} 
          />
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 2 }}>
            <Button variant="outline" onClick={() => setShowReturnModal(false)} disabled={isReturnSubmitting}>Cancel</Button>
            <Button variant="primary" onClick={handleReturnSubmit} loading={isReturnSubmitting}>Confirm Return</Button>
          </Box>
        </Box>
      </Modal>

      {/* Import Modal */}
      <ImportModal
        isOpen={showImportModal}
        onClose={() => { setShowImportModal(false); setImportResult(null); }}
        onImport={handleImport}
        onDownloadTemplate={handleDownloadTemplate}
        isImporting={isImporting}
        importResult={importResult}
        title="Import Transactions"
        description="Upload Excel or TXT file with transaction data. Transactions will be imported as PENDING and need approval."
      />
    </div>
  );
};

export default AssetTransactionsMenu;