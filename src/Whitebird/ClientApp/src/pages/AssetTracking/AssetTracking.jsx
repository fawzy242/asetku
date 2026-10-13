// src/pages/AssetTracking/AssetTracking.jsx
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip as ChartTooltip,
  Legend,
} from 'chart.js';
import { Doughnut } from 'react-chartjs-2';
import {
  FiSearch,
  FiBox,
  FiUser,
  FiMapPin,
  FiRefreshCw,
  FiCalendar,
  FiClock,
  FiAlertTriangle,
  FiActivity,
  FiTool,
  FiMove,
  FiPieChart,
  FiUsers,
} from 'react-icons/fi';
import {
  Grid,
  Box,
  Typography,
  Avatar,
  Chip,
  Paper,
  IconButton,
  Tooltip,
  Tabs as MuiTabs,
  Tab,
} from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useLocation } from 'react-router-dom';
import { AssetTrackingData } from '../../core/services';
import DataTable from '../../components/molecules/DataTable/DataTable';
import Card from '../../components/atoms/Card/Card';
import Select from '../../components/atoms/Select/Select';
import Spinner from '../../components/atoms/Spinner/Spinner';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import InfoRow from '../../components/molecules/InfoRow/InfoRow';
import { useOptions } from '../../hooks/useOptions';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useUIStore } from '../../stores/uiStore';
import { formatDate, formatDateTime } from '../../core/utils/date.utils';
import AssetTimeline from './components/AssetTimeline';
import './AssetTracking.scss';

ChartJS.register(ArcElement, ChartTooltip, Legend);

const trackingData = new AssetTrackingData();

const TRANSACTION_TYPE_COLORS = {
  HANDOVER: '#3b82f6',
  TRANSFER: '#8b5cf6',
  LOAN: '#f59e0b',
  RETURN: '#10b981',
  LOAN_RETURN: '#34d399',
  MAINTENANCE: '#f97316',
  POST_MAINTENANCE: '#14b8a6',
  DISPOSAL: '#ef4444',
  OPERATIONAL_OFFICE: '#6366f1',
};

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
      return val ? <span>{formatDateTime(val)}</span> : <span className="u-text-muted">-</span>;
    },
  },
  { field: 'totalTransactions', headerName: 'Total Txns', width: 120 },
  { field: 'assetsHeld', headerName: 'Assets Held', width: 120 },
];

const scheduleStatsColumns = [
  {
    field: 'scheduleDate',
    headerName: 'Schedule Date',
    width: 180,
    renderCell: (params) => {
      const val = params?.row?.scheduleDate ?? params?.value;
      return val ? <span>{formatDateTime(val)}</span> : <span className="u-text-muted">-</span>;
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
      return isCompleted ? (
        <Chip label="Completed" size="small" color="success" />
      ) : (
        <Chip label="Pending" size="small" color="warning" />
      );
    },
  },
];

const historyFilterTabs = [
  { id: 'all', label: 'All History' },
  { id: 'handover', label: 'Handover' },
  { id: 'transfer', label: 'Transfer' },
  { id: 'loan', label: 'Loan' },
  { id: 'movement', label: 'Movement' },
  { id: 'maintenance', label: 'Maintenance' },
  { id: 'disposal', label: 'Disposal' },
];

const AssetTracking = () => {
  const location = useLocation();
  const { toast } = useSweetAlert();
  const queryClient = useQueryClient();
  const theme = useUIStore((s) => s.theme);
  const isDark = theme === 'dark';
  const textColor = isDark ? '#f9fafb' : '#111827';
  const gridColor = isDark ? '#374151' : '#e5e7eb';

  const [selectedAssetId, setSelectedAssetId] = useState(null);
  const [historyFilter, setHistoryFilter] = useState('all');
  const [schedulePage, setSchedulePage] = useState(1);
  const [schedulePageSize, setSchedulePageSize] = useState(10);
  const [employeePage, setEmployeePage] = useState(1);
  const [employeePageSize, setEmployeePageSize] = useState(10);

  // ============================================================
  // QUERIES
  // ============================================================

  const { data: assets = [], isLoading: loading } = useQuery({
    queryKey: ['asset-tracking-assets'],
    queryFn: async () => {
      const res = await trackingData.fetchAssets();
      return res.success ? res.data : [];
    },
    staleTime: 5 * 60 * 1000,
  });

  const { data: currentAsset = null, isLoading: loadingData } = useQuery({
    queryKey: ['asset-tracking-detail', selectedAssetId],
    queryFn: async () => {
      const res = await trackingData.fetchAssetTracking(selectedAssetId);
      if (res.success && res.data) return res.data;
      const detailRes = await trackingData.fetchAssetDetail(selectedAssetId);
      return detailRes.success ? detailRes.data : null;
    },
    enabled: !!selectedAssetId,
    staleTime: 60 * 1000,
  });

  const { data: transactionTypeStats = null } = useQuery({
    queryKey: ['asset-tracking-tx-stats', selectedAssetId],
    queryFn: async () => {
      const res = await trackingData.fetchTransactionTypeStats(selectedAssetId);
      return res.success ? res.data : null;
    },
    enabled: !!selectedAssetId,
    staleTime: 5 * 60 * 1000,
  });

  const {
    data: scheduleStatsData = { data: [], totalCount: 0 },
    isLoading: scheduleLoading,
  } = useQuery({
    queryKey: [
      'asset-tracking-schedule',
      selectedAssetId,
      schedulePage,
      schedulePageSize,
    ],
    queryFn: async () => {
      const res = await trackingData.fetchScheduleStats(selectedAssetId, {
        page: schedulePage,
        pageSize: schedulePageSize,
      });
      return res.success ? res.data : { data: [], totalCount: 0 };
    },
    enabled: !!selectedAssetId,
    staleTime: 2 * 60 * 1000,
  });

  const {
    data: employeeHistoryData = { data: [], totalCount: 0 },
    isLoading: employeeLoading,
  } = useQuery({
    queryKey: [
      'asset-tracking-employees',
      selectedAssetId,
      employeePage,
      employeePageSize,
    ],
    queryFn: async () => {
      const res = await trackingData.fetchEmployeeHistory(selectedAssetId, {
        page: employeePage,
        pageSize: employeePageSize,
      });
      return res.success ? res.data : { data: [], totalCount: 0 };
    },
    enabled: !!selectedAssetId,
    staleTime: 2 * 60 * 1000,
  });

  // ============================================================
  // URL PARAM PRESELECTION
  // ============================================================
  useEffect(() => {
    if (!assets || assets.length === 0) return;
    const params = new URLSearchParams(location.search);
    const assetIdParam = params.get('assetId');
    if (!assetIdParam) return;
    const assetId = parseInt(assetIdParam, 10);
    if (isNaN(assetId)) return;
    const exists = assets.some((a) => a.assetId === assetId);
    if (exists && selectedAssetId !== assetId) {
      setSelectedAssetId(assetId);
      setSchedulePage(1);
      setEmployeePage(1);
    }
  }, [assets, location.search, selectedAssetId]);

  // ============================================================
  // DERIVED DATA
  // ============================================================
  const timelineData = useMemo(() => {
    if (!currentAsset?.timeline) return [];
    return currentAsset.timeline.map((item) => ({
      ...item,
      employeeName: item.employeeName || item.newHolder || '-',
      status: item.status || item.newStatus || '-',
    }));
  }, [currentAsset]);

  const scheduleStats = scheduleStatsData?.data || [];
  const scheduleTotal = scheduleStatsData?.totalCount || 0;
  const employeeHistory = employeeHistoryData?.data || [];
  const employeeTotal = employeeHistoryData?.totalCount || 0;

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
    return timelineData.filter((t) => {
      const type = t.activityType?.toUpperCase() || '';
      return allowedTypes.some((at) => type.includes(at));
    });
  }, [timelineData, historyFilter]);

  const stats = useMemo(() => {
    const result = {
      totalMovements: 0,
      totalLoans: 0,
      totalMaintenance: 0,
      totalDisposals: 0,
      totalHandovers: 0,
      totalTransfers: 0,
    };
    timelineData.forEach((t) => {
      const type = t.activityType?.toUpperCase() || '';
      if (type.includes('HANDOVER')) result.totalHandovers++;
      if (type.includes('TRANSFER')) result.totalTransfers++;
      if (['HANDOVER', 'TRANSFER', 'RETURN', 'LOAN_RETURN'].some((at) => type.includes(at)))
        result.totalMovements++;
      if (type.includes('LOAN')) result.totalLoans++;
      if (['MAINTENANCE', 'POST_MAINTENANCE'].some((at) => type.includes(at)))
        result.totalMaintenance++;
      if (type.includes('DISPOSAL')) result.totalDisposals++;
    });
    return result;
  }, [timelineData]);

  const transactionTypeChartData = useMemo(() => {
    const list = transactionTypeStats?.data || [];
    return {
      labels: list.map((item) => item.transactionType || 'Unknown'),
      values: list.map((item) => item.count || 0),
      colors: list.map(
        (item) => TRANSACTION_TYPE_COLORS[item.transactionType] || '#6b7280'
      ),
      details: list.map((item) => ({
        label: item.transactionType || 'Unknown',
        count: item.count || 0,
        percentage: item.percentage || 0,
        color: TRANSACTION_TYPE_COLORS[item.transactionType] || '#6b7280',
      })),
    };
  }, [transactionTypeStats]);

  const hasChartData =
    transactionTypeChartData.values.length > 0 &&
    transactionTypeChartData.values.some((v) => v > 0);

  // ============================================================
  // HANDLERS
  // ============================================================
  const handleAssetSelect = useCallback((assetId) => {
    if (!assetId) {
      setSelectedAssetId(null);
      return;
    }
    setSelectedAssetId(assetId);
    setSchedulePage(1);
    setEmployeePage(1);
  }, []);

  const handleRefresh = useCallback(() => {
    if (!selectedAssetId) return;
    queryClient.invalidateQueries({
      queryKey: ['asset-tracking-detail', selectedAssetId],
    });
    queryClient.invalidateQueries({
      queryKey: ['asset-tracking-tx-stats', selectedAssetId],
    });
    queryClient.invalidateQueries({
      queryKey: ['asset-tracking-schedule', selectedAssetId],
    });
    queryClient.invalidateQueries({
      queryKey: ['asset-tracking-employees', selectedAssetId],
    });
    toast.success('Data refreshed');
  }, [selectedAssetId, queryClient, toast]);

  const handleSchedulePageChange = (newPage) => {
    setSchedulePage(newPage);
  };
  const handleSchedulePageSizeChange = (newSize) => {
    setSchedulePageSize(newSize);
    setSchedulePage(1);
  };
  const handleEmployeePageChange = (newPage) => {
    setEmployeePage(newPage);
  };
  const handleEmployeePageSizeChange = (newSize) => {
    setEmployeePageSize(newSize);
    setEmployeePage(1);
  };

  const assetOptions = useOptions(
    Array.isArray(assets)
      ? assets.map((a) => ({
          value: a.assetId,
          label: `${a.assetCode} - ${a.assetName}`,
        }))
      : [],
    'Choose an asset to track...'
  );

  const doughnutData = {
    labels: transactionTypeChartData.labels,
    datasets: [
      {
        data: transactionTypeChartData.values,
        backgroundColor: transactionTypeChartData.colors,
        borderWidth: 0,
        hoverOffset: 10,
        borderRadius: 6,
        spacing: 2,
      },
    ],
  };

  const doughnutOptions = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '62%',
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          color: textColor,
          padding: 12,
          usePointStyle: true,
          boxWidth: 10,
          boxHeight: 10,
          font: { size: 11, family: "'Inter', sans-serif" },
        },
      },
      tooltip: {
        backgroundColor: isDark ? '#1f2937' : '#ffffff',
        titleColor: textColor,
        bodyColor: textColor,
        borderColor: isDark ? '#4b5563' : '#d1d5db',
        borderWidth: 1,
        padding: 10,
        cornerRadius: 8,
        callbacks: {
          label: (context) => {
            const label = context.label || '';
            const value = context.raw || 0;
            const total = context.dataset.data.reduce((a, b) => a + b, 0);
            const pct = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
            return `${label}: ${value} (${pct}%)`;
          },
        },
      },
    },
  };

  const statCards = [
    {
      icon: FiBox,
      label: 'Total Transactions',
      value: currentAsset?.totalTransactions || 0,
      color: '#3b82f6',
    },
    {
      icon: FiMove,
      label: 'Movements',
      value: stats.totalMovements,
      color: '#3b82f6',
    },
    {
      icon: FiMove,
      label: 'Handovers',
      value: stats.totalHandovers,
      color: '#8b5cf6',
    },
    {
      icon: FiMove,
      label: 'Transfers',
      value: stats.totalTransfers,
      color: '#6366f1',
    },
    {
      icon: FiClock,
      label: 'Loans',
      value: stats.totalLoans,
      color: '#f59e0b',
    },
    {
      icon: FiTool,
      label: 'Maintenance',
      value: stats.totalMaintenance,
      color: '#f97316',
    },
    {
      icon: FiActivity,
      label: 'Disposals',
      value: stats.totalDisposals,
      color: '#ef4444',
    },
  ];

  // ============================================================
  // RENDER — LOADING
  // ============================================================
  if (loading) {
    return (
      <div className="asset-tracking">
        <div className="page-header">
          <h1 className="page-title">Asset Tracking</h1>
          <p className="page-description">
            Track asset location, status, and transaction history
          </p>
        </div>
        <div className="page-loading">
          <Spinner size="lg" />
        </div>
      </div>
    );
  }

  return (
    <div className="asset-tracking">
      <div className="page-header">
        <h1 className="page-title">Asset Tracking</h1>
        <p className="page-description">
          Track asset location, status, and transaction history
        </p>
      </div>

      <Grid container spacing={3}>
        {/* Asset selector */}
        <Grid item xs={12}>
          <Card className="asset-tracking__selection-card">
            <div className="asset-tracking__selection-content">
              <div className="asset-tracking__selection-icon">
                <FiSearch size={24} />
              </div>
              <div className="asset-tracking__selection-select">
                <Select
                  label="Select Asset"
                  value={selectedAssetId || ''}
                  onChange={(e) => handleAssetSelect(e.target.value)}
                  options={assetOptions}
                />
              </div>
              {selectedAssetId && (
                <Tooltip title="Refresh Data">
                  <IconButton
                    onClick={handleRefresh}
                    size="small"
                    className="asset-tracking__refresh-btn"
                  >
                    <FiRefreshCw size={18} />
                  </IconButton>
                </Tooltip>
              )}
            </div>
          </Card>
        </Grid>

        {/* Loading skeleton while detail loads */}
        {selectedAssetId && loadingData && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__loading-container">
                <Spinner size="lg" />
              </div>
            </Card>
          </Grid>
        )}

        {/* Detail content */}
        {selectedAssetId && !loadingData && currentAsset && (
          <>
            {/* Profile card */}
            <Grid item xs={12}>
              <Card className="asset-tracking__profile-card">
                <Grid container spacing={3} alignItems="center">
                  <Grid item>
                    <Avatar className="asset-tracking__profile-avatar">
                      <FiBox size={36} />
                    </Avatar>
                  </Grid>
                  <Grid item xs>
                    <Typography
                      variant="h5"
                      fontWeight={700}
                      className="asset-tracking__profile-name"
                    >
                      {currentAsset.assetName || '-'}
                    </Typography>
                    <div className="asset-tracking__profile-tags">
                      <Chip
                        label={`Code: ${currentAsset.assetCode || '-'}`}
                        size="small"
                        variant="outlined"
                      />
                      <Chip
                        label={`Category: ${currentAsset.categoryName || '-'}`}
                        size="small"
                        variant="outlined"
                      />
                      <StatusChip status={currentAsset.currentStatus || 'Available'} />
                      {currentAsset.isOnLoan && (
                        <Chip
                          icon={<FiClock />}
                          label="On Loan"
                          size="small"
                          color="warning"
                          variant="outlined"
                        />
                      )}
                      {currentAsset.isInMaintenance && (
                        <Chip
                          icon={<FiActivity />}
                          label="In Maintenance"
                          size="small"
                          color="info"
                          variant="outlined"
                        />
                      )}
                      {currentAsset.isOverdue && (
                        <Chip
                          icon={<FiAlertTriangle />}
                          label="Overdue"
                          size="small"
                          color="error"
                        />
                      )}
                    </div>
                  </Grid>
                  <Grid item>
                    <InfoRow
                      icon={FiUser}
                      label="Holder"
                      value={currentAsset.currentHolderName || '-'}
                    />
                    <InfoRow
                      icon={FiMapPin}
                      label="Location"
                      value={currentAsset.currentLocation || '-'}
                    />
                    <InfoRow
                      icon={FiActivity}
                      label="Condition"
                      value={currentAsset.conditionName || '-'}
                    />
                    {currentAsset.loanDueDate && (
                      <InfoRow
                        icon={FiCalendar}
                        label="Due Date"
                        value={formatDate(currentAsset.loanDueDate)}
                        className={currentAsset.isOverdue ? 'u-text-error' : ''}
                      />
                    )}
                    <InfoRow
                      icon={FiRefreshCw}
                      label="Total Transactions"
                      value={currentAsset.totalTransactions || 0}
                    />
                  </Grid>
                </Grid>
              </Card>
            </Grid>

            {/* Stat cards */}
            <Grid item xs={12}>
              <Box
                sx={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fill, minmax(140px, 1fr))',
                  gap: 2,
                }}
              >
                {statCards.map((card, index) => {
                  const Icon = card.icon;
                  return (
                    <Paper
                      elevation={0}
                      className="asset-tracking__stat-card"
                      key={index}
                    >
                      <Icon
                        size={24}
                        className="asset-tracking__stat-icon"
                        style={{ color: card.color }}
                      />
                      <Typography variant="h5" fontWeight={700}>
                        {card.value}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {card.label}
                      </Typography>
                    </Paper>
                  );
                })}
              </Box>
            </Grid>

            {/* Transaction Types — chart */}
            <Grid item xs={12} md={4}>
              <Card
                title="Transaction Types"
                subtitle="Distribution of transactions"
              >
                {!hasChartData ? (
                  <div className="asset-tracking__empty-chart">
                    <FiPieChart size={32} />
                    <Typography variant="body2" color="text.secondary">
                      No transaction data
                    </Typography>
                  </div>
                ) : (
                  <>
                    <div className="asset-tracking__chart-wrapper">
                      <Doughnut data={doughnutData} options={doughnutOptions} />
                    </div>
                    <div className="asset-tracking__chart-list">
                      {transactionTypeChartData.details.map((item, index) => (
                        <div
                          key={index}
                          className="asset-tracking__chart-item"
                        >
                          <div className="asset-tracking__chart-label">
                            <span
                              className="asset-tracking__chart-dot"
                              style={{ backgroundColor: item.color }}
                            />
                            <span>{item.label}</span>
                          </div>
                          <div className="asset-tracking__chart-value">
                            <span>{item.count}</span>
                            <span className="asset-tracking__chart-percentage">
                              ({item.percentage}%)
                            </span>
                          </div>
                        </div>
                      ))}
                    </div>
                  </>
                )}
              </Card>
            </Grid>

            {/* Schedule Stats */}
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
                  getRowId={(row) =>
                    row.scheduleId || `schedule-${Math.random()}`
                  }
                  hideFooter={false}
                  ariaLabel="Schedule stats table"
                  paginationMode="server"
                  autoHeight={false}
                />
              </Card>
            </Grid>

            {/* Employee History */}
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

            {/* Timeline — visual timeline */}
            <Grid item xs={12}>
              <Card title="Timeline" subtitle="Transaction history">
                <Box
                  sx={{
                    borderBottom: 1,
                    borderColor: 'var(--border)',
                    mb: 2,
                  }}
                >
                  <MuiTabs
                    value={historyFilter}
                    onChange={(e, v) => setHistoryFilter(v)}
                    variant="scrollable"
                    scrollButtons="auto"
                  >
                    {historyFilterTabs.map((tab) => (
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
                <AssetTimeline items={filteredTimeline} />
              </Card>
            </Grid>
          </>
        )}

        {/* Empty state — no asset selected */}
        {!selectedAssetId && !loading && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__empty-state">
                <FiSearch size={64} />
                <Typography variant="h6" gutterBottom>
                  Select an Asset to Track
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Choose an asset from the dropdown above to view its tracking
                  history, current status, and transaction timeline.
                </Typography>
              </div>
            </Card>
          </Grid>
        )}

        {/* Empty state — asset selected but no data */}
        {selectedAssetId && !loadingData && !currentAsset && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__empty-state">
                <FiUsers size={64} />
                <Typography variant="h6" gutterBottom>
                  No Data Available
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  No tracking data was found for the selected asset.
                </Typography>
              </div>
            </Card>
          </Grid>
        )}
      </Grid>
    </div>
  );
};

export default AssetTracking;