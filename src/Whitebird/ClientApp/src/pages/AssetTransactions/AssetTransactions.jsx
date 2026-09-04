// src/pages/AssetTransactions/AssetTransactions.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useQueryClient } from '@tanstack/react-query';
import { transactionSchema, getTransactionDefaultValues } from '../../schemas/transactionSchema';
import { getFieldErrorProps } from '../../core/utils/validation.utils';
import { formatDate, formatDateTime } from '../../core/utils/date.utils';
import { AssetTransactionsData } from '../../core/services';
import Spinner from '../../components/atoms/Spinner/Spinner';
import Button from '../../components/atoms/Button/Button';
import SearchToolbar from '../../components/molecules/SearchToolbar/SearchToolbar';
import Tabs from '../../components/molecules/Tabs/Tabs';
import TransactionFilter from '../../components/molecules/TransactionFilter/TransactionFilter';
import ImportModal from '../../components/molecules/ImportModal/ImportModal';
import { FiUpload, FiPlus, FiCheckSquare } from 'react-icons/fi';
import { TRANSACTION_TYPES, TRANSACTION_TYPES_REQUIRING_PAIR, TRANSACTION_TYPES_REQUIRING_EMPLOYEE, TRANSACTION_TYPES_REQUIRING_OFFICE, TRANSACTION_TYPES_RETURNABLE, isPrimaryTransactionType } from '../../core/constants/transactionTypes';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useReferenceData } from '../../hooks/useReferenceData';
import { useOptions } from '../../hooks/useOptions';
import { getTransactionTypeName } from '../../core/constants/transactionTypes';
import { baseTransactionColumns, getMonitoringColumns, getManagementColumns, getManagementReturnColumns } from '../../core/constants/columns';
import PrimaryTransactionsGrid from './components/PrimaryTransactionsGrid';
import SecondaryTransactionsGrid from './components/SecondaryTransactionsGrid';
import TransactionDialogs from './components/TransactionDialogs';
import './AssetTransactions.scss';

const transactionsData = new AssetTransactionsData();

// ============================================================
// TABS DEFINITION
// ============================================================
const MONITORING_SUB_TABS_1 = [
  { id: 'uncomplete', label: 'Uncomplete' },
  { id: 'complete', label: 'Complete' },
];

const MONITORING_SUB_TABS_2 = [
  { id: 'active-loans', label: 'Active Loans' },
  { id: 'overdue-loans', label: 'Overdue Loans' },
  { id: 'under-maintenance', label: 'Under Maintenance' },
];

const MANAGEMENT_SUB_TABS = [
  { id: 'approved', label: 'Approved' },
  { id: 'pending', label: 'Pending' },
  { id: 'rejected', label: 'Rejected' },
];

const AssetTransactions = ({ initialMainTab = 'monitoring' }) => {
  const [activeMainTab] = useState(initialMainTab);
  const defaultSubTab1 = 'uncomplete';
  const defaultSubTab2 = 'active-loans';
  const defaultSubTabManagement = 'approved';
  
  const [activeSubTab1, setActiveSubTab1] = useState(defaultSubTab1);
  const [activeSubTab2, setActiveSubTab2] = useState(defaultSubTab2);
  const [activeSubTabManagement, setActiveSubTabManagement] = useState(defaultSubTabManagement);
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
  const [showModal, setShowModal] = useState(false);
  const [editingTransaction, setEditingTransaction] = useState(null);

  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirm, confirmDelete } = useSweetAlert();

  const { employees, offices, assets: refAssets, conditions, maintenanceTypes } = useReferenceData();

  // ============================================================
  // BULK SELECTION - HARUS DI ATAS useEffect
  // ============================================================
  const { selectedRowIds: primarySelected, selectionCount: primaryCount, hasSelection: hasPrimary, handleSelectionChange: handlePrimaryChange, clearSelection: clearPrimary, getSelectedIds: getPrimaryIds } =
    useBulkSelection({ idField: 'assetTransactionId' });

  const { selectedRowIds: secondarySelected, selectionCount: secondaryCount, hasSelection: hasSecondary, handleSelectionChange: handleSecondaryChange, clearSelection: clearSecondary, getSelectedIds: getSecondaryIds } =
    useBulkSelection({ idField: 'assetTransactionId' });

  // ============================================================
  // REACT HOOK FORM WITH ZOD
  // ============================================================
  const {
    register,
    handleSubmit: handleFormSubmit,
    formState: { errors: formErrors, isSubmitting },
    setValue,
    reset,
    watch,
  } = useForm({
    resolver: zodResolver(transactionSchema),
    defaultValues: getTransactionDefaultValues(),
  });

  const formData = watch();

  const setFormField = useCallback((field) => (value) => {
    setValue(field, value, { shouldValidate: true });
  }, [setValue]);

  // ============================================================
  // BUILD FILTERS
  // ============================================================
  const buildManagementFilters = useCallback(() => {
    const filters = {};
    if (activeSubTabManagement === 'pending') filters.status = 'pending';
    else if (activeSubTabManagement === 'approved') filters.status = 'approve';
    else if (activeSubTabManagement === 'rejected') filters.status = 'reject';
    if (searchTerm) filters.search = searchTerm;
    if (dateFilterStart) filters.startDate = dateFilterStart;
    if (dateFilterEnd) filters.endDate = dateFilterEnd;
    return filters;
  }, [activeSubTabManagement, searchTerm, dateFilterStart, dateFilterEnd]);

  // ============================================================
  // DATA FETCHING - MONITORING GRID 1 (Uncomplete & Complete)
  // ============================================================
  const fetchMonitoringData1 = useCallback(async (params) => {
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
    if (activeSubTab1 === 'uncomplete') {
      result = await transactionsData.api.getGridUncomplete(requestParams);
    } else {
      result = await transactionsData.api.getGridComplete(requestParams);
    }

    return {
      success: true,
      data: {
        data: result.data?.data || [],
        totalCount: result.data?.totalCount || 0,
      },
    };
  }, [activeSubTab1, searchTerm, dateFilterStart, dateFilterEnd]);

  // ============================================================
  // DATA FETCHING - MONITORING GRID 2 (Active Loans, Overdue, Under Maintenance)
  // ============================================================
  const fetchMonitoringData2 = useCallback(async (params) => {
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
    switch (activeSubTab2) {
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
        result = await transactionsData.api.getActiveLoans(requestParams);
    }

    return {
      success: true,
      data: {
        data: result.data?.data || [],
        totalCount: result.data?.totalCount || 0,
      },
    };
  }, [activeSubTab2, searchTerm, dateFilterStart, dateFilterEnd]);

  // ============================================================
  // DATA FETCHING - MANAGEMENT
  // ============================================================
  const fetchManagementData = useCallback(async (params) => {
    const filters = buildManagementFilters();
    const requestParams = { page: params.page || 1, pageSize: params.pageSize || 10, ...filters, ...params };
    const result = await transactionsData.api.getGridAdd(requestParams);
    return { success: true, data: { data: result.data?.data || [], totalCount: result.data?.totalCount || 0 } };
  }, [buildManagementFilters]);

  const fetchManagementReturnData = useCallback(async (params) => {
    const filters = buildManagementFilters();
    const requestParams = { page: params.page || 1, pageSize: params.pageSize || 10, ...filters, ...params };
    const result = await transactionsData.api.getGridReturn(requestParams);
    return { success: true, data: { data: result.data?.data || [], totalCount: result.data?.totalCount || 0 } };
  }, [buildManagementFilters]);

  // ============================================================
  // GRID DATA
  // ============================================================
  const { data: monitoringData1, totalCount: monitoringTotal1, loading: monitoringLoading1, page: monitoringPage1, setPage: setMonitoringPage1, pageSize: monitoringPageSize1, setPageSize: setMonitoringPageSize1, reload: reloadMonitoring1 } =
    useGridData(['monitoring1', activeSubTab1, searchTerm, dateFilterStart, dateFilterEnd], fetchMonitoringData1);

  const { data: monitoringData2, totalCount: monitoringTotal2, loading: monitoringLoading2, page: monitoringPage2, setPage: setMonitoringPage2, pageSize: monitoringPageSize2, setPageSize: setMonitoringPageSize2, reload: reloadMonitoring2 } =
    useGridData(['monitoring2', activeSubTab2, searchTerm, dateFilterStart, dateFilterEnd], fetchMonitoringData2);

  const { data: managementData, totalCount: managementTotal, loading: managementLoading, page: managementPage, setPage: setManagementPage, pageSize: managementPageSize, setPageSize: setManagementPageSize, reload: reloadManagement } =
    useGridData(['management', activeSubTabManagement, searchTerm, dateFilterStart, dateFilterEnd], fetchManagementData);

  const { data: managementReturnData, totalCount: managementReturnTotal, loading: managementReturnLoading, page: managementReturnPage, setPage: setManagementReturnPage, pageSize: managementReturnPageSize, setPageSize: setManagementReturnPageSize, reload: reloadManagementReturn } =
    useGridData(['management-return', activeSubTabManagement, searchTerm, dateFilterStart, dateFilterEnd], fetchManagementReturnData);

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
                label: `${getTransactionTypeName(t.transactionType)} - ${formatDate(t.transactionDate)} (${t.assetCode || 'N/A'})`,
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
      reloadMonitoring1();
      reloadMonitoring2();
    } else {
      reloadManagement();
      reloadManagementReturn();
    }
    return () => { isMountedRef.current = false; };
  }, [activeMainTab, activeSubTab1, activeSubTab2, activeSubTabManagement, reloadMonitoring1, reloadMonitoring2, reloadManagement, reloadManagementReturn]);

  // ============================================================
  // CRUD OPERATIONS
  // ============================================================
  const handleCreate = useCallback(() => {
    reset(getTransactionDefaultValues());
    setEditingTransaction(null);
    setIsEditMode(false);
    setCurrentTransactionId(null);
    setAttachmentFiles([]);
    setExistingAttachments([]);
    setShowModal(true);
  }, [reset]);

  const handleEdit = useCallback(async (record) => {
    const recordId = record?.assetTransactionId;
    if (!recordId) return;
    
    setIsEditMode(true);
    setCurrentTransactionId(recordId);
    setEditingTransaction(record);
    
    try {
      const result = await transactionsData.fetchById(recordId);
      if (result.success && result.data) {
        const data = result.data;
        const defaults = getTransactionDefaultValues();
        Object.keys(defaults).forEach(key => {
          if (data[key] !== undefined && data[key] !== null) {
            setValue(key, data[key]);
          } else {
            setValue(key, '');
          }
        });
        
        if (data.attachments && Array.isArray(data.attachments)) {
          setExistingAttachments(data.attachments);
        } else {
          setExistingAttachments([]);
        }
        setShowModal(true);
      } else {
        setExistingAttachments([]);
        toast.error('Failed to load transaction data');
      }
    } catch (error) {
      console.warn('Failed to fetch transaction:', error);
      setExistingAttachments([]);
      toast.error('Failed to load transaction data');
    }
  }, [setValue, toast]);

  const handleClose = useCallback(() => {
    if (!isSubmitting) {
      setShowModal(false);
      setIsEditMode(false);
      setCurrentTransactionId(null);
      setEditingTransaction(null);
      reset(getTransactionDefaultValues());
      setAttachmentFiles([]);
      setExistingAttachments([]);
    }
  }, [isSubmitting, reset]);

  const onSubmit = useCallback(async (data) => {
    if (isSubmitting) return;

    const submitData = {
      ...data,
      attachmentFiles: attachmentFiles,
      attachmentIdsToKeep: isEditMode ? existingAttachments.map(a => a.fileAttachmentId) : [],
      replaceAttachments: isEditMode ? true : false,
    };

    try {
      let result;
      if (isEditMode && editingTransaction) {
        result = await transactionsData.updateWithAttachments(editingTransaction.assetTransactionId, submitData);
      } else {
        result = await transactionsData.create(submitData);
      }
      
      if (result.success) {
        setAttachmentFiles([]);
        setExistingAttachments([]);
        setIsEditMode(false);
        setCurrentTransactionId(null);
        setEditingTransaction(null);
        setShowModal(false);
        reset(getTransactionDefaultValues());
        
        queryClient.invalidateQueries({ queryKey: ['transactions'] });
        queryClient.invalidateQueries({ queryKey: ['monitoring'] });
        if (activeMainTab === 'monitoring') {
          reloadMonitoring1();
          reloadMonitoring2();
        } else {
          reloadManagement();
          reloadManagementReturn();
        }
        clearPrimary();
        clearSecondary();
        
        const message = isEditMode && editingTransaction 
          ? 'Transaction updated successfully' 
          : 'Transaction created (Pending approval)';
        toast.success(message);
        return true;
      } else {
        const errorMsg = result.error || result.errors?.join('\n') || 'Failed to save transaction';
        toast.error(errorMsg);
        return false;
      }
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to save transaction';
      toast.error(errorMsg);
      return false;
    }
  }, [isSubmitting, isEditMode, editingTransaction, attachmentFiles, existingAttachments, queryClient, reloadManagement, reloadManagementReturn, reloadMonitoring1, reloadMonitoring2, clearPrimary, clearSecondary, toast, reset, activeMainTab]);

  // ============================================================
  // HANDLERS
  // ============================================================
  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setMonitoringPage1(1);
    setMonitoringPage2(1);
    setManagementPage(1);
    setManagementReturnPage(1);
    clearPrimary();
    clearSecondary();
  }, [setMonitoringPage1, setMonitoringPage2, setManagementPage, setManagementReturnPage, clearPrimary, clearSecondary]);

  const handleDateFilterChange = useCallback((start, end) => {
    setDateFilterStart(start);
    setDateFilterEnd(end);
    setMonitoringPage1(1);
    setMonitoringPage2(1);
    setManagementPage(1);
    setManagementReturnPage(1);
    clearPrimary();
    clearSecondary();
  }, [setMonitoringPage1, setMonitoringPage2, setManagementPage, setManagementReturnPage, clearPrimary, clearSecondary]);

  const handleComplete = useCallback(async (id) => {
    const confirmed = await confirm({ title: 'Complete Transaction', text: 'Are you sure you want to complete this transaction?', confirmButtonText: 'Yes, Complete' });
    if (!confirmed) return;
    const r = await transactionsData.complete(id);
    if (r.success && isMountedRef.current) {
      toast.success('Transaction completed successfully');
      reloadMonitoring1();
      reloadMonitoring2();
      reloadManagement();
      reloadManagementReturn();
    }
  }, [reloadMonitoring1, reloadMonitoring2, reloadManagement, reloadManagementReturn, toast, confirm]);

  const handleUncomplete = useCallback(async (id) => {
    const confirmed = await confirm({ title: 'Uncomplete Transaction', text: 'Are you sure you want to uncomplete this transaction?', icon: 'warning', confirmButtonText: 'Yes, Uncomplete', confirmButtonColor: '#f59e0b' });
    if (!confirmed) return;
    const r = await transactionsData.uncomplete(id);
    if (r.success && isMountedRef.current) {
      toast.success('Transaction uncompleted successfully');
      reloadMonitoring1();
      reloadMonitoring2();
      reloadManagement();
      reloadManagementReturn();
    }
  }, [reloadMonitoring1, reloadMonitoring2, reloadManagement, reloadManagementReturn, toast, confirm]);

  // ============================================================
  // BULK ACTION HANDLERS
  // ============================================================
  const handleBulkApprove = useCallback(async (ids, gridType = 'primary') => {
    if (!ids || ids.length === 0) {
      toast.warning('No transactions selected');
      return;
    }
    const confirmed = await confirm({ 
      title: 'Approve Transactions', 
      text: `Are you sure you want to approve ${ids.length} transaction(s)?`, 
      confirmButtonText: 'Yes, Approve' 
    });
    if (!confirmed) return;
    const r = await transactionsData.bulkApprove(ids);
    if (r.success && isMountedRef.current) {
      toast.success(`${ids.length} transaction(s) approved`);
      if (gridType === 'primary') clearPrimary();
      else clearSecondary();
      reloadManagement();
      reloadManagementReturn();
      reloadMonitoring1();
      reloadMonitoring2();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring1, reloadMonitoring2, toast, confirm, clearPrimary, clearSecondary]);

  const handleBulkReject = useCallback(async (ids, gridType = 'primary') => {
    if (!ids || ids.length === 0) {
      toast.warning('No transactions selected');
      return;
    }
    const confirmed = await confirm({ 
      title: 'Reject Transactions', 
      text: `Are you sure you want to reject ${ids.length} transaction(s)?`, 
      icon: 'warning', 
      confirmButtonText: 'Yes, Reject', 
      confirmButtonColor: '#ef4444' 
    });
    if (!confirmed) return;
    const r = await transactionsData.bulkReject(ids);
    if (r.success && isMountedRef.current) {
      toast.success(`${ids.length} transaction(s) rejected`);
      if (gridType === 'primary') clearPrimary();
      else clearSecondary();
      reloadManagement();
      reloadManagementReturn();
      reloadMonitoring1();
      reloadMonitoring2();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring1, reloadMonitoring2, toast, confirm, clearPrimary, clearSecondary]);

  const handleBulkPending = useCallback(async (ids, gridType = 'primary') => {
    if (!ids || ids.length === 0) {
      toast.warning('No transactions selected');
      return;
    }
    const confirmed = await confirm({ 
      title: 'Move to Pending', 
      text: `Are you sure you want to move ${ids.length} transaction(s) back to pending?`, 
      confirmButtonText: 'Yes, Revert' 
    });
    if (!confirmed) return;
    const r = await transactionsData.bulkPending(ids);
    if (r.success && isMountedRef.current) {
      toast.success(`${ids.length} transaction(s) moved to pending`);
      if (gridType === 'primary') clearPrimary();
      else clearSecondary();
      reloadManagement();
      reloadManagementReturn();
      reloadMonitoring1();
      reloadMonitoring2();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring1, reloadMonitoring2, toast, confirm, clearPrimary, clearSecondary]);

  // ============================================================
  // SHORTCUT HANDLERS
  // ============================================================
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
      reloadMonitoring1();
      reloadMonitoring2();
      reloadManagement();
      reloadManagementReturn();
      clearPrimary();
      clearSecondary();
    }
  }, [selectedTransaction, isReturnSubmitting, returnData, reloadMonitoring1, reloadMonitoring2, reloadManagement, reloadManagementReturn, clearPrimary, clearSecondary]);

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
      reloadMonitoring1();
      reloadMonitoring2();
      reloadManagement();
      reloadManagementReturn();
      clearPrimary();
      clearSecondary();
    }
  }, [selectedTransaction, isPostMaintenanceSubmitting, postMaintenanceData, reloadMonitoring1, reloadMonitoring2, reloadManagement, reloadManagementReturn, clearPrimary, clearSecondary]);

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
      reloadMonitoring1();
      reloadMonitoring2();
      clearPrimary();
      clearSecondary();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring1, reloadMonitoring2, toast, clearPrimary, clearSecondary]);

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

  // ============================================================
  // GRID ACTIONS
  // ============================================================
  const getMonitoringActions = useCallback((row) => {
    const actions = [];
    const isPending = row.approved === null;
    const isApproved = row.approved === true;
    const isCompleted = row.completed === true;

    actions.push(ACTION_TYPES.VIEW);
    if (isPending && activeSubTab1 === 'uncomplete') {
      actions.push(ACTION_TYPES.EDIT);
      actions.push(ACTION_TYPES.DELETE);
    }
    if (isApproved && !isCompleted && activeSubTab1 === 'uncomplete') {
      actions.push('complete');
    }
    if (isCompleted && activeSubTab1 === 'uncomplete') {
      actions.push('uncomplete');
    }
    return actions;
  }, [activeSubTab1]);

  // ============================================================
  // VIEW HANDLER - BUKAN ALERT, TAPI BUKA MODAL VIEW
  // ============================================================
  const handleView = useCallback((row) => {
    // View mode: buka modal dengan field disabled
    handleEdit(row);
    // Set view mode - tidak ada tombol submit/update
    // Kita akan handle di TransactionDialogs
  }, [handleEdit]);

  const handleMonitoringAction = useCallback((actionType, row) => {
    if (actionType === ACTION_TYPES.VIEW) {
      handleView(row);
      return;
    }
    if (actionType === ACTION_TYPES.EDIT) handleEdit(row);
    else if (actionType === ACTION_TYPES.DELETE) {
      confirmDelete('Cancel Transaction', `Cancel transaction for ${row.assetCode}?`).then(confirmed => {
        if (confirmed) transactionsData.delete(row.assetTransactionId).then(() => {
          toast.success('Transaction cancelled');
          reloadMonitoring1();
          reloadMonitoring2();
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
  }, [handleView, handleEdit, handleComplete, handleUncomplete, toast, reloadMonitoring1, reloadMonitoring2, reloadManagement, reloadManagementReturn, clearPrimary, clearSecondary, confirmDelete]);

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
      handleView(row);
      return;
    }
    if (actionType === ACTION_TYPES.EDIT) handleEdit(row);
    else if (actionType === ACTION_TYPES.DELETE) {
      confirmDelete('Cancel Transaction', `Cancel transaction for ${row.assetCode}?`).then(confirmed => {
        if (confirmed) transactionsData.delete(row.assetTransactionId).then(() => {
          toast.success('Transaction cancelled');
          reloadManagement();
          reloadManagementReturn();
          reloadMonitoring1();
          reloadMonitoring2();
          clearPrimary();
          clearSecondary();
        });
      });
    } else if (actionType === ACTION_TYPES.RETURN) handleReturnShortcut(row);
    else if (actionType === ACTION_TYPES.POST_MAINTENANCE) handlePostMaintenanceShortcut(row);
  }, [handleView, handleEdit, confirmDelete, toast, reloadManagement, reloadManagementReturn, reloadMonitoring1, reloadMonitoring2, clearPrimary, clearSecondary, handleReturnShortcut, handlePostMaintenanceShortcut]);

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
      handleView(row);
      return;
    }
    if (actionType === ACTION_TYPES.EDIT) handleEdit(row);
    else if (actionType === ACTION_TYPES.DELETE) {
      confirmDelete('Cancel Transaction', `Cancel transaction for ${row.assetCode}?`).then(confirmed => {
        if (confirmed) transactionsData.delete(row.assetTransactionId).then(() => {
          toast.success('Transaction cancelled');
          reloadManagement();
          reloadManagementReturn();
          reloadMonitoring1();
          reloadMonitoring2();
          clearPrimary();
          clearSecondary();
        });
      });
    }
  }, [handleView, handleEdit, confirmDelete, toast, reloadManagement, reloadManagementReturn, reloadMonitoring1, reloadMonitoring2, clearPrimary, clearSecondary]);

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
  const monitoringColumns1 = useMemo(() => {
    return getMonitoringColumns(monitoringActionColumn, activeSubTab1);
  }, [activeSubTab1, monitoringActionColumn]);

  const monitoringColumns2 = useMemo(() => {
    // Untuk grid 2, kita pakai columns tanpa action yang tidak relevan
    // Tapi tetap pakai monitoringActionColumn
    return getMonitoringColumns(monitoringActionColumn, activeSubTab2);
  }, [activeSubTab2, monitoringActionColumn]);

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
  const isManagement = activeMainTab === 'management';
  const showEmployee = TRANSACTION_TYPES_REQUIRING_EMPLOYEE.includes(parseInt(formData.transactionType));
  const showOffice = TRANSACTION_TYPES_REQUIRING_OFFICE.includes(parseInt(formData.transactionType));
  const showPaired = TRANSACTION_TYPES_REQUIRING_PAIR.includes(parseInt(formData.transactionType));
  const showMaintenance = parseInt(formData.transactionType) === TRANSACTION_TYPES.MAINTENANCE;
  const showExpectedReturn = parseInt(formData.transactionType) === TRANSACTION_TYPES.LOAN;
  const showCheckbox = activeSubTabManagement === 'pending' || activeSubTabManagement === 'approved';
  const showFileUploads = isEditMode && currentTransactionId;

  // ============================================================
  // BULK BUTTON RENDER
  // ============================================================
  const renderBulkButton = (ids, gridType) => {
    if (!ids || ids.length === 0) return null;
    
    if (activeSubTabManagement === 'pending') {
      return (
        <>
          <Button variant="primary" size="sm" onClick={() => handleBulkApprove(ids, gridType)} className="u-inline-flex u-btn-gap">
            <FiCheckSquare size={16} /> Approve ({ids.length})
          </Button>
          <Button variant="danger" size="sm" onClick={() => handleBulkReject(ids, gridType)} className="u-inline-flex u-btn-gap">
            <FiCheckSquare size={16} /> Reject ({ids.length})
          </Button>
        </>
      );
    }
    
    if (activeSubTabManagement === 'approved') {
      return (
        <Button variant="warning" size="sm" onClick={() => handleBulkPending(ids, gridType)} className="u-inline-flex u-btn-gap">
          <FiCheckSquare size={16} /> Pending ({ids.length})
        </Button>
      );
    }
    
    return null;
  };

  // ============================================================
  // HEADER ACTIONS
  // ============================================================
  const headerActions = (
    <>
      {isManagement && (
        <>
          <Button variant="outline" size="sm" onClick={() => setShowImportModal(true)} className="u-inline-flex u-btn-gap">
            <FiUpload size={16} /> Import
          </Button>
          <Button variant="primary" size="sm" onClick={handleCreate} className="u-inline-flex u-btn-gap">
            <FiPlus size={16} /> New Transaction
          </Button>
        </>
      )}
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

      {isMonitoring && (
        <>
          {/* GRID 1 - Uncomplete & Complete */}
          <div className="transactions-menu__grid-section">
            <Tabs tabs={MONITORING_SUB_TABS_1} activeTab={activeSubTab1} onTabChange={setActiveSubTab1} />
            <PrimaryTransactionsGrid
              data={monitoringData1}
              loading={monitoringLoading1}
              totalCount={monitoringTotal1}
              page={monitoringPage1}
              pageSize={monitoringPageSize1}
              onPageChange={setMonitoringPage1}
              onPageSizeChange={setMonitoringPageSize1}
              columns={monitoringColumns1}
              checkboxSelection={false}
              getRowId={(row) => row?.assetTransactionId || `mon1-${Math.random()}`}
            />
          </div>

          <div className="transactions-menu__grid-section">
            <Tabs tabs={MONITORING_SUB_TABS_2} activeTab={activeSubTab2} onTabChange={setActiveSubTab2} />
            <PrimaryTransactionsGrid
              data={monitoringData2}
              loading={monitoringLoading2}
              totalCount={monitoringTotal2}
              page={monitoringPage2}
              pageSize={monitoringPageSize2}
              onPageChange={setMonitoringPage2}
              onPageSizeChange={setMonitoringPageSize2}
              columns={monitoringColumns2}
              checkboxSelection={false}
              getRowId={(row) => row?.assetTransactionId || `mon2-${Math.random()}`}
            />
          </div>
        </>
      )}

      {isManagement && (
        <>
          <Tabs tabs={MANAGEMENT_SUB_TABS} activeTab={activeSubTabManagement} onTabChange={setActiveSubTabManagement} />

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
            bulkActions={renderBulkButton(getPrimaryIds(managementData), 'primary')}
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
            bulkActions={renderBulkButton(getSecondaryIds(managementReturnData), 'secondary')}
          />
        </>
      )}

      <TransactionDialogs
        showModal={showModal}
        editingTransaction={editingTransaction}
        isSubmitting={isSubmitting}
        formData={formData}
        setFormField={setFormField}
        onSubmit={handleFormSubmit(onSubmit)}
        onClose={handleClose}
        isViewMode={editingTransaction !== null} // View mode jika editingTransaction ada
        showReturnModal={showReturnModal}
        isReturnSubmitting={isReturnSubmitting}
        returnData={returnData}
        setReturnData={setReturnData}
        selectedTransaction={selectedTransaction}
        onReturnSubmit={handleReturnSubmit}
        onReturnCancel={() => setShowReturnModal(false)}
        showPostMaintenanceModal={showPostMaintenanceModal}
        isPostMaintenanceSubmitting={isPostMaintenanceSubmitting}
        postMaintenanceData={postMaintenanceData}
        setPostMaintenanceData={setPostMaintenanceData}
        onPostMaintenanceSubmit={handlePostMaintenanceSubmit}
        onPostMaintenanceCancel={() => setShowPostMaintenanceModal(false)}
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