// src/pages/AssetTransactions/AssetTransactions.menu.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { Grid, Chip, Box } from '@mui/material';
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
import { FiUpload, FiCheckSquare, FiPlus } from 'react-icons/fi';
import {
  TRANSACTION_TYPE_OPTIONS,
  TRANSACTION_TYPES,
  TRANSACTION_TYPES_REQUIRING_PAIR,
  TRANSACTION_TYPES_REQUIRING_EMPLOYEE,
  TRANSACTION_TYPES_REQUIRING_OFFICE,
  TRANSACTION_TYPES_RETURNABLE,
  isPrimaryTransactionType,
  isSecondaryTransactionType,
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

const TRANSACTION_TABS = [
  { id: 'approved', label: 'Approved' },
  { id: 'pending', label: 'Pending' },
  { id: 'rejected', label: 'Rejected' },
  { id: 'active-loans', label: 'Active Loans' },
  { id: 'overdue-loans', label: 'Overdue Loans' },
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

const TABS_WITH_CHECKBOX = ['pending', 'approved'];

const AssetTransactionsMenu = () => {
  const [activeTab, setActiveTab] = useState('approved');
  const [searchTerm, setSearchTerm] = useState('');
  const [dateFilterStart, setDateFilterStart] = useState('');
  const [dateFilterEnd, setDateFilterEnd] = useState('');
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
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, transactionsData, {
    idField: 'assetTransactionId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

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

  const showCheckbox = TABS_WITH_CHECKBOX.includes(activeTab);

  const buildFilters = useCallback(() => {
    const filters = {};
    if (activeTab === 'active-loans' || activeTab === 'overdue-loans') return filters;
    if (activeTab === 'pending') filters.approved = null;
    else if (activeTab === 'approved') filters.approved = true;
    else if (activeTab === 'rejected') filters.approved = false;

    if (searchTerm) filters.search = searchTerm;
    if (dateFilterStart) filters.startDate = dateFilterStart;
    if (dateFilterEnd) filters.endDate = dateFilterEnd;
    return filters;
  }, [activeTab, searchTerm, dateFilterStart, dateFilterEnd]);

  const fetchGridData = useCallback(
    async (params) => {
      const filters = buildFilters();

      if (activeTab === 'active-loans') {
        const result = await transactionsData.api.getActiveLoans({ ...params, ...filters });
        return { success: true, data: { data: result.data?.data || [], totalCount: result.data?.totalCount || 0 } };
      }
      if (activeTab === 'overdue-loans') {
        const result = await transactionsData.api.getOverdueLoans({ ...params, ...filters });
        return { success: true, data: { data: result.data?.data || [], totalCount: result.data?.totalCount || 0 } };
      }
      if (activeTab === 'rejected') {
        const result = await transactionsData.api.getGridPendingApproval({ ...params, ...filters, approved: false });
        return { success: true, data: { data: result.data?.data || [], totalCount: result.data?.totalCount || 0 } };
      }
      if (activeTab === 'pending') {
        const result = await transactionsData.api.getGridPendingApproval({ ...params, ...filters });
        return { success: true, data: { data: result.data?.data || [], totalCount: result.data?.totalCount || 0 } };
      }

      const result = await transactionsData.api.getGridAdd({ ...params, ...filters });
      return { success: true, data: { data: result.data?.data || [], totalCount: result.data?.totalCount || 0 } };
    },
    [activeTab, buildFilters]
  );

  const {
    data: transactions,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    reload,
  } = useGridData(['transactions', activeTab, searchTerm, dateFilterStart, dateFilterEnd], fetchGridData);

  useEffect(() => {
    isMountedRef.current = true;
    setPage(1);
    clearPrimarySelection();
    clearSecondarySelection();
    reload();
    return () => { isMountedRef.current = false; };
  }, [activeTab, reload, setPage, clearPrimarySelection, clearSecondarySelection]);

  // Fetch paired transaction options
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

  const handleSearch = useCallback(
    (search) => {
      setSearchTerm(search);
      setPage(1);
      clearPrimarySelection();
      clearSecondarySelection();
    },
    [setPage, clearPrimarySelection, clearSecondarySelection]
  );

  const handleDateFilterChange = useCallback(
    (start, end) => {
      setDateFilterStart(start);
      setDateFilterEnd(end);
      setPage(1);
      clearPrimarySelection();
      clearSecondarySelection();
    },
    [setPage, clearPrimarySelection, clearSecondarySelection]
  );

  // Bulk actions
  const handleBulkApprove = useCallback(
    async (ids, gridType = 'primary') => {
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
        reload();
      }
    },
    [reload, toast, confirm, clearPrimarySelection, clearSecondarySelection]
  );

  const handleBulkReject = useCallback(
    async (ids, gridType = 'primary') => {
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
        reload();
      }
    },
    [reload, toast, confirm, clearPrimarySelection, clearSecondarySelection]
  );

  const handleBulkPending = useCallback(
    async (ids, gridType = 'primary') => {
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
        reload();
      }
    },
    [reload, toast, confirm, clearPrimarySelection, clearSecondarySelection]
  );

  // Shortcuts
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
      reload();
      clearPrimarySelection();
      clearSecondarySelection();
    }
  }, [selectedTransaction, isReturnSubmitting, returnData, reload, clearPrimarySelection, clearSecondarySelection]);

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
      reload();
      clearPrimarySelection();
      clearSecondarySelection();
    }
  }, [selectedTransaction, isPostMaintenanceSubmitting, postMaintenanceData, reload, clearPrimarySelection, clearSecondarySelection]);

  const onSubmit = useCallback(async () => {
    const success = await crudHandleSubmit();
    if (success && isMountedRef.current) {
      toast.success(editingTransaction ? 'Transaction updated' : 'Transaction created (Pending approval)');
      reload();
      clearPrimarySelection();
      clearSecondarySelection();
    }
    return success;
  }, [crudHandleSubmit, reload, toast, editingTransaction, clearPrimarySelection, clearSecondarySelection]);

  const handleTabChange = useCallback(
    (tab) => {
      setActiveTab(tab);
      setPage(1);
      setSearchTerm('');
      setDateFilterStart('');
      setDateFilterEnd('');
      clearPrimarySelection();
      clearSecondarySelection();
    },
    [setPage, clearPrimarySelection, clearSecondarySelection]
  );

  const handleImport = useCallback(
    async (file) => {
      setIsImporting(true);
      const r = await transactionsData.importTransactions(file);
      setIsImporting(false);
      if (r.success && isMountedRef.current) {
        setImportResult(r.data);
        if (r.data.errorCount === 0) toast.success(`Import completed: ${r.data.successCount} transactions`);
        else toast.warning(`Import completed: ${r.data.successCount} success, ${r.data.errorCount} errors`);
        reload();
        clearPrimarySelection();
        clearSecondarySelection();
      }
    },
    [reload, toast, clearPrimarySelection, clearSecondarySelection]
  );

  const handleDownloadTemplate = useCallback(async () => {
    await transactionsData.downloadTemplate();
  }, []);

  // Grid actions
  const getConditionalActions = useCallback(
    (row) => {
      const isPending = row.approved === null;
      const isApproved = row.approved === true;
      const isPrimary = isPrimaryTransactionType(row.transactionType);
      const hasPair = row.fromAssetTransactionId !== null && row.fromAssetTransactionId !== undefined;

      const actions = [];
      if (isPending) {
        actions.push(ACTION_TYPES.EDIT);
        actions.push(ACTION_TYPES.DELETE);
      }
      if (isApproved) {
        actions.push(ACTION_TYPES.EDIT);
        actions.push(ACTION_TYPES.DELETE);
        if (isPrimary && TRANSACTION_TYPES_RETURNABLE.includes(row.transactionType) && !hasPair) {
          actions.push(ACTION_TYPES.RETURN);
        }
        if (isPrimary && row.transactionType === TRANSACTION_TYPES.MAINTENANCE && !hasPair) {
          actions.push(ACTION_TYPES.POST_MAINTENANCE);
        }
      }
      return actions;
    },
    []
  );

  const handleActionClick = useCallback(
    (actionType, row) => {
      if (actionType === ACTION_TYPES.EDIT) handleEdit(row);
      else if (actionType === ACTION_TYPES.DELETE) {
        confirmDelete('Cancel Transaction', `Cancel transaction for ${row.assetCode}?`).then(confirmed => {
          if (confirmed) transactionsData.delete(row.assetTransactionId).then(() => {
            toast.success('Transaction cancelled');
            reload();
            clearPrimarySelection();
            clearSecondarySelection();
          });
        });
      } else if (actionType === ACTION_TYPES.RETURN) handleReturnShortcut(row);
      else if (actionType === ACTION_TYPES.POST_MAINTENANCE) handlePostMaintenanceShortcut(row);
    },
    [handleEdit, confirmDelete, toast, reload, clearPrimarySelection, clearSecondarySelection, handleReturnShortcut, handlePostMaintenanceShortcut]
  );

  const { actionColumn } = useGridActions({
    actions: [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleActionClick,
    getConditionalActions,
    rowIdField: 'assetTransactionId',
  });

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

  const columns = useMemo(
    () => [
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
      actionColumn,
    ],
    [actionColumn]
  );

  const bulkAction = activeTab === 'pending' ? 'approve' : activeTab === 'approved' ? 'reject' : '';
  const bulkActionLabel = bulkAction === 'approve' ? 'Approve' : bulkAction === 'reject' ? 'Reject' : '';

  const renderBulkButton = (ids, gridType) => {
    if (!ids.length || !showCheckbox || !bulkAction) return null;
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

  if (loading && !transactions.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const primaryData = transactions.filter(t => isPrimaryTransactionType(t.transactionType));
  const secondaryData = transactions.filter(t => isSecondaryTransactionType(t.transactionType));

  return (
    <div className="transactions-menu">
      <div className="page-header">
        <div>
          <h1 className="page-title">Asset Transactions</h1>
          <p className="page-description">Manage asset transaction history and approvals</p>
        </div>
        <div className="transactions-menu__header-actions">{headerActions}</div>
      </div>

      <Tabs tabs={TRANSACTION_TABS} activeTab={activeTab} onTabChange={handleTabChange} />

      <div className="transactions-menu__date-filter-container">
        <TransactionFilter
          startDate={dateFilterStart}
          endDate={dateFilterEnd}
          onStartDateChange={(v) => handleDateFilterChange(v, dateFilterEnd)}
          onEndDateChange={(v) => handleDateFilterChange(dateFilterStart, v)}
          onClear={() => handleDateFilterChange('', '')}
          hasFilter={!!(dateFilterStart || dateFilterEnd)}
        />
      </div>

      <SearchToolbar onSearch={handleSearch} placeholder="Search transactions..." />

      {/* Grid Primary (ADD types) */}
      <Card
        className="transactions-menu__grid-card"
        title="Primary Transactions"
        subtitle="Handover, Transfer, Loan, Maintenance, Disposal, Operational Office"
        actions={renderBulkButton(getPrimarySelectedIds(primaryData), 'primary')}
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
          checkboxSelection={showCheckbox}
          onRowSelectionModelChange={handlePrimarySelectionChange}
          ariaLabel="Primary transactions"
          paginationMode="server"
        />
      </Card>

      {/* Grid Secondary (RETURN types) */}
      <Card
        className="transactions-menu__grid-card"
        title="Secondary Transactions"
        subtitle="Return, Loan Return, Post Maintenance"
        actions={renderBulkButton(getSecondarySelectedIds(secondaryData), 'secondary')}
      >
        <DataTable
          rows={secondaryData}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          page={1}
          onPageChange={() => {}}
          onPageSizeChange={() => {}}
          getRowId={(row) => row?.assetTransactionId || `secondary-${Math.random()}`}
          checkboxSelection={showCheckbox}
          onRowSelectionModelChange={handleSecondarySelectionChange}
          ariaLabel="Secondary transactions"
          paginationMode="client"
        />
      </Card>

      {/* MODAL FORM */}
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

      {/* Return Modal */}
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

      {/* Post Maintenance Modal */}
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

      {/* Import Modal */}
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