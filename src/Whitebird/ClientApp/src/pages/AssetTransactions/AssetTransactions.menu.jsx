import React, { useState, useMemo, useCallback, useEffect, useRef } from "react";
import { Grid, Chip, Box } from "@mui/material";
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
import StatusChip from "../../components/atoms/StatusChip/StatusChip";
import TransactionFilter from "../../components/molecules/TransactionFilter/TransactionFilter";
import ReturnModalContent from "../../components/molecules/ReturnModalContent/ReturnModalContent";
import PostMaintenanceModalContent from "../../components/molecules/PostMaintenanceModalContent/PostMaintenanceModalContent";
import { FiUpload, FiCheckSquare, FiPlus } from "react-icons/fi";
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
import { useOptions } from "../../hooks/useOptions";
import { cleanTransactionFormData } from "../../core/utils/formHelpers";
import utilsHelper from "../../core/utils/utils.helper";
import "./AssetTransactions.scss";

const transactionsData = new AssetTransactionsData();
transactionsData.transformFormData = cleanTransactionFormData;

const TRANSACTION_TABS = [
  { id: "approved", label: "Approved" },
  { id: "pending", label: "Pending" },
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

const getTransactionTypeColor = (typeName) => {
  const colors = {
    "HANDOVER": { bg: "rgba(59, 130, 246, 0.15)", color: "#3b82f6", label: "Handover" },
    "TRANSFER": { bg: "rgba(139, 92, 246, 0.15)", color: "#8b5cf6", label: "Transfer" },
    "LOAN": { bg: "rgba(245, 158, 11, 0.15)", color: "#f59e0b", label: "Loan" },
    "RETURN": { bg: "rgba(16, 185, 129, 0.15)", color: "#10b981", label: "Return" },
    "LOAN_RETURN": { bg: "rgba(16, 185, 129, 0.15)", color: "#10b981", label: "Loan Return" },
    "MAINTENANCE": { bg: "rgba(245, 158, 11, 0.15)", color: "#d97706", label: "Maintenance" },
    "POST_MAINTENANCE": { bg: "rgba(16, 185, 129, 0.15)", color: "#10b981", label: "Post Maintenance" },
    "DISPOSAL": { bg: "rgba(239, 68, 68, 0.15)", color: "#ef4444", label: "Disposal" },
  };
  return colors[typeName] || { bg: "rgba(107, 114, 128, 0.1)", color: "#6b7280", label: typeName || "Unknown" };
};

const AssetTransactionsMenu = () => {
  const [activeTab, setActiveTab] = useState("approved");
  const [searchTerm, setSearchTerm] = useState("");
  const [dateFilterStart, setDateFilterStart] = useState("");
  const [dateFilterEnd, setDateFilterEnd] = useState("");
  const [showImportModal, setShowImportModal] = useState(false);
  const [showBulkActivateModalPrimary, setShowBulkActivateModalPrimary] = useState(false);
  const [showBulkActivateModalSecondary, setShowBulkActivateModalSecondary] = useState(false);
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
    idField: "assetTransactionId",
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
  });

  const {
    selectedRowIds: selectedPrimaryIds,
    selectionCount: primarySelectionCount,
    hasSelection: hasPrimarySelection,
    handleSelectionChange: handlePrimarySelectionChange,
    clearSelection: clearPrimarySelection,
    getSelectedIds: getPrimarySelectedIds
  } = useBulkSelection({ idField: "assetTransactionId" });

  const {
    selectedRowIds: selectedSecondaryIds,
    selectionCount: secondarySelectionCount,
    hasSelection: hasSecondarySelection,
    handleSelectionChange: handleSecondarySelectionChange,
    clearSelection: clearSecondarySelection,
    getSelectedIds: getSecondarySelectedIds
  } = useBulkSelection({ idField: "assetTransactionId" });

  const showCheckbox = TABS_WITH_CHECKBOX.includes(activeTab);

  const getBulkAction = () => {
    if (activeTab === "pending") return "approve";
    if (activeTab === "approved") return "reject";
    return "";
  };

  const buildFilters = useCallback(() => {
    const filters = {};

    if (activeTab === "active-loans" || activeTab === "overdue-loans" || activeTab === "rejected") {
      return filters;
    }

    if (activeTab === "pending") {
      filters.approved = null;
    } else if (activeTab === "approved") {
      filters.approved = true;
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

    if (activeTab === "active-loans") {
      try {
        const result = await transactionsData.api.getActiveLoans();
        if (result?.data && isMountedRef.current) {
          const loans = result.data?.data || result.data || [];
          return { success: true, data: { data: loans, totalCount: loans.length } };
        }
      } catch { }
      return { success: true, data: { data: [], totalCount: 0 } };
    } else if (activeTab === "overdue-loans") {
      try {
        const result = await transactionsData.api.getOverdueLoans();
        if (result?.data && isMountedRef.current) {
          const loans = result.data?.data || result.data || [];
          return { success: true, data: { data: loans, totalCount: loans.length } };
        }
      } catch { }
      return { success: true, data: { data: [], totalCount: 0 } };
    } else if (activeTab === "rejected") {
      try {
        const result = await transactionsData.api.getByApprovalStatus(false);
        if (result?.data && isMountedRef.current) {
          const rejected = result.data?.data || result.data || [];
          return { success: true, data: { data: rejected, totalCount: rejected.length } };
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
  } = useGridData(["transactions", activeTab, searchTerm, dateFilterStart, dateFilterEnd], fetchGridData);

  useEffect(() => {
    isMountedRef.current = true;
    setPage(1);
    clearPrimarySelection();
    clearSecondarySelection();
    reload();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, setPage, clearPrimarySelection, clearSecondarySelection]);

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
            t.fromAssetTransactionId === null
          );
          if (isMountedRef.current) {
            setPairedTransactionOptions(validPairs.map(t => ({
              value: t.assetTransactionId,
              label: `${getTransactionTypeName(t.transactionType)} - ${utilsHelper.formatDate(t.transactionDate)} (${t.assetCode || "N/A"})`
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
        setFormField("fromAssetTransactionId")("");
      }
    }
  }, [formData.transactionType, formData.assetId, setFormField]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
    clearPrimarySelection();
    clearSecondarySelection();
  }, [setPage, clearPrimarySelection, clearSecondarySelection]);

  const handleDateFilterChange = useCallback((start, end) => {
    setDateFilterStart(start);
    setDateFilterEnd(end);
    setPage(1);
    clearPrimarySelection();
    clearSecondarySelection();
  }, [setPage, clearPrimarySelection, clearSecondarySelection]);

  const handleApprove = useCallback(async (tx) => {
    const confirmed = await confirm({
      title: "Approve Transaction",
      text: `Are you sure you want to approve transaction for ${tx.assetCode}?`,
      confirmButtonText: "Yes, Approve",
    });
    if (!confirmed) return;
    const r = await transactionsData.approve(tx.assetTransactionId, true);
    if (r.success && isMountedRef.current) {
      toast.success("Transaction approved");
      reload();
      clearPrimarySelection();
      clearSecondarySelection();
    }
  }, [reload, toast, confirm, clearPrimarySelection, clearSecondarySelection]);

  const handleReject = useCallback(async (tx) => {
    const confirmed = await confirm({
      title: "Reject Transaction",
      text: `Are you sure you want to reject transaction for ${tx.assetCode}?`,
      icon: "warning",
      confirmButtonText: "Yes, Reject",
      confirmButtonColor: "#ef4444",
    });
    if (!confirmed) return;
    const r = await transactionsData.approve(tx.assetTransactionId, false);
    if (r.success && isMountedRef.current) {
      toast.success("Transaction rejected");
      reload();
      clearPrimarySelection();
      clearSecondarySelection();
    }
  }, [reload, toast, confirm, clearPrimarySelection, clearSecondarySelection]);

  const handleCancel = useCallback(async (tx) => {
    const confirmed = await confirmDelete("Cancel Transaction", `Are you sure you want to cancel transaction for ${tx.assetCode}?`);
    if (!confirmed) return;
    const r = await transactionsData.cancel(tx.assetTransactionId);
    if (r.success && isMountedRef.current) {
      toast.success("Transaction cancelled");
      reload();
      clearPrimarySelection();
      clearSecondarySelection();
    }
  }, [reload, toast, confirmDelete, clearPrimarySelection, clearSecondarySelection]);

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
      toast.success("Return transaction created");
      setShowReturnModal(false);
      reload();
      clearPrimarySelection();
      clearSecondarySelection();
    } else {
      toast.error(r.message || "Failed to create return transaction");
    }
  }, [selectedTransaction, isReturnSubmitting, returnData, reload, toast, clearPrimarySelection, clearSecondarySelection]);

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
      toast.success("Post-maintenance transaction created");
      setShowPostMaintenanceModal(false);
      reload();
      clearPrimarySelection();
      clearSecondarySelection();
    } else {
      toast.error(r.message || "Failed to create post-maintenance transaction");
    }
  }, [selectedTransaction, isPostMaintenanceSubmitting, postMaintenanceData, reload, toast, clearPrimarySelection, clearSecondarySelection]);

  const handleBulkAction = useCallback(async (ids, action, gridType = "primary") => {
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
      if (gridType === "primary") {
        clearPrimarySelection();
      } else {
        clearSecondarySelection();
      }
      reload();
    }
  }, [reload, toast, confirm, clearPrimarySelection, clearSecondarySelection]);

  const onSubmit = useCallback(async () => {
    const success = await crudHandleSubmit();
    if (success && isMountedRef.current) {
      toast.success(editingTransaction ? "Transaction updated successfully" : "Transaction created successfully (Pending approval)");
      reload();
      clearPrimarySelection();
      clearSecondarySelection();
    }
    return success;
  }, [crudHandleSubmit, reload, toast, editingTransaction, clearPrimarySelection, clearSecondarySelection]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
    setSearchTerm("");
    setDateFilterStart("");
    setDateFilterEnd("");
    clearPrimarySelection();
    clearSecondarySelection();
  }, [setPage, clearPrimarySelection, clearSecondarySelection]);

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
      clearPrimarySelection();
      clearSecondarySelection();
    }
  }, [reload, toast, clearPrimarySelection, clearSecondarySelection]);

  const handleDownloadTemplate = useCallback(async () => {
    await transactionsData.downloadTemplate();
  }, []);

  const getConditionalActions = useCallback((row) => {
    const isPending = row.approved === null;
    const isApproved = row.approved === true;
    const isPrimary = isPrimaryTransactionType(row.transactionType);
    const hasPair = row.fromAssetTransactionId !== null && row.fromAssetTransactionId !== undefined;

    const canReturn = isApproved && !hasPair &&
      TRANSACTION_TYPES_RETURNABLE.includes(row.transactionType);
    const canPostMaintenance = isApproved && !hasPair &&
      row.transactionType === TRANSACTION_TYPES.MAINTENANCE;

    const actions = [];

    if (isPending) {
      actions.push(ACTION_TYPES.EDIT);
      actions.push(ACTION_TYPES.DELETE);
    }

    if (isApproved) {
      actions.push(ACTION_TYPES.EDIT);
      actions.push(ACTION_TYPES.DELETE);

      if (isPrimary && canReturn) {
        actions.push(ACTION_TYPES.RETURN);
      }

      if (isPrimary && canPostMaintenance) {
        actions.push(ACTION_TYPES.POST_MAINTENANCE);
      }
    }

    return actions;
  }, []);

  const handleActionClick = useCallback((actionType, row) => {
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

  const { actionColumn } = useGridActions({
    actions: [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleActionClick,
    getConditionalActions,
    rowIdField: "assetTransactionId",
  });

  const showFromEmployee = TRANSACTION_TYPES_REQUIRING_FROM_EMPLOYEE.includes(parseInt(formData.transactionType));
  const showToEmployee = TRANSACTION_TYPES_REQUIRING_TO_EMPLOYEE.includes(parseInt(formData.transactionType));
  const showPairedTransaction = TRANSACTION_TYPES_REQUIRING_PAIR.includes(parseInt(formData.transactionType));
  const showMaintenanceFields = parseInt(formData.transactionType) === TRANSACTION_TYPES.MAINTENANCE || parseInt(formData.transactionType) === TRANSACTION_TYPES.POST_MAINTENANCE;
  const showExpectedReturnDate = parseInt(formData.transactionType) === TRANSACTION_TYPES.LOAN;

  const conditionOptions = useOptions(assetConditions, "Select Condition");

  const filteredAssetOptions = useMemo(() => {
    const availableAssets = refAssets.filter(a => {
      if (a.operasionalOffice === true || a.operasionalOffice === "true" || a.operasionalOffice === 1) {
        return false;
      }
      return true;
    });
    return [
      { value: "", label: "Select Asset" },
      ...availableAssets.map(a => ({ value: a.value, label: a.label }))
    ];
  }, [refAssets]);

  const employeeOptions = useOptions(employees, "Select Employee");
  const officeOptionsSelect = useOptions(offices, "Select Office");

  const getDisplayStatus = (row) => {
    if (row.approved === true) return "Approved";
    if (row.approved === false) return "Rejected";
    return "Pending";
  };

  const columns = useMemo(() => {
    const dataColumns = [
      {
        field: "transactionDate",
        headerName: "Date",
        width: 180,
        renderCell: (params) => {
          const row = params?.row || {};
          const value = row.transactionDate || params?.value;
          if (!value) return <span className="u-text-muted">-</span>;
          return <span>{utilsHelper.formatDateTime(value)}</span>;
        }
      },
      {
        field: "assetCode",
        headerName: "Asset Code",
        width: 120,
        renderCell: (params) => {
          const row = params?.row || {};
          const value = row.assetCode || params?.value || "-";
          return <span>{value}</span>;
        }
      },
      {
        field: "assetName",
        headerName: "Asset Name",
        flex: 1,
        minWidth: 160,
        renderCell: (params) => {
          const row = params?.row || {};
          const value = row.assetName || params?.value || "-";
          return <span>{value}</span>;
        }
      },
      {
        field: "transactionTypeName",
        headerName: "Type",
        width: 150,
        renderCell: (params) => {
          const row = params?.row || {};
          const typeName = row.transactionTypeName || params?.value || "";
          const colors = getTransactionTypeColor(typeName);
          return <Chip label={colors.label} size="small" sx={{ bgcolor: colors.bg, color: colors.color, fontWeight: 500 }} />;
        }
      },
      {
        field: "fromEmployeeName",
        headerName: "From",
        width: 150,
        renderCell: (params) => {
          const row = params?.row || {};
          const value = row.fromEmployeeName || params?.value || "-";
          return <span>{value}</span>;
        }
      },
      {
        field: "toEmployeeName",
        headerName: "To",
        width: 150,
        renderCell: (params) => {
          const row = params?.row || {};
          const value = row.toEmployeeName || params?.value || "-";
          return <span>{value}</span>;
        }
      },
      {
        field: "approved",
        headerName: "Status",
        width: 120,
        renderCell: (params) => {
          const row = params?.row || {};
          const status = getDisplayStatus(row);
          return <StatusChip status={status} />;
        }
      },
      {
        field: "expectedReturnDate",
        headerName: "Exp. Return",
        width: 130,
        renderCell: (params) => {
          const row = params?.row || {};
          const value = row.expectedReturnDate || params?.value;
          if (!value) return <span className="u-text-muted">-</span>;
          return <span>{utilsHelper.formatDate(value)}</span>;
        }
      },
      {
        field: "hasPair",
        headerName: "Paired",
        width: 90,
        renderCell: (params) => {
          const row = params?.row || {};
          const hasPair = row.fromAssetTransactionId !== null && row.fromAssetTransactionId !== undefined;
          return hasPair ? <Chip label="Yes" size="small" color="success" /> : <Chip label="No" size="small" variant="outlined" />;
        }
      },
    ];
    return [...dataColumns, actionColumn];
  }, [actionColumn]);

  const handleCreatePrimary = useCallback(() => {
    handleCreate();
  }, [handleCreate]);

  const primaryData = useMemo(() => {
    if (!transactions || transactions.length === 0) return [];
    return transactions.filter(t => isPrimaryTransactionType(t.transactionType));
  }, [transactions]);

  const secondaryData = useMemo(() => {
    if (!transactions || transactions.length === 0) return [];
    return transactions.filter(t => isSecondaryTransactionType(t.transactionType));
  }, [transactions]);

  const bulkActionButtonPrimary = (selectedPrimaryIds.length > 0 && showCheckbox) && (
    <Button
      variant="primary"
      size="sm"
      onClick={() => setShowBulkActivateModalPrimary(true)}
      className="u-inline-flex u-btn-gap"
    >
      <FiCheckSquare size={16} />
      {getBulkAction() === "approve" ? "Approve" : "Reject"} ({selectedPrimaryIds.length})
    </Button>
  );

  const bulkActionButtonSecondary = (selectedSecondaryIds.length > 0 && showCheckbox) && (
    <Button
      variant="primary"
      size="sm"
      onClick={() => setShowBulkActivateModalSecondary(true)}
      className="u-inline-flex u-btn-gap"
    >
      <FiCheckSquare size={16} />
      {getBulkAction() === "approve" ? "Approve" : "Reject"} ({selectedSecondaryIds.length})
    </Button>
  );

  const headerActions = (
    <>
      <Button variant="outline" size="sm" onClick={() => setShowImportModal(true)} className="u-inline-flex u-btn-gap">
        <FiUpload size={16} /> Import
      </Button>
      <Button variant="primary" size="sm" onClick={handleCreatePrimary} className="u-inline-flex u-btn-gap">
        <FiPlus size={16} /> New Transaction
      </Button>
    </>
  );

  if (loading && !transactions.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkAction = getBulkAction();
  const bulkTitle = bulkAction === "approve" ? "Approve Transactions" : "Reject Transactions";
  const bulkDescription = `This action will ${bulkAction} the selected transactions.`;

  return (
    <div className="transactions-menu">
      <div className="page-header">
        <div>
          <h1 className="page-title">Asset Transactions</h1>
          <p className="page-description">Manage asset transaction history and approvals</p>
        </div>
        <div className="transactions-menu__header-actions">
          {headerActions}
        </div>
      </div>

      <Tabs tabs={TRANSACTION_TABS} activeTab={activeTab} onTabChange={handleTabChange} />

      <div className="transactions-menu__date-filter-container">
        <TransactionFilter
          startDate={dateFilterStart}
          endDate={dateFilterEnd}
          onStartDateChange={handleDateFilterChange}
          onEndDateChange={(value) => handleDateFilterChange(dateFilterStart, value)}
          onClear={() => handleDateFilterChange("", "")}
          hasFilter={!!(dateFilterStart || dateFilterEnd)}
        />
      </div>

      <SearchToolbar onSearch={handleSearch} placeholder="Search transactions..." />

      <Card
        className="transactions-menu__grid-card"
        title="Primary Transactions"
        subtitle="Handover, Transfer, Loan, Maintenance, Disposal"
        actions={bulkActionButtonPrimary}
      >
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
          onRowSelectionModelChange={handlePrimarySelectionChange}
          ariaLabel="Primary transactions data table"
          disableColumnFilter={false}
          disableColumnMenu={false}
          paginationMode="server"
        />
      </Card>

      <Card
        className="transactions-menu__grid-card"
        title="Secondary Transactions"
        subtitle="Return, Loan Return, Post Maintenance"
        actions={bulkActionButtonSecondary}
      >
        <DataTable
          rows={secondaryData}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          page={page}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
          getRowId={(row) => row?.assetTransactionId || `secondary-${Math.random()}`}
          hideFooter={false}
          autoHeight={false}
          checkboxSelection={showCheckbox}
          onRowSelectionModelChange={handleSecondarySelectionChange}
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
        size="xl"
      >
        <FormSection title="Transaction Details" description="Transaction type, asset, and parties involved">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Select
                label="Transaction Type"
                value={formData.transactionType || ""}
                onChange={(e) => setFormField("transactionType")(e.target.value)}
                options={TRANSACTION_TYPE_OPTIONS}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Asset"
                value={formData.assetId || ""}
                onChange={(e) => setFormField("assetId")(e.target.value)}
                options={filteredAssetOptions}
                required
              />
            </Grid>
            {showFromEmployee && (
              <Grid item xs={12} sm={6}>
                <Select
                  label="From Employee"
                  value={formData.fromEmployeeId || ""}
                  onChange={(e) => setFormField("fromEmployeeId")(e.target.value)}
                  options={employeeOptions}
                />
              </Grid>
            )}
            {showToEmployee && (
              <Grid item xs={12} sm={6}>
                <Select
                  label="To Employee"
                  value={formData.toEmployeeId || ""}
                  onChange={(e) => setFormField("toEmployeeId")(e.target.value)}
                  options={employeeOptions}
                  required
                />
              </Grid>
            )}
            <Grid item xs={12} sm={6}>
              <Select
                label="To Office"
                value={formData.toLocationId || ""}
                onChange={(e) => setFormField("toLocationId")(e.target.value)}
                options={officeOptionsSelect}
              />
            </Grid>
            {showPairedTransaction && (
              <Grid item xs={12} sm={6}>
                <Select
                  label="Paired Transaction"
                  value={formData.fromAssetTransactionId || ""}
                  onChange={(e) => setFormField("fromAssetTransactionId")(e.target.value)}
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
                  onChange={(e) => setFormField("expectedReturnDate")(e.target.value)}
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
                onChange={(e) => setFormField("conditionBefore")(e.target.value)}
                options={conditionOptions}
              />
            </Grid>
            {showMaintenanceFields && (
              <>
                <Grid item xs={12} sm={6}>
                  <Select
                    label="Maintenance Type"
                    value={formData.maintenanceType || ""}
                    onChange={(e) => setFormField("maintenanceType")(e.target.value)}
                    options={[{ value: "", label: "Select Type" }]}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <NumberInput
                    label="Maintenance Cost"
                    value={formData.maintenanceCost}
                    onChange={(e) => setFormField("maintenanceCost")(e.target.value)}
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
                onChange={(e) => setFormField("notes")(e.target.value)}
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
        <ReturnModalContent
          transaction={selectedTransaction}
          returnData={returnData}
          onReturnDataChange={setReturnData}
          onSubmit={handleReturnSubmit}
          onCancel={() => setShowReturnModal(false)}
          isSubmitting={isReturnSubmitting}
          conditionOptions={conditionOptions}
        />
      </Modal>

      <Modal isOpen={showPostMaintenanceModal} onClose={() => !isPostMaintenanceSubmitting && setShowPostMaintenanceModal(false)} title="Post Maintenance" size="md">
        <PostMaintenanceModalContent
          transaction={selectedTransaction}
          postData={postMaintenanceData}
          onPostDataChange={setPostMaintenanceData}
          onSubmit={handlePostMaintenanceSubmit}
          onCancel={() => setShowPostMaintenanceModal(false)}
          isSubmitting={isPostMaintenanceSubmitting}
          conditionOptions={conditionOptions}
        />
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
        isOpen={showBulkActivateModalPrimary}
        onClose={() => setShowBulkActivateModalPrimary(false)}
        onConfirm={(ids) => handleBulkAction(ids, bulkAction, "primary")}
        selectedIds={getPrimarySelectedIds(primaryData)}
        itemName="transactions"
        title={bulkTitle}
        description={bulkDescription}
      />

      <BulkActivateModal
        isOpen={showBulkActivateModalSecondary}
        onClose={() => setShowBulkActivateModalSecondary(false)}
        onConfirm={(ids) => handleBulkAction(ids, bulkAction, "secondary")}
        selectedIds={getSecondarySelectedIds(secondaryData)}
        itemName="transactions"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default AssetTransactionsMenu;