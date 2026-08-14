// src/pages/AssetTransactions/AssetTransactions.menu.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { Grid, Chip, Box, Typography } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import AssetTransactionsData from './AssetTransactions.data';
import DataTable from '../../components/molecules/DataTable/DataTable';
import CrudModal from '../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../components/atoms/FormSection/FormSection';
import Input from '../../components/atoms/Input/Input';
import Select from '../../components/atoms/Select/Select';
import DatePickerInput from '../../components/atoms/Input/DatePickerInput';
import NumberInput from '../../components/atoms/Input/NumberInput';
import Spinner from '../../components/atoms/Spinner/Spinner';
import FileUploader from '../../components/molecules/FileUploader/FileUploader';
import ImportModal from '../../components/molecules/ImportModal/ImportModal';
import Modal from '../../components/molecules/Modal/Modal';
import Button from '../../components/atoms/Button/Button';
import Card from '../../components/atoms/Card/Card';
import SearchToolbar from '../../components/molecules/SearchToolbar/SearchToolbar';
import Tabs from '../../components/molecules/Tabs/Tabs';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import TransactionFilter from '../../components/molecules/TransactionFilter/TransactionFilter';
import ReturnModalContent from '../../components/molecules/ReturnModalContent/ReturnModalContent';
import PostMaintenanceModalContent from '../../components/molecules/PostMaintenanceModalContent/PostMaintenanceModalContent';
import { FiUpload, FiCheckSquare, FiPlus, FiCheck, FiX, FiRotateCcw } from 'react-icons/fi';
import {
  TRANSACTION_TYPE_OPTIONS,
  TRANSACTION_TYPES,
  TRANSACTION_TYPES_REQUIRING_PAIR,
  TRANSACTION_TYPES_REQUIRING_EMPLOYEE,
  TRANSACTION_TYPES_REQUIRING_OFFICE,
  TRANSACTION_TYPES_RETURNABLE,
  isPrimaryTransactionType,
  getTransactionTypeName,
  getTransactionTypeColor,
} from '../../core/constants/transactionTypes';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useReferenceData } from '../../hooks/useReferenceData';
import { useCrudFormBase } from '../../hooks/useCrudFormBase';
import { useOptions } from '../../hooks/useOptions';
import { cleanTransactionFormData } from '../../core/utils/formHelpers';
import utilsHelper from '../../core/utils/utils.helper';
import './AssetTransactions.scss';

const transactionsData = new AssetTransactionsData();
transactionsData.transformFormData = cleanTransactionFormData;

// FIX: Hanya sub-tabs berdasarkan main tab
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

const AssetTransactionsMenu = ({ initialMainTab = 'monitoring' }) => {
  // Gunakan initialMainTab dari props, tanpa tab utama
  const [activeMainTab] = useState(initialMainTab);
  // Default sub tab berdasarkan main tab
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
    },
  });

  const handleEdit = useCallback(async (record) => {
    const recordId = record?.assetTransactionId;
    if (!recordId) return;
    setIsEditMode(true);
    setCurrentTransactionId(recordId);
    try {
      const response = await apiClient.transaction.getById(recordId);
      const data = response.data?.data || response.data || {};
      if (data.attachments && Array.isArray(data.attachments)) {
        setExistingAttachments(data.attachments);
      } else {
        setExistingAttachments([]);
      }
    } catch (error) {
      console.warn('Failed to fetch attachments:', error);
      setExistingAttachments([]);
    }
    handleEditBase(record);
  }, [handleEditBase]);

  const {
    selectedRowIds: selectedPrimaryIds,
    selectionCount: primarySelectionCount,
    hasSelection: hasPrimarySelection,
    handleSelectionChange: handlePrimarySelectionChange,
    clearSelection: clearPrimarySelection,
    getSelectedIds: getPrimarySelectedIds,
  } = useBulkSelection({ idField: 'assetTransactionId' });

  const {
    selectedRowIds: selectedSecondaryIds,
    selectionCount: secondarySelectionCount,
    hasSelection: hasSecondarySelection,
    handleSelectionChange: handleSecondarySelectionChange,
    clearSelection: clearSecondarySelection,
    getSelectedIds: getSecondarySelectedIds,
  } = useBulkSelection({ idField: 'assetTransactionId' });

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

  const {
    data: monitoringData,
    totalCount: monitoringTotal,
    loading: monitoringLoading,
    page: monitoringPage,
    setPage: setMonitoringPage,
    pageSize: monitoringPageSize,
    setPageSize: setMonitoringPageSize,
    reload: reloadMonitoring,
  } = useGridData(['monitoring', activeSubTab, searchTerm, dateFilterStart, dateFilterEnd], fetchMonitoringData);

  const buildManagementFilters = useCallback(() => {
    const filters = {};
    if (activeSubTab === 'pending') {
      filters.status = 'pending';
    } else if (activeSubTab === 'approved') {
      filters.status = 'approve';
    } else if (activeSubTab === 'rejected') {
      filters.status = 'reject';
    }
    if (searchTerm) filters.search = searchTerm;
    if (dateFilterStart) filters.startDate = dateFilterStart;
    if (dateFilterEnd) filters.endDate = dateFilterEnd;
    return filters;
  }, [activeSubTab, searchTerm, dateFilterStart, dateFilterEnd]);

  const fetchManagementData = useCallback(
    async (params) => {
      const filters = buildManagementFilters();
      const requestParams = {
        page: params.page || 1,
        pageSize: params.pageSize || 10,
        ...filters,
        ...params,
      };
      const result = await transactionsData.api.getGridAdd(requestParams);
      return {
        success: true,
        data: {
          data: result.data?.data || [],
          totalCount: result.data?.totalCount || 0,
        },
      };
    },
    [buildManagementFilters]
  );

  const {
    data: managementData,
    totalCount: managementTotal,
    loading: managementLoading,
    page: managementPage,
    setPage: setManagementPage,
    pageSize: managementPageSize,
    setPageSize: setManagementPageSize,
    reload: reloadManagement,
  } = useGridData(['management', activeSubTab, searchTerm, dateFilterStart, dateFilterEnd], fetchManagementData);

  const fetchManagementReturnData = useCallback(
    async (params) => {
      const filters = buildManagementFilters();
      const requestParams = {
        page: params.page || 1,
        pageSize: params.pageSize || 10,
        ...filters,
        ...params,
      };
      const result = await transactionsData.api.getGridReturn(requestParams);
      return {
        success: true,
        data: {
          data: result.data?.data || [],
          totalCount: result.data?.totalCount || 0,
        },
      };
    },
    [buildManagementFilters]
  );

  const {
    data: managementReturnData,
    totalCount: managementReturnTotal,
    loading: managementReturnLoading,
    page: managementReturnPage,
    setPage: setManagementReturnPage,
    pageSize: managementReturnPageSize,
    setPageSize: setManagementReturnPageSize,
    reload: reloadManagementReturn,
  } = useGridData(['management-return', activeSubTab, searchTerm, dateFilterStart, dateFilterEnd], fetchManagementReturnData);

  useEffect(() => {
    isMountedRef.current = true;
    clearPrimarySelection();
    clearSecondarySelection();
    if (activeMainTab === 'monitoring') {
      reloadMonitoring();
    } else {
      reloadManagement();
      reloadManagementReturn();
    }
    return () => { isMountedRef.current = false; };
  }, [activeMainTab, activeSubTab, reloadMonitoring, reloadManagement, reloadManagementReturn]);

  useEffect(() => {
    const type = parseInt(formData.transactionType);
    if (formData.assetId && TRANSACTION_TYPES_REQUIRING_PAIR.includes(type)) {
      setLoadingPairedOptions(true);
      (async () => {
        try {
          const res = await transactionsData.api.getByAssetId(formData.assetId);
          const allTxns = res.data?.data || res.data || [];
          const sourceType = type === TRANSACTION_TYPES.LOAN_RETURN ? TRANSACTION_TYPES.LOAN : TRANSACTION_TYPES.MAINTENANCE;
          const validPairs = allTxns.filter(
            t => t.transactionType === sourceType && t.approved === true && t.fromAssetTransactionId === null
          );
          if (isMountedRef.current) {
            setPairedTransactionOptions(
              validPairs.map(t => ({
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

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setMonitoringPage(1);
    setManagementPage(1);
    setManagementReturnPage(1);
    clearPrimarySelection();
    clearSecondarySelection();
  }, [setMonitoringPage, setManagementPage, setManagementReturnPage, clearPrimarySelection, clearSecondarySelection]);

  const handleDateFilterChange = useCallback((start, end) => {
    setDateFilterStart(start);
    setDateFilterEnd(end);
    setMonitoringPage(1);
    setManagementPage(1);
    setManagementReturnPage(1);
    clearPrimarySelection();
    clearSecondarySelection();
  }, [setMonitoringPage, setManagementPage, setManagementReturnPage, clearPrimarySelection, clearSecondarySelection]);

  const handleComplete = useCallback(async (id) => {
    const confirmed = await confirm({
      title: 'Complete Transaction',
      text: 'Are you sure you want to complete this transaction? All required documents must be uploaded and approved.',
      confirmButtonText: 'Yes, Complete',
    });
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
    const confirmed = await confirm({
      title: 'Uncomplete Transaction',
      text: 'Are you sure you want to uncomplete this transaction? This will revert it back to approved status.',
      icon: 'warning',
      confirmButtonText: 'Yes, Uncomplete',
      confirmButtonColor: '#f59e0b',
    });
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
    const confirmed = await confirm({
      title: 'Approve Transactions',
      text: `Are you sure you want to approve ${ids.length} transaction(s)?`,
      confirmButtonText: 'Yes, Approve',
    });
    if (!confirmed) return;
    const r = await transactionsData.bulkApprove(ids);
    if (r.success && isMountedRef.current) {
      toast.success(`${ids.length} transaction(s) approved`);
      if (gridType === 'primary') clearPrimarySelection();
      else clearSecondarySelection();
      reloadManagement();
      reloadManagementReturn();
      reloadMonitoring();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring, toast, confirm, clearPrimarySelection, clearSecondarySelection]);

  const handleBulkReject = useCallback(async (ids, gridType = 'primary') => {
    const confirmed = await confirm({
      title: 'Reject Transactions',
      text: `Are you sure you want to reject ${ids.length} transaction(s)?`,
      icon: 'warning',
      confirmButtonText: 'Yes, Reject',
      confirmButtonColor: '#ef4444',
    });
    if (!confirmed) return;
    const r = await transactionsData.bulkReject(ids);
    if (r.success && isMountedRef.current) {
      toast.success(`${ids.length} transaction(s) rejected`);
      if (gridType === 'primary') clearPrimarySelection();
      else clearSecondarySelection();
      reloadManagement();
      reloadManagementReturn();
      reloadMonitoring();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring, toast, confirm, clearPrimarySelection, clearSecondarySelection]);

  const handleBulkPending = useCallback(async (ids, gridType = 'primary') => {
    const confirmed = await confirm({
      title: 'Move to Pending',
      text: `Are you sure you want to move ${ids.length} transaction(s) back to pending?`,
      confirmButtonText: 'Yes, Revert',
    });
    if (!confirmed) return;
    const r = await transactionsData.bulkPending(ids);
    if (r.success && isMountedRef.current) {
      toast.success(`${ids.length} transaction(s) moved to pending`);
      if (gridType === 'primary') clearPrimarySelection();
      else clearSecondarySelection();
      reloadManagement();
      reloadManagementReturn();
      reloadMonitoring();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring, toast, confirm, clearPrimarySelection, clearSecondarySelection]);

  const handleReturnShortcut = useCallback((tx) => {
    setSelectedTransaction(tx);
    setReturnData({
      actualReturnDate: new Date().toISOString().split('T')[0],
      condition: '',
      notes: '',
    });
    setShowReturnModal(true);
  }, []);

  const handlePostMaintenanceShortcut = useCallback((tx) => {
    setSelectedTransaction(tx);
    setPostMaintenanceData({
      completionDate: new Date().toISOString().split('T')[0],
      condition: '',
      maintenanceCost: '',
      notes: '',
    });
    setShowPostMaintenanceModal(true);
  }, []);

  const handleReturnSubmit = useCallback(async () => {
    if (!selectedTransaction || isReturnSubmitting) return;
    setIsReturnSubmitting(true);
    const r = await transactionsData.createReturn(selectedTransaction.assetTransactionId, {
      actualReturnDate: returnData.actualReturnDate,
      condition: returnData.condition,
      notes: returnData.notes,
    });
    setIsReturnSubmitting(false);
    if (r.success && isMountedRef.current) {
      setShowReturnModal(false);
      reloadMonitoring();
      reloadManagement();
      reloadManagementReturn();
      clearPrimarySelection();
      clearSecondarySelection();
    }
  }, [selectedTransaction, isReturnSubmitting, returnData, reloadMonitoring, reloadManagement, reloadManagementReturn, clearPrimarySelection, clearSecondarySelection]);

  const handlePostMaintenanceSubmit = useCallback(async () => {
    if (!selectedTransaction || isPostMaintenanceSubmitting) return;
    setIsPostMaintenanceSubmitting(true);
    const r = await transactionsData.createPostMaintenance(selectedTransaction.assetTransactionId, {
      completionDate: postMaintenanceData.completionDate,
      condition: postMaintenanceData.condition,
      maintenanceCost: postMaintenanceData.maintenanceCost,
      notes: postMaintenanceData.notes,
    });
    setIsPostMaintenanceSubmitting(false);
    if (r.success && isMountedRef.current) {
      setShowPostMaintenanceModal(false);
      reloadMonitoring();
      reloadManagement();
      reloadManagementReturn();
      clearPrimarySelection();
      clearSecondarySelection();
    }
  }, [selectedTransaction, isPostMaintenanceSubmitting, postMaintenanceData, reloadMonitoring, reloadManagement, reloadManagementReturn, clearPrimarySelection, clearSecondarySelection]);

  const handleFilesChange = useCallback((files) => {
    setAttachmentFiles(files.filter(f => f.isNew));
    const existing = files.filter(f => !f.isNew);
    if (existing.length > 0) {
      setExistingAttachments(existing);
    }
  }, []);

  const handleUploadComplete = useCallback((newFiles) => {
    console.log('Files added for upload:', newFiles);
  }, []);

  // ============================================================
  // FIX: DEFINISI handleImport DAN handleDownloadTemplate
  // ============================================================
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
      clearPrimarySelection();
      clearSecondarySelection();
    }
  }, [reloadManagement, reloadManagementReturn, reloadMonitoring, toast, clearPrimarySelection, clearSecondarySelection]);

  const handleDownloadTemplate = useCallback(async () => {
    await transactionsData.downloadTemplate();
  }, []);

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
        clearPrimarySelection();
        clearSecondarySelection();
        toast.success('Transaction updated successfully');
        return true;
      }
      return false;
    } else {
      const success = await crudHandleSubmit();
      if (success && isMountedRef.current) {
        toast.success('Transaction created (Pending approval)');
        reloadManagement();
        reloadManagementReturn();
        reloadMonitoring();
        clearPrimarySelection();
        clearSecondarySelection();
        setAttachmentFiles([]);
        setExistingAttachments([]);
        setIsEditMode(false);
        setCurrentTransactionId(null);
      }
      return success;
    }
  }, [
    crudHandleSubmit,
    reloadManagement,
    reloadManagementReturn,
    reloadMonitoring,
    toast,
    editingTransaction,
    clearPrimarySelection,
    clearSecondarySelection,
    isEditMode,
    attachmentFiles,
    existingAttachments,
    formData,
    handleClose,
    queryClient
  ]);

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
      handleEdit(row);
      toast.info(`Viewing transaction ${row.assetTransactionCode || row.assetTransactionId}`);
      return;
    }
    if (actionType === ACTION_TYPES.EDIT) {
      handleEdit(row);
    } else if (actionType === ACTION_TYPES.DELETE) {
      confirmDelete('Cancel Transaction', `Cancel transaction for ${row.assetCode}?`).then(confirmed => {
        if (confirmed) transactionsData.delete(row.assetTransactionId).then(() => {
          toast.success('Transaction cancelled');
          reloadMonitoring();
          reloadManagement();
          reloadManagementReturn();
          clearPrimarySelection();
          clearSecondarySelection();
        });
      });
    } else if (actionType === 'complete') {
      handleComplete(row.assetTransactionId);
    } else if (actionType === 'uncomplete') {
      handleUncomplete(row.assetTransactionId);
    }
  }, [handleEdit, handleComplete, handleUncomplete, toast, reloadMonitoring, reloadManagement, reloadManagementReturn, clearPrimarySelection, clearSecondarySelection, confirmDelete]);

  const { actionColumn: monitoringActionColumn } = useGridActions({
    actions: [ACTION_TYPES.VIEW, ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleMonitoringAction,
    getConditionalActions: getMonitoringActions,
    rowIdField: 'assetTransactionId',
  });

  const getManagementActions = useCallback((row) => {
    const isPending = row.approved === null;
    const isApproved = row.approved === true;
    const isPrimary = isPrimaryTransactionType(row.transactionType);
    const hasPair = row.fromAssetTransactionId !== null && row.fromAssetTransactionId !== undefined;

    const actions = [];
    actions.push(ACTION_TYPES.VIEW);

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
          clearPrimarySelection();
          clearSecondarySelection();
        });
      });
    } else if (actionType === ACTION_TYPES.RETURN) handleReturnShortcut(row);
    else if (actionType === ACTION_TYPES.POST_MAINTENANCE) handlePostMaintenanceShortcut(row);
  }, [handleEdit, confirmDelete, toast, reloadManagement, reloadManagementReturn, reloadMonitoring, clearPrimarySelection, clearSecondarySelection, handleReturnShortcut, handlePostMaintenanceShortcut]);

  const { actionColumn: managementActionColumn } = useGridActions({
    actions: [ACTION_TYPES.VIEW, ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleManagementAction,
    getConditionalActions: getManagementActions,
    rowIdField: 'assetTransactionId',
  });

  const getManagementReturnActions = useCallback((row) => {
    const isPending = row.approved === null;
    const actions = [];
    actions.push(ACTION_TYPES.VIEW);
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
          clearPrimarySelection();
          clearSecondarySelection();
        });
      });
    }
  }, [handleEdit, confirmDelete, toast, reloadManagement, reloadManagementReturn, reloadMonitoring, clearPrimarySelection, clearSecondarySelection]);

  const { actionColumn: managementReturnActionColumn } = useGridActions({
    actions: [ACTION_TYPES.VIEW, ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleManagementReturnAction,
    getConditionalActions: getManagementReturnActions,
    rowIdField: 'assetTransactionId',
  });

  const baseColumns = [
    {
      field: 'transactionDate',
      headerName: 'Date',
      width: 180,
      renderCell: (p) => {
        const val = p?.row?.transactionDate ?? p?.value;
        return val ? <span>{utilsHelper.formatDateTime(val)}</span> : <span className="u-text-muted">-</span>;
      },
    },
    {
      field: 'assetCode',
      headerName: 'Asset Code',
      width: 120,
      renderCell: (p) => <span>{p?.row?.assetCode ?? p?.value ?? '-'}</span>,
    },
    {
      field: 'assetName',
      headerName: 'Asset Name',
      flex: 1,
      minWidth: 160,
      renderCell: (p) => <span>{p?.row?.assetName ?? p?.value ?? '-'}</span>,
    },
    {
      field: 'transactionTypeName',
      headerName: 'Type',
      width: 150,
      renderCell: (p) => {
        const name = p?.row?.transactionTypeName ?? p?.value ?? '';
        const colors = getTransactionTypeColor(name);
        return <Chip label={colors.label} size="small" sx={{ bgcolor: colors.bg, color: colors.color, fontWeight: 500 }} />;
      },
    },
    {
      field: 'employeeName',
      headerName: 'Employee',
      width: 150,
      renderCell: (p) => <span>{p?.row?.employeeName ?? p?.value ?? '-'}</span>,
    },
  ];

  const monitoringColumns = useMemo(() => {
    const statusColumns = [
      {
        field: 'approved',
        headerName: 'Status',
        width: 120,
        renderCell: (p) => {
          const row = p?.row || {};
          let status = 'Pending';
          if (row.approved === true) status = 'Approved';
          if (row.approved === false) status = 'Rejected';
          if (row.completed === true) status = 'Completed';
          return <StatusChip status={status} />;
        },
      },
    ];

    if (activeSubTab === 'uncomplete' || activeSubTab === 'complete') {
      return [
        ...baseColumns,
        {
          field: 'conditionName',
          headerName: 'Condition',
          width: 120,
          renderCell: (p) => <span>{p?.row?.conditionName ?? p?.value ?? '-'}</span>,
        },
        ...statusColumns,
        ...(activeSubTab === 'uncomplete' ? [
          {
            field: 'hasNeedToChange',
            headerName: 'Need Revision',
            width: 130,
            renderCell: (p) => {
              const hasNeedToChange = p?.row?.hasNeedToChange || false;
              return hasNeedToChange ? <Chip label="⚠️ Need Revision" size="small" color="warning" /> : <Chip label="✓ OK" size="small" color="success" />;
            },
          },
          {
            field: 'allAttachmentsApproved',
            headerName: 'Docs Approved',
            width: 130,
            renderCell: (p) => {
              const allApproved = p?.row?.allAttachmentsApproved || false;
              return allApproved ? <Chip label="✓ All Approved" size="small" color="success" /> : <Chip label="⏳ Pending" size="small" color="warning" />;
            },
          },
        ] : []),
        monitoringActionColumn,
      ];
    }

    return [
      ...baseColumns,
      {
        field: 'conditionName',
        headerName: 'Condition',
        width: 120,
        renderCell: (p) => <span>{p?.row?.conditionName ?? p?.value ?? '-'}</span>,
      },
      ...statusColumns,
      {
        field: 'expectedLoanReturnDate',
        headerName: 'Exp. Return',
        width: 130,
        renderCell: (p) => {
          const val = p?.row?.expectedLoanReturnDate ?? p?.value;
          return val ? <span>{utilsHelper.formatDate(val)}</span> : <span className="u-text-muted">-</span>;
        },
      },
    ];
  }, [activeSubTab, baseColumns, monitoringActionColumn]);

  const managementColumns = useMemo(() => {
    return [
      ...baseColumns,
      {
        field: 'officeName',
        headerName: 'Office',
        width: 150,
        renderCell: (p) => <span>{p?.row?.officeName ?? p?.value ?? '-'}</span>,
      },
      {
        field: 'approved',
        headerName: 'Status',
        width: 120,
        renderCell: (p) => {
          const row = p?.row || {};
          let status = 'Pending';
          if (row.approved === true) status = 'Approved';
          if (row.approved === false) status = 'Rejected';
          return <StatusChip status={status} />;
        },
      },
      {
        field: 'expectedLoanReturnDate',
        headerName: 'Exp. Return',
        width: 130,
        renderCell: (p) => {
          const val = p?.row?.expectedLoanReturnDate ?? p?.value;
          return val ? <span>{utilsHelper.formatDate(val)}</span> : <span className="u-text-muted">-</span>;
        },
      },
      managementActionColumn,
    ];
  }, [baseColumns, managementActionColumn]);

  const managementReturnColumns = useMemo(() => {
    return [
      ...baseColumns,
      {
        field: 'officeName',
        headerName: 'Office',
        width: 150,
        renderCell: (p) => <span>{p?.row?.officeName ?? p?.value ?? '-'}</span>,
      },
      {
        field: 'approved',
        headerName: 'Status',
        width: 120,
        renderCell: (p) => {
          const row = p?.row || {};
          let status = 'Pending';
          if (row.approved === true) status = 'Approved';
          if (row.approved === false) status = 'Rejected';
          return <StatusChip status={status} />;
        },
      },
      managementReturnActionColumn,
    ];
  }, [baseColumns, managementReturnActionColumn]);

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

  const showEmployee = TRANSACTION_TYPES_REQUIRING_EMPLOYEE.includes(parseInt(formData.transactionType));
  const showOffice = TRANSACTION_TYPES_REQUIRING_OFFICE.includes(parseInt(formData.transactionType));
  const showPaired = TRANSACTION_TYPES_REQUIRING_PAIR.includes(parseInt(formData.transactionType));
  const showMaintenance = parseInt(formData.transactionType) === TRANSACTION_TYPES.MAINTENANCE;
  const showExpectedReturn = parseInt(formData.transactionType) === TRANSACTION_TYPES.LOAN;

  const employeeOptions = useOptions(employees, 'Select Employee');
  const officeOptions = useOptions(offices, 'Select Office');
  const conditionOptions = useOptions(conditions, 'Select Condition');
  const maintenanceTypeOptions = useOptions(maintenanceTypes, 'Select Maintenance Type');

  const filteredAssetOptions = useMemo(() => {
    return [{ value: '', label: 'Select Asset' }, ...refAssets.map(a => ({ value: a.value, label: a.label }))];
  }, [refAssets]);

  const isMonitoring = activeMainTab === 'monitoring';

  if (managementLoading && !managementData.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const primaryData = managementData;
  const secondaryData = managementReturnData;

  const showCheckbox = activeSubTab === 'pending' || activeSubTab === 'approved';
  const showFileUploads = isEditMode && currentTransactionId;

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

      {/* Hanya sub-tabs yang sesuai */}
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
        <Card
          className="transactions-menu__grid-card"
          title={
            activeSubTab === 'uncomplete' ? '📋 Uncomplete Transactions' :
            activeSubTab === 'complete' ? '✅ Completed Transactions' :
            activeSubTab === 'active-loans' ? '📌 Active Loans' :
            activeSubTab === 'overdue-loans' ? '⚠️ Overdue Loans' :
            '🔧 Under Maintenance'
          }
          subtitle={
            activeSubTab === 'uncomplete' ? 'Approved transactions pending completion' :
            activeSubTab === 'complete' ? 'Transactions that have been fully completed' :
            activeSubTab === 'active-loans' ? 'Currently active loan transactions' :
            activeSubTab === 'overdue-loans' ? 'Loan transactions past expected return date' :
            'Assets currently under maintenance'
          }
        >
          <DataTable
            rows={monitoringData}
            columns={monitoringColumns}
            loading={monitoringLoading}
            pageSize={monitoringPageSize}
            page={monitoringPage}
            totalRowCount={monitoringTotal}
            onPageChange={setMonitoringPage}
            onPageSizeChange={setMonitoringPageSize}
            getRowId={(row) => row?.assetTransactionId || `mon-${Math.random()}`}
            checkboxSelection={false}
            ariaLabel="Monitoring transactions"
            paginationMode="server"
          />
        </Card>
      )}

      {!isMonitoring && (
        <>
          <Card
            className="transactions-menu__grid-card"
            title="Primary Transactions"
            subtitle="Handover, Transfer, Loan, Maintenance, Disposal, Operational Office"
            actions={renderBulkButton(getPrimarySelectedIds(primaryData), 'primary')}
          >
            <DataTable
              rows={primaryData}
              columns={managementColumns}
              loading={managementLoading}
              pageSize={managementPageSize}
              page={managementPage}
              totalRowCount={managementTotal}
              onPageChange={setManagementPage}
              onPageSizeChange={setManagementPageSize}
              getRowId={(row) => row?.assetTransactionId || `primary-${Math.random()}`}
              checkboxSelection={showCheckbox}
              onRowSelectionModelChange={handlePrimarySelectionChange}
              ariaLabel="Primary transactions"
              paginationMode="server"
            />
          </Card>

          <Card
            className="transactions-menu__grid-card"
            title="Secondary Transactions"
            subtitle="Return, Loan Return, Post Maintenance"
            actions={renderBulkButton(getSecondarySelectedIds(secondaryData), 'secondary')}
          >
            <DataTable
              rows={secondaryData}
              columns={managementReturnColumns}
              loading={managementReturnLoading}
              pageSize={managementReturnPageSize}
              page={managementReturnPage}
              totalRowCount={managementReturnTotal}
              onPageChange={setManagementReturnPage}
              onPageSizeChange={setManagementReturnPageSize}
              getRowId={(row) => row?.assetTransactionId || `secondary-${Math.random()}`}
              checkboxSelection={showCheckbox}
              onRowSelectionModelChange={handleSecondarySelectionChange}
              ariaLabel="Secondary transactions"
              paginationMode="server"
            />
          </Card>
        </>
      )}

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingTransaction ? 'Edit Transaction' : 'New Transaction'}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingTransaction ? 'Update' : 'Create'}
        size="xl"
      >
        <FormSection title="Transaction Details" description="Type, asset, and parties">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Select
                label="Transaction Type"
                value={formData.transactionType || ''}
                onChange={(e) => setFormField('transactionType')(e.target.value)}
                options={TRANSACTION_TYPE_OPTIONS}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select
                label="Asset"
                value={formData.assetId || ''}
                onChange={(e) => setFormField('assetId')(e.target.value)}
                options={filteredAssetOptions}
                required
              />
            </Grid>
            {showEmployee && (
              <Grid item xs={12} sm={6}>
                <Select
                  label="Employee"
                  value={formData.employeeId || ''}
                  onChange={(e) => setFormField('employeeId')(e.target.value)}
                  options={employeeOptions}
                  required
                />
              </Grid>
            )}
            {showOffice && (
              <Grid item xs={12} sm={6}>
                <Select
                  label="Office"
                  value={formData.officeId || ''}
                  onChange={(e) => setFormField('officeId')(e.target.value)}
                  options={officeOptions}
                  required
                />
              </Grid>
            )}
            {showPaired && (
              <Grid item xs={12} sm={6}>
                <Select
                  label="Paired Transaction"
                  value={formData.fromAssetTransactionId || ''}
                  onChange={(e) => setFormField('fromAssetTransactionId')(e.target.value)}
                  options={[{ value: '', label: loadingPairedOptions ? 'Loading...' : 'Select Paired' }, ...pairedTransactionOptions]}
                  required
                  disabled={loadingPairedOptions}
                />
              </Grid>
            )}
            {showExpectedReturn && (
              <Grid item xs={12} sm={6}>
                <DatePickerInput
                  label="Expected Loan Return Date"
                  value={formData.expectedLoanReturnDate || ''}
                  onChange={(e) => setFormField('expectedLoanReturnDate')(e.target.value)}
                />
              </Grid>
            )}
          </Grid>
        </FormSection>

        <FormSection title="Condition & Maintenance">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Select
                label="Condition"
                value={formData.condition || ''}
                onChange={(e) => setFormField('condition')(e.target.value)}
                options={conditionOptions}
              />
            </Grid>
            {showMaintenance && (
              <>
                <Grid item xs={12} sm={6}>
                  <Select
                    label="Maintenance Type"
                    value={formData.maintenanceType || ''}
                    onChange={(e) => setFormField('maintenanceType')(e.target.value)}
                    options={maintenanceTypeOptions}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <NumberInput
                    label="Maintenance Cost"
                    value={formData.maintenanceCost}
                    onChange={(e) => setFormField('maintenanceCost')(e.target.value)}
                    prefix="Rp "
                    thousandSeparator
                    decimalScale={0}
                  />
                </Grid>
              </>
            )}
          </Grid>
        </FormSection>

        <FormSection title="Notes & Attachments">
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input
                label="Notes"
                value={formData.notes || ''}
                onChange={(e) => setFormField('notes')(e.target.value)}
                multiline
                rows={3}
              />
            </Grid>
            {showFileUploads && (
              <Grid item xs={12}>
                <FileUploader
                  referenceType="AssetTransaction"
                  referenceId={currentTransactionId}
                  entityType="transaction"
                  isAttachment={true}
                  existingFiles={[...existingAttachments, ...attachmentFiles]}
                  onFilesChange={handleFilesChange}
                  onUploadComplete={handleUploadComplete}
                  onFileDeleted={() => {
                    const fetchAttachments = async () => {
                      try {
                        const response = await apiClient.transaction.getById(currentTransactionId);
                        const data = response.data?.data || response.data || {};
                        if (data.attachments && Array.isArray(data.attachments)) {
                          setExistingAttachments(data.attachments);
                        }
                      } catch (error) {
                        console.warn('Failed to refresh attachments:', error);
                      }
                    };
                    fetchAttachments();
                  }}
                />
              </Grid>
            )}
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
        description="Upload Excel file. Transactions will be imported as PENDING."
      />
    </div>
  );
};

export default AssetTransactionsMenu;