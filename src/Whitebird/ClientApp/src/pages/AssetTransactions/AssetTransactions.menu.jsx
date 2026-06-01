import React, { useState, useMemo, useCallback, useEffect, useRef } from "react";
import { Grid, Box, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import AssetTransactionsData from "./AssetTransactions.data";
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
import Modal from "../../components/molecules/Modal/Modal";
import { FiEdit2, FiCheck, FiX, FiRotateCcw, FiUpload, FiCheckSquare } from "react-icons/fi";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { TRANSACTION_TYPE_OPTIONS, TRANSACTION_TYPE_FILTER_OPTIONS, TRANSACTION_TYPES_REQUIRING_PAIR, TRANSACTION_TYPES_REQUIRING_TO_EMPLOYEE, TRANSACTION_TYPES_REQUIRING_FROM_EMPLOYEE, TRANSACTION_TYPES, getTransactionTypeName } from "../../core/constants/transactionTypes";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudFormBase } from "../../hooks/useCrudFormBase";
import { cleanTransactionFormData } from "../../core/utils/formHelpers";
import utilsHelper from "../../core/utils/utils.helper";
import "./AssetTransactions.scss";

const transactionsData = new AssetTransactionsData();
transactionsData.transformFormData = cleanTransactionFormData;

const TRANSACTION_TABS = [
  { id: "pending", label: "Pending" },
  { id: "approved", label: "Approved" },
  { id: "rejected", label: "Rejected" },
  { id: "active-loans", label: "Active Loans" },
  { id: "overdue-loans", label: "Overdue Loans" },
];

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
  transactionDate: new Date().toISOString(),
};

const TABS_WITH_CHECKBOX = ["pending", "approved"];

const AssetTransactionsMenu = () => {
  // ========== ALL HOOKS MUST BE CALLED BEFORE ANY CONDITIONAL RETURN ==========
  const [activeTab, setActiveTab] = useState("pending");
  const [showFilters, setShowFilters] = useState(false);
  const [approvalFilter, setApprovalFilter] = useState("");
  const [typeFilter, setTypeFilter] = useState("");
  const [selectedRows, setSelectedRows] = useState([]);
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
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);
  const [searchTerm, setSearchTerm] = useState("");
  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();

  const { employees, offices, assets: refAssets, assetConditions } = useReferenceData();

  const {
    showModal,
    editingRecord: editingTransaction,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, transactionsData, {
    idField: 'assetTransactionId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

  const showCheckbox = TABS_WITH_CHECKBOX.includes(activeTab);

  const getBulkButtonText = () => {
    if (activeTab === "pending") return "Approve Selected";
    if (activeTab === "approved") return "Reject Selected";
    return "";
  };

  const getBulkAction = () => {
    if (activeTab === "pending") return "approve";
    if (activeTab === "approved") return "reject";
    return "";
  };

  // Build filters based on activeTab
  const buildFilters = useCallback(() => {
    const filters = {};
    
    // For active-loans and overdue-loans tabs, we use different endpoints
    if (activeTab === 'active-loans' || activeTab === 'overdue-loans') {
      return filters;
    }
    
    if (activeTab === 'pending') {
      filters.approved = null;
    } else if (activeTab === 'approved') {
      filters.approved = true;
    } else if (activeTab === 'rejected') {
      filters.approved = false;
    }
    
    if (approvalFilter === 'pending') {
      filters.approved = null;
    } else if (approvalFilter === 'approved') {
      filters.approved = true;
    } else if (approvalFilter === 'rejected') {
      filters.approved = false;
    }
    
    if (typeFilter) {
      filters.transactionType = typeFilter;
    }
    
    if (searchTerm) {
      filters.search = searchTerm;
    }
    
    return filters;
  }, [activeTab, approvalFilter, typeFilter, searchTerm]);

  const fetchGridData = useCallback(async (params) => {
    const filters = buildFilters();
    
    // Handle special tabs (active-loans, overdue-loans)
    if (activeTab === 'active-loans') {
      try {
        const result = await transactionsData.api.getActiveLoans();
        if (result?.data && isMountedRef.current) {
          const loans = result.data?.data || result.data || [];
          return { success: true, data: { data: loans, totalCount: loans.length } };
        }
      } catch { }
      return { success: true, data: { data: [], totalCount: 0 } };
    } else if (activeTab === 'overdue-loans') {
      try {
        const result = await transactionsData.api.getOverdueLoans();
        if (result?.data && isMountedRef.current) {
          const loans = result.data?.data || result.data || [];
          return { success: true, data: { data: loans, totalCount: loans.length } };
        }
      } catch { }
      return { success: true, data: { data: [], totalCount: 0 } };
    }
    
    const requestParams = {
      page: params.page || 1,
      pageSize: params.pageSize || 10,
      ...filters,
      ...params,
    };
    
    const result = await transactionsData.fetchGridData(requestParams);
    return result;
  }, [activeTab, buildFilters]);

  const {
    data: transactions,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    reload
  } = useGridData(['transactions', activeTab, approvalFilter, typeFilter, searchTerm], fetchGridData);

  // Force reload when tab changes
  useEffect(() => {
    isMountedRef.current = true;
    setPage(1);
    setSelectedRows([]);
    reload();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, setPage]);

  // Load paired transaction options when transaction type requires pairing
  useEffect(() => {
    if (formData.transactionType && TRANSACTION_TYPES_REQUIRING_PAIR.includes(parseInt(formData.transactionType)) && formData.assetId) {
      setLoadingPairedOptions(true);
      (async () => {
        try {
          const result = await transactionsData.api.getByAssetId(formData.assetId);
          const allTxns = result?.data?.data || result?.data || [];
          const transactionTypeNum = parseInt(formData.transactionType);
          const pairSourceType = transactionTypeNum === TRANSACTION_TYPES.LOAN_RETURN ? TRANSACTION_TYPES.LOAN : TRANSACTION_TYPES.MAINTENANCE;
          const validPairs = allTxns.filter(t => 
            t.transactionType === pairSourceType && 
            t.approved === true && 
            !t.fromAssetTransactionId
          );
          if (isMountedRef.current) {
            setPairedTransactionOptions(validPairs.map(t => ({ 
              value: t.assetTransactionId, 
              label: `${getTransactionTypeName(t.transactionType)} - ${utilsHelper.formatDate(t.transactionDate)} (${t.assetCode || 'N/A'})` 
            })));
          }
        } catch { 
          if (isMountedRef.current) setPairedTransactionOptions([]); 
        }
        if (isMountedRef.current) setLoadingPairedOptions(false);
      })();
    } else {
      setPairedTransactionOptions([]);
      if (!TRANSACTION_TYPES_REQUIRING_PAIR.includes(parseInt(formData.transactionType))) {
        setFormField('fromAssetTransactionId')("");
      }
    }
  }, [formData.transactionType, formData.assetId, setFormField]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
  }, [setPage]);

  const handleApprove = useCallback(async (tx) => { 
    const r = await transactionsData.approve(tx.assetTransactionId, true); 
    if (r.success && isMountedRef.current) reload(); 
  }, [reload]);

  const handleReject = useCallback(async (tx) => { 
    const r = await transactionsData.approve(tx.assetTransactionId, false); 
    if (r.success && isMountedRef.current) reload(); 
  }, [reload]);

  const handleBulkAction = useCallback(async (ids, action) => {
    let successCount = 0;
    for (const id of ids) {
      let result;
      if (action === "approve") {
        result = await transactionsData.approve(id, true);
      } else {
        result = await transactionsData.approve(id, false);
      }
      if (result.success) successCount++;
    }
    if (successCount > 0 && isMountedRef.current) {
      setSelectedRows([]);
      reload();
    }
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
    if (r.success && isMountedRef.current) { 
      setShowReturnModal(false); 
      reload(); 
    } 
  }, [selectedTransaction, isReturnSubmitting, returnData, reload]);

  const handleCancel = useCallback(async (tx) => { 
    const r = await transactionsData.cancel(tx.assetTransactionId); 
    if (r.success && isMountedRef.current) reload(); 
  }, [reload]);

  const onSubmit = useCallback(async () => { 
    const success = await crudHandleSubmit(); 
    if (success && isMountedRef.current) reload(); 
    return success;
  }, [crudHandleSubmit, reload]);

  const handleTabChange = useCallback((tab) => { 
    setActiveTab(tab); 
    setPage(1);
    setApprovalFilter("");
    setTypeFilter("");
    setSearchTerm("");
    setSelectedRows([]);
    setShowFilters(false);
  }, [setPage]);

  const handleImport = useCallback(async (file) => {
    setIsImporting(true);
    const r = await transactionsData.importTransactions(file);
    setIsImporting(false);
    if (r.success && isMountedRef.current) {
      setImportResult(r.data);
      reload();
    }
  }, [reload]);

  const handleDownloadTemplate = useCallback(async () => {
    await transactionsData.downloadTemplate();
  }, []);

  const handleResetFilters = useCallback(() => {
    setApprovalFilter("");
    setTypeFilter("");
    setSearchTerm("");
    setPage(1);
  }, [setPage]);

  const getDisplayStatus = (row) => {
    if (row.approved === true) return 'Approved';
    if (row.approved === false) return 'Rejected';
    return 'Pending';
  };

  const showFromEmployee = TRANSACTION_TYPES_REQUIRING_FROM_EMPLOYEE.includes(parseInt(formData.transactionType));
  const showToEmployee = TRANSACTION_TYPES_REQUIRING_TO_EMPLOYEE.includes(parseInt(formData.transactionType));
  const showPairedTransaction = TRANSACTION_TYPES_REQUIRING_PAIR.includes(parseInt(formData.transactionType));
  const showMaintenanceFields = parseInt(formData.transactionType) === TRANSACTION_TYPES.MAINTENANCE || parseInt(formData.transactionType) === TRANSACTION_TYPES.POST_MAINTENANCE;

  const conditionOptions = useMemo(() => [
    { value: "", label: "Select Condition" },
    ...(assetConditions || []).map(c => ({ value: c.value, label: c.label }))
  ], [assetConditions]);

  const columns = useMemo(() => [
    { field: "assetCode", headerName: "Asset Code", width: 120 },
    { field: "assetName", headerName: "Asset Name", flex: 1, minWidth: 160 },
    { field: "transactionTypeName", headerName: "Type", width: 150 },
    { field: "fromEmployeeName", headerName: "From", width: 150 },
    { field: "toEmployeeName", headerName: "To", width: 150 },
    { 
      field: "transactionDate", 
      headerName: "Date", 
      width: 180, 
      valueFormatter: (p) => {
        if (!p || !p.value) return '-';
        return utilsHelper.formatDateTime(p.value);
      }
    },
    { 
      field: "approved", 
      headerName: "Status", 
      width: 120, 
      renderCell: (p) => {
        const status = getDisplayStatus(p?.row);
        return <Chip label={status} size="small" sx={getStatusChipStyles(status)} />;
      }
    },
    { 
      field: "expectedReturnDate", 
      headerName: "Exp. Return", 
      width: 130, 
      valueFormatter: (p) => {
        if (!p || !p.value) return '-';
        return utilsHelper.formatDate(p.value);
      }
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
            {isApproved && (
              <>
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

  const filterContent = (
    <>
      <Select 
        label="Approval Status" 
        value={approvalFilter} 
        onChange={(e) => { setApprovalFilter(e.target.value); setPage(1); }} 
        options={[{ value: "", label: "All" }, { value: "pending", label: "Pending" }, { value: "approved", label: "Approved" }, { value: "rejected", label: "Rejected" }]} 
      />
      <Select 
        label="Type" 
        value={typeFilter} 
        onChange={(e) => { setTypeFilter(e.target.value); setPage(1); }} 
        options={TRANSACTION_TYPE_FILTER_OPTIONS} 
      />
    </>
  );

  const extraActions = (
    <Button variant="outline" onClick={() => setShowImportModal(true)} startIcon={<FiUpload />}>
      Import
    </Button>
  );

  const assetOptions = useMemo(() => [
    { value: "", label: "Select Asset" },
    ...refAssets.map(a => ({ value: a.value, label: a.label }))
  ], [refAssets]);

  const employeeOptions = useMemo(() => [
    { value: "", label: "Select Employee" },
    ...employees.map(e => ({ value: e.value, label: e.label }))
  ], [employees]);

  const officeOptionsSelect = useMemo(() => [
    { value: "", label: "None" },
    ...offices.map(o => ({ value: o.value, label: o.label }))
  ], [offices]);

  // ========== CONDITIONAL RETURN ==========
  if (loading && !transactions.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkButtonText = getBulkButtonText();
  const bulkAction = getBulkAction();

  // Return modal content
  const returnModalContent = (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Box sx={{ p: 2, bgcolor: 'var(--surface)', borderRadius: 2 }}>
        <strong style={{ color: 'var(--text-primary)' }}>
          {selectedTransaction?.assetCode} - {selectedTransaction?.assetName}
        </strong>
        <p style={{ margin: '4px 0 0', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
          Transaction ID: {selectedTransaction?.assetTransactionId}
        </p>
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
  );

  return (
    <div className="transactions-menu" style={{ background: 'transparent' }}>
      <GridView
        title="Asset Transactions"
        tabs={TRANSACTION_TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={transactions}
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
        createButtonText="New Transaction"
        ariaLabel="Transactions data table"
        extraActions={extraActions}
        filterChildren={filterContent}
        showFilters={showFilters}
        onFilterToggle={() => setShowFilters(!showFilters)}
        onResetFilters={handleResetFilters}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingTransaction ? "Edit Transaction" : "New Transaction"}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingTransaction ? "Update" : "Create"}
        size="lg"
      >
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Select 
              label="Transaction Type" 
              value={formData.transactionType || ""} 
              onChange={(e) => setFormField('transactionType')(e.target.value)} 
              options={TRANSACTION_TYPE_OPTIONS} 
              required 
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select 
              label="Asset" 
              value={formData.assetId || ""} 
              onChange={(e) => setFormField('assetId')(e.target.value)} 
              options={assetOptions} 
              required 
            />
          </Grid>
          {showFromEmployee && (
            <Grid item xs={12} sm={6}>
              <Select 
                label="From Employee" 
                value={formData.fromEmployeeId || ""} 
                onChange={(e) => setFormField('fromEmployeeId')(e.target.value)} 
                options={employeeOptions} 
              />
            </Grid>
          )}
          {showToEmployee && (
            <Grid item xs={12} sm={6}>
              <Select 
                label="To Employee" 
                value={formData.toEmployeeId || ""} 
                onChange={(e) => setFormField('toEmployeeId')(e.target.value)} 
                options={employeeOptions} 
                required 
              />
            </Grid>
          )}
          <Grid item xs={12} sm={6}>
            <Select 
              label="To Office" 
              value={formData.toLocationId || ""} 
              onChange={(e) => setFormField('toLocationId')(e.target.value)} 
              options={officeOptionsSelect} 
            />
          </Grid>
          {showPairedTransaction && (
            <Grid item xs={12} sm={6}>
              <Select 
                label="Paired Transaction" 
                value={formData.fromAssetTransactionId || ""} 
                onChange={(e) => setFormField('fromAssetTransactionId')(e.target.value)} 
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
              onChange={(e) => setFormField('expectedReturnDate')(e.target.value)} 
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <Select 
              label="Condition Before" 
              value={formData.conditionBefore || ""} 
              onChange={(e) => setFormField('conditionBefore')(e.target.value)} 
              options={conditionOptions} 
            />
          </Grid>
          {showMaintenanceFields && (
            <>
              <Grid item xs={12} sm={6}>
                <Select 
                  label="Maintenance Type" 
                  value={formData.maintenanceType || ""} 
                  onChange={(e) => setFormField('maintenanceType')(e.target.value)} 
                  options={[{ value: "", label: "Select Type" }]} 
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <NumberInput 
                  label="Maintenance Cost" 
                  value={formData.maintenanceCost} 
                  onChange={(e) => setFormField('maintenanceCost')(e.target.value)} 
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
              onChange={(e) => setFormField('notes')(e.target.value)} 
              multiline 
              rows={2} 
            />
          </Grid>
          <Grid item xs={12}>
            <FileUploader 
              referenceTable="AssetTransaction"
              referenceId={editingTransaction?.assetTransactionId}
              onUploadComplete={reload}
            />
          </Grid>
        </Grid>
      </CrudModal>

      {/* Return Modal */}
      <Modal isOpen={showReturnModal} onClose={() => !isReturnSubmitting && setShowReturnModal(false)} title="Return Asset" size="md">
        {returnModalContent}
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

      {/* Bulk Action Modal */}
      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkAction(ids, bulkAction)}
        selectedIds={selectedRows}
        itemName="transactions"
        title={bulkButtonText === "Approve Selected" ? "Approve Transactions" : "Reject Transactions"}
        description={`This action will ${bulkAction === "approve" ? "approve" : "reject"} the selected transactions.`}
      />
    </div>
  );
};

export default AssetTransactionsMenu;