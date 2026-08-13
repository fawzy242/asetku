// src/pages/AssetTracking/AssetTracking.menu.jsx
import React, { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { FiSearch, FiBox, FiUser, FiMapPin, FiRefreshCw, FiCalendar, FiClock, FiAlertTriangle, FiActivity, FiTool, FiMove, FiPieChart, FiList, FiUsers } from 'react-icons/fi';
import { Grid, Box, Typography, Avatar, Chip, Paper, IconButton, Tooltip, Tabs as MuiTabs, Tab, Card as MuiCard } from '@mui/material';
import { useLocation } from 'react-router-dom';
import AssetTrackingData from './AssetTracking.data';
import DataTable from '../../components/molecules/DataTable/DataTable';
import Card from '../../components/atoms/Card/Card';
import Select from '../../components/atoms/Select/Select';
import Spinner from '../../components/atoms/Spinner/Spinner';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import InfoRow from '../../components/molecules/InfoRow/InfoRow';
import { useOptions } from '../../hooks/useOptions';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import utilsHelper from '../../core/utils/utils.helper';
import './AssetTracking.scss';

const trackingData = new AssetTrackingData();

// Timeline columns
const timelineColumns = [
  {
    field: 'date',
    headerName: 'Date & Time',
    width: 180,
    renderCell: (params) => {
      const val = params?.row?.date ?? params?.value;
      return val ? <span>{utilsHelper.formatDateTime(val)}</span> : <span className="u-text-muted">-</span>;
    },
  },
  {
    field: 'activityType',
    headerName: 'Activity Type',
    width: 160,
    renderCell: (params) => {
      const val = params?.row?.activityType ?? params?.value ?? '-';
      return <span>{val}</span>;
    },
  },
  {
    field: 'description',
    headerName: 'Description',
    flex: 1,
    minWidth: 200,
    renderCell: (params) => {
      const val = params?.row?.description ?? params?.value ?? '-';
      return <span>{val}</span>;
    },
  },
  {
    field: 'employeeName',
    headerName: 'Employee',
    width: 150,
    renderCell: (params) => {
      const val = params?.row?.employeeName ?? params?.value ?? '-';
      return <span>{val}</span>;
    },
  },
  {
    field: 'status',
    headerName: 'Status',
    width: 120,
    renderCell: (params) => {
      const val = params?.row?.status ?? params?.value ?? '-';
      return <span>{val}</span>;
    },
  },
  {
    field: 'notes',
    headerName: 'Notes',
    width: 150,
    renderCell: (params) => {
      const val = params?.row?.notes ?? params?.value ?? '-';
      return <span>{val}</span>;
    },
  },
];

// Employee history columns
const employeeHistoryColumns = [
  { field: 'employeeCode', headerName: 'Code', width: 120 },
  { field: 'fullName', headerName: 'Full Name', flex: 1, minWidth: 180 },
  { field: 'departmentName', headerName: 'Department', width: 150 },
  { field: 'positionName', headerName: 'Position', width: 150 },
  {
    field: 'lastTransactionDate',
    headerName: 'Last Transaction',
    width: 180,
    renderCell: (params) => {
      const val = params?.row?.lastTransactionDate ?? params?.value;
      return val ? <span>{utilsHelper.formatDateTime(val)}</span> : <span className="u-text-muted">-</span>;
    },
  },
  { field: 'totalTransactions', headerName: 'Total Txns', width: 120 },
  { field: 'assetsHeld', headerName: 'Assets Held', width: 120 },
];

// Schedule stats columns
const scheduleStatsColumns = [
  {
    field: 'scheduleDate',
    headerName: 'Schedule Date',
    width: 180,
    renderCell: (params) => {
      const val = params?.row?.scheduleDate ?? params?.value;
      return val ? <span>{utilsHelper.formatDateTime(val)}</span> : <span className="u-text-muted">-</span>;
    },
  },
  { field: 'assetCode', headerName: 'Asset Code', width: 120 },
  { field: 'assetName', headerName: 'Asset Name', flex: 1, minWidth: 180 },
  { field: 'employeeName', headerName: 'Employee', width: 150 },
  { field: 'type', headerName: 'Type', width: 120 },
  {
    field: 'status',
    headerName: 'Status',
    width: 120,
    renderCell: (params) => {
      const val = params?.row?.status ?? 'Pending';
      const isCompleted = val === 'Completed';
      return isCompleted ? 
        <Chip label="Completed" size="small" color="success" /> : 
        <Chip label="Pending" size="small" color="warning" />;
    },
  },
];

// Transaction type stats chart data
const transactionTypeColors = {
  'HANDOVER': '#3b82f6',
  'TRANSFER': '#8b5cf6',
  'LOAN': '#f59e0b',
  'RETURN': '#10b981',
  'LOAN_RETURN': '#34d399',
  'MAINTENANCE': '#f97316',
  'POST_MAINTENANCE': '#14b8a6',
  'DISPOSAL': '#ef4444',
  'OPERATIONAL_OFFICE': '#6366f1',
};

const AssetTrackingMenu = () => {
  const [loading, setLoading] = useState(true);
  const [assets, setAssets] = useState([]);
  const [selectedAssetId, setSelectedAssetId] = useState(null);
  const [trackingData_, setTrackingData_] = useState(null);
  const [statsData, setStatsData] = useState(null);
  const [timelineData, setTimelineData] = useState([]);
  const [scheduleStats, setScheduleStats] = useState([]);
  const [scheduleTotal, setScheduleTotal] = useState(0);
  const [schedulePage, setSchedulePage] = useState(1);
  const [schedulePageSize, setSchedulePageSize] = useState(10);
  const [scheduleLoading, setScheduleLoading] = useState(false);
  const [employeeHistory, setEmployeeHistory] = useState([]);
  const [employeeTotal, setEmployeeTotal] = useState(0);
  const [employeePage, setEmployeePage] = useState(1);
  const [employeePageSize, setEmployeePageSize] = useState(10);
  const [employeeLoading, setEmployeeLoading] = useState(false);
  const [transactionTypeStats, setTransactionTypeStats] = useState(null);
  const [loadingData, setLoadingData] = useState(false);
  const [historyFilter, setHistoryFilter] = useState('all');
  const isMountedRef = useRef(true);
  const location = useLocation();
  const { toast } = useSweetAlert();

  // ============================================================
  // LOAD ASSETS FOR DROPDOWN
  // ============================================================
  useEffect(() => {
    isMountedRef.current = true;
    loadAssets();
    return () => { isMountedRef.current = false; };
  }, []);

  const loadAssets = async () => {
    setLoading(true);
    const assetsRes = await trackingData.fetchAssets();
    if (isMountedRef.current && assetsRes.success) {
      const assetsData = Array.isArray(assetsRes.data) ? assetsRes.data : [];
      setAssets(assetsData);
      
      const params = new URLSearchParams(location.search);
      const assetIdParam = params.get('assetId');
      if (assetIdParam) {
        const assetId = parseInt(assetIdParam, 10);
        const assetExists = assetsData.some(a => a.assetId === assetId);
        if (assetExists) handleAssetSelect(assetId);
      }
    }
    if (isMountedRef.current) setLoading(false);
  };

  // ============================================================
  // HANDLE ASSET SELECT
  // ============================================================
  const handleAssetSelect = useCallback(async (assetId) => {
    if (!assetId) {
      setSelectedAssetId(null);
      setTrackingData_(null);
      setStatsData(null);
      setTimelineData([]);
      setTransactionTypeStats(null);
      setScheduleStats([]);
      setEmployeeHistory([]);
      return;
    }
    setSelectedAssetId(assetId);
    setLoadingData(true);

    try {
      // Fetch all data in parallel
      const [trackingRes, statsRes, timelineRes, transactionStatsRes, scheduleRes, employeeRes] = await Promise.all([
        trackingData.fetchAssetTracking(assetId),
        trackingData.fetchAssetStats(assetId),
        trackingData.fetchTimeline(assetId),
        trackingData.fetchTransactionTypeStats(assetId),
        trackingData.fetchScheduleStats(assetId, { page: 1, pageSize: 10 }),
        trackingData.fetchEmployeeHistory(assetId, { page: 1, pageSize: 10 }),
      ]);

      if (isMountedRef.current) {
        // Tracking Detail
        if (trackingRes.success && trackingRes.data) {
          const data = trackingRes.data;
          const timeline = (data.timeline || []).map(item => ({
            ...item,
            employeeName: item.employeeName || item.newHolder || '-',
            status: item.status || item.newStatus || '-',
          }));
          setTrackingData_(data);
          setTimelineData(timeline);
        } else {
          // Fallback ke asset detail
          const detailRes = await trackingData.fetchAssetDetail(assetId);
          if (detailRes.success && detailRes.data) {
            setTrackingData_(detailRes.data);
          }
        }

        // Stats
        if (statsRes.success && statsRes.data) {
          setStatsData(statsRes.data);
        }

        // Transaction Type Stats
        if (transactionStatsRes.success && transactionStatsRes.data) {
          setTransactionTypeStats(transactionStatsRes.data);
        }

        // Schedule Stats
        if (scheduleRes.success && scheduleRes.data) {
          setScheduleStats(scheduleRes.data?.data || []);
          setScheduleTotal(scheduleRes.data?.totalCount || 0);
        }

        // Employee History
        if (employeeRes.success && employeeRes.data) {
          setEmployeeHistory(employeeRes.data?.data || []);
          setEmployeeTotal(employeeRes.data?.totalCount || 0);
        }

        setLoadingData(false);
      }
    } catch (error) {
      console.error('Error fetching asset tracking:', error);
      if (isMountedRef.current) {
        toast.error('Failed to load asset tracking data');
        setLoadingData(false);
      }
    }
  }, [toast]);

  // ============================================================
  // LOAD SCHEDULE STATS (pagination)
  // ============================================================
  const loadScheduleStats = useCallback(async (page = schedulePage, pageSize = schedulePageSize) => {
    if (!selectedAssetId) return;
    setScheduleLoading(true);
    try {
      const res = await trackingData.fetchScheduleStats(selectedAssetId, { page, pageSize });
      if (isMountedRef.current && res.success && res.data) {
        setScheduleStats(res.data?.data || []);
        setScheduleTotal(res.data?.totalCount || 0);
        setSchedulePage(page);
        setSchedulePageSize(pageSize);
      }
    } catch (error) {
      console.error('Failed to load schedule stats:', error);
    }
    setScheduleLoading(false);
  }, [selectedAssetId, schedulePage, schedulePageSize]);

  // ============================================================
  // LOAD EMPLOYEE HISTORY (pagination)
  // ============================================================
  const loadEmployeeHistory = useCallback(async (page = employeePage, pageSize = employeePageSize) => {
    if (!selectedAssetId) return;
    setEmployeeLoading(true);
    try {
      const res = await trackingData.fetchEmployeeHistory(selectedAssetId, { page, pageSize });
      if (isMountedRef.current && res.success && res.data) {
        setEmployeeHistory(res.data?.data || []);
        setEmployeeTotal(res.data?.totalCount || 0);
        setEmployeePage(page);
        setEmployeePageSize(pageSize);
      }
    } catch (error) {
      console.error('Failed to load employee history:', error);
    }
    setEmployeeLoading(false);
  }, [selectedAssetId, employeePage, employeePageSize]);

  // ============================================================
  // HANDLE SCHEDULE PAGE CHANGE
  // ============================================================
  const handleSchedulePageChange = (newPage) => {
    setSchedulePage(newPage);
    loadScheduleStats(newPage, schedulePageSize);
  };

  const handleSchedulePageSizeChange = (newSize) => {
    setSchedulePageSize(newSize);
    loadScheduleStats(1, newSize);
  };

  // ============================================================
  // HANDLE EMPLOYEE HISTORY PAGE CHANGE
  // ============================================================
  const handleEmployeePageChange = (newPage) => {
    setEmployeePage(newPage);
    loadEmployeeHistory(newPage, employeePageSize);
  };

  const handleEmployeePageSizeChange = (newSize) => {
    setEmployeePageSize(newSize);
    loadEmployeeHistory(1, newSize);
  };

  // ============================================================
  // REFRESH
  // ============================================================
  const handleRefresh = useCallback(async () => {
    if (!selectedAssetId) return;
    await handleAssetSelect(selectedAssetId);
    toast.success('Data refreshed');
  }, [selectedAssetId, handleAssetSelect, toast]);

  // ============================================================
  // TIMELINE FILTER
  // ============================================================
  const filteredTimeline = useMemo(() => {
    if (historyFilter === 'all') return timelineData;
    const filterMap = {
      movement: ['HANDOVER', 'TRANSFER', 'RETURN', 'LOAN_RETURN'],
      loan: ['LOAN'],
      maintenance: ['MAINTENANCE', 'POST_MAINTENANCE'],
      disposal: ['DISPOSAL'],
      handover: ['HANDOVER'],
      transfer: ['TRANSFER'],
    };
    const allowedTypes = filterMap[historyFilter] || [];
    return timelineData.filter(t => {
      const type = t.activityType?.toUpperCase() || '';
      return allowedTypes.some(at => type.includes(at));
    });
  }, [timelineData, historyFilter]);

  const historyFilterTabs = [
    { id: 'all', label: 'All History' },
    { id: 'handover', label: 'Handover' },
    { id: 'transfer', label: 'Transfer' },
    { id: 'loan', label: 'Loan' },
    { id: 'movement', label: 'Movement' },
    { id: 'maintenance', label: 'Maintenance' },
    { id: 'disposal', label: 'Disposal' },
  ];

  // ============================================================
  // STATS CALCULATIONS
  // ============================================================
  const stats = useMemo(() => {
    const result = { totalMovements: 0, totalLoans: 0, totalMaintenance: 0, totalDisposals: 0, totalHandovers: 0, totalTransfers: 0 };
    timelineData.forEach(t => {
      const type = t.activityType?.toUpperCase() || '';
      if (['HANDOVER'].some(at => type.includes(at))) result.totalHandovers++;
      if (['TRANSFER'].some(at => type.includes(at))) result.totalTransfers++;
      if (['HANDOVER', 'TRANSFER', 'RETURN', 'LOAN_RETURN'].some(at => type.includes(at))) result.totalMovements++;
      if (['LOAN'].some(at => type.includes(at))) result.totalLoans++;
      if (['MAINTENANCE', 'POST_MAINTENANCE'].some(at => type.includes(at))) result.totalMaintenance++;
      if (type.includes('DISPOSAL')) result.totalDisposals++;
    });
    return result;
  }, [timelineData]);

  // ============================================================
  // COMPUTE STATS CARDS
  // ============================================================
  const statCards = [
    { icon: FiBox, label: 'Total Transactions', value: trackingData_?.totalTransactions || 0, color: '#3b82f6', bg: 'rgba(59, 130, 246, 0.1)' },
    { icon: FiMove, label: 'Movements', value: stats.totalMovements, color: '#3b82f6', bg: 'rgba(59, 130, 246, 0.1)' },
    { icon: FiMove, label: 'Handovers', value: stats.totalHandovers, color: '#8b5cf6', bg: 'rgba(139, 92, 246, 0.1)' },
    { icon: FiMove, label: 'Transfers', value: stats.totalTransfers, color: '#6366f1', bg: 'rgba(99, 102, 241, 0.1)' },
    { icon: FiClock, label: 'Loans', value: stats.totalLoans, color: '#f59e0b', bg: 'rgba(245, 158, 11, 0.1)' },
    { icon: FiTool, label: 'Maintenance', value: stats.totalMaintenance, color: '#f97316', bg: 'rgba(249, 115, 22, 0.1)' },
    { icon: FiActivity, label: 'Disposals', value: stats.totalDisposals, color: '#ef4444', bg: 'rgba(239, 68, 68, 0.1)' },
  ];

  // ============================================================
  // TRANSACTION TYPE CHART
  // ============================================================
  const transactionTypeChartData = useMemo(() => {
    if (!transactionTypeStats?.data) return [];
    return transactionTypeStats.data.map(item => ({
      label: item.transactionType || 'Unknown',
      count: item.count || 0,
      percentage: item.percentage || 0,
      color: transactionTypeColors[item.transactionType] || '#6b7280',
    }));
  }, [transactionTypeStats]);

  // ============================================================
  // RENDER
  // ============================================================
  const getStatusChip = (status) => <StatusChip status={status || 'Available'} />;

  const assetOptions = useOptions(
    Array.isArray(assets) ? assets.map(a => ({ value: a.assetId, label: `${a.assetCode} - ${a.assetName}` })) : [],
    'Choose an asset to track...'
  );

  const currentAsset = trackingData_;

  if (loading) {
    return (
      <div className="asset-tracking">
        <div className="page-header">
          <h1 className="page-title">Asset Tracking</h1>
          <p className="page-description">Track asset location, status, and transaction history</p>
        </div>
        <div className="page-loading"><Spinner size="lg" /></div>
      </div>
    );
  }

  return (
    <div className="asset-tracking">
      <div className="page-header">
        <h1 className="page-title">Asset Tracking</h1>
        <p className="page-description">Track asset location, status, and transaction history</p>
      </div>

      <Grid container spacing={3}>
        {/* ============================================================ */}
        {/* SELECTOR */}
        {/* ============================================================ */}
        <Grid item xs={12}>
          <Card className="asset-tracking__selection-card">
            <div className="asset-tracking__selection-content">
              <div className="asset-tracking__selection-icon"><FiSearch size={24} /></div>
              <div className="asset-tracking__selection-select">
                <Select
                  label="Select Asset"
                  value={selectedAssetId || ''}
                  onChange={e => handleAssetSelect(e.target.value)}
                  options={assetOptions}
                />
              </div>
              {selectedAssetId && (
                <Tooltip title="Refresh Data">
                  <IconButton onClick={handleRefresh} size="small" className="asset-tracking__refresh-btn">
                    <FiRefreshCw size={18} />
                  </IconButton>
                </Tooltip>
              )}
            </div>
          </Card>
        </Grid>

        {/* ============================================================ */}
        {/* ASSET DETAILS — ketika asset dipilih */}
        {/* ============================================================ */}
        {selectedAssetId && !loadingData && currentAsset && (
          <>
            {/* Profile Card */}
            <Grid item xs={12}>
              <Card className="asset-tracking__profile-card">
                <Grid container spacing={3} alignItems="center">
                  <Grid item>
                    <Avatar className="asset-tracking__profile-avatar">
                      <FiBox size={36} />
                    </Avatar>
                  </Grid>
                  <Grid item xs>
                    <Typography variant="h5" fontWeight={700} className="asset-tracking__profile-name">
                      {currentAsset.assetName || '-'}
                    </Typography>
                    <div className="asset-tracking__profile-tags">
                      <Chip label={`Code: ${currentAsset.assetCode || '-'}`} size="small" variant="outlined" />
                      <Chip label={`Category: ${currentAsset.categoryName || '-'}`} size="small" variant="outlined" />
                      {getStatusChip(currentAsset.currentStatus)}
                      {currentAsset.isOnLoan && <Chip icon={<FiClock />} label="On Loan" size="small" color="warning" variant="outlined" />}
                      {currentAsset.isInMaintenance && <Chip icon={<FiActivity />} label="In Maintenance" size="small" color="info" variant="outlined" />}
                      {currentAsset.isOverdue && <Chip icon={<FiAlertTriangle />} label="Overdue" size="small" color="error" />}
                    </div>
                  </Grid>
                  <Grid item>
                    <InfoRow icon={FiUser} label="Holder" value={currentAsset.currentHolderName || '-'} />
                    <InfoRow icon={FiMapPin} label="Location" value={currentAsset.currentLocation || '-'} />
                    <InfoRow icon={FiActivity} label="Condition" value={currentAsset.conditionName || '-'} />
                    {currentAsset.loanDueDate && (
                      <InfoRow
                        icon={FiCalendar}
                        label="Due Date"
                        value={utilsHelper.formatDate(currentAsset.loanDueDate)}
                        className={currentAsset.isOverdue ? 'u-text-error' : ''}
                      />
                    )}
                    <InfoRow icon={FiRefreshCw} label="Total Transactions" value={currentAsset.totalTransactions || 0} />
                  </Grid>
                </Grid>
              </Card>
            </Grid>

            {/* Stat Cards */}
            <Grid item xs={12}>
              <Grid container spacing={2}>
                {statCards.map((card, index) => (
                  <Grid item xs={6} sm={3} md={1.7} key={index}>
                    <Paper elevation={0} className="asset-tracking__stat-card">
                      <card.icon size={24} className="asset-tracking__stat-icon" style={{ color: card.color }} />
                      <Typography variant="h5" fontWeight={700}>{card.value}</Typography>
                      <Typography variant="caption" color="text.secondary">{card.label}</Typography>
                    </Paper>
                  </Grid>
                ))}
              </Grid>
            </Grid>

            {/* ============================================================ */}
            {/* TRANSACTION TYPE STATS CHART */}
            {/* ============================================================ */}
            <Grid item xs={12} md={4}>
              <Card title="Transaction Types" subtitle="Distribution of transactions">
                {transactionTypeChartData.length === 0 ? (
                  <div className="asset-tracking__empty-chart">
                    <FiPieChart size={32} />
                    <Typography variant="body2" color="text.secondary">No transaction data</Typography>
                  </div>
                ) : (
                  <div className="asset-tracking__chart-list">
                    {transactionTypeChartData.map((item, index) => (
                      <div key={index} className="asset-tracking__chart-item">
                        <div className="asset-tracking__chart-label">
                          <span className="asset-tracking__chart-dot" style={{ backgroundColor: item.color }}></span>
                          <span>{item.label}</span>
                        </div>
                        <div className="asset-tracking__chart-value">
                          <span>{item.count}</span>
                          <span className="asset-tracking__chart-percentage">({item.percentage}%)</span>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </Card>
            </Grid>

            {/* ============================================================ */}
            {/* SCHEDULE STATS */}
            {/* ============================================================ */}
            <Grid item xs={12} md={8}>
              <Card
                title="Schedule Stats"
                subtitle="Upcoming and completed schedules"
              >
                <DataTable
                  rows={scheduleStats}
                  columns={scheduleStatsColumns}
                  loading={scheduleLoading}
                  pageSize={schedulePageSize}
                  page={schedulePage}
                  totalRowCount={scheduleTotal}
                  onPageChange={handleSchedulePageChange}
                  onPageSizeChange={handleSchedulePageSizeChange}
                  getRowId={(row) => row.scheduleId || `schedule-${Math.random()}`}
                  hideFooter={false}
                  ariaLabel="Schedule stats table"
                  paginationMode="server"
                  autoHeight={false}
                />
              </Card>
            </Grid>

            {/* ============================================================ */}
            {/* EMPLOYEE HISTORY */}
            {/* ============================================================ */}
            <Grid item xs={12}>
              <Card
                title="Employee History"
                subtitle={`${employeeTotal} employee(s) have handled this asset`}
              >
                <DataTable
                  rows={employeeHistory}
                  columns={employeeHistoryColumns}
                  loading={employeeLoading}
                  pageSize={employeePageSize}
                  page={employeePage}
                  totalRowCount={employeeTotal}
                  onPageChange={handleEmployeePageChange}
                  onPageSizeChange={handleEmployeePageSizeChange}
                  getRowId={(row) => row.employeeId || `emp-${Math.random()}`}
                  hideFooter={false}
                  ariaLabel="Employee history table"
                  paginationMode="server"
                  autoHeight={false}
                />
              </Card>
            </Grid>

            {/* ============================================================ */}
            {/* TIMELINE */}
            {/* ============================================================ */}
            <Grid item xs={12}>
              <Card title="Timeline" subtitle="Transaction history">
                <Box sx={{ borderBottom: 1, borderColor: 'var(--border)', mb: 2 }}>
                  <MuiTabs
                    value={historyFilter}
                    onChange={(e, v) => setHistoryFilter(v)}
                    variant="scrollable"
                    scrollButtons="auto"
                  >
                    {historyFilterTabs.map(tab => (
                      <Tab
                        key={tab.id}
                        value={tab.id}
                        label={tab.label}
                        sx={{
                          textTransform: 'none',
                          fontWeight: 500,
                          color: 'var(--text-secondary)',
                          '&.Mui-selected': { color: 'var(--primary)' },
                        }}
                      />
                    ))}
                  </MuiTabs>
                </Box>
                <DataTable
                  rows={filteredTimeline}
                  columns={timelineColumns}
                  pageSize={10}
                  getRowId={(row) => row.id || row.assetTransactionId || `timeline-${row.date}-${row.activityType}-${Math.random()}`}
                  hideFooter={false}
                  ariaLabel="Asset timeline table"
                />
              </Card>
            </Grid>
          </>
        )}

        {/* ============================================================ */}
        {/* LOADING STATE */}
        {/* ============================================================ */}
        {selectedAssetId && loadingData && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__loading-container"><Spinner size="lg" /></div>
            </Card>
          </Grid>
        )}

        {/* ============================================================ */}
        {/* EMPTY STATE */}
        {/* ============================================================ */}
        {!selectedAssetId && !loading && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__empty-state">
                <FiSearch size={64} />
                <Typography variant="h6" gutterBottom>Select an Asset to Track</Typography>
                <Typography variant="body2" color="text.secondary">
                  Choose an asset from the dropdown above to view its tracking history, current status, and transaction timeline.
                </Typography>
              </div>
            </Card>
          </Grid>
        )}
      </Grid>
    </div>
  );
};

export default AssetTrackingMenu;