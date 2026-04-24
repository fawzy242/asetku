import React, { useState, useEffect } from "react";
import { FiSearch, FiUser, FiBox, FiDollarSign, FiCalendar, FiMail, FiPhone, FiMapPin, FiBriefcase, FiRefreshCw } from "react-icons/fi";
import { Grid, Box, Typography, Avatar, Chip, Card as MuiCard, CardContent, LinearProgress } from "@mui/material";
import EmployeeSummaryData from "./EmployeeSummary.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Card from "../../components/atoms/Card/Card";
import Button from "../../components/atoms/Button/Button";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import utilsHelper from "../../core/utils/utils.helper";
import "./EmployeeSummary.scss";

const summaryData = new EmployeeSummaryData();

const EmployeeSummaryMenu = () => {
  const [loading, setLoading] = useState(true);
  const [employees, setEmployees] = useState([]);
  const [selectedEmployee, setSelectedEmployee] = useState(null);
  const [employeeDetail, setEmployeeDetail] = useState(null);
  const [assets, setAssets] = useState([]);
  const [transactions, setTransactions] = useState([]);
  const [loadingData, setLoadingData] = useState(false);

  useEffect(() => { loadEmployees(); }, []);

  const loadEmployees = async () => {
    setLoading(true);
    const r = await summaryData.fetchEmployees();
    if (r.success) setEmployees(r.data);
    setLoading(false);
  };

  const handleEmployeeSelect = async (employeeId) => {
    if (!employeeId) { setSelectedEmployee(null); setEmployeeDetail(null); setAssets([]); setTransactions([]); return; }
    setSelectedEmployee(employeeId);
    setLoadingData(true);
    const [detailRes, assetsRes, transRes] = await Promise.all([
      summaryData.fetchEmployeeDetail(employeeId),
      summaryData.fetchAssets(employeeId),
      summaryData.fetchTransactions(employeeId)
    ]);
    if (detailRes.success) setEmployeeDetail(detailRes.data);
    if (assetsRes.success) setAssets(assetsRes.data);
    if (transRes.success) setTransactions(transRes.data);
    setLoadingData(false);
  };

  const employeeOptions = employees.map(e => ({ value: e.employeeId, label: `${e.employeeCode} - ${e.fullName}` }));

  const totalAssetValue = assets.reduce((sum, a) => sum + (a.purchasePrice || 0), 0);
  const assetByStatus = { Available: 0, Assigned: 0, 'Under Repair': 0, Retired: 0 };
  assets.forEach(a => { if (assetByStatus.hasOwnProperty(a.status)) assetByStatus[a.status]++; });

  const assetColumns = [
    { field: "assetCode", headerName: "Asset Code", width: 130 },
    { field: "assetName", headerName: "Asset Name", width: 200 },
    { field: "categoryName", headerName: "Category", width: 150 },
    { field: "brand", headerName: "Brand", width: 120 },
    { field: "model", headerName: "Model", width: 120 },
    { field: "serialNumber", headerName: "Serial", width: 150 },
    { field: "status", headerName: "Status", width: 130, renderCell: (p) => <Chip label={p.value} size="small" sx={{ bgcolor: p.value === 'Assigned' ? '#3b82f620' : '#10b98120', color: p.value === 'Assigned' ? '#3b82f6' : '#10b981', fontWeight: 500 }} /> },
    { field: "condition", headerName: "Condition", width: 100 },
    { field: "purchasePrice", headerName: "Price", width: 140, valueFormatter: (p) => utilsHelper.formatCurrency(p.value) },
  ];

  const transactionColumns = [
    { field: "transactionDate", headerName: "Date", width: 160, valueFormatter: (p) => utilsHelper.formatDateTime(p.value) },
    { field: "transactionType", headerName: "Type", width: 130 },
    { field: "assetCode", headerName: "Asset", width: 130 },
    { field: "assetName", headerName: "Asset Name", width: 200 },
    { field: "transactionStatus", headerName: "Status", width: 110, renderCell: (p) => <Chip label={p.value} size="small" sx={{ bgcolor: utilsHelper.getStatusColor(p.value) === 'success' ? '#10b98120' : '#f59e0b20', color: utilsHelper.getStatusColor(p.value) === 'success' ? '#10b981' : '#f59e0b', fontWeight: 500 }} /> },
  ];

  if (loading) return <div className="summary-loading"><Spinner size="lg" /></div>;

  return (
    <div className="employee-summary">
      <div className="page-header">
        <h1 className="page-title">Employee Summary</h1>
      </div>

      <Grid container spacing={3}>
        {/* Employee Selector */}
        <Grid item xs={12}>
          <Card>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <FiSearch size={20} style={{ color: 'var(--primary)' }} />
              <Box sx={{ flex: 1, maxWidth: 500 }}>
                <Select
                  label="Select Employee"
                  value={selectedEmployee || ""}
                  onChange={e => handleEmployeeSelect(e.target.value)}
                  options={[{ value: "", label: "Choose an employee..." }, ...employeeOptions]}
                  placeholder="Choose an employee..."
                />
              </Box>
            </Box>
          </Card>
        </Grid>

        {selectedEmployee && (
          <>
            {/* Employee Info */}
            <Grid item xs={12}>
              <Card>
                <Grid container spacing={3} alignItems="center">
                  <Grid item>
                    <Avatar sx={{ width: 80, height: 80, bgcolor: 'var(--primary)', fontSize: 32, fontWeight: 600 }}>
                      {employeeDetail?.fullName?.charAt(0) || '?'}
                    </Avatar>
                  </Grid>
                  <Grid item xs>
                    <Typography variant="h5" fontWeight={700}>{employeeDetail?.fullName || '-'}</Typography>
                    <Box sx={{ display: 'flex', gap: 2, mt: 1, flexWrap: 'wrap' }}>
                      <Chip icon={<FiBriefcase />} label={employeeDetail?.employeeCode || '-'} size="small" variant="outlined" />
                      <Chip icon={<FiBriefcase />} label={employeeDetail?.position || '-'} size="small" variant="outlined" />
                      <Chip icon={<FiBriefcase />} label={employeeDetail?.department || '-'} size="small" variant="outlined" />
                      <Chip icon={<FiMail />} label={employeeDetail?.email || '-'} size="small" variant="outlined" />
                      <Chip icon={<FiPhone />} label={employeeDetail?.phoneNumber || '-'} size="small" variant="outlined" />
                    </Box>
                  </Grid>
                  <Grid item>
                    <Box sx={{ textAlign: 'right' }}>
                      <Typography variant="body2" color="text.secondary">Join Date</Typography>
                      <Typography variant="body1" fontWeight={600}>{utilsHelper.formatDate(employeeDetail?.joinDate) || '-'}</Typography>
                      <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>Status</Typography>
                      <Chip label={employeeDetail?.employmentStatus || '-'} size="small" color={employeeDetail?.employmentStatus === 'Active' ? 'success' : 'default'} />
                    </Box>
                  </Grid>
                </Grid>
              </Card>
            </Grid>

            {/* Stats */}
            <Grid item xs={12} sm={6} md={3}>
              <Card className="summary-stat-card">
                <FiBox size={28} style={{ color: 'var(--primary)' }} />
                <Typography variant="h4" fontWeight={700}>{assets.length}</Typography>
                <Typography variant="body2" color="text.secondary">Total Assets</Typography>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card className="summary-stat-card">
                <FiDollarSign size={28} style={{ color: 'var(--success)' }} />
                <Typography variant="h4" fontWeight={700}>{utilsHelper.formatCurrency(totalAssetValue)}</Typography>
                <Typography variant="body2" color="text.secondary">Total Value</Typography>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card className="summary-stat-card">
                <FiRefreshCw size={28} style={{ color: 'var(--info)' }} />
                <Typography variant="h4" fontWeight={700}>{transactions.length}</Typography>
                <Typography variant="body2" color="text.secondary">Transactions</Typography>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card className="summary-stat-card">
                <FiCalendar size={28} style={{ color: 'var(--warning)' }} />
                <Typography variant="h4" fontWeight={700}>{utilsHelper.formatDate(employeeDetail?.joinDate) || '-'}</Typography>
                <Typography variant="body2" color="text.secondary">Member Since</Typography>
              </Card>
            </Grid>

            {/* Asset Distribution Chart */}
            <Grid item xs={12} md={4}>
              <Card title="Asset Distribution">
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                  {Object.entries(assetByStatus).map(([status, count]) => (
                    <Box key={status}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                        <Typography variant="body2">{status}</Typography>
                        <Typography variant="body2" fontWeight={600}>{count}</Typography>
                      </Box>
                      <LinearProgress 
                        variant="determinate" 
                        value={assets.length > 0 ? (count / assets.length) * 100 : 0} 
                        sx={{ height: 8, borderRadius: 4, bgcolor: 'var(--surface)', '& .MuiLinearProgress-bar': { bgcolor: status === 'Assigned' ? '#3b82f6' : status === 'Available' ? '#10b981' : status === 'Under Repair' ? '#f59e0b' : '#6b7280', borderRadius: 4 } }} 
                      />
                    </Box>
                  ))}
                </Box>
              </Card>
            </Grid>

            {/* Assigned Assets */}
            <Grid item xs={12} md={8}>
              <Card title={`Assets (${assets.length})`}>
                {loadingData ? <div className="summary-loading"><Spinner size="lg" /></div> : assets.length === 0 ? (
                  <Box sx={{ textAlign: 'center', py: 4, color: 'var(--text-secondary)' }}><FiBox size={40} /><Typography>No assets assigned</Typography></Box>
                ) : (
                  <DataTable rows={assets} columns={assetColumns} pageSize={5} getRowId={(row) => row.assetId} hideFooter={true} />
                )}
              </Card>
            </Grid>

            {/* Recent Transactions */}
            <Grid item xs={12}>
              <Card title={`Recent Transactions (${transactions.length})`}>
                {transactions.length === 0 ? (
                  <Box sx={{ textAlign: 'center', py: 4, color: 'var(--text-secondary)' }}><FiRefreshCw size={40} /><Typography>No transactions</Typography></Box>
                ) : (
                  <DataTable rows={transactions.slice(0, 10)} columns={transactionColumns} pageSize={5} getRowId={(row, i) => i} hideFooter={true} />
                )}
              </Card>
            </Grid>
          </>
        )}
      </Grid>
    </div>
  );
};

export default EmployeeSummaryMenu;