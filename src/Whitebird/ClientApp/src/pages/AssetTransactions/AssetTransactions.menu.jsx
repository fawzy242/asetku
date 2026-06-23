import React, { useState, useMemo, useCallback, useEffect, useRef } from "react";
import { Grid, Chip, Box, Typography, Divider } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import AssetTransactionsData from "./AssetTransactions.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
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
import Modal from "../../components/molecules/Modal/Modal";
import Button from "../../components/atoms/Button/Button";
import Card from "../../components/atoms/Card/Card";
import SearchToolbar from "../../components/molecules/SearchToolbar/SearchToolbar";
import Tabs from "../../components/molecules/Tabs/Tabs";
import { FiUpload, FiCheckSquare, FiPlus, FiCalendar } from "react-icons/fi";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { 
  TRANSACTION_TYPE_OPTIONS, 
  TRANSACTION_TYPES_REQUIRING_PAIR, 
  TRANSACTION_TYPES_REQUIRING_TO_EMPLOYEE, 
  TRANSACTION_TYPES_REQUIRING_FROM_EMPLOYEE, 
  TRANSACTION_TYPES, 
  getTransactionTypeName, 
  TRANSACTION_TYPES_RETURNABLE,
  isPrimaryTransactionType,
  isSecondaryTransactionType
} from "../../core/constants/transactionTypes";
import { ACTION_TYPES, useGridActions } from "../../hooks/useGridActions";
import { useBulkSelection } from "../../hooks/useBulkSelection";
import { useSweetAlert } from "../../hooks/useSweetAlert";
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
  const [activeTab, setActiveTab] = useState("pending");
  const [searchTerm, setSearchTerm] = useState("");
  const [dateFilterStart, setDateFilterStart] = useState("");
  const [dateFilterEnd, setDateFilterEnd] = useState("");
  const [showImportModal, setShowImportModal] = useState(false);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const [showReturnModal, setShowReturnModal] = useState(false);
  const [showPostMaintenanceModal, setShowPostMaintenanceModal] = useState(false);
  const [selectedTransaction, setSelectedTransaction] = useState(null);
  const [isReturnSubmitting, setIsReturnSubmitting] = useState(false);
  const [isPostMaintenanceSubmitting, setIsPostMaintenanceSubmitting] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);
  const [returnData, setReturnData] = useState({ 
    actualReturnDate: new Date().toISOString().split("T")[0], 
    conditionAfter: "", 
    notes: "" 
  });
  const [postMaintenanceData, setPostMaintenanceData] = useState({
    completionDate: new Date().toISOString().split("T")[0],
    conditionAfter: "",
    notes: ""
  });
  const [pairedTransactionOptions, setPairedTransactionOptions] = useState([]);
  const [loadingPairedOptions, setLoadingPairedOptions] = useState(false);
  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirm, confirmDelete } = useSweetAlert();

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

  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } = useBulkSelection({ idField: 'assetTransactionId' });

  const showCheckbox = TABS_WITH_CHECKBOX.includes(activeTab);

  const getBulkAction = () => {
    if (activeTab === "pending") return "approve";
    if (activeTab === "approved") return "reject";
    return "";
  };

  const buildFilters = useCallback(() => {
    const filters = {};
    
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
    
    if (searchTerm) {
      filters.search = searchTerm;
    }
    
    if (dateFilterStart) {
      filters.startDate = dateFilterStart;
    }
    if (dateFilterEnd) {
      filters.endDate = dateFilterEnd;
    }
    
    return filters;
  }, [activeTab, searchTerm, dateFilterStart, dateFilterEnd]);

  const fetchGridData = useCallback(async (params) => {
    const filters = buildFilters();
    
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
  } = useGridData(['transactions', activeTab, searchTerm, dateFilterStart, dateFilterEnd], fetchGridData);

  useEffect(() => {
    isMountedRef.current = true;
    setPage(1);
    clearSelection();
    reload();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, setPage, clearSelection]);

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
    clearSelection();
  }, [setPage, clearSelection]);

  const handleDateFilterChange = useCallback((start, end) => {
    setDateFilterStart(start);
    setDateFilterEnd(end);
    setPage(1);
    clearSelection();
  }, [setPage, clearSelection]);

  const handleApprove = useCallback(async (tx) => {
    const confirmed = await confirm({
      title: 'Approve Transaction',
      text: `Are you sure you want to approve transaction for ${tx.assetCode}?`,
      confirmButtonText: 'Yes, Approve',
    });
    if (!confirmed) return;
    const r = await transactionsData.approve(tx.assetTransactionId, true);
    if (r.success && isMountedRef.current) {
      toast.success('Transaction approved');
      reload();
      clearSelection();
    }
  }, [reload, toast, confirm, clearSelection]);

  const handleReject = useCallback(async (tx) => {
    const confirmed = await confirm({
      title: 'Reject Transaction',
      text: `Are you sure you want to reject transaction for ${tx.assetCode}?`,
      icon: 'warning',
      confirmButtonText: 'Yes, Reject',
      confirmButtonColor: '#ef4444',
    });
    if (!confirmed) return;
    const r = await transactionsData.approve(tx.assetTransactionId, false);
    if (r.success && isMountedRef.current) {
      toast.success('Transaction rejected');
      reload();
      clearSelection();
    }
  }, [reload, toast, confirm, clearSelection]);

  const handleCancel = useCallback(async (tx) => {
    const confirmed = await confirmDelete('Cancel Transaction', `Are you sure you want to cancel transaction for ${tx.assetCode}?`);
    if (!confirmed) return;
    const r = await transactionsData.cancel(tx.assetTransactionId);
    if (r.success && isMountedRef.current) {
      toast.success('Transaction cancelled');
      reload();
      clearSelection();
    }
  }, [reload, toast, confirmDelete, clearSelection]);

  const handleReturnShortcut = useCallback((tx) => { 
    setSelectedTransaction(tx); 
    setReturnData({ 
      actualReturnDate: new Date().toISOString().split("T")[0], 
      conditionAfter: "", 
      notes: "" 
    }); 
    setShowReturnModal(true); 
  }, []);

  const handlePostMaintenanceShortcut = useCallback((tx) => {
    setSelectedTransaction(tx);
    setPostMaintenanceData({
      completionDate: new Date().toISOString().split("T")[0],
      conditionAfter: "",
      notes: ""
    });
    setShowPostMaintenanceModal(true);
  }, []);

  const handleReturnSubmit = useCallback(async () => { 
    if (!selectedTransaction || isReturnSubmitting) return; 
    setIsReturnSubmitting(true); 
    const r = await transactionsData.api.createReturnTransaction(selectedTransaction.assetTransactionId, {
      actualReturnDate: returnData.actualReturnDate,
      conditionAfter: returnData.conditionAfter,
      notes: returnData.notes
    });
    setIsReturnSubmitting(false); 
    if (r.isSuccess && isMountedRef.current) { 
      toast.success('Return transaction created');
      setShowReturnModal(false); 
      reload();
      clearSelection();
    } else {
      toast.error(r.message || 'Failed to create return transaction');
    }
  }, [selectedTransaction, isReturnSubmitting, returnData, reload, toast, clearSelection]);

  const handlePostMaintenanceSubmit = useCallback(async () => {
    if (!selectedTransaction || isPostMaintenanceSubmitting) return;
    setIsPostMaintenanceSubmitting(true);
    const r = await transactionsData.api.createPostMaintenanceTransaction(selectedTransaction.assetTransactionId, {
      completionDate: postMaintenanceData.completionDate,
      conditionAfter: postMaintenanceData.conditionAfter,
      notes: postMaintenanceData.notes
    });
    setIsPostMaintenanceSubmitting(false);
    if (r.isSuccess && isMountedRef.current) {
      toast.success('Post-maintenance transaction created');
      setShowPostMaintenanceModal(false);
      reload();
      clearSelection();
    } else {
      toast.error(r.message || 'Failed to create post-maintenance transaction');
    }
  }, [selectedTransaction, isPostMaintenanceSubmitting, postMaintenanceData, reload, toast, clearSelection]);

  const handleBulkAction = useCallback(async (ids, action) => {
    const actionText = action === "approve" ? "approve" : "reject";
    const confirmed = await confirm({
      title: action === "approve" ? "Approve Transactions" : "Reject Transactions",
      text: `Are you sure you want to ${actionText} ${ids.length} transaction(s)?`,
      confirmButtonText: action === "approve" ? "Yes, Approve" : "Yes, Reject",
      confirmButtonColor: action === "approve" ? "#10b981" : "#ef4444",
    });
    if (!confirmed) return;
    
    let successCount = 0;
    for (const id of ids) {
      const result = await transactionsData.approve(id, action === "approve");
      if (result.success) successCount++;
    }
    if (successCount > 0 && isMountedRef.current) {
      toast.success(`${successCount} transaction(s) ${actionText}d successfully`);
      clearSelection();
      reload();
    }
  }, [reload, toast, confirm, clearSelection]);

  const onSubmit = useCallback(async () => { 
    const success = await crudHandleSubmit(); 
    if (success && isMountedRef.current) {
      toast.success(editingTransaction ? 'Transaction updated successfully' : 'Transaction created successfully (Pending approval)');
      reload();
      clearSelection();
    }
    return success;
  }, [crudHandleSubmit, reload, toast, editingTransaction, clearSelection]);

  const handleTabChange = useCallback((tab) => { 
    setActiveTab(tab); 
    setPage(1);
    setSearchTerm("");
    setDateFilterStart("");
    setDateFilterEnd("");
    clearSelection();
  }, [setPage, clearSelection]);

  const handleImport = useCallback(async (file) => {
    setIsImporting(true);
    const r = await transactionsData.importTransactions(file);
    setIsImporting(false);
    if (r.success && isMountedRef.current) {
      setImportResult(r.data);
      if (r.data.errorCount === 0) {
        toast.success(`Import completed: ${r.data.successCount} transactions imported`);
      } else {
        toast.warning(`Import completed: ${r.data.successCount} success, ${r.data.errorCount} errors`);
      }
      reload();
      clearSelection();
    }
  }, [reload, toast, clearSelection]);

  const handleDownloadTemplate = useCallback(async () => {
    await transactionsData.downloadTemplate();
  }, []);

  // Filter data for Primary and Secondary grids
  const primaryData = useMemo(() => {
    if (!transactions || transactions.length === 0) return [];
    return transactions.filter(t => isPrimaryTransactionType(t.transactionType));
  }, [transactions]);

  const secondaryData = useMemo(() => {
    if (!transactions || transactions.length === 0) return [];
    return transactions.filter(t => isSecondaryTransactionType(t.transactionType));
  }, [transactions]);

  const handleGridAction = useCallback((actionType, row) => {
    switch (actionType) {
      case ACTION_TYPES.EDIT:
        handleEdit(row);
        break;
      case ACTION_TYPES.DELETE:
        handleCancel(row);
        break;
      case ACTION_TYPES.APPROVE:
        handleApprove(row);
        break;
      case ACTION_TYPES.REJECT:
        handleReject(row);
        break;
      case ACTION_TYPES.RETURN:
        handleReturnShortcut(row);
        break;
      case ACTION_TYPES.POST_MAINTENANCE:
        handlePostMaintenanceShortcut(row);
        break;
      default:
        break;
    }
  }, [handleEdit, handleCancel, handleApprove, handleReject, handleReturnShortcut, handlePostMaintenanceShortcut]);

  const getConditionalActions = useCallback((row) => {
    const isPending = row.approved === null;
    const isApproved = row.approved === true;
    const isPrimary = isPrimaryTransactionType(row.transactionType);
    const isSecondary = isSecondaryTransactionType(row.transactionType);
    const isPaired = row.fromAssetTransactionId !== null && row.fromAssetTransactionId !== undefined;
    
    const hasReturnOrPost = isPaired;
    
    const canReturn = isApproved && !hasReturnOrPost && 
                      TRANSACTION_TYPES_RETURNABLE.includes(row.transactionType);
    const canPostMaintenance = isApproved && !hasReturnOrPost && 
                               row.transactionType === TRANSACTION_TYPES.MAINTENANCE;
    
    const actions = [];
    
    // Edit only for pending primary transactions
    if (isPrimary && isPending) {
      actions.push(ACTION_TYPES.EDIT);
    }
    
    // Approve/Reject only for pending primary transactions
    if (isPrimary && isPending) {
      actions.push(ACTION_TYPES.APPROVE);
      actions.push(ACTION_TYPES.REJECT);
    }
    
    // Return only for approved primary transactions that haven't been returned yet
    if (isPrimary && isApproved && canReturn) {
      actions.push(ACTION_TYPES.RETURN);
    }
    
    // Post-Maintenance only for approved MAINTENANCE that hasn't been posted yet
    if (isPrimary && isApproved && canPostMaintenance) {
      actions.push(ACTION_TYPES.POST_MAINTENANCE);
    }
    
    // Delete only for pending primary transactions
    if (isPrimary && isPending) {
      actions.push(ACTION_TYPES.DELETE);
    }
    
    // For secondary transactions (Grid 2) - only Edit and Delete
    if (isSecondary && isPending) {
      actions.push(ACTION_TYPES.EDIT);
      actions.push(ACTION_TYPES.DELETE);
    }
    
    return actions;
  }, []);

  const { actionColumn } = useGridActions({
    actions: [ACTION_TYPES.APPROVE, ACTION_TYPES.REJECT, ACTION_TYPES.EDIT, ACTION_TYPES.RETURN, ACTION_TYPES.POST_MAINTENANCE, ACTION_TYPES.DELETE],
    onAction: handleGridAction,
    getConditionalActions,
    rowIdField: 'assetTransactionId',
  });

  const showFromEmployee = TRANSACTION_TYPES_REQUIRING_FROM_EMPLOYEE.includes(parseInt(formData.transactionType));
  const showToEmployee = TRANSACTION_TYPES_REQUIRING_TO_EMPLOYEE.includes(parseInt(formData.transactionType));
  const showPairedTransaction = TRANSACTION_TYPES_REQUIRING_PAIR.includes(parseInt(formData.transactionType));
  const showMaintenanceFields = parseInt(formData.transactionType) === TRANSACTION_TYPES.MAINTENANCE || parseInt(formData.transactionType) === TRANSACTION_TYPES.POST_MAINTENANCE;
  const showExpectedReturnDate = parseInt(formData.transactionType) === TRANSACTION_TYPES.LOAN;

  const conditionOptions = useMemo(() => [
    { value: "", label: "Select Condition" },
    ...(assetConditions || []).map(c => ({ value: c.value, label: c.label }))
  ], [assetConditions]);

  // Filter assets - exclude Operational Office = Yes
  const filteredAssetOptions = useMemo(() => {
    const availableAssets = refAssets.filter(a => {
      if (a.operasionalOffice === true || a.operasionalOffice === 'true' || a.operasionalOffice === 1) {
        return false;
      }
      return true;
    });
    
    return [
      { value: "", label: "Select Asset" },
      ...availableAssets.map(a => ({ value: a.value, label: a.label }))
    ];
  }, [refAssets]);

  const employeeOptions = useMemo(() => [
    { value: "", label: "Select Employee" },
    ...employees.map(e => ({ value: e.value, label: e.label }))
  ], [employees]);

  const officeOptionsSelect = useMemo(() => [
    { value: "", label: "Select Office" },
    ...offices.map(o => ({ value: o.value, label: o.label }))
  ], [offices]);

  const getDisplayStatus = (row) => {
    if (row.approved === true) return 'Approved';
    if (row.approved === false) return 'Rejected';
    return 'Pending';
  };

  const columns = useMemo(() => {
    const dataColumns = [
      { field: "assetCode", headerName: "Asset Code", width: 120 },
      { field: "assetName", headerName: "Asset Name", flex: 1, minWidth: 160 },
      { field: "transactionTypeName", headerName: "Type", width: 150 },
      { field: "fromEmployeeName", headerName: "From", width: 150 },
      { field: "toEmployeeName", headerName: "To", width: 150 },
      { 
        field: "transactionDate", 
        headerName: "Date", 
        width: 180, 
        valueFormatter: (p) => p?.value ? utilsHelper.formatDateTime(p.value) : '-'
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
        valueFormatter: (p) => p?.value ? utilsHelper.formatDate(p.value) : '-'
      },
    ];
    return [...dataColumns, actionColumn];
  }, [actionColumn]);

  const handleCreatePrimary = useCallback(() => {
    handleCreate();
  }, [handleCreate]);

  // Date filter component
  const dateFilterComponent = (
    <div className="transactions-menu__date-filter">
      <FiCalendar size={16} />
      <span>Date Range:</span>
      <DatePickerInput 
        label="Start Date" 
        value={dateFilterStart} 
        onChange={e => handleDateFilterChange(e.target.value, dateFilterEnd)} 
        size="small"
        sx={{ minWidth: '150px' }}
      />
      <span>to</span>
      <DatePickerInput 
        label="End Date" 
        value={dateFilterEnd} 
        onChange={e => handleDateFilterChange(dateFilterStart, e.target.value)} 
        size="small"
        sx={{ minWidth: '150px' }}
      />
      {(dateFilterStart || dateFilterEnd) && (
        <button 
          className="btn btn--text btn--sm" 
          onClick={() => handleDateFilterChange("", "")}
        >
          Clear
        </button>
      )}
    </div>
  );

  const extraActions = (
    <>
      {dateFilterComponent}
      <button className="btn btn--outline btn--sm" onClick={() => setShowImportModal(true)} style={{ display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
        <FiUpload size={16} /> Import
      </button>
      {hasSelection && showCheckbox && (
        <button className="btn btn--primary btn--sm" onClick={() => setShowBulkActivateModal(true)} style={{ display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
          <FiCheckSquare size={16} /> {getBulkAction() === "approve" ? "Approve" : "Reject"} ({selectionCount})
        </button>
      )}
    </>
  );

  if (loading && !transactions.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkAction = getBulkAction();
  const bulkTitle = bulkAction === "approve" ? "Approve Transactions" : "Reject Transactions";
  const bulkDescription = `This action will ${bulkAction} the selected transactions.`;

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

  const postMaintenanceModalContent = (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Box sx={{ p: 2, bgcolor: 'var(--surface)', borderRadius: 2 }}>
        <strong style={{ color: 'var(--text-primary)' }}>
          {selectedTransaction?.assetCode} - {selectedTransaction?.assetName}
        </strong>
        <p style={{ margin: '4px 0 0', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
          Transaction ID: {selectedTransaction?.assetTransactionId}
        </p>
        <p style={{ margin: '4px 0 0', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
          Type: {selectedTransaction?.transactionTypeName} | Maintenance Transaction
        </p>
      </Box>
      <DatePickerInput 
        label="Completion Date" 
        value={postMaintenanceData.completionDate} 
        onChange={e => setPostMaintenanceData({ ...postMaintenanceData, completionDate: e.target.value })} 
        required 
      />
      <Select 
        label="Condition After" 
        value={postMaintenanceData.conditionAfter} 
        onChange={e => setPostMaintenanceData({ ...postMaintenanceData, conditionAfter: e.target.value })} 
        options={conditionOptions} 
        required 
      />
      <Input 
        label="Notes" 
        value={postMaintenanceData.notes || ""} 
        onChange={e => setPostMaintenanceData({ ...postMaintenanceData, notes: e.target.value })} 
        multiline 
        rows={2} 
      />
      <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 2 }}>
        <Button variant="outline" onClick={() => setShowPostMaintenanceModal(false)} disabled={isPostMaintenanceSubmitting}>Cancel</Button>
        <Button variant="primary" onClick={handlePostMaintenanceSubmit} loading={isPostMaintenanceSubmitting}>Confirm Post-Maintenance</Button>
      </Box>
    </Box>
  );

  return (
    <div className="transactions-menu" style={{ background: 'transparent' }}>
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Asset Transactions</h1>
          <p className="page-description">Manage asset transaction history and approvals</p>
        </div>
        <div className="transactions-menu__header-actions">
          <button className="btn btn--outline btn--sm" onClick={() => setShowImportModal(true)} style={{ display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
            <FiUpload size={16} /> Import
          </button>
          <button className="btn btn--primary btn--sm" onClick={handleCreatePrimary} style={{ display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
            <FiPlus size={16} /> New Transaction
          </button>
        </div>
      </div>

      {/* Tabs */}
      <Tabs tabs={TRANSACTION_TABS} activeTab={activeTab} onTabChange={handleTabChange} />

      {/* Search Toolbar */}
      <SearchToolbar onSearch={handleSearch} placeholder="Search transactions..." />

      {/* ============================================================ */}
      {/* GRID 1: PRIMARY TRANSACTIONS */}
      {/* ============================================================ */}
      <Card className="transactions-menu__grid-card" title="Primary Transactions" subtitle="Handover, Transfer, Loan, Maintenance, Disposal">
        <DataTable
          rows={primaryData}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          page={page}
          totalRowCount={totalCount}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
          getRowId={(row) => row?.assetTransactionId || `primary-${Math.random()}`}
          hideFooter={false}
          autoHeight={false}
          checkboxSelection={showCheckbox}
          onRowSelectionModelChange={handleSelectionChange}
          ariaLabel="Primary transactions data table"
          disableColumnFilter={false}
          disableColumnMenu={false}
          paginationMode="server"
        />
      </Card>

      {/* ============================================================ */}
      {/* GRID 2: SECONDARY TRANSACTIONS */}
      {/* ============================================================ */}
      <Card className="transactions-menu__grid-card" title="Secondary Transactions" subtitle="Return, Loan Return, Post Maintenance">
        <DataTable
          rows={secondaryData}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          page={page}
          totalRowCount={secondaryData.length}
          getRowId={(row) => row?.assetTransactionId || `secondary-${Math.random()}`}
          hideFooter={false}
          autoHeight={false}
          checkboxSelection={showCheckbox}
          onRowSelectionModelChange={handleSelectionChange}
          ariaLabel="Secondary transactions data table"
          disableColumnFilter={false}
          disableColumnMenu={false}
          paginationMode="client"
        />
      </Card>

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingTransaction ? "Edit Transaction" : "New Transaction"}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingTransaction ? "Update" : "Create"}
        size="lg"
      >
        <FormSection title="Transaction Details" description="Transaction type, asset, and parties involved">
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
                options={filteredAssetOptions} 
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
            {showExpectedReturnDate && (
              <Grid item xs={12} sm={6}>
                <DatePickerInput 
                  label="Expected Return Date" 
                  value={formData.expectedReturnDate || ""} 
                  onChange={(e) => setFormField('expectedReturnDate')(e.target.value)} 
                />
              </Grid>
            )}
          </Grid>
        </FormSection>

        <FormSection title="Condition & Maintenance" description="Asset condition and maintenance details">
          <Grid container spacing={2}>
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
          </Grid>
        </FormSection>

        <FormSection title="Notes & Attachments" description="Additional information">
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input 
                label="Notes" 
                value={formData.notes || ""} 
                onChange={(e) => setFormField('notes')(e.target.value)} 
                multiline 
                rows={3} 
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
        </FormSection>
      </CrudModal>

      <Modal isOpen={showReturnModal} onClose={() => !isReturnSubmitting && setShowReturnModal(false)} title="Return Asset" size="md">
        {returnModalContent}
      </Modal>

      <Modal isOpen={showPostMaintenanceModal} onClose={() => !isPostMaintenanceSubmitting && setShowPostMaintenanceModal(false)} title="Post Maintenance" size="md">
        {postMaintenanceModalContent}
      </Modal>

      <ImportModal
        isOpen={showImportModal}
        onClose={() => { setShowImportModal(false); setImportResult(null); }}
        onImport={handleImport}
        onDownloadTemplate={handleDownloadTemplate}
        isImporting={isImporting}
        importResult={importResult}
        title="Import Transactions"
        description="Upload Excel file with transaction data. Transactions will be imported as PENDING and need approval."
      />

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkAction(ids, bulkAction)}
        selectedIds={getSelectedIds(primaryData)}
        itemName="transactions"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default AssetTransactionsMenu;