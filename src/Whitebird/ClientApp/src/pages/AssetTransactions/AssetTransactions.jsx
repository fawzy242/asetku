// src/pages/AssetTransactions/AssetTransactions.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useQueryClient } from '@tanstack/react-query';
import {
  transactionSchema,
  getTransactionDefaultValues,
} from './schema';
import { formatDate } from '../../core/utils/date.utils';
import { AssetTransactionsData } from '../../core/services';
import Spinner from '../../components/atoms/Spinner/Spinner';
import Button from '../../components/atoms/Button/Button';
import BulkActionBar from '../../components/atoms/BulkActionBar';
import SearchToolbar from '../../components/molecules/SearchToolbar/SearchToolbar';
import Tabs from '../../components/molecules/Tabs/Tabs';
import TransactionFilter from '../../components/molecules/TransactionFilter/TransactionFilter';
import ImportModal from '../../components/molecules/ImportModal/ImportModal';
import ActionMenu from '../../components/molecules/ActionMenu';
import { FiUpload, FiPlus, FiCheckSquare } from 'react-icons/fi';
import {
  TRANSACTION_TYPES,
  TRANSACTION_TYPES_REQUIRING_PAIR,
  TRANSACTION_TYPES_REQUIRING_EMPLOYEE,
  TRANSACTION_TYPES_REQUIRING_OFFICE,
  TRANSACTION_TYPES_RETURNABLE,
  isPrimaryTransactionType,
  getTransactionTypeName,
} from '../../core/constants/transactionTypes';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useReferenceData } from '../../hooks/useReferenceData';
import { useOptions } from '../../hooks/useOptions';
import {
  baseTransactionColumns,
  getMonitoringColumns,
  getManagementColumns,
  getManagementReturnColumns,
} from './columns';
import PrimaryTransactionsGrid from './components/PrimaryTransactionsGrid';
import SecondaryTransactionsGrid from './components/SecondaryTransactionsGrid';
import TransactionDialogs from './components/TransactionDialogs';
import './AssetTransactions.scss';

const transactionsData = new AssetTransactionsData();

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
  const [activeSubTab1, setActiveSubTab1] = useState('uncomplete');
  const [activeSubTab2, setActiveSubTab2] = useState('active-loans');
  const [activeSubTabManagement, setActiveSubTabManagement] = useState('approved');
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
  const [isPostMaintenanceSubmitting, setIsPostMaintenanceSubmitting] =
    useState(false);
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
  const [isViewMode, setIsViewMode] = useState(false);
  const [currentTransactionId, setCurrentTransactionId] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [editingTransaction, setEditingTransaction] = useState(null);

  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirm, confirmDelete } = useSweetAlert();

  const {
    employees,
    offices,
    assets: refAssets,
    conditions,
    maintenanceTypes,
  } = useReferenceData();

  const {
    selectedRowIds: primarySelected,
    selectionCount: primaryCount,
    hasSelection: hasPrimary,
    handleSelectionChange: handlePrimaryChange,
    clearSelection: clearPrimary,
    getSelectedIds: getPrimaryIds,
  } = useBulkSelection({ idField: 'assetTransactionId' });

  const {
    selectedRowIds: secondarySelected,
    selectionCount: secondaryCount,
    hasSelection: hasSecondary,
    handleSelectionChange: handleSecondaryChange,
    clearSelection: clearSecondary,
    getSelectedIds: getSecondaryIds,
  } = useBulkSelection({ idField: 'assetTransactionId' });

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

  const setFormField = useCallback(
    (field) => (value) => {
      setValue(field, value, { shouldValidate: true });
    },
    [setValue]
  );

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

  const fetchMonitoringData1 = useCallback(
    async (params) => {
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
    },
    [activeSubTab1, searchTerm, dateFilterStart, dateFilterEnd]
  );

  const fetchMonitoringData2 = useCallback(
    async (params) => {
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
    },
    [activeSubTab2, searchTerm, dateFilterStart, dateFilterEnd]
  );

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
    data: monitoringData1,
    totalCount: monitoringTotal1,
    loading: monitoringLoading1,
    page: monitoringPage1,
    setPage: setMonitoringPage1,
    pageSize: monitoringPageSize1,
    setPageSize: setMonitoringPageSize1,
    sortModel: monitoringSort1,
    setSortModel: setMonitoringSort1,
    reload: reloadMonitoring1,
  } = useGridData(
    ['monitoring1', activeSubTab1, searchTerm, dateFilterStart, dateFilterEnd],
    fetchMonitoringData1
  );

  const {
    data: monitoringData2,
    totalCount: monitoringTotal2,
    loading: monitoringLoading2,
    page: monitoringPage2,
    setPage: setMonitoringPage2,
    pageSize: monitoringPageSize2,
    setPageSize: setMonitoringPageSize2,
    sortModel: monitoringSort2,
    setSortModel: setMonitoringSort2,
    reload: reloadMonitoring2,
  } = useGridData(
    ['monitoring2', activeSubTab2, searchTerm, dateFilterStart, dateFilterEnd],
    fetchMonitoringData2
  );

  const {
    data: managementData,
    totalCount: managementTotal,
    loading: managementLoading,
    page: managementPage,
    setPage: setManagementPage,
    pageSize: managementPageSize,
    setPageSize: setManagementPageSize,
    sortModel: managementSort,
    setSortModel: setManagementSort,
    reload: reloadManagement,
  } = useGridData(
    ['management', activeSubTabManagement, searchTerm, dateFilterStart, dateFilterEnd],
    fetchManagementData
  );

  const {
    data: managementReturnData,
    totalCount: managementReturnTotal,
    loading: managementReturnLoading,
    page: managementReturnPage,
    setPage: setManagementReturnPage,
    pageSize: managementReturnPageSize,
    setPageSize: setManagementReturnPageSize,
    sortModel: managementReturnSort,
    setSortModel: setManagementReturnSort,
    reload: reloadManagementReturn,
  } = useGridData(
    ['management-return', activeSubTabManagement, searchTerm, dateFilterStart, dateFilterEnd],
    fetchManagementReturnData
  );

  useEffect(() => {
    const type = parseInt(formData.transactionType);
    if (formData.assetId && TRANSACTION_TYPES_REQUIRING_PAIR.includes(type)) {
      setLoadingPairedOptions(true);
      (async () => {
        try {
          const sourceType =
            type === TRANSACTION_TYPES.LOAN_RETURN
              ? TRANSACTION_TYPES.LOAN
              : TRANSACTION_TYPES.MAINTENANCE;
          const result = await transactionsData.getPairedOptions(
            parseInt(formData.assetId, 10),
            sourceType
          );
          if (isMountedRef.current && result.success) {
            setPairedTransactionOptions(
              result.data.map((t) => ({
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
    return () => {
      isMountedRef.current = false;
    };
  }, [
    activeMainTab,
    activeSubTab1,
    activeSubTab2,
    activeSubTabManagement,
    reloadMonitoring1,
    reloadMonitoring2,
    reloadManagement,
    reloadManagementReturn,
  ]);

  const loadRecordForModal = useCallback(
    async (record) => {
      const recordId = record?.assetTransactionId;
      if (!recordId) return;

      setCurrentTransactionId(recordId);
      setEditingTransaction(record);

      try {
        const result = await transactionsData.fetchById(recordId);
        if (result.success && result.data) {
          const data = result.data;
          const defaults = getTransactionDefaultValues();
          Object.keys(defaults).forEach((key) => {
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
    },
    [setValue, toast]
  );

  const handleCreate = useCallback(() => {
    reset(getTransactionDefaultValues());
    setEditingTransaction(null);
    setIsEditMode(false);
    setIsViewMode(false);
    setCurrentTransactionId(null);
    setAttachmentFiles([]);
    setExistingAttachments([]);
    setShowModal(true);
  }, [reset]);

  const handleEdit = useCallback(
    async (record) => {
      setIsViewMode(false);
      setIsEditMode(true);
      await loadRecordForModal(record);
    },
    [loadRecordForModal]
  );

  const handleView = useCallback(
    async (record) => {
      setIsViewMode(true);
      setIsEditMode(true);
      await loadRecordForModal(record);
    },
    [loadRecordForModal]
  );

  const handleClose = useCallback(() => {
    if (!isSubmitting) {
      setShowModal(false);
      setIsEditMode(false);
      setIsViewMode(false);
      setCurrentTransactionId(null);
      setEditingTransaction(null);
      reset(getTransactionDefaultValues());
      setAttachmentFiles([]);
      setExistingAttachments([]);
    }
  }, [isSubmitting, reset]);

  const onSubmit = useCallback(
    async (data) => {
      if (isSubmitting || isViewMode) return;

      const submitData = {
        ...data,
        attachmentFiles: attachmentFiles,
        attachmentIdsToKeep: isEditMode
          ? existingAttachments.map((a) => a.fileAttachmentId)
          : [],
        replaceAttachments: isEditMode ? true : false,
      };

      try {
        let result;
        if (isEditMode && editingTransaction) {
          result = await transactionsData.updateWithAttachments(
            editingTransaction.assetTransactionId,
            submitData
          );
        } else {
          result = await transactionsData.create(submitData);
        }

        if (result.success) {
          setAttachmentFiles([]);
          setExistingAttachments([]);
          setIsEditMode(false);
          setIsViewMode(false);
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

          const message =
            isEditMode && editingTransaction
              ? 'Transaction updated successfully'
              : 'Transaction created (Pending approval)';
          toast.success(message);
          return true;
        } else {
          const errorMsg =
            result.error ||
            result.errors?.join('\n') ||
            'Failed to save transaction';
          toast.error(errorMsg);
          return false;
        }
      } catch (error) {
        const errorMsg =
          error.userMessage || error.message || 'Failed to save transaction';
        toast.error(errorMsg);
        return false;
      }
    },
    [
      isSubmitting,
      isViewMode,
      isEditMode,
      editingTransaction,
      attachmentFiles,
      existingAttachments,
      queryClient,
      reloadManagement,
      reloadManagementReturn,
      reloadMonitoring1,
      reloadMonitoring2,
      clearPrimary,
      clearSecondary,
      toast,
      reset,
      activeMainTab,
    ]
  );

  const handleSearch = useCallback(
    (search) => {
      setSearchTerm(search);
      setMonitoringPage1(1);
      setMonitoringPage2(1);
      setManagementPage(1);
      setManagementReturnPage(1);
      clearPrimary();
      clearSecondary();
    },
    [
      setMonitoringPage1,
      setMonitoringPage2,
      setManagementPage,
      setManagementReturnPage,
      clearPrimary,
      clearSecondary,
    ]
  );

  const handleDateFilterChange = useCallback(
    (start, end) => {
      setDateFilterStart(start);
      setDateFilterEnd(end);
      setMonitoringPage1(1);
      setMonitoringPage2(1);
      setManagementPage(1);
      setManagementReturnPage(1);
      clearPrimary();
      clearSecondary();
    },
    [
      setMonitoringPage1,
      setMonitoringPage2,
      setManagementPage,
      setManagementReturnPage,
      clearPrimary,
      clearSecondary,
    ]
  );

  const handleComplete = useCallback(
    async (id) => {
      const confirmed = await confirm({
        title: 'Complete Transaction',
        text: 'Are you sure you want to complete this transaction?',
        confirmButtonText: 'Yes, Complete',
      });
      if (!confirmed) return;
      const r = await transactionsData.complete(id);
      if (r.success && isMountedRef.current) {
        toast.success('Transaction completed successfully');
        reloadMonitoring1();
        reloadMonitoring2();
        reloadManagement();
        reloadManagementReturn();
      }
    },
    [
      reloadMonitoring1,
      reloadMonitoring2,
      reloadManagement,
      reloadManagementReturn,
      toast,
      confirm,
    ]
  );

  const handleUncomplete = useCallback(
    async (id) => {
      const confirmed = await confirm({
        title: 'Uncomplete Transaction',
        text: 'Are you sure you want to uncomplete this transaction?',
        icon: 'warning',
        confirmButtonText: 'Yes, Uncomplete',
        confirmButtonColor: '#f59e0b',
      });
      if (!confirmed) return;
      const r = await transactionsData.uncomplete(id);
      if (r.success && isMountedRef.current) {
        toast.success('Transaction uncompleted successfully');
        reloadMonitoring1();
        reloadMonitoring2();
        reloadManagement();
        reloadManagementReturn();
      }
    },
    [
      reloadMonitoring1,
      reloadMonitoring2,
      reloadManagement,
      reloadManagementReturn,
      toast,
      confirm,
    ]
  );

  const handleBulkApprove = useCallback(
    async (ids, gridType = 'primary') => {
      if (!ids || ids.length === 0) {
        toast.warning('No transactions selected');
        return;
      }
      const confirmed = await confirm({
        title: 'Approve Transactions',
        text: `Are you sure you want to approve ${ids.length} transaction(s)?`,
        confirmButtonText: 'Yes, Approve',
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
    },
    [
      reloadManagement,
      reloadManagementReturn,
      reloadMonitoring1,
      reloadMonitoring2,
      toast,
      confirm,
      clearPrimary,
      clearSecondary,
    ]
  );

  const handleBulkReject = useCallback(
    async (ids, gridType = 'primary') => {
      if (!ids || ids.length === 0) {
        toast.warning('No transactions selected');
        return;
      }
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
        if (gridType === 'primary') clearPrimary();
        else clearSecondary();
        reloadManagement();
        reloadManagementReturn();
        reloadMonitoring1();
        reloadMonitoring2();
      }
    },
    [
      reloadManagement,
      reloadManagementReturn,
      reloadMonitoring1,
      reloadMonitoring2,
      toast,
      confirm,
      clearPrimary,
      clearSecondary,
    ]
  );

  const handleBulkPending = useCallback(
    async (ids, gridType = 'primary') => {
      if (!ids || ids.length === 0) {
        toast.warning('No transactions selected');
        return;
      }
      const confirmed = await confirm({
        title: 'Move to Pending',
        text: `Are you sure you want to move ${ids.length} transaction(s) back to pending?`,
        confirmButtonText: 'Yes, Revert',
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
    },
    [
      reloadManagement,
      reloadManagementReturn,
      reloadMonitoring1,
      reloadMonitoring2,
      toast,
      confirm,
      clearPrimary,
      clearSecondary,
    ]
  );

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
    const r = await transactionsData.createReturn(
      selectedTransaction.assetTransactionId,
      {
        actualReturnDate: returnData.actualReturnDate,
        condition: returnData.condition ? parseInt(returnData.condition, 10) : null,
        notes: returnData.notes,
      }
    );
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
  }, [
    selectedTransaction,
    isReturnSubmitting,
    returnData,
    reloadMonitoring1,
    reloadMonitoring2,
    reloadManagement,
    reloadManagementReturn,
    clearPrimary,
    clearSecondary,
  ]);

  const handlePostMaintenanceSubmit = useCallback(async () => {
    if (!selectedTransaction || isPostMaintenanceSubmitting) return;
    setIsPostMaintenanceSubmitting(true);
    const r = await transactionsData.createPostMaintenance(
      selectedTransaction.assetTransactionId,
      {
        completionDate: postMaintenanceData.completionDate,
        condition: postMaintenanceData.condition
          ? parseInt(postMaintenanceData.condition, 10)
          : null,
        maintenanceCost: postMaintenanceData.maintenanceCost
          ? parseFloat(postMaintenanceData.maintenanceCost)
          : null,
        notes: postMaintenanceData.notes,
      }
    );
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
  }, [
    selectedTransaction,
    isPostMaintenanceSubmitting,
    postMaintenanceData,
    reloadMonitoring1,
    reloadMonitoring2,
    reloadManagement,
    reloadManagementReturn,
    clearPrimary,
    clearSecondary,
  ]);

  const handleImport = useCallback(
    async (file) => {
      setIsImporting(true);
      const r = await transactionsData.importTransactions(file);
      setIsImporting(false);
      if (r.success && isMountedRef.current) {
        setImportResult(r.data);
        if (r.data.errorCount === 0) {
          toast.success(`Import completed: ${r.data.successCount} transactions`);
        } else {
          toast.warning(
            `Import completed: ${r.data.successCount} success, ${r.data.errorCount} errors`
          );
        }
        reloadManagement();
        reloadManagementReturn();
        reloadMonitoring1();
        reloadMonitoring2();
        clearPrimary();
        clearSecondary();
      }
    },
    [
      reloadManagement,
      reloadManagementReturn,
      reloadMonitoring1,
      reloadMonitoring2,
      toast,
      clearPrimary,
      clearSecondary,
    ]
  );

  const handleDownloadTemplate = useCallback(async () => {
    const result = await transactionsData.downloadTemplate();
    if (!result.success) {
      toast.error(result.error || 'Failed to download template');
    }
  }, [toast]);

  const handleFilesChange = useCallback((files) => {
    setAttachmentFiles(files.filter((f) => f.isNew));
    const existing = files.filter((f) => !f.isNew);
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

  const getMonitoringActions = useCallback(
    (row) => {
      const actions = [];
      const isPending = row.approved === null;
      const isApproved = row.approved === true;
      const isCompleted = row.completed === true;

      actions.push(ACTION_TYPES.VIEW);

      if (activeSubTab1 === 'uncomplete') {
        if (isPending) {
          actions.push(ACTION_TYPES.EDIT);
          actions.push(ACTION_TYPES.DELETE);
        }
        if (isApproved && !isCompleted) {
          actions.push('complete');
        }
      }

      return actions;
    },
    [activeSubTab1]
  );

  const handleMonitoringAction = useCallback(
    (actionType, row) => {
      if (actionType === ACTION_TYPES.VIEW) {
        handleView(row);
        return;
      }
      if (actionType === ACTION_TYPES.EDIT) {
        handleEdit(row);
      } else if (actionType === ACTION_TYPES.DELETE) {
        confirmDelete(
          'Cancel Transaction',
          `Cancel transaction for ${row.assetCode}?`
        ).then((confirmed) => {
          if (confirmed) {
            transactionsData.delete(row.assetTransactionId).then(() => {
              toast.success('Transaction cancelled');
              reloadMonitoring1();
              reloadMonitoring2();
              reloadManagement();
              reloadManagementReturn();
              clearPrimary();
              clearSecondary();
            });
          }
        });
      } else if (actionType === 'complete') {
        handleComplete(row.assetTransactionId);
      } else if (actionType === 'uncomplete') {
        handleUncomplete(row.assetTransactionId);
      }
    },
    [
      handleView,
      handleEdit,
      handleComplete,
      handleUncomplete,
      toast,
      reloadMonitoring1,
      reloadMonitoring2,
      reloadManagement,
      reloadManagementReturn,
      clearPrimary,
      clearSecondary,
      confirmDelete,
    ]
  );

  const getManagementActions = useCallback((row) => {
    const isPending = row.approved === null;
    const isApproved = row.approved === true;
    const isPrimary = isPrimaryTransactionType(row.transactionType);
    const hasPair =
      row.fromAssetTransactionId !== null &&
      row.fromAssetTransactionId !== undefined;

    const actions = [ACTION_TYPES.VIEW];
    if (isPending) {
      actions.push(ACTION_TYPES.EDIT);
      actions.push(ACTION_TYPES.DELETE);
    }
    if (isApproved) {
      actions.push(ACTION_TYPES.EDIT);
      actions.push(ACTION_TYPES.DELETE);
      if (
        isPrimary &&
        TRANSACTION_TYPES_RETURNABLE.includes(row.transactionType) &&
        !hasPair &&
        row.completed === true
      ) {
        actions.push(ACTION_TYPES.RETURN);
      }
      if (
        isPrimary &&
        row.transactionType === TRANSACTION_TYPES.MAINTENANCE &&
        !hasPair &&
        row.completed === true
      ) {
        actions.push(ACTION_TYPES.POST_MAINTENANCE);
      }
    }
    return actions;
  }, []);

  const handleManagementAction = useCallback(
    (actionType, row) => {
      if (actionType === ACTION_TYPES.VIEW) {
        handleView(row);
        return;
      }
      if (actionType === ACTION_TYPES.EDIT) {
        handleEdit(row);
      } else if (actionType === ACTION_TYPES.DELETE) {
        confirmDelete(
          'Cancel Transaction',
          `Cancel transaction for ${row.assetCode}?`
        ).then((confirmed) => {
          if (confirmed) {
            transactionsData.delete(row.assetTransactionId).then(() => {
              toast.success('Transaction cancelled');
              reloadManagement();
              reloadManagementReturn();
              reloadMonitoring1();
              reloadMonitoring2();
              clearPrimary();
              clearSecondary();
            });
          }
        });
      } else if (actionType === ACTION_TYPES.RETURN) {
        handleReturnShortcut(row);
      } else if (actionType === ACTION_TYPES.POST_MAINTENANCE) {
        handlePostMaintenanceShortcut(row);
      }
    },
    [
      handleView,
      handleEdit,
      confirmDelete,
      toast,
      reloadManagement,
      reloadManagementReturn,
      reloadMonitoring1,
      reloadMonitoring2,
      clearPrimary,
      clearSecondary,
      handleReturnShortcut,
      handlePostMaintenanceShortcut,
    ]
  );

  const getManagementReturnActions = useCallback((row) => {
    const isPending = row.approved === null;
    const actions = [ACTION_TYPES.VIEW];
    if (isPending) {
      actions.push(ACTION_TYPES.EDIT);
      actions.push(ACTION_TYPES.DELETE);
    }
    return actions;
  }, []);

  const handleManagementReturnAction = useCallback(
    (actionType, row) => {
      if (actionType === ACTION_TYPES.VIEW) {
        handleView(row);
        return;
      }
      if (actionType === ACTION_TYPES.EDIT) {
        handleEdit(row);
      } else if (actionType === ACTION_TYPES.DELETE) {
        confirmDelete(
          'Cancel Transaction',
          `Cancel transaction for ${row.assetCode}?`
        ).then((confirmed) => {
          if (confirmed) {
            transactionsData.delete(row.assetTransactionId).then(() => {
              toast.success('Transaction cancelled');
              reloadManagement();
              reloadManagementReturn();
              reloadMonitoring1();
              reloadMonitoring2();
              clearPrimary();
              clearSecondary();
            });
          }
        });
      }
    },
    [
      handleView,
      handleEdit,
      confirmDelete,
      toast,
      reloadManagement,
      reloadManagementReturn,
      reloadMonitoring1,
      reloadMonitoring2,
      clearPrimary,
      clearSecondary,
    ]
  );

  const monitoringGridActions = useGridActions({
    actions: [ACTION_TYPES.VIEW, ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleMonitoringAction,
    getConditionalActions: getMonitoringActions,
    rowIdField: 'assetTransactionId',
  });

  const managementGridActions = useGridActions({
    actions: [ACTION_TYPES.VIEW, ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleManagementAction,
    getConditionalActions: getManagementActions,
    rowIdField: 'assetTransactionId',
  });

  const managementReturnGridActions = useGridActions({
    actions: [ACTION_TYPES.VIEW, ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleManagementReturnAction,
    getConditionalActions: getManagementReturnActions,
    rowIdField: 'assetTransactionId',
  });

  const monitoringColumns1 = useMemo(
    () => getMonitoringColumns(monitoringGridActions.actionColumn, activeSubTab1),
    [activeSubTab1, monitoringGridActions.actionColumn]
  );

  const monitoringColumns2 = useMemo(
    () => getMonitoringColumns(monitoringGridActions.actionColumn, activeSubTab2),
    [activeSubTab2, monitoringGridActions.actionColumn]
  );

  const managementColumns = useMemo(
    () =>
      getManagementColumns(baseTransactionColumns, managementGridActions.actionColumn),
    [managementGridActions.actionColumn]
  );

  const managementReturnColumns = useMemo(
    () =>
      getManagementReturnColumns(
        baseTransactionColumns,
        managementReturnGridActions.actionColumn
      ),
    [managementReturnGridActions.actionColumn]
  );

  const employeeOptions = useOptions(employees, 'Select Employee');
  const officeOptions = useOptions(offices, 'Select Office');
  const conditionOptions = useOptions(conditions, 'Select Condition');
  const maintenanceTypeOptions = useOptions(
    maintenanceTypes,
    'Select Maintenance Type'
  );

  const filteredAssetOptions = useMemo(() => {
    return [
      { value: '', label: 'Select Asset' },
      ...refAssets.map((a) => ({ value: a.value, label: a.label })),
    ];
  }, [refAssets]);

  const isMonitoring = activeMainTab === 'monitoring';
  const isManagement = activeMainTab === 'management';
  const showEmployee = TRANSACTION_TYPES_REQUIRING_EMPLOYEE.includes(
    parseInt(formData.transactionType)
  );
  const showOffice = TRANSACTION_TYPES_REQUIRING_OFFICE.includes(
    parseInt(formData.transactionType)
  );
  const showPaired = TRANSACTION_TYPES_REQUIRING_PAIR.includes(
    parseInt(formData.transactionType)
  );
  const showMaintenance =
    parseInt(formData.transactionType) === TRANSACTION_TYPES.MAINTENANCE;
  const showExpectedReturn =
    parseInt(formData.transactionType) === TRANSACTION_TYPES.LOAN;
  const showCheckbox =
    activeSubTabManagement === 'pending' ||
    activeSubTabManagement === 'approved';
  const showFileUploads = isEditMode && currentTransactionId;

  const renderBulkActions = (ids, gridType) => {
    if (!ids || ids.length === 0) return [];

    if (activeSubTabManagement === 'pending') {
      return [
        {
          key: 'bulk-approve',
          label: `Approve (${ids.length})`,
          variant: 'primary',
          icon: <FiCheckSquare size={16} />,
          onClick: () => handleBulkApprove(ids, gridType),
        },
        {
          key: 'bulk-reject',
          label: `Reject (${ids.length})`,
          variant: 'danger',
          icon: <FiCheckSquare size={16} />,
          onClick: () => handleBulkReject(ids, gridType),
        },
      ];
    }

    if (activeSubTabManagement === 'approved') {
      return [
        {
          key: 'bulk-pending',
          label: `Pending (${ids.length})`,
          variant: 'warning',
          icon: <FiCheckSquare size={16} />,
          onClick: () => handleBulkPending(ids, gridType),
        },
      ];
    }

    return [];
  };

  const headerActions = (
    <>
      {isManagement && (
        <>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowImportModal(true)}
            className="u-inline-flex u-btn-gap"
          >
            <FiUpload size={16} /> Import
          </Button>
          <Button
            variant="primary"
            size="sm"
            onClick={handleCreate}
            className="u-inline-flex u-btn-gap"
          >
            <FiPlus size={16} /> New Transaction
          </Button>
        </>
      )}
    </>
  );

  if (managementLoading && !managementData.length && isManagement) {
    return (
      <div className="page-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="transactions-menu">
      <div className="page-header">
        <div>
          <h1 className="page-title">
            {isMonitoring
              ? 'Asset Transactions - Monitoring'
              : 'Asset Transactions - Management'}
          </h1>
          <p className="page-description">
            {isMonitoring
              ? 'Monitor transaction status, loans, maintenance, and pending completions'
              : 'Manage primary and secondary transactions with approvals'}
          </p>
        </div>
        <div className="transactions-menu__header-actions">{headerActions}</div>
      </div>

      {isMonitoring && (
        <>
          <div className="transactions-menu__grid-section">
            <Tabs
              tabs={MONITORING_SUB_TABS_1}
              activeTab={activeSubTab1}
              onTabChange={setActiveSubTab1}
            />
            <PrimaryTransactionsGrid
              data={monitoringData1}
              loading={monitoringLoading1}
              totalCount={monitoringTotal1}
              page={monitoringPage1}
              pageSize={monitoringPageSize1}
              onPageChange={setMonitoringPage1}
              onPageSizeChange={setMonitoringPageSize1}
              sortModel={monitoringSort1}
              onSortModelChange={setMonitoringSort1}
              columns={monitoringColumns1}
              checkboxSelection={false}
              getRowId={(row) => row?.assetTransactionId || `mon1-${Math.random()}`}
              onRowClick={handleView}
            />
          </div>

          <div className="transactions-menu__grid-section">
            <Tabs
              tabs={MONITORING_SUB_TABS_2}
              activeTab={activeSubTab2}
              onTabChange={setActiveSubTab2}
            />
            <PrimaryTransactionsGrid
              data={monitoringData2}
              loading={monitoringLoading2}
              totalCount={monitoringTotal2}
              page={monitoringPage2}
              pageSize={monitoringPageSize2}
              onPageChange={setMonitoringPage2}
              onPageSizeChange={setMonitoringPageSize2}
              sortModel={monitoringSort2}
              onSortModelChange={setMonitoringSort2}
              columns={monitoringColumns2}
              checkboxSelection={false}
              getRowId={(row) => row?.assetTransactionId || `mon2-${Math.random()}`}
              onRowClick={handleView}
            />
          </div>
        </>
      )}

      {isManagement && (
        <>
          <Tabs
            tabs={MANAGEMENT_SUB_TABS}
            activeTab={activeSubTabManagement}
            onTabChange={setActiveSubTabManagement}
          />

          <div className="transactions-menu__filter-container">
            <TransactionFilter
              startDate={dateFilterStart}
              endDate={dateFilterEnd}
              onStartDateChange={(v) => handleDateFilterChange(v, dateFilterEnd)}
              onEndDateChange={(v) => handleDateFilterChange(dateFilterStart, v)}
              onClear={() => handleDateFilterChange('', '')}
              hasFilter={!!(dateFilterStart || dateFilterEnd)}
            />
            <SearchToolbar
              onSearch={handleSearch}
              placeholder="Search transactions..."
            />
          </div>

          <PrimaryTransactionsGrid
            data={managementData}
            loading={managementLoading}
            totalCount={managementTotal}
            page={managementPage}
            pageSize={managementPageSize}
            onPageChange={setManagementPage}
            onPageSizeChange={setManagementPageSize}
            sortModel={managementSort}
            onSortModelChange={setManagementSort}
            columns={managementColumns}
            checkboxSelection={showCheckbox}
            onSelectionChange={handlePrimaryChange}
            getRowId={(row) =>
              row?.assetTransactionId || `primary-${Math.random()}`
            }
            onRowClick={handleView}
            bulkActions={
              <BulkActionBar
                visible={hasPrimary}
                actions={renderBulkActions(
                  getPrimaryIds(managementData),
                  'primary'
                )}
              />
            }
          />

          <SecondaryTransactionsGrid
            data={managementReturnData}
            loading={managementReturnLoading}
            totalCount={managementReturnTotal}
            page={managementReturnPage}
            pageSize={managementReturnPageSize}
            onPageChange={setManagementReturnPage}
            onPageSizeChange={setManagementReturnPageSize}
            sortModel={managementReturnSort}
            onSortModelChange={setManagementReturnSort}
            columns={managementReturnColumns}
            checkboxSelection={showCheckbox}
            onSelectionChange={handleSecondaryChange}
            getRowId={(row) =>
              row?.assetTransactionId || `secondary-${Math.random()}`
            }
            onRowClick={handleView}
            bulkActions={
              <BulkActionBar
                visible={hasSecondary}
                actions={renderBulkActions(
                  getSecondaryIds(managementReturnData),
                  'secondary'
                )}
              />
            }
          />
        </>
      )}

      <ActionMenu
        anchorEl={monitoringGridActions.menuAnchorEl}
        open={monitoringGridActions.menuOpen}
        onClose={monitoringGridActions.handleMenuClose}
        actions={monitoringGridActions.menuActions}
        row={monitoringGridActions.selectedRow}
        onAction={monitoringGridActions.handleActionClick}
      />
      <ActionMenu
        anchorEl={managementGridActions.menuAnchorEl}
        open={managementGridActions.menuOpen}
        onClose={managementGridActions.handleMenuClose}
        actions={managementGridActions.menuActions}
        row={managementGridActions.selectedRow}
        onAction={managementGridActions.handleActionClick}
      />
      <ActionMenu
        anchorEl={managementReturnGridActions.menuAnchorEl}
        open={managementReturnGridActions.menuOpen}
        onClose={managementReturnGridActions.handleMenuClose}
        actions={managementReturnGridActions.menuActions}
        row={managementReturnGridActions.selectedRow}
        onAction={managementReturnGridActions.handleActionClick}
      />

      <TransactionDialogs
        showModal={showModal}
        editingTransaction={editingTransaction}
        isSubmitting={isSubmitting}
        formData={formData}
        setFormField={setFormField}
        onSubmit={handleFormSubmit(onSubmit)}
        onClose={handleClose}
        isViewMode={isViewMode}
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
        onClose={() => {
          setShowImportModal(false);
          setImportResult(null);
        }}
        onImport={handleImport}
        onDownloadTemplate={handleDownloadTemplate}
        isImporting={isImporting}
        importResult={importResult}
        title="Import Transactions"
        description="Upload an .xlsx file. Transactions will be imported as PENDING."
      />
    </div>
  );
};

export default AssetTransactions;