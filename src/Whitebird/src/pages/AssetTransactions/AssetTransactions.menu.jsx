import React, { useState, useEffect, useMemo, useCallback } from "react";
import { FiEdit2, FiCheck, FiX, FiRotateCcw, FiPlus } from "react-icons/fi";
import { Grid, Box } from "@mui/material";
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
import { useGridData } from "../../hooks/useGridData";
import utilsHelper from "../../core/utils/utils.helper";
import "./AssetTransactions.scss";

const transactionsData = new AssetTransactionsData();

const AssetTransactionsMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showFilters, setShowFilters] = useState(false);
  const [statusFilter, setStatusFilter] = useState("");
  const [typeFilter, setTypeFilter] = useState("");
  const [showModal, setShowModal] = useState(false);
  const [showReturnModal, setShowReturnModal] = useState(false);
  const [editingTransaction, setEditingTransaction] = useState(null);
  const [selectedTransaction, setSelectedTransaction] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [dropdownData, setDropdownData] = useState({ assets: [], employees: [], locations: [] });
  const [formData, setFormData] = useState({ assetId: "", transactionType: "Assignment", toEmployeeId: "", fromEmployeeId: "", toLocationId: "", fromLocationId: "", expectedReturnDate: "", notes: "", conditionBefore: "" });
  const [returnData, setReturnData] = useState({ actualReturnDate: new Date().toISOString().split("T")[0], conditionAfter: "Good", notes: "" });

  const fetchGridData = useCallback(async (params) => {
    let apiStatus = "";
    if (activeTab === "pending") apiStatus = "Pending";
    else if (activeTab === "approved") apiStatus = "Approved";
    return transactionsData.fetchGridData({ ...params, status: apiStatus || statusFilter, type: typeFilter });
  }, [activeTab, statusFilter, typeFilter]);

  const { data: transactions, totalCount, loading, page, setPage, pageSize, setPageSize, updateFilters, reload } = useGridData(fetchGridData);

  useEffect(() => { transactionsData.fetchDropdownData().then(r => { if (r.success) setDropdownData(r.data); }); }, []);

  const handleSearch = (search) => updateFilters({ search });
  const handleCreate = () => { setEditingTransaction(null); setFormData({ assetId: "", transactionType: "Assignment", toEmployeeId: "", fromEmployeeId: "", toLocationId: "", fromLocationId: "", expectedReturnDate: "", notes: "", conditionBefore: "" }); setShowModal(true); };
const handleEdit = async (tx) => {
  const r = await transactionsData.fetchById(tx.assetTransactionId);
  if (r.success) {
    setEditingTransaction(r.data);
    setFormData({
      assetId: r.data.assetId || "",
      transactionType: r.data.transactionType || "Assignment",
      toEmployeeId: r.data.toEmployeeId || "",
      fromEmployeeId: r.data.fromEmployeeId || "",
      toLocationId: r.data.toLocationId || "",
      fromLocationId: r.data.fromLocationId || "",
      expectedReturnDate: r.data.expectedReturnDate?.split("T")[0] || "",
      notes: r.data.notes || "",
      conditionBefore: r.data.conditionBefore || ""
    });
    setShowModal(true);
  }
};
  const handleApprove = async (tx) => { const r = await transactionsData.approve(tx.assetTransactionId, true); if (r.success) reload(); };
  const handleReject = async (tx) => { const r = await transactionsData.approve(tx.assetTransactionId, false); if (r.success) reload(); };
  const handleReturn = (tx) => { setSelectedTransaction(tx); setReturnData({ actualReturnDate: new Date().toISOString().split("T")[0], conditionAfter: tx.conditionBefore || "Good", notes: "" }); setShowReturnModal(true); };
  const handleReturnSubmit = async () => { if (!selectedTransaction) return; setIsSubmitting(true); const r = await transactionsData.returnAsset(selectedTransaction.assetTransactionId, returnData.actualReturnDate, returnData.conditionAfter, returnData.notes); setIsSubmitting(false); if (r.success) { setShowReturnModal(false); reload(); } };
  const handleCancel = async (tx) => { const r = await transactionsData.cancel(tx.assetTransactionId); if (r.success) reload(); };
  const handleSubmit = async (e) => { e.preventDefault(); if (isSubmitting) return; setIsSubmitting(true); const r = editingTransaction ? await transactionsData.update(editingTransaction.assetTransactionId, formData) : await transactionsData.create(formData); setIsSubmitting(false); if (r.success) { setShowModal(false); reload(); } };

  const columns = useMemo(() => [
    { field: "assetCode", headerName: "Asset Code", width: 120 },
    { field: "assetName", headerName: "Asset Name", width: 180 },
    { field: "transactionType", headerName: "Type", width: 120 },
    { field: "fromEmployeeName", headerName: "From", width: 150 },
    { field: "toEmployeeName", headerName: "To", width: 150 },
    { field: "transactionDate", headerName: "Date", width: 160, valueFormatter: (p) => utilsHelper.formatDateTime(p.value) },
    { field: "transactionStatus", headerName: "Status", width: 120, renderCell: (p) => <span className={`status-badge status-badge--${utilsHelper.getStatusColor(p.value)}`}>{p.value}</span> },
    { field: "expectedReturnDate", headerName: "Expected Return", width: 130, valueFormatter: (p) => p.value ? utilsHelper.formatDate(p.value) : "-" },
    { field: "actions", headerName: "Actions", width: 180, sortable: false, renderCell: (p) => (<div className="table-actions">{p.row.transactionStatus === "Pending" && (<><button className="icon-btn icon-btn--success icon-btn--lg" onClick={() => handleApprove(p.row)} title="Approve"><FiCheck size={18} /></button><button className="icon-btn icon-btn--danger icon-btn--lg" onClick={() => handleReject(p.row)} title="Reject"><FiX size={18} /></button><button className="icon-btn icon-btn--lg" onClick={() => handleEdit(p.row)} title="Edit"><FiEdit2 size={18} /></button><button className="icon-btn icon-btn--lg" onClick={() => handleCancel(p.row)} title="Cancel"><FiX size={18} /></button></>)}{p.row.transactionStatus === "Approved" && p.row.transactionType === "Assignment" && !p.row.actualReturnDate && (<Button variant="primary" size="sm" onClick={() => handleReturn(p.row)}><FiRotateCcw /> Return</Button>)}</div>) },
  ], []);

  const tabs = [{ id: "all", label: "All" }, { id: "pending", label: "Pending" }, { id: "approved", label: "Approved" }];

  if (loading && !transactions.length) return <div className="transactions-loading"><Spinner size="lg" /></div>;

  return (
    <div className="transactions-menu">
      <PageHeader title="Asset Transactions" buttonText="New Transaction" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      <Tabs tabs={tabs} activeTab={activeTab} onTabChange={(tab) => { setActiveTab(tab); setPage(1); }} />
      <SearchToolbar onSearch={handleSearch} onFilterToggle={() => setShowFilters(!showFilters)} showFilters={showFilters} placeholder="Search..." />

      {showFilters && (
        <Box sx={{ display: 'flex', gap: 2, mb: 3, p: 2, bgcolor: 'var(--surface)', borderRadius: 2 }}>
          <Select label="Status" value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} options={[{ value: "", label: "All" }, { value: "Pending", label: "Pending" }, { value: "Approved", label: "Approved" }, { value: "Rejected", label: "Rejected" }]} />
          <Select label="Type" value={typeFilter} onChange={(e) => { setTypeFilter(e.target.value); setPage(1); }} options={[{ value: "", label: "All" }, { value: "Assignment", label: "Assignment" }, { value: "Return", label: "Return" }, { value: "Maintenance", label: "Maintenance" }]} />
        </Box>
      )}

      <div className="transactions-menu__table"><DataTable rows={transactions} columns={columns} loading={loading} pageSize={pageSize} getRowId={(row) => row.assetTransactionId} hideFooter={true} /></div>
      <Pagination currentPage={page} totalPages={Math.ceil(totalCount / pageSize)} pageSize={pageSize} totalItems={totalCount} onPageChange={setPage} onPageSizeChange={setPageSize} />

      <Modal isOpen={showModal} onClose={() => !isSubmitting && setShowModal(false)} title={editingTransaction ? "Edit" : "New Transaction"} size="lg">
        <form onSubmit={handleSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}><Select label="Asset" value={formData.assetId} onChange={e => setFormData({...formData, assetId: e.target.value})} options={dropdownData.assets.map(a => ({value: a.assetId, label: `${a.assetCode} - ${a.assetName}`}))} required /></Grid>
            <Grid item xs={12} sm={6}><Select label="Type" value={formData.transactionType} onChange={e => setFormData({...formData, transactionType: e.target.value})} options={[{value: "Assignment", label: "Assignment"}, {value: "Return", label: "Return"}, {value: "Maintenance", label: "Maintenance"}]} required /></Grid>
            <Grid item xs={12} sm={6}><Select label="From Employee" value={formData.fromEmployeeId} onChange={e => setFormData({...formData, fromEmployeeId: e.target.value})} options={[{value: "", label: "None"}, ...dropdownData.employees.map(e => ({value: e.employeeId, label: e.fullName}))]} /></Grid>
            <Grid item xs={12} sm={6}><Select label="To Employee" value={formData.toEmployeeId} onChange={e => setFormData({...formData, toEmployeeId: e.target.value})} options={[{value: "", label: "None"}, ...dropdownData.employees.map(e => ({value: e.employeeId, label: e.fullName}))]} /></Grid>
            <Grid item xs={12} sm={6}><Select label="From Location" value={formData.fromLocationId} onChange={e => setFormData({...formData, fromLocationId: e.target.value})} options={[{value: "", label: "None"}, ...dropdownData.locations.map(l => ({value: l.locationId, label: l.locationName}))]} /></Grid>
            <Grid item xs={12} sm={6}><Select label="To Location" value={formData.toLocationId} onChange={e => setFormData({...formData, toLocationId: e.target.value})} options={[{value: "", label: "None"}, ...dropdownData.locations.map(l => ({value: l.locationId, label: l.locationName}))]} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Expected Return" type="date" value={formData.expectedReturnDate} onChange={e => setFormData({...formData, expectedReturnDate: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Input label="Condition Before" value={formData.conditionBefore} onChange={e => setFormData({...formData, conditionBefore: e.target.value})} /></Grid>
            <Grid item xs={12}><Input label="Notes" value={formData.notes} onChange={e => setFormData({...formData, notes: e.target.value})} multiline rows={2} /></Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}><Button variant="outline" onClick={() => setShowModal(false)} type="button">Cancel</Button><Button type="submit" variant="primary" loading={isSubmitting}>{editingTransaction ? "Update" : "Create"}</Button></Box>
        </form>
      </Modal>

      <Modal isOpen={showReturnModal} onClose={() => setShowReturnModal(false)} title="Return Asset" size="md">
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <Box sx={{ p: 2, bgcolor: 'var(--surface)', borderRadius: 2 }}><strong>{selectedTransaction?.assetCode} - {selectedTransaction?.assetName}</strong></Box>
          <Input label="Actual Return Date" type="date" value={returnData.actualReturnDate} onChange={e => setReturnData({...returnData, actualReturnDate: e.target.value})} required />
          <Select label="Condition After" value={returnData.conditionAfter} onChange={e => setReturnData({...returnData, conditionAfter: e.target.value})} options={[{value: "Good", label: "Good"}, {value: "Fair", label: "Fair"}, {value: "Poor", label: "Poor"}]} required />
          <Input label="Notes" value={returnData.notes} onChange={e => setReturnData({...returnData, notes: e.target.value})} multiline rows={2} />
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 2 }}><Button variant="outline" onClick={() => setShowReturnModal(false)}>Cancel</Button><Button variant="primary" onClick={handleReturnSubmit} loading={isSubmitting}>Confirm</Button></Box>
        </Box>
      </Modal>
    </div>
  );
};

export default AssetTransactionsMenu;