// src/pages/AssetTransactions/AssetTransactions.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { AssetTransactionsData } from '../../core/services';
import Spinner from '../../components/atoms/Spinner/Spinner';
import Button from '../../components/atoms/Button/Button';
import SearchToolbar from '../../components/molecules/SearchToolbar/SearchToolbar';
import Tabs from '../../components/molecules/Tabs/Tabs';
import TransactionFilter from '../../components/molecules/TransactionFilter/TransactionFilter';
import ImportModal from '../../components/molecules/ImportModal/ImportModal';
import { FiUpload, FiPlus } from 'react-icons/fi';
import { TRANSACTION_TYPES, TRANSACTION_TYPES_REQUIRING_PAIR, TRANSACTION_TYPES_REQUIRING_EMPLOYEE, TRANSACTION_TYPES_REQUIRING_OFFICE, TRANSACTION_TYPES_RETURNABLE, isPrimaryTransactionType } from '../../core/constants/transactionTypes';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useReferenceData } from '../../hooks/useReferenceData';
import { useCrudFormBase } from '../../hooks/useCrudFormBase';
import { useOptions } from '../../hooks/useOptions';
import { getTransactionTypeName } from '../../core/constants/transactionTypes';
import { baseTransactionColumns, getMonitoringColumns, getManagementColumns, getManagementReturnColumns } from '../../core/constants/columns';
import PrimaryTransactionsGrid from './components/PrimaryTransactionsGrid';
import SecondaryTransactionsGrid from './components/SecondaryTransactionsGrid';
import TransactionDialogs from './components/TransactionDialogs';
import utilsHelper from '../../core/utils/utils.helper';
import './AssetTransactions.scss';

const transactionsData = new AssetTransactionsData();

const MONITORING_SUB_TABS = [
  { id: 'uncomplete', label: 'Uncomplete' },
  { id: 'active-loans', label: 'Active Loans' },
  { id: 'overdue-loans', label: 'Overdue Loans' },
  { id: 'under-maintenance', label: 'Under Maintenance' },
  { id: 'complete', label: 'Complete' },
];

const MANAGEMENT_SUB_TABS = [
  { id: 'approved', label: 'Approved' },
  { id: 'pending', label: 'Pending' },
  { id: 'rejected', label: 'Rejected' },
];

const INITIAL_FORM_DATA = {
  assetId: '',
  transactionType: '',
  employeeId: '',
  officeId: '',
  expectedLoanReturnDate: '',
  notes: '',
  condition: '',
  maintenanceType: '',
  maintenanceCost: '',
  fromAssetTransactionId: '',
  transactionDate: new Date().toISOString(),
};

const AssetTransactions = ({ initialMainTab = 'monitoring' }) => {
  const [activeMainTab] = useState(initialMainTab);
  const defaultSubTab = initialMainTab === 'monitoring' ? 'uncomplete' : 'approved';
  const [activeSubTab, setActiveSubTab] = useState(defaultSubTab);
  const [searchTerm, setSearchTerm] = useState('');
  const [dateFilterStart, setDateFilterStart] = useState('');
  const [dateFilterEnd, setDateFilterEnd] = useState('');

  const [showImportModal, setShowImportModal] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);
  const [showReturnModal, setShowReturnModal] = useState(false);
  const [showPostMaintenanceModal, setShowPostMaintenanceModal] = useState(false);
  const [selectedTransaction, setSelectedTransaction] = useState(null);
  const [isReturnSubmitting, setIsReturnSubmitting] = useState(false);
  const [isPostMaintenanceSubmitting, setIsPostMaintenanceSubmitting] = useState(false);
  const [returnData, setReturnData] = useState({
    actualReturnDate: new Date().toISOString().split('T')[0],
    condition: '',
    notes: '',
  });
  const [postMaintenanceData, setPostMaintenanceData] = useState({
    completionDate: new Date().toISOString().split('T')[0],
    condition: '',
    maintenanceCost: '',
    notes: '',
  });
  const [pairedTransactionOptions, setPairedTransactionOptions] = useState([]);
  const [loadingPairedOptions, setLoadingPairedOptions] = useState(false);

  const [attachmentFiles, setAttachmentFiles] = useState([]);
  const [existingAttachments, setExistingAttachments] = useState([]);
  const [isEditMode, setIsEditMode] = useState(false);
  const [currentTransactionId, setCurrentTransactionId] = useState(null);

  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirm, confirmDelete } = useSweetAlert();

  const { employees, offices, assets: refAssets, conditions, maintenanceTypes } = useReferenceData();

  // ============================================================
  // CRUD FORM
  // ============================================================
  const {
    showModal,
    editingRecord: editingTransaction,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit: handleEditBase,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, transactionsData, {
    idField: 'assetTransactionId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['monitoring'] });
      setAttachmentFiles([]);
      setExistingAttachments([]);
      setIsEditMode(false);
      setCurrentTransactionId(null);
      toast.success(editingTransaction ? 'Transaction updated successfully' : 'Transaction created (Pending approval)');
      reloadManagement();
      reloadManagementReturn();
      reloadMonitoring();
      clearPrimary();
      clearSecondary();
    },
    onError: (error) => {
      toast.error(error || 'Failed to save transaction');
    },
  });

  // ============================================================
  // BULK SELECTION
  // ============================================================
  const { selectedRowIds: primarySelected, selectionCount: primaryCount, hasSelection: hasPrimary, handleSelectionChange: handlePrimaryChange, clearSelection: clearPrimary, getSelectedIds: getPrimaryIds } =
    useBulkSelection({ idField: 'assetTransactionId' });

  const { selectedRowIds: secondarySelected, selectionCount: secondaryCount, hasSelection: hasSecondary, handleSelectionChange: handleSecondaryChange, clearSelection: clearSecondary, getSelectedIds: getSecondaryIds } =
    useBulkSelection({ idField: 'assetTransactionId' });

  // ============================================================
  // EDIT WITH ATTACHMENTS
  // ============================================================
  const handleEdit = useCallback(async (record) => {
    const recordId = record?.assetTransactionId;
    if (!recordId) return;
    setIsEditMode(true);
    setCurrentTransactionId(recordId);
    try {
      const result = await transactionsData.fetchById(recordId);
      if (result.success && result.data) {
        const data = result.data;
        if (data.attachments && Array.isArray(data.attachments)) {
          setExistingAttachments(data.attachments);
        } else {
          setExistingAttachments([]);
        }
      } else {
        setExistingAttachments([]);
      }
    } catch (error) {
      console.warn('Failed to fetch attachments:', error);
      setExistingAttachments([]);
    }
    handleEditBase(record);
  }, [handleEditBase]);

  // ============================================================
  // DATA FETCHING
  // ============================================================
  const fetchMonitoringData = useCallback(async (params) => {
    const filters = {};
    if (searchTerm) filters.search = searchTerm;
    if (dateFilterStart) filters.startDate = dateFilterStart;
    if (dateFilterEnd) filters.endDate = dateFilterEnd;

    const requestParams = {
      page: params.page || 1,
      pageSize: params.pageSize || 10,
      ...filters,
      ...params,
    };

    let result;
    switch (activeSubTab) {
      case 'uncomplete':
        result = await transactionsData.api.getGridUncomplete(requestParams);
        break;
      case 'complete':
        result = await transactionsData.api.getGridComplete(requestParams);
        break;
      case 'active-loans':
        result = await transactionsData.api.getActiveLoans(requestParams);
        break;
      case 'overdue-loans':
        result = await transactionsData.api.getOverdueLoans(requestParams);
        break;
      case 'under-maintenance':
        result = await transactionsData.api.getUnderMaintenance(requestParams);
        break;
      default:
        result = await transactionsData.api.getGridUncomplete(requestParams);
    }

    return {
      success: true,
      data: {
        data: result.data?.data || [],
        totalCount: result.data?.totalCount || 0,
      },
    };
  }, [activeSubTab, searchTerm, dateFilterStart, dateFilterEnd]);

  const { data: monitoringData, totalCount: monitoringTotal, loading: monitoringLoading, page: monitoringPage, setPage: setMonitoringPage, pageSize: monitoringPageSize, setPageSize: setMonitoringPageSize, reload: reloadMonitoring } =
    useGridData(['monitoring', activeSubTab, searchTerm, dateFilterStart, dateFilterEnd], fetchMonitoringData);

  const buildManagementFilters = useCallback(() => {
    const filters = {};
    if (activeSubTab === 'pending') filters.status = 'pending';
    else if (activeSubTab === 'approved') filters.status = 'approve';
    else if (activeSubTab === 'rejected') filters.status = 'reject';
    if (searchTerm) filters.search = searchTerm;
    if (dateFilterStart) filters.startDate = dateFilterStart;
    if (dateFilterEnd) filters.endDate = dateFilterEnd;
    return filters;
  }, [activeSubTab, searchTerm, dateFilterStart, dateFilterEnd]);

  const fetchManagementData = useCallback(async (params) => {
    const filters = buildManagementFilters();
    const requestParams = { page: params.page || 1, pageSize: params.pageSize || 10, ...filters, ...params };
    const result = await transactionsData.api.getGridAdd(requestParams);
    return { success: true, data: { data: result.data?.data || [], totalCount: result.data?.totalCount || 0 } };
  }, [buildManagementFilters]);

  const { data: managementData, totalCount: managementTotal, loading: managementLoading, page: managementPage, setPage: setManagementPage, pageSize: managementPageSize, setPageSize: setManagementPageSize, reload: reloadManagement } =
    useGridData(['management', activeSubTab, searchTerm, dateFilterStart, dateFilterEnd], fetchManagementData);

  const fetchManagementReturnData = useCallback(async (params) => {
    const filters = buildManagementFilters();
    const requestParams = { page: params.page || 1, pageSize: params.pageSize || 10, ...filters, ...params };
    const result = await transactionsData.api.getGridReturn(requestParams);
    return { success: true, data: { data: result.data?.data || [], totalCount: result.data?.totalCount || 0 } };
  }, [buildManagementFilters]);

  const { data: managementReturnData, totalCount: managementReturnTotal, loading: managementReturnLoading, page: managementReturnPage, setPage: setManagementReturnPage, pageSize: managementReturnPageSize, setPageSize: setManagementReturnPageSize, reload: reloadManagementReturn } =
    useGridData(['management-return', activeSubTab, searchTerm, dateFilterStart, dateFilterEnd], fetchManagementReturnData);

  // ============================================================
  // PAIRED TRANSACTION OPTIONS
  // ============================================================
  useEffect(() => {
    const type = parseInt(formData.transactionType);
    if (formData.assetId && TRANSACTION_TYPES_REQUIRING_PAIR.includes(type)) {
      setLoadingPairedOptions(true);
      (async () => {
        try {
          const sourceType = type === TRANSACTION_TYPES.LOAN_RETURN ? TRANSACTION_TYPES.LOAN : TRANSACTION_TYPES.MAINTENANCE;
          const result = await transactionsData.getPairedOptions(parseInt(formData.assetId, 10), sourceType);
          if (isMountedRef.current && result.success) {
            setPairedTransactionOptions(
              result.data.map(t => ({
                value: t.assetTransactionId,
                label: `${getTransactionTypeName(t.transactionType)} - ${utilsHelper.formatDate(t.transactionDate)} (${t.assetCode || 'N/A'})`,
              }))
            );
          }
        } catch {
          if (isMountedRef.current) setPairedTransactionOptions([]);
        }
        if (isMountedRef.current) setLoadingPairedOptions(false);
      })();
    } else {
      setPairedTransactionOptions([]);
      if (!TRANSACTION_TYPES_REQUIRING_PAIR.includes(type)) {
        setFormField('fromAssetTransactionId')('');
      }
    }
  }, [formData.transactionType, formData.assetId, setFormField]);

  // ============================================================
  // EFFECTS
  // ============================================================
  useEffect(() => {
    isMountedRef.current = true;
    clearPrimary();
    clearSecondary();
    if (activeMainTab === 'monitoring') {
      reloadMonitoring();
    } else {
      reloadManagement();
      reloadManagementReturn();
    }
    return () => { isMountedRef.current = false; };
  }, [activeMainTab, activeSubTab, reloadMonitoring, reloadManagement, reloadManagementReturn]);

  // ============================================================
  // HANDLERS
  // ============================================================
  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setMonitoringPage(1);
    setManagementPage(1);
    setManagementReturnPage(1);
    clearPrimary();
    clearSecondary();
  }, [setMonitoringPage, setManagementPage, setManagementReturnPage, clearPrimary, clearSecondary]);

  const handleDateFilterChange = useCallback((start, end) => {
    setDateFilterStart(start);
    setDateFilterEnd(end);
    setMonitoringPage(1);
    setManagementPage(1);
    setManagementReturnPage(1);
    clearPrimary();
    clearSecondary();
  }, [setMonitoringPage, setManagementPage, setManagementReturnPage, clearPrimary, clearSecondary]);

  const handleComplete = useCallback(async (id) => {
    const confirmed = await confirm({ title: 'Complete Transaction', text: 'Are you sure you want to complete this transaction?', confirmButtonText: 'Yes, Complete' });
    if (!confirmed) return;
    const r = await transactionsData.complete(id);
    if (r.success && isMountedRef.current) {
      toast.success('Transaction completed successfully');
      reloadMonitoring();
      reloadManagement();
      reloadManagementReturn();
    }
  }, [reloadMonitoring, reloadManagement, reloadManagementReturn, toast, confirm]);

  const handleUncomplete = useCallback(async (id) => {
    const confirmed = await confirm({ title: 'Uncomplete Transaction', text: 'Are you sure you want to uncomplete this transaction?', icon: 'warning', confirmButtonText: 'Yes, Uncomplete', confirmButtonColor: '#f59e0b' });
    if (!confirmed) return;
    const r = await transactionsData.uncomplete(id);
    if (r.success && isMountedRef.current) {
      toast.success('Transaction uncompleted successfully');
      reloadMonitoring();
      reloadManagement();
      reloadManagementReturn();
    }
  }, [reloadMonitoring, reloadManagement, reloadManagementReturn, toast, confirm]);

  const handleBulkApprove = useCallback(async (ids, gridType = 'primary') => {
    const confirmed = await confirm({ title: 'Approve Transactions', text: `Are you sure you want to approve ${ids.length} transaction(s)?`, confirmButtonText: 'Yes, Approve' });
    if (!confirmed) return;
    const r = await transactionsData.bulkApprove(ids);
    if (r.success && isMountedRef.current) {
      toast.success(`${ids.length} transaction(s) approved`);
      if (gridType === 'primary') clearPrimary();
      else clearSecondary();
      reloadManagement();
      reloadManagementReturn();
      reloadMonitoring();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring, toast, confirm, clearPrimary, clearSecondary]);

  const handleBulkReject = useCallback(async (ids, gridType = 'primary') => {
    const confirmed = await confirm({ title: 'Reject Transactions', text: `Are you sure you want to reject ${ids.length} transaction(s)?`, icon: 'warning', confirmButtonText: 'Yes, Reject', confirmButtonColor: '#ef4444' });
    if (!confirmed) return;
    const r = await transactionsData.bulkReject(ids);
    if (r.success && isMountedRef.current) {
      toast.success(`${ids.length} transaction(s) rejected`);
      if (gridType === 'primary') clearPrimary();
      else clearSecondary();
      reloadManagement();
      reloadManagementReturn();
      reloadMonitoring();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring, toast, confirm, clearPrimary, clearSecondary]);

  const handleBulkPending = useCallback(async (ids, gridType = 'primary') => {
    const confirmed = await confirm({ title: 'Move to Pending', text: `Are you sure you want to move ${ids.length} transaction(s) back to pending?`, confirmButtonText: 'Yes, Revert' });
    if (!confirmed) return;
    const r = await transactionsData.bulkPending(ids);
    if (r.success && isMountedRef.current) {
      toast.success(`${ids.length} transaction(s) moved to pending`);
      if (gridType === 'primary') clearPrimary();
      else clearSecondary();
      reloadManagement();
      reloadManagementReturn();
      reloadMonitoring();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring, toast, confirm, clearPrimary, clearSecondary]);

  const handleReturnShortcut = useCallback((tx) => {
    setSelectedTransaction(tx);
    setReturnData({ actualReturnDate: new Date().toISOString().split('T')[0], condition: '', notes: '' });
    setShowReturnModal(true);
  }, []);

  const handlePostMaintenanceShortcut = useCallback((tx) => {
    setSelectedTransaction(tx);
    setPostMaintenanceData({ completionDate: new Date().toISOString().split('T')[0], condition: '', maintenanceCost: '', notes: '' });
    setShowPostMaintenanceModal(true);
  }, []);

  const handleReturnSubmit = useCallback(async () => {
    if (!selectedTransaction || isReturnSubmitting) return;
    setIsReturnSubmitting(true);
    const r = await transactionsData.createReturn(selectedTransaction.assetTransactionId, {
      actualReturnDate: returnData.actualReturnDate,
      condition: returnData.condition ? parseInt(returnData.condition, 10) : null,
      notes: returnData.notes,
    });
    setIsReturnSubmitting(false);
    if (r.success && isMountedRef.current) {
      setShowReturnModal(false);
      reloadMonitoring();
      reloadManagement();
      reloadManagementReturn();
      clearPrimary();
      clearSecondary();
    }
  }, [selectedTransaction, isReturnSubmitting, returnData, reloadMonitoring, reloadManagement, reloadManagementReturn, clearPrimary, clearSecondary]);

  const handlePostMaintenanceSubmit = useCallback(async () => {
    if (!selectedTransaction || isPostMaintenanceSubmitting) return;
    setIsPostMaintenanceSubmitting(true);
    const r = await transactionsData.createPostMaintenance(selectedTransaction.assetTransactionId, {
      completionDate: postMaintenanceData.completionDate,
      condition: postMaintenanceData.condition ? parseInt(postMaintenanceData.condition, 10) : null,
      maintenanceCost: postMaintenanceData.maintenanceCost ? parseFloat(postMaintenanceData.maintenanceCost) : null,
      notes: postMaintenanceData.notes,
    });
    setIsPostMaintenanceSubmitting(false);
    if (r.success && isMountedRef.current) {
      setShowPostMaintenanceModal(false);
      reloadMonitoring();
      reloadManagement();
      reloadManagementReturn();
      clearPrimary();
      clearSecondary();
    }
  }, [selectedTransaction, isPostMaintenanceSubmitting, postMaintenanceData, reloadMonitoring, reloadManagement, reloadManagementReturn, clearPrimary, clearSecondary]);

  const handleImport = useCallback(async (file) => {
    setIsImporting(true);
    const r = await transactionsData.importTransactions(file);
    setIsImporting(false);
    if (r.success && isMountedRef.current) {
      setImportResult(r.data);
      if (r.data.errorCount === 0) {
        toast.success(`Import completed: ${r.data.successCount} transactions`);
      } else {
        toast.warning(`Import completed: ${r.data.successCount} success, ${r.data.errorCount} errors`);
      }
      reloadManagement();
      reloadManagementReturn();
      reloadMonitoring();
      clearPrimary();
      clearSecondary();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring, toast, clearPrimary, clearSecondary]);

  const handleDownloadTemplate = useCallback(async () => {
    const result = await transactionsData.downloadTemplate();
    if (!result.success) {
      toast.error(result.error || 'Failed to download template');
    }
  }, [toast]);

  const handleFilesChange = useCallback((files) => {
    setAttachmentFiles(files.filter(f => f.isNew));
    const existing = files.filter(f => !f.isNew);
    if (existing.length > 0) setExistingAttachments(existing);
  }, []);

  const handleUploadComplete = useCallback((newFiles) => {
    console.log('Files added for upload:', newFiles);
  }, []);

  const handleFileDeleted = useCallback(() => {
    const fetchAttachments = async () => {
      try {
        const result = await transactionsData.fetchById(currentTransactionId);
        if (result.success && result.data) {
          const data = result.data;
          if (data.attachments && Array.isArray(data.attachments)) {
            setExistingAttachments(data.attachments);
          }
        }
      } catch (error) {
        console.warn('Failed to refresh attachments:', error);
      }
    };
    fetchAttachments();
  }, [currentTransactionId]);

  const onSubmit = useCallback(async () => {
    const submitData = {
      ...formData,
      attachmentFiles: attachmentFiles,
      attachmentIdsToKeep: isEditMode ? existingAttachments.map(a => a.fileAttachmentId) : [],
      replaceAttachments: isEditMode ? true : false,
    };

    if (isEditMode && editingTransaction) {
      const result = await transactionsData.updateWithAttachments(editingTransaction.assetTransactionId, submitData);
      if (result.success) {
        setAttachmentFiles([]);
        setExistingAttachments([]);
        setIsEditMode(false);
        setCurrentTransactionId(null);
        handleClose();
        queryClient.invalidateQueries({ queryKey: ['transactions'] });
        queryClient.invalidateQueries({ queryKey: ['monitoring'] });
        reloadManagement();
        reloadManagementReturn();
        reloadMonitoring();
        clearPrimary();
        clearSecondary();
        toast.success('Transaction updated successfully');
        return true;
      }
      return false;
    } else {
      const success = await crudHandleSubmit();
      if (success && isMountedRef.current) {
        reloadManagement();
        reloadManagementReturn();
        reloadMonitoring();
        clearPrimary();
        clearSecondary();
        setAttachmentFiles([]);
        setExistingAttachments([]);
        setIsEditMode(false);
        setCurrentTransactionId(null);
      }
      return success;
    }
  }, [crudHandleSubmit, reloadManagement, reloadManagementReturn, reloadMonitoring, toast, editingTransaction, clearPrimary, clearSecondary, isEditMode, attachmentFiles, existingAttachments, formData, handleClose, queryClient]);

  // ============================================================
  // GRID ACTIONS
  // ============================================================
  const getMonitoringActions = useCallback((row) => {
    const actions = [];
    const isPending = row.approved === null;
    const isApproved = row.approved === true;
    const isCompleted = row.completed === true;

    actions.push(ACTION_TYPES.VIEW);
    if (isPending && activeSubTab === 'uncomplete') {
      actions.push(ACTION_TYPES.EDIT);
      actions.push(ACTION_TYPES.DELETE);
    }
    if (isApproved && !isCompleted && activeSubTab === 'uncomplete') {
      actions.push('complete');
    }
    if (isCompleted && activeSubTab === 'uncomplete') {
      actions.push('uncomplete');
    }
    return actions;
  }, [activeSubTab]);

  const handleMonitoringAction = useCallback((actionType, row) => {
    if (actionType === ACTION_TYPES.VIEW) {
      toast.info(`Viewing transaction ${row.assetTransactionCode || row.assetTransactionId}`);
      return;
    }
    if (actionType === ACTION_TYPES.EDIT) handleEdit(row);
    else if (actionType === ACTION_TYPES.DELETE) {
      confirmDelete('Cancel Transaction', `Cancel transaction for ${row.assetCode}?`).then(confirmed => {
        if (confirmed) transactionsData.delete(row.assetTransactionId).then(() => {
          toast.success('Transaction cancelled');
          reloadMonitoring();
          reloadManagement();
          reloadManagementReturn();
          clearPrimary();
          clearSecondary();
        });
      });
    } else if (actionType === 'complete') {
      handleComplete(row.assetTransactionId);
    } else if (actionType === 'uncomplete') {
      handleUncomplete(row.assetTransactionId);
    }
  }, [handleEdit, handleComplete, handleUncomplete, toast, reloadMonitoring, reloadManagement, reloadManagementReturn, clearPrimary, clearSecondary, confirmDelete]);

  const getManagementActions = useCallback((row) => {
    const isPending = row.approved === null;
    const isApproved = row.approved === true;
    const isPrimary = isPrimaryTransactionType(row.transactionType);
    const hasPair = row.fromAssetTransactionId !== null && row.fromAssetTransactionId !== undefined;

    const actions = [ACTION_TYPES.VIEW];
    if (isPending) {
      actions.push(ACTION_TYPES.EDIT);
      actions.push(ACTION_TYPES.DELETE);
    }
    if (isApproved) {
      actions.push(ACTION_TYPES.EDIT);
      actions.push(ACTION_TYPES.DELETE);
      if (isPrimary && TRANSACTION_TYPES_RETURNABLE.includes(row.transactionType) && !hasPair && row.completed === true) {
        actions.push(ACTION_TYPES.RETURN);
      }
      if (isPrimary && row.transactionType === TRANSACTION_TYPES.MAINTENANCE && !hasPair && row.completed === true) {
        actions.push(ACTION_TYPES.POST_MAINTENANCE);
      }
    }
    return actions;
  }, []);

  const handleManagementAction = useCallback((actionType, row) => {
    if (actionType === ACTION_TYPES.VIEW) {
      toast.info(`Viewing transaction ${row.assetTransactionCode || row.assetTransactionId}`);
      return;
    }
    if (actionType === ACTION_TYPES.EDIT) handleEdit(row);
    else if (actionType === ACTION_TYPES.DELETE) {
      confirmDelete('Cancel Transaction', `Cancel transaction for ${row.assetCode}?`).then(confirmed => {
        if (confirmed) transactionsData.delete(row.assetTransactionId).then(() => {
          toast.success('Transaction cancelled');
          reloadManagement();
          reloadManagementReturn();
          reloadMonitoring();
          clearPrimary();
          clearSecondary();
        });
      });
    } else if (actionType === ACTION_TYPES.RETURN) handleReturnShortcut(row);
    else if (actionType === ACTION_TYPES.POST_MAINTENANCE) handlePostMaintenanceShortcut(row);
  }, [handleEdit, confirmDelete, toast, reloadManagement, reloadManagementReturn, reloadMonitoring, clearPrimary, clearSecondary, handleReturnShortcut, handlePostMaintenanceShortcut]);

  const getManagementReturnActions = useCallback((row) => {
    const isPending = row.approved === null;
    const actions = [ACTION_TYPES.VIEW];
    if (isPending) {
      actions.push(ACTION_TYPES.EDIT);
      actions.push(ACTION_TYPES.DELETE);
    }
    return actions;
  }, []);

  const handleManagementReturnAction = useCallback((actionType, row) => {
    if (actionType === ACTION_TYPES.VIEW) {
      toast.info(`Viewing transaction ${row.assetTransactionCode || row.assetTransactionId}`);
      return;
    }
    if (actionType === ACTION_TYPES.EDIT) handleEdit(row);
    else if (actionType === ACTION_TYPES.DELETE) {
      confirmDelete('Cancel Transaction', `Cancel transaction for ${row.assetCode}?`).then(confirmed => {
        if (confirmed) transactionsData.delete(row.assetTransactionId).then(() => {
          toast.success('Transaction cancelled');
          reloadManagement();
          reloadManagementReturn();
          reloadMonitoring();
          clearPrimary();
          clearSecondary();
        });
      });
    }
  }, [handleEdit, confirmDelete, toast, reloadManagement, reloadManagementReturn, reloadMonitoring, clearPrimary, clearSecondary]);

  // ============================================================
  // ACTION COLUMNS
  // ============================================================
  const { actionColumn: monitoringActionColumn } = useGridActions({
    actions: [ACTION_TYPES.VIEW, ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleMonitoringAction,
    getConditionalActions: getMonitoringActions,
    rowIdField: 'assetTransactionId',
  });

  const { actionColumn: managementActionColumn } = useGridActions({
    actions: [ACTION_TYPES.VIEW, ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleManagementAction,
    getConditionalActions: getManagementActions,
    rowIdField: 'assetTransactionId',
  });

  const { actionColumn: managementReturnActionColumn } = useGridActions({
    actions: [ACTION_TYPES.VIEW, ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleManagementReturnAction,
    getConditionalActions: getManagementReturnActions,
    rowIdField: 'assetTransactionId',
  });

  // ============================================================
  // COLUMNS
  // ============================================================
  const monitoringColumns = useMemo(() => {
    return getMonitoringColumns(monitoringActionColumn, activeSubTab);
  }, [activeSubTab, monitoringActionColumn]);

  const managementColumns = useMemo(() => {
    return getManagementColumns(baseTransactionColumns, managementActionColumn);
  }, [managementActionColumn]);

  const managementReturnColumns = useMemo(() => {
    return getManagementReturnColumns(baseTransactionColumns, managementReturnActionColumn);
  }, [managementReturnActionColumn]);

  // ============================================================
  // OPTIONS
  // ============================================================
  const employeeOptions = useOptions(employees, 'Select Employee');
  const officeOptions = useOptions(offices, 'Select Office');
  const conditionOptions = useOptions(conditions, 'Select Condition');
  const maintenanceTypeOptions = useOptions(maintenanceTypes, 'Select Maintenance Type');

  const filteredAssetOptions = useMemo(() => {
    return [{ value: '', label: 'Select Asset' }, ...refAssets.map(a => ({ value: a.value, label: a.label }))];
  }, [refAssets]);

  // ============================================================
  // COMPUTED
  // ============================================================
  const isMonitoring = activeMainTab === 'monitoring';
  const showEmployee = TRANSACTION_TYPES_REQUIRING_EMPLOYEE.includes(parseInt(formData.transactionType));
  const showOffice = TRANSACTION_TYPES_REQUIRING_OFFICE.includes(parseInt(formData.transactionType));
  const showPaired = TRANSACTION_TYPES_REQUIRING_PAIR.includes(parseInt(formData.transactionType));
  const showMaintenance = parseInt(formData.transactionType) === TRANSACTION_TYPES.MAINTENANCE;
  const showExpectedReturn = parseInt(formData.transactionType) === TRANSACTION_TYPES.LOAN;
  const showCheckbox = activeSubTab === 'pending' || activeSubTab === 'approved';
  const showFileUploads = isEditMode && currentTransactionId;

  const bulkAction = activeSubTab === 'pending' ? 'approve' : activeSubTab === 'approved' ? 'reject' : '';
  const bulkActionLabel = bulkAction === 'approve' ? 'Approve' : bulkAction === 'reject' ? 'Reject' : '';

  const renderBulkButton = (ids, gridType) => {
    if (!ids.length || !bulkAction) return null;
    const onClick = bulkAction === 'approve'
      ? () => handleBulkApprove(ids, gridType)
      : () => handleBulkReject(ids, gridType);
    return (
      <Button variant="primary" size="sm" onClick={onClick} className="u-inline-flex u-btn-gap">
        <FiCheckSquare size={16} /> {bulkActionLabel} ({ids.length})
      </Button>
    );
  };

  const headerActions = (
    <>
      <Button variant="outline" size="sm" onClick={() => setShowImportModal(true)} className="u-inline-flex u-btn-gap">
        <FiUpload size={16} /> Import
      </Button>
      <Button variant="primary" size="sm" onClick={handleCreate} className="u-inline-flex u-btn-gap">
        <FiPlus size={16} /> New Transaction
      </Button>
    </>
  );

  // ============================================================
  // RENDER
  // ============================================================
  if (managementLoading && !managementData.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="transactions-menu">
      <div className="page-header">
        <div>
          <h1 className="page-title">
            {isMonitoring ? 'Asset Transactions - Monitoring' : 'Asset Transactions - Management'}
          </h1>
          <p className="page-description">
            {isMonitoring 
              ? 'Monitor transaction status, loans, maintenance, and pending completions'
              : 'Manage primary and secondary transactions with approvals'
            }
          </p>
        </div>
        <div className="transactions-menu__header-actions">{headerActions}</div>
      </div>

      {isMonitoring ? (
        <Tabs tabs={MONITORING_SUB_TABS} activeTab={activeSubTab} onTabChange={setActiveSubTab} />
      ) : (
        <Tabs tabs={MANAGEMENT_SUB_TABS} activeTab={activeSubTab} onTabChange={setActiveSubTab} />
      )}

      <div className="transactions-menu__filter-container">
        <TransactionFilter
          startDate={dateFilterStart}
          endDate={dateFilterEnd}
          onStartDateChange={(v) => handleDateFilterChange(v, dateFilterEnd)}
          onEndDateChange={(v) => handleDateFilterChange(dateFilterStart, v)}
          onClear={() => handleDateFilterChange('', '')}
          hasFilter={!!(dateFilterStart || dateFilterEnd)}
        />
        <SearchToolbar onSearch={handleSearch} placeholder="Search transactions..." />
      </div>

      {isMonitoring && (
        <PrimaryTransactionsGrid
          data={monitoringData}
          loading={monitoringLoading}
          totalCount={monitoringTotal}
          page={monitoringPage}
          pageSize={monitoringPageSize}
          onPageChange={setMonitoringPage}
          onPageSizeChange={setMonitoringPageSize}
          columns={monitoringColumns}
          checkboxSelection={false}
          getRowId={(row) => row?.assetTransactionId || `mon-${Math.random()}`}
        />
      )}

      {!isMonitoring && (
        <>
          <PrimaryTransactionsGrid
            data={managementData}
            loading={managementLoading}
            totalCount={managementTotal}
            page={managementPage}
            pageSize={managementPageSize}
            onPageChange={setManagementPage}
            onPageSizeChange={setManagementPageSize}
            columns={managementColumns}
            checkboxSelection={showCheckbox}
            onSelectionChange={handlePrimaryChange}
            getRowId={(row) => row?.assetTransactionId || `primary-${Math.random()}`}
          />

          <SecondaryTransactionsGrid
            data={managementReturnData}
            loading={managementReturnLoading}
            totalCount={managementReturnTotal}
            page={managementReturnPage}
            pageSize={managementReturnPageSize}
            onPageChange={setManagementReturnPage}
            onPageSizeChange={setManagementReturnPageSize}
            columns={managementReturnColumns}
            checkboxSelection={showCheckbox}
            onSelectionChange={handleSecondaryChange}
            getRowId={(row) => row?.assetTransactionId || `secondary-${Math.random()}`}
          />
        </>
      )}

      <TransactionDialogs
        // CRUD Modal
        showModal={showModal}
        editingTransaction={editingTransaction}
        isSubmitting={isSubmitting}
        formData={formData}
        setFormField={setFormField}
        onSubmit={onSubmit}
        onClose={handleClose}
        // Return Modal
        showReturnModal={showReturnModal}
        isReturnSubmitting={isReturnSubmitting}
        returnData={returnData}
        setReturnData={setReturnData}
        selectedTransaction={selectedTransaction}
        onReturnSubmit={handleReturnSubmit}
        onReturnCancel={() => setShowReturnModal(false)}
        // Post Maintenance Modal
        showPostMaintenanceModal={showPostMaintenanceModal}
        isPostMaintenanceSubmitting={isPostMaintenanceSubmitting}
        postMaintenanceData={postMaintenanceData}
        setPostMaintenanceData={setPostMaintenanceData}
        onPostMaintenanceSubmit={handlePostMaintenanceSubmit}
        onPostMaintenanceCancel={() => setShowPostMaintenanceModal(false)}
        // Common
        employeeOptions={employeeOptions}
        officeOptions={officeOptions}
        conditionOptions={conditionOptions}
        maintenanceTypeOptions={maintenanceTypeOptions}
        filteredAssetOptions={filteredAssetOptions}
        pairedTransactionOptions={pairedTransactionOptions}
        loadingPairedOptions={loadingPairedOptions}
        showEmployee={showEmployee}
        showOffice={showOffice}
        showPaired={showPaired}
        showMaintenance={showMaintenance}
        showExpectedReturn={showExpectedReturn}
        showFileUploads={showFileUploads}
        currentTransactionId={currentTransactionId}
        existingAttachments={existingAttachments}
        attachmentFiles={attachmentFiles}
        handleFilesChange={handleFilesChange}
        handleUploadComplete={handleUploadComplete}
        handleFileDeleted={handleFileDeleted}
      />

      <ImportModal
        isOpen={showImportModal}
        onClose={() => { setShowImportModal(false); setImportResult(null); }}
        onImport={handleImport}
        onDownloadTemplate={handleDownloadTemplate}
        isImporting={isImporting}
        importResult={importResult}
        title="Import Transactions"
        description="Upload Excel file. Transactions will be imported as PENDING."
      />
    </div>
  );
};

export default AssetTransactions;