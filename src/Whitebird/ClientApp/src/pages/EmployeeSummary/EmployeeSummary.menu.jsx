import React, { useState, useEffect, useCallback, useRef, useMemo } from "react";
import { FiSearch, FiUser, FiBox, FiDollarSign, FiCalendar, FiMail, FiPhone, FiBriefcase, FiRefreshCw, FiAlertTriangle, FiLayers, FiCheckCircle, FiClock, FiTool, FiPieChart } from "react-icons/fi";
import { Grid, Box, Typography, Avatar, Chip, Paper, IconButton, Tooltip } from "@mui/material";
import { useLocation } from "react-router-dom";
import EmployeeSummaryData from "./EmployeeSummary.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Card from "../../components/atoms/Card/Card";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import Skeleton from "../../components/atoms/Skeleton/Skeleton";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { useSweetAlert } from "../../hooks/useSweetAlert";
import utilsHelper from "../../core/utils/utils.helper";
import "./EmployeeSummary.scss";

const summaryData = new EmployeeSummaryData();

const assetColumns = [
  { field: "assetCode", headerName: "Asset Code", width: 130 },
  { field: "assetName", headerName: "Asset Name", flex: 1, minWidth: 180 },
  { field: "categoryName", headerName: "Category", width: 150 },
  { field: "status", headerName: "Status", width: 140, renderCell: (p) => {
    const status = p?.value || '-';
    return <Chip label={status} size="small" sx={getStatusChipStyles(status)} />;
  }},
  { field: "associationType", headerName: "Type", width: 110 },
  { 
    field: "sinceDate", 
    headerName: "Since", 
    width: 120, 
    valueFormatter: (p) => p?.value ? utilsHelper.formatDate(p.value) : '-' 
  },
  { 
    field: "expectedReturnDate", 
    headerName: "Expected Return", 
    width: 130, 
    valueFormatter: (p) => p?.value ? utilsHelper.formatDate(p.value) : '-' 
  },
  { 
    field: "isOverdue", 
    headerName: "Overdue", 
    width: 90, 
    renderCell: (p) => p?.value ? (
      <Chip label="Yes" size="small" className="employee-summary__chip-overdue" />
    ) : (
      <Chip label="No" size="small" className="employee-summary__chip-ontime" />
    )
  },
  { field: "conditionName", headerName: "Condition", width: 100 },
  { 
    field: "purchasePrice", 
    headerName: "Purchase Price", 
    width: 130, 
    valueFormatter: (p) => p?.value ? utilsHelper.formatCurrency(p.value) : '-' 
  },
];

const historyColumns = [
  { 
    field: "transactionDate", 
    headerName: "Date", 
    width: 180, 
    valueFormatter: (p) => p?.value ? utilsHelper.formatDateTime(p.value) : '-' 
  },
  { field: "transactionTypeName", headerName: "Type", width: 150 },
  { field: "assetCode", headerName: "Asset", width: 130 },
  { field: "assetName", headerName: "Asset Name", flex: 1, minWidth: 180 },
  { field: "fromEmployeeName", headerName: "From", width: 150 },
  { field: "toEmployeeName", headerName: "To", width: 150 },
  { field: "conditionAfterName", headerName: "Condition", width: 100 },
  { field: "notes", headerName: "Notes", width: 200 },
];

const EmployeeSummaryMenu = () => {
  const [loading, setLoading] = useState(true);
  const [employees, setEmployees] = useState([]);
  const [selectedEmployeeId, setSelectedEmployeeId] = useState(null);
  const [employeeDetail, setEmployeeDetail] = useState(null);
  const [assetSummary, setAssetSummary] = useState(null);
  const [loadingData, setLoadingData] = useState(false);
  const isMountedRef = useRef(true);
  const location = useLocation();
  const { toast } = useSweetAlert();

  useEffect(() => {
    isMountedRef.current = true;
    loadEmployees();
    return () => {
      isMountedRef.current = false;
    };
  }, []);

  const loadEmployees = async () => {
    setLoading(true);
    const r = await summaryData.fetchEmployees();
    if (isMountedRef.current && r.success) {
      setEmployees(r.data);
      const params = new URLSearchParams(location.search);
      const employeeIdParam = params.get('employeeId');
      if (employeeIdParam) {
        const employeeId = parseInt(employeeIdParam, 10);
        const employeeExists = r.data.some(e => e.employeeId === employeeId);
        if (employeeExists) {
          handleEmployeeSelect(employeeId);
        }
      }
    }
    if (isMountedRef.current) {
      setLoading(false);
    }
  };

  const handleEmployeeSelect = useCallback(async (employeeId) => {
    if (!employeeId) { 
      setSelectedEmployeeId(null); 
      setEmployeeDetail(null); 
      setAssetSummary(null); 
      return; 
    }
    setSelectedEmployeeId(employeeId);
    setLoadingData(true);
    const [detailRes, summaryRes] = await Promise.all([
      summaryData.fetchEmployeeDetail(employeeId),
      summaryData.fetchAssetSummary(employeeId)
    ]);
    if (isMountedRef.current) {
      if (detailRes.success) setEmployeeDetail(detailRes.data);
      if (summaryRes.success) {
        setAssetSummary(summaryRes.data);
      } else {
        setAssetSummary({
          currentlyHeldAssets: 0,
          assetsOnLoan: 0,
          maintenanceAssets: 0,
          overdueLoans: 0,
          totalHistoricalAssets: 0,
          returnedAssets: 0,
          damagedReturns: 0,
          currentAssets: [],
          assetHistory: []
        });
      }
      setLoadingData(false);
    }
  }, []);

  const handleRefresh = useCallback(async () => {
    if (!selectedEmployeeId) return;
    setLoadingData(true);
    const [detailRes, summaryRes] = await Promise.all([
      summaryData.fetchEmployeeDetail(selectedEmployeeId),
      summaryData.fetchAssetSummary(selectedEmployeeId)
    ]);
    if (isMountedRef.current) {
      if (detailRes.success) setEmployeeDetail(detailRes.data);
      if (summaryRes.success) {
        setAssetSummary(summaryRes.data);
      }
      setLoadingData(false);
      toast.success('Data refreshed');
    }
  }, [selectedEmployeeId, toast]);

  useEffect(() => {
    if (employees.length > 0) {
      const params = new URLSearchParams(location.search);
      const employeeIdParam = params.get('employeeId');
      if (employeeIdParam) {
        const employeeId = parseInt(employeeIdParam, 10);
        const employeeExists = employees.some(e => e.employeeId === employeeId);
        if (employeeExists && selectedEmployeeId !== employeeId) {
          handleEmployeeSelect(employeeId);
        }
      }
    }
  }, [location.search, employees, selectedEmployeeId, handleEmployeeSelect]);

  const totalAssetValue = (assetSummary?.currentAssets || []).reduce(
    (sum, a) => sum + (a.purchasePrice || 0), 0
  );

  const maintenanceAssets = (assetSummary?.currentAssets || []).filter(
    a => a.status === 'In Maintenance' || a.associationType === 'Maintenance'
  ).length;

  // Asset Category Summary
  const categorySummary = useMemo(() => {
    const currentAssets = assetSummary?.currentAssets || [];
    if (currentAssets.length === 0) return [];
    
    const categoryMap = {};
    currentAssets.forEach(asset => {
      const category = asset.categoryName || 'Uncategorized';
      categoryMap[category] = (categoryMap[category] || 0) + 1;
    });
    
    return Object.entries(categoryMap)
      .map(([category, count]) => ({ category, count }))
      .sort((a, b) => b.count - a.count);
  }, [assetSummary?.currentAssets]);

  const employeeOptions = useMemo(() => [
    { value: "", label: "Choose an employee..." },
    ...employees.map(e => ({ value: e.employeeId, label: `${e.employeeCode} - ${e.fullName}` }))
  ], [employees]);

  const currentAssets = assetSummary?.currentAssets || [];
  const assetHistory = assetSummary?.assetHistory || [];

  if (loading) {
    return (
      <div className="employee-summary">
        <div className="page-header">
          <h1 className="page-title">Employee Summary</h1>
          <p className="page-description">View asset assignments and transaction history by employee</p>
        </div>
        <div className="page-loading"><Spinner size="lg" /></div>
      </div>
    );
  }

  return (
    <div className="employee-summary">
      <div className="page-header">
        <h1 className="page-title">Employee Summary</h1>
        <p className="page-description">View asset assignments and transaction history by employee</p>
      </div>
      
      <Grid container spacing={3}>
        {/* Employee Selection Card */}
        <Grid item xs={12}>
          <Card className="employee-summary__selection-card">
            <div className="employee-summary__selection-content">
              <div className="employee-summary__selection-icon">
                <FiSearch size={24} />
              </div>
              <div className="employee-summary__selection-select">
                <Select 
                  label="Select Employee" 
                  value={selectedEmployeeId || ""} 
                  onChange={e => handleEmployeeSelect(e.target.value)}
                  options={employeeOptions} 
                />
              </div>
              {selectedEmployeeId && (
                <Tooltip title="Refresh">
                  <IconButton 
                    onClick={handleRefresh} 
                    size="small" 
                    className="employee-summary__refresh-btn"
                  >
                    <FiRefreshCw size={18} />
                  </IconButton>
                </Tooltip>
              )}
            </div>
          </Card>
        </Grid>
        
        {selectedEmployeeId && !loadingData && employeeDetail && (
          <>
            {/* Employee Profile Card */}
            <Grid item xs={12}>
              <Card className="employee-summary__profile-card">
                <Grid container spacing={3} alignItems="center">
                  <Grid item>
                    <Avatar className="employee-summary__profile-avatar">
                      {employeeDetail?.fullName?.charAt(0) || '?'}
                    </Avatar>
                  </Grid>
                  <Grid item xs>
                    <Typography variant="h5" fontWeight={700} className="employee-summary__profile-name">
                      {employeeDetail?.fullName || '-'}
                    </Typography>
                    <div className="employee-summary__profile-tags">
                      <Chip icon={<FiBriefcase />} label={employeeDetail?.employeeCode || '-'} size="small" variant="outlined" />
                      <Chip icon={<FiBriefcase />} label={employeeDetail?.positionName || '-'} size="small" variant="outlined" />
                      <Chip icon={<FiBriefcase />} label={employeeDetail?.departmentName || '-'} size="small" variant="outlined" />
                      <Chip icon={<FiMail />} label={employeeDetail?.email || '-'} size="small" variant="outlined" />
                      <Chip icon={<FiPhone />} label={employeeDetail?.phoneNumber || '-'} size="small" variant="outlined" />
                      <Chip label={employeeDetail?.employmentStatusName || '-'} size="small" sx={getStatusChipStyles(employeeDetail?.employmentStatusName)} />
                    </div>
                  </Grid>
                  <Grid item>
                    <div className="employee-summary__info-row">
                      <FiCalendar size={16} className="employee-summary__info-icon" />
                      <span className="employee-summary__info-label">Join Date:</span>
                      <span className="employee-summary__info-value">
                        {utilsHelper.formatDate(employeeDetail?.joinDate) || '-'}
                      </span>
                    </div>
                    {employeeDetail?.resignDate && (
                      <div className="employee-summary__info-row">
                        <FiCalendar size={16} className="employee-summary__info-icon" />
                        <span className="employee-summary__info-label">Resign Date:</span>
                        <span className="employee-summary__info-value">
                          {utilsHelper.formatDate(employeeDetail?.resignDate)}
                        </span>
                      </div>
                    )}
                  </Grid>
                </Grid>
              </Card>
            </Grid>

            {/* Statistics Cards Row */}
            <Grid item xs={12}>
              <Grid container spacing={2}>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiBox size={28} className="employee-summary__stat-icon" style={{ color: '#dc2626' }} />
                    <Typography variant="h4" fontWeight={700}>{assetSummary?.currentlyHeldAssets || currentAssets.length}</Typography>
                    <Typography variant="caption" color="text.secondary">Total Assets</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiLayers size={28} className="employee-summary__stat-icon" style={{ color: '#8b5cf6' }} />
                    <Typography variant="h4" fontWeight={700}>{assetSummary?.assetsOnLoan || 0}</Typography>
                    <Typography variant="caption" color="text.secondary">On Loan</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiTool size={28} className="employee-summary__stat-icon" style={{ color: '#f59e0b' }} />
                    <Typography variant="h4" fontWeight={700}>{assetSummary?.maintenanceAssets || maintenanceAssets}</Typography>
                    <Typography variant="caption" color="text.secondary">In Maintenance</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiDollarSign size={28} className="employee-summary__stat-icon" style={{ color: '#10b981' }} />
                    <Typography variant="h4" fontWeight={700}>{utilsHelper.formatCurrency(totalAssetValue)}</Typography>
                    <Typography variant="caption" color="text.secondary">Total Value</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiRefreshCw size={28} className="employee-summary__stat-icon" style={{ color: '#3b82f6' }} />
                    <Typography variant="h4" fontWeight={700}>{assetSummary?.returnedAssets || assetHistory.filter(t => 
                      t.transactionTypeName === 'RETURN' || t.transactionTypeName === 'LOAN_RETURN'
                    ).length}</Typography>
                    <Typography variant="caption" color="text.secondary">Returns</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiAlertTriangle size={28} className="employee-summary__stat-icon" style={{ color: '#ef4444' }} />
                    <Typography variant="h4" fontWeight={700}>{assetSummary?.damagedReturns || 0}</Typography>
                    <Typography variant="caption" color="text.secondary">Damaged</Typography>
                  </Paper>
                </Grid>
              </Grid>
            </Grid>

            {/* Asset Category Summary Card */}
            <Grid item xs={12} md={6}>
              <Card 
                title="Asset Category Summary" 
                subtitle="Distribution of assets by category"
              >
                {currentAssets.length === 0 ? (
                  <div className="employee-summary__empty-state" style={{ padding: '24px' }}>
                    <FiPieChart size={32} />
                    <Typography variant="body2">No assets to summarize</Typography>
                  </div>
                ) : (
                  <div className="employee-summary__category-grid">
                    {categorySummary.map(({ category, count }) => (
                      <div key={category} className="employee-summary__category-item">
                        <span className="employee-summary__category-name">{category}</span>
                        <span className="employee-summary__category-count">{count}</span>
                      </div>
                    ))}
                  </div>
                )}
              </Card>
            </Grid>

            {/* Last Transaction Card */}
            <Grid item xs={12} md={6}>
              <Card 
                title="Last Transaction" 
                subtitle="Most recent activity"
              >
                {assetHistory.length === 0 ? (
                  <div className="employee-summary__empty-state" style={{ padding: '24px' }}>
                    <FiRefreshCw size={32} />
                    <Typography variant="body2">No transactions found</Typography>
                  </div>
                ) : (
                  <div className="employee-summary__last-transaction">
                    <div className="employee-summary__last-transaction-row">
                      <span className="employee-summary__last-transaction-label">Date:</span>
                      <span className="employee-summary__last-transaction-value">
                        {utilsHelper.formatDateTime(assetHistory[0]?.transactionDate) || '-'}
                      </span>
                    </div>
                    <div className="employee-summary__last-transaction-row">
                      <span className="employee-summary__last-transaction-label">Type:</span>
                      <span className="employee-summary__last-transaction-value">
                        {assetHistory[0]?.transactionTypeName || '-'}
                      </span>
                    </div>
                    <div className="employee-summary__last-transaction-row">
                      <span className="employee-summary__last-transaction-label">Asset:</span>
                      <span className="employee-summary__last-transaction-value">
                        {assetHistory[0]?.assetCode || '-'} - {assetHistory[0]?.assetName || '-'}
                      </span>
                    </div>
                    <div className="employee-summary__last-transaction-row">
                      <span className="employee-summary__last-transaction-label">Condition:</span>
                      <span className="employee-summary__last-transaction-value">
                        {assetHistory[0]?.conditionAfterName || '-'}
                      </span>
                    </div>
                  </div>
                )}
              </Card>
            </Grid>

            {/* Current Assets Table */}
            <Grid item xs={12}>
              <Card 
                title="Current Assets" 
                subtitle={`${currentAssets.length} asset(s) currently assigned`}
              >
                {loadingData ? (
                  <Box sx={{ py: 4 }}><Skeleton variant="rect" height={300} /></Box>
                ) : currentAssets.length === 0 ? (
                  <div className="employee-summary__empty-state">
                    <FiBox size={48} />
                    <Typography variant="h6" gutterBottom>No Assets Assigned</Typography>
                    <Typography variant="body2">This employee has no assets currently assigned.</Typography>
                  </div>
                ) : (
                  <DataTable 
                    rows={currentAssets} 
                    columns={assetColumns} 
                    pageSize={10} 
                    getRowId={(row) => row.assetId || `asset-${Math.random()}`} 
                    hideFooter={false}
                    ariaLabel="Employee current assets table"
                  />
                )}
              </Card>
            </Grid>

            {/* Asset History Table */}
            <Grid item xs={12}>
              <Card 
                title="Asset History" 
                subtitle={`${assetHistory.length} transaction(s) found`}
              >
                {assetHistory.length === 0 ? (
                  <div className="employee-summary__empty-state">
                    <FiRefreshCw size={48} />
                    <Typography variant="h6" gutterBottom>No Transaction History</Typography>
                    <Typography variant="body2">No asset transactions found for this employee.</Typography>
                  </div>
                ) : (
                  <DataTable 
                    rows={assetHistory} 
                    columns={historyColumns} 
                    pageSize={10} 
                    getRowId={(row) => row.assetTransactionId || `hist-${Math.random()}`} 
                    hideFooter={false}
                    ariaLabel="Employee asset history table"
                  />
                )}
              </Card>
            </Grid>
          </>
        )}
        
        {selectedEmployeeId && loadingData && (
          <Grid item xs={12}>
            <Card>
              <div className="employee-summary__loading-container">
                <Spinner size="lg" />
              </div>
            </Card>
          </Grid>
        )}

        {!selectedEmployeeId && !loading && (
          <Grid item xs={12}>
            <Card>
              <div className="employee-summary__empty-state">
                <FiUser size={64} />
                <Typography variant="h6" gutterBottom>Select an Employee to View Summary</Typography>
                <Typography variant="body2">
                  Choose an employee from the dropdown above to view their asset summary and transaction history.
                </Typography>
              </div>
            </Card>
          </Grid>
        )}
      </Grid>
    </div>
  );
};

export default EmployeeSummaryMenu;