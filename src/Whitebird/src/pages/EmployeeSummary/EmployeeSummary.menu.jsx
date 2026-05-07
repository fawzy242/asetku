import React, { useState, useEffect, useCallback } from "react";
import { FiSearch, FiUser, FiBox, FiDollarSign, FiCalendar, FiMail, FiPhone, FiBriefcase, FiRefreshCw, FiAlertTriangle, FiLayers } from "react-icons/fi";
import { Grid, Box, Typography, Avatar, Chip } from "@mui/material";
import EmployeeSummaryData from "./EmployeeSummary.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Card from "../../components/atoms/Card/Card";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import utilsHelper from "../../core/utils/utils.helper";
import "./EmployeeSummary.scss";

const summaryData = new EmployeeSummaryData();

const EmployeeSummaryMenu = () => {
  const [loading, setLoading] = useState(true);
  const [employees, setEmployees] = useState([]);
  const [selectedEmployee, setSelectedEmployee] = useState(null);
  const [employeeDetail, setEmployeeDetail] = useState(null);
  const [assetSummary, setAssetSummary] = useState(null);
  const [loadingData, setLoadingData] = useState(false);
  const [useNewEndpoint, setUseNewEndpoint] = useState(true);

  useEffect(() => { loadEmployees(); }, []);

  const loadEmployees = async () => {
    setLoading(true);
    const r = await summaryData.fetchEmployees();
    if (r.success) setEmployees(r.data);
    setLoading(false);
  };

  const handleEmployeeSelect = useCallback(async (employeeId) => {
    if (!employeeId) { setSelectedEmployee(null); setEmployeeDetail(null); setAssetSummary(null); return; }
    setSelectedEmployee(employeeId);
    setLoadingData(true);
    if (useNewEndpoint) {
      const detailRes = await summaryData.fetchEmployeeDetail(employeeId);
      const summaryRes = await summaryData.fetchAssetSummary(employeeId);
      if (detailRes.success) setEmployeeDetail(detailRes.data);
      if (summaryRes.success) { setAssetSummary(summaryRes.data); }
      else {
        setUseNewEndpoint(false);
        const [assetsRes, transRes] = await Promise.all([summaryData.fetchAssets(employeeId), summaryData.fetchTransactions(employeeId)]);
        setAssetSummary({ currentlyHeldAssets: (assetsRes.data || []).length, assetsOnLoan: (assetsRes.data || []).filter(a => a.status === 'On Loan').length, overdueLoans: 0, totalHistoricalAssets: 0, returnedAssets: (transRes.data || []).filter(t => t.transactionType === 'RETURN' || t.transactionType === 'LOAN_RETURN').length, damagedReturns: 0, currentAssets: assetsRes.data || [], assetHistory: transRes.data || [] });
      }
    } else {
      const [detailRes, assetsRes, transRes] = await Promise.all([summaryData.fetchEmployeeDetail(employeeId), summaryData.fetchAssets(employeeId), summaryData.fetchTransactions(employeeId)]);
      if (detailRes.success) setEmployeeDetail(detailRes.data);
      setAssetSummary({ currentlyHeldAssets: (assetsRes.data || []).length, assetsOnLoan: (assetsRes.data || []).filter(a => a.status === 'On Loan').length, overdueLoans: 0, totalHistoricalAssets: 0, returnedAssets: (transRes.data || []).filter(t => t.transactionType === 'RETURN' || t.transactionType === 'LOAN_RETURN').length, damagedReturns: 0, currentAssets: assetsRes.data || [], assetHistory: transRes.data || [] });
    }
    setLoadingData(false);
  }, [useNewEndpoint]);

  const getStatusChip = (status) => <Chip label={status || '-'} size="small" sx={getStatusChipStyles(status)} />;
  const totalAssetValue = (assetSummary?.currentAssets || []).reduce((sum, a) => sum + (a.purchasePrice || 0), 0);

  const assetColumns = [
    { field: "assetCode", headerName: "Asset Code", width: 130 },
    { field: "assetName", headerName: "Asset Name", flex: 1, minWidth: 180 },
    { field: "categoryName", headerName: "Category", width: 150 },
    { field: "status", headerName: "Status", width: 140, renderCell: (p) => getStatusChip(p?.value) },
    { field: "associationType", headerName: "Type", width: 110 },
    { field: "sinceDate", headerName: "Since", width: 120, valueFormatter: (p) => p?.value ? utilsHelper.formatDate(p.value) : '-' },
    { field: "isOverdue", headerName: "Overdue", width: 90, renderCell: (p) => p?.value ? <Chip label="Yes" size="small" sx={{ bgcolor: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', fontWeight: 500, fontSize: '0.75rem', height: 24, borderRadius: '4px' }} /> : null },
    { field: "condition", headerName: "Condition", width: 100 },
  ];

  const historyColumns = [
    { field: "transactionDate", headerName: "Date", width: 160, valueFormatter: (p) => p?.value ? utilsHelper.formatDateTime(p.value) : '-' },
    { field: "transactionType", headerName: "Type", width: 150 },
    { field: "assetCode", headerName: "Asset", width: 130 },
    { field: "assetName", headerName: "Asset Name", flex: 1, minWidth: 180 },
    { field: "fromEmployeeName", headerName: "From", width: 150 },
    { field: "toEmployeeName", headerName: "To", width: 150 },
    { field: "conditionAfter", headerName: "Condition", width: 100 },
  ];

  if (loading) return <div className="page-loading"><Spinner size="lg" /></div>;
  const currentAssets = assetSummary?.currentAssets || [];
  const assetHistory = assetSummary?.assetHistory || [];

  return (
    <div className="employee-summary">
      <div className="page-header"><h1 className="page-title">Employee Summary</h1></div>
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Card>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <FiSearch size={20} style={{ color: 'var(--primary)' }} />
              <Box sx={{ flex: 1, maxWidth: 500 }}>
                <Select label="Select Employee" value={selectedEmployee || ""} onChange={e => handleEmployeeSelect(e.target.value)}
                  options={[{ value: "", label: "Choose an employee..." }, ...employees.map(e => ({ value: e.employeeId, label: `${e.employeeCode} - ${e.fullName}` }))]} />
              </Box>
            </Box>
          </Card>
        </Grid>
        {selectedEmployee && (<>
          <Grid item xs={12}>
            <Card>
              <Grid container spacing={3} alignItems="center">
                <Grid item><Avatar sx={{ width: 80, height: 80, bgcolor: 'var(--primary)', fontSize: 32, fontWeight: 600 }}>{employeeDetail?.fullName?.charAt(0) || '?'}</Avatar></Grid>
                <Grid item xs>
                  <Typography variant="h5" fontWeight={700}>{employeeDetail?.fullName || '-'}</Typography>
                  <Box sx={{ display: 'flex', gap: 2, mt: 1, flexWrap: 'wrap' }}>
                    <Chip icon={<FiBriefcase />} label={employeeDetail?.employeeCode || '-'} size="small" variant="outlined" />
                    <Chip icon={<FiBriefcase />} label={employeeDetail?.position || '-'} size="small" variant="outlined" />
                    <Chip icon={<FiBriefcase />} label={employeeDetail?.department || '-'} size="small" variant="outlined" />
                    <Chip icon={<FiMail />} label={employeeDetail?.email || '-'} size="small" variant="outlined" />
                    <Chip icon={<FiPhone />} label={employeeDetail?.phoneNumber || '-'} size="small" variant="outlined" />
                    {getStatusChip(employeeDetail?.employmentStatus)}
                  </Box>
                </Grid>
                <Grid item><Box sx={{ textAlign: 'right' }}><Typography variant="body2" color="text.secondary">Join Date</Typography><Typography variant="body1" fontWeight={600}>{utilsHelper.formatDate(employeeDetail?.joinDate) || '-'}</Typography></Box></Grid>
              </Grid>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={2}><Card className="summary-stat-card"><FiBox size={28} style={{ color: 'var(--primary)' }} /><Typography variant="h4" fontWeight={700}>{assetSummary?.currentlyHeldAssets || currentAssets.length}</Typography><Typography variant="body2" color="text.secondary">Total Assets</Typography></Card></Grid>
          <Grid item xs={12} sm={6} md={2}><Card className="summary-stat-card"><FiLayers size={28} style={{ color: '#8b5cf6' }} /><Typography variant="h4" fontWeight={700}>{assetSummary?.assetsOnLoan || 0}</Typography><Typography variant="body2" color="text.secondary">On Loan</Typography></Card></Grid>
          <Grid item xs={12} sm={6} md={2}><Card className="summary-stat-card"><FiDollarSign size={28} style={{ color: 'var(--success)' }} /><Typography variant="h4" fontWeight={700}>{utilsHelper.formatCurrency(totalAssetValue)}</Typography><Typography variant="body2" color="text.secondary">Total Value</Typography></Card></Grid>
          <Grid item xs={12} sm={6} md={2}><Card className="summary-stat-card"><FiRefreshCw size={28} style={{ color: 'var(--info)' }} /><Typography variant="h4" fontWeight={700}>{assetSummary?.returnedAssets || assetHistory.filter(t => t.transactionType === 'RETURN' || t.transactionType === 'LOAN_RETURN').length}</Typography><Typography variant="body2" color="text.secondary">Returns</Typography></Card></Grid>
          <Grid item xs={12} sm={6} md={2}><Card className="summary-stat-card"><FiAlertTriangle size={28} style={{ color: 'var(--error)' }} /><Typography variant="h4" fontWeight={700}>{assetSummary?.damagedReturns || assetHistory.filter(t => t.damageReason).length}</Typography><Typography variant="body2" color="text.secondary">Damaged</Typography></Card></Grid>
          <Grid item xs={12} sm={6} md={2}><Card className="summary-stat-card"><FiAlertTriangle size={28} style={{ color: 'var(--warning)' }} /><Typography variant="h4" fontWeight={700}>{assetSummary?.overdueLoans || 0}</Typography><Typography variant="body2" color="text.secondary">Overdue</Typography></Card></Grid>
          <Grid item xs={12}>
            <Card title={`Current Assets (${currentAssets.length})`}>
              {loadingData ? <div className="page-loading"><Spinner size="lg" /></div> : currentAssets.length === 0 ? (
                <Box sx={{ textAlign: 'center', py: 4, color: 'var(--text-secondary)' }}><FiBox size={40} /><Typography>No assets assigned</Typography></Box>
              ) : <DataTable rows={currentAssets} columns={assetColumns} pageSize={10} getRowId={(row) => row.assetId || `asset-${Math.random()}`} hideFooter={true} ariaLabel="Employee current assets table" />}
            </Card>
          </Grid>
          <Grid item xs={12}>
            <Card title={`Asset History (${assetHistory.length})`}>
              {assetHistory.length === 0 ? (
                <Box sx={{ textAlign: 'center', py: 4, color: 'var(--text-secondary)' }}><FiRefreshCw size={40} /><Typography>No transaction history</Typography></Box>
              ) : <DataTable rows={assetHistory.slice(0, 20)} columns={historyColumns} pageSize={10} getRowId={(row) => row.assetTransactionId || `hist-${Math.random()}`} hideFooter={true} ariaLabel="Employee asset history table" />}
            </Card>
          </Grid>
        </>)}
      </Grid>
    </div>
  );
};

export default EmployeeSummaryMenu;