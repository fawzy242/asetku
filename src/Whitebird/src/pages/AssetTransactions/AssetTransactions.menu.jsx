import React, { useState, useMemo, useCallback } from "react";
import { FiEdit2, FiCheck, FiX, FiRotateCcw, FiPlus } from "react-icons/fi";
import { Grid, Box, Chip } from "@mui/material";
import AssetTransactionsData from "./AssetTransactions.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Modal from "../../components/molecules/Modal/Modal";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import SearchToolbar from "../../components/molecules/SearchToolbar/SearchToolbar";
import Tabs from "../../components/molecules/Tabs/Tabs";
import FilterPanel from "../../components/molecules/FilterPanel/FilterPanel";
import IconButton from "../../components/atoms/IconButton/IconButton";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudForm } from "../../hooks/useCrudForm";
import utilsHelper from "../../core/utils/utils.helper";
import "./AssetTransactions.scss";

const transactionsData = new AssetTransactionsData();

const INITIAL_FORM_DATA = {
  assetId: "", transactionType: "Assignment", toEmployeeId: "", fromEmployeeId: "",
  toLocationId: "", fromLocationId: "", expectedReturnDate: "", notes: "", conditionBefore: "",
};

const CRUD_OPTIONS = {
  idField: 'assetTransactionId',
};

const TABS = [
  { id: "all", label: "All" },
  { id: "pending", label: "Pending" },
  { id: "approved", label: "Approved" },
];

const STATUS_CHIP_COLORS = {
  'Pending': { bg: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b' },
  'Approved': { bg: 'rgba(16, 185, 129, 0.1)', color: '#10b981' },
  'Rejected': { bg: 'rgba(239, 68, 68, 0.1)', color: '#ef4444' },
  'Completed': { bg: 'rgba(59, 130, 246, 0.1)', color: '#3b82f6' },
  'Cancelled': { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280' },
};

const TRANSACTION_TYPE_OPTIONS = [
  { value: "Assignment", label: "Assignment" },
  { value: "Return", label: "Return" },
  { value: "Maintenance", label: "Maintenance" },
];

const AssetTransactionsMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showFilters, setShowFilters] = useState(false);
  const [statusFilter, setStatusFilter] = useState("");
  const [typeFilter, setTypeFilter] = useState("");
  const [showReturnModal, setShowReturnModal] = useState(false);
  const [selectedTransaction, setSelectedTransaction] = useState(null);
  const [isReturnSubmitting, setIsReturnSubmitting] = useState(false);
  const [returnData, setReturnData] = useState({
    actualReturnDate: new Date().toISOString().split("T")[0],
    conditionAfter: "Good",
    notes: ""
  });

  const { employees, locations, assets: refAssets } = useReferenceData();

  const {
    showModal,
    editingRecord: editingTransaction,
    isSubmitting,
    formData,
    setFormData,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudForm(INITIAL_FORM_DATA, transactionsData, CRUD_OPTIONS);

  const fetchGridData = useCallback(async (params) => {
    let apiStatus = "";
    if (activeTab === "pending") apiStatus = "Pending";
    else if (activeTab === "approved") apiStatus = "Approved";
    return transactionsData.fetchGridData({
      ...params,
      status: apiStatus || statusFilter,
      type: typeFilter
    });
  }, [activeTab, statusFilter, typeFilter]);

  const {
    data: transactions,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    updateFilters,
    reload
  } = useGridData(['transactions', activeTab, statusFilter, typeFilter], fetchGridData);

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
      conditionAfter: tx.conditionBefore || "Good",
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
    if (r.success) { setShowReturnModal(false); reload(); }
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

  const columns = useMemo(() => [
    { field: "assetCode", headerName: "Asset Code", width: 120 },
    { field: "assetName", headerName: "Asset Name", flex: 1, minWidth: 160 },
    { field: "transactionType", headerName: "Type", width: 120 },
    { field: "fromEmployeeName", headerName: "From", width: 150 },
    { field: "toEmployeeName", headerName: "To", width: 150 },
    {
      field: "transactionDate",
      headerName: "Date",
      width: 160,
      valueFormatter: (p) => utilsHelper.formatDateTime(p.value),
    },
    {
      field: "transactionStatus",
      headerName: "Status",
      width: 120,
      renderCell: (p) => {
        const colors = STATUS_CHIP_COLORS[p.value] || STATUS_CHIP_COLORS['Pending'];
        return (
          <Chip
            label={p.value || '-'}
            size="small"
            sx={{
              bgcolor: colors.bg,
              color: colors.color,
              fontWeight: 500,
              fontSize: '0.75rem',
              height: 24,
              borderRadius: '4px',
            }}
          />
        );
      },
    },
    {
      field: "expectedReturnDate",
      headerName: "Expected Return",
      width: 130,
      valueFormatter: (p) => p.value ? utilsHelper.formatDate(p.value) : "-",
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 200,
      sortable: false,
      renderCell: (p) => (
        <div className="table-actions">
          {p.row.transactionStatus === "Pending" && (
            <>
              <IconButton onClick={() => handleApprove(p.row)} title="Approve" variant="success" size="lg">
                <FiCheck size={18} />
              </IconButton>
              <IconButton onClick={() => handleReject(p.row)} title="Reject" variant="danger" size="lg">
                <FiX size={18} />
              </IconButton>
              <IconButton onClick={() => handleEdit(p.row)} title="Edit" size="lg">
                <FiEdit2 size={18} />
              </IconButton>
              <IconButton onClick={() => handleCancel(p.row)} title="Cancel" variant="danger" size="lg">
                <FiX size={18} />
              </IconButton>
            </>
          )}
          {p.row.transactionStatus === "Approved" && p.row.transactionType === "Assignment" && !p.row.actualReturnDate && (
            <Button variant="primary" size="sm" onClick={() => handleReturn(p.row)} startIcon={<FiRotateCcw />}>
              Return
            </Button>
          )}
        </div>
      )
    },
  ], [handleEdit, handleApprove, handleReject, handleCancel, handleReturn]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
  }, [setPage]);

  if (loading && !transactions.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="transactions-menu">
      <PageHeader title="Asset Transactions" buttonText="New Transaction" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} onFilterToggle={() => setShowFilters(!showFilters)} showFilters={showFilters} placeholder="Search..." />

      <FilterPanel visible={showFilters}>
        <Select label="Status" value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
          options={[
            { value: "", label: "All" },
            { value: "Pending", label: "Pending" },
            { value: "Approved", label: "Approved" },
            { value: "Rejected", label: "Rejected" },
          ]}
        />
        <Select label="Type" value={typeFilter} onChange={(e) => { setTypeFilter(e.target.value); setPage(1); }}
          options={[
            { value: "", label: "All" },
            ...TRANSACTION_TYPE_OPTIONS,
          ]}
        />
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
              <Select label="Asset" value={formData.assetId} onChange={e => setFormData({ ...formData, assetId: e.target.value })}
                options={refAssets.map(a => ({ value: a.value, label: a.label }))} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Type" value={formData.transactionType} onChange={e => setFormData({ ...formData, transactionType: e.target.value })}
                options={TRANSACTION_TYPE_OPTIONS} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="From Employee" value={formData.fromEmployeeId} onChange={e => setFormData({ ...formData, fromEmployeeId: e.target.value })}
                options={[{ value: "", label: "None" }, ...employees.map(e => ({ value: e.value, label: e.label }))]} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="To Employee" value={formData.toEmployeeId} onChange={e => setFormData({ ...formData, toEmployeeId: e.target.value })}
                options={[{ value: "", label: "None" }, ...employees.map(e => ({ value: e.value, label: e.label }))]} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="From Location" value={formData.fromLocationId} onChange={e => setFormData({ ...formData, fromLocationId: e.target.value })}
                options={[{ value: "", label: "None" }, ...locations.map(l => ({ value: l.value, label: l.label }))]} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="To Location" value={formData.toLocationId} onChange={e => setFormData({ ...formData, toLocationId: e.target.value })}
                options={[{ value: "", label: "None" }, ...locations.map(l => ({ value: l.value, label: l.label }))]} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Expected Return" type="date" value={formData.expectedReturnDate} onChange={e => setFormData({ ...formData, expectedReturnDate: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="Condition Before" value={formData.conditionBefore} onChange={e => setFormData({ ...formData, conditionBefore: e.target.value })} />
            </Grid>
            <Grid item xs={12}>
              <Input label="Notes" value={formData.notes} onChange={e => setFormData({ ...formData, notes: e.target.value })} multiline rows={2} />
            </Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={handleClose} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>
              {editingTransaction ? "Update" : "Create"}
            </Button>
          </Box>
        </form>
      </Modal>

      {/* Return Modal */}
      <Modal isOpen={showReturnModal} onClose={() => !isReturnSubmitting && setShowReturnModal(false)} title="Return Asset" size="md">
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <Box sx={{ p: 2, bgcolor: 'var(--surface)', borderRadius: 2 }}>
            <strong>{selectedTransaction?.assetCode} - {selectedTransaction?.assetName}</strong>
          </Box>
          <Input label="Actual Return Date" type="date" value={returnData.actualReturnDate}
            onChange={e => setReturnData({ ...returnData, actualReturnDate: e.target.value })} required />
          <Select label="Condition After" value={returnData.conditionAfter}
            onChange={e => setReturnData({ ...returnData, conditionAfter: e.target.value })}
            options={[{ value: "Good", label: "Good" }, { value: "Fair", label: "Fair" }, { value: "Poor", label: "Poor" }]} required />
          <Input label="Notes" value={returnData.notes} onChange={e => setReturnData({ ...returnData, notes: e.target.value })} multiline rows={2} />
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 2 }}>
            <Button variant="outline" onClick={() => setShowReturnModal(false)} disabled={isReturnSubmitting}>Cancel</Button>
            <Button variant="primary" onClick={handleReturnSubmit} loading={isReturnSubmitting}>Confirm Return</Button>
          </Box>
        </Box>
      </Modal>
    </div>
  );
};

export default AssetTransactionsMenu;