// src/pages/Dashboard/Dashboard.jsx
import React, { useEffect, useState } from 'react';
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ArcElement, PointElement, LineElement, Filler } from 'chart.js';
import { Bar, Doughnut, Line } from 'react-chartjs-2';
import {
  FiPackage, FiCheckCircle, FiUser, FiTool, FiDollarSign, FiClock, FiAlertTriangle,
  FiCalendar, FiLayers, FiShield, FiTrendingUp, FiBarChart2, FiGrid, FiPieChart, FiList,
  FiFileText, FiUsers, FiBriefcase, FiMapPin
} from 'react-icons/fi';
import { Chip, Box, Typography } from '@mui/material';
import { DashboardData } from '../../core/services';
import Card from '../../components/atoms/Card/Card';
import DataTable from '../../components/molecules/DataTable/DataTable';
import Spinner from '../../components/atoms/Spinner/Spinner';
import Tabs from '../../components/molecules/Tabs/Tabs';
import DrilldownModal from '../../components/molecules/DrilldownModal/DrilldownModal';
import StatCard from '../../components/molecules/StatCard/StatCard';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import { useUIStore } from '../../stores/uiStore';
import { formatDate, formatDateTime } from '../../core/utils/date.utils';
import { formatCurrency } from '../../core/utils/number.utils';
import './Dashboard.scss';

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ArcElement, PointElement, LineElement, Filler);

const dashboardData = new DashboardData();

// ============================================================
// CHART COLORS - Defined locally (replacing removed constant)
// ============================================================
const ASSET_STATUS_CHART_COLORS = {
  'Available': '#10b981',
  'Assigned': '#3b82f6',
  'On Loan': '#8b5cf6',
  'In Maintenance': '#f59e0b',
  'Damaged': '#ef4444',
  'Retired': '#6b7280',
  'Disposed': '#4b5563',
};

const DASHBOARD_TABS = [
  { id: 'summary', label: 'Summary', icon: <FiGrid size={16} /> },
  { id: 'charts', label: 'Charts & Analytics', icon: <FiPieChart size={16} /> },
  { id: 'recent', label: 'Recent Activity', icon: <FiList size={16} /> },
];

const transactionColumns = [
  {
    field: 'assetCode',
    headerName: 'Code',
    width: 120,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.assetCode || params?.value || '-';
      return <span>{value}</span>;
    },
  },
  {
    field: 'assetName',
    headerName: 'Asset Name',
    flex: 1,
    minWidth: 180,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.assetName || params?.value || '-';
      return <span>{value}</span>;
    },
  },
  {
    field: 'transactionTypeName',
    headerName: 'Type',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.transactionTypeName || params?.value || '-';
      return <span>{value}</span>;
    },
  },
  {
    field: 'fromEmployeeName',
    headerName: 'From',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.fromEmployeeName || params?.value || '-';
      return <span>{value}</span>;
    },
  },
  {
    field: 'toEmployeeName',
    headerName: 'To',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.toEmployeeName || params?.value || '-';
      return <span>{value}</span>;
    },
  },
  {
    field: 'approved',
    headerName: 'Status',
    width: 120,
    renderCell: (params) => {
      const row = params?.row || {};
      let status = 'Pending';
      if (row.approved === true || params?.value === true) status = 'Approved';
      if (row.approved === false || params?.value === false) status = 'Rejected';
      return <StatusChip status={status} />;
    },
  },
  {
    field: 'transactionDate',
    headerName: 'Date',
    width: 180,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.transactionDate || params?.value;
      if (!value) return <span>-</span>;
      return <span>{formatDateTime(value)}</span>;
    },
  },
];

const expiredWarrantyColumns = [
  {
    field: 'assetCode',
    headerName: 'Code',
    width: 120,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.assetCode || params?.value || '-';
      return <span>{value}</span>;
    },
  },
  {
    field: 'assetName',
    headerName: 'Name',
    flex: 1,
    minWidth: 180,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.assetName || params?.value || '-';
      return <span>{value}</span>;
    },
  },
  {
    field: 'categoryName',
    headerName: 'Category',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.categoryName || params?.value || '-';
      return <span>{value}</span>;
    },
  },
  {
    field: 'supplierName',
    headerName: 'Supplier',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.supplierName || params?.value || '-';
      return <span>{value}</span>;
    },
  },
  {
    field: 'warrantyExpiryDate',
    headerName: 'Expiry Date',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.warrantyExpiryDate || params?.value;
      if (!value) return <span>-</span>;
      return <span>{formatDate(value)}</span>;
    },
  },
  {
    field: 'purchasePrice',
    headerName: 'Price',
    width: 130,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.purchasePrice || params?.value;
      if (!value) return <span>-</span>;
      return <span>{formatCurrency(value)}</span>;
    },
  },
];

const categoryBreakdownColumns = [
  { field: 'category', headerName: 'Category', flex: 1, minWidth: 200 },
  { field: 'assetCount', headerName: 'Asset Count', width: 130 },
  {
    field: 'totalValue',
    headerName: 'Total Value',
    width: 180,
    renderCell: (params) => {
      const value = params?.row?.totalValue || params?.value;
      if (!value) return <span className="u-text-muted">-</span>;
      return <span>{formatCurrency(value)}</span>;
    },
  },
];

const monthlyStatsColumns = [
  {
    field: 'month',
    headerName: 'Month',
    width: 120,
    renderCell: (params) => {
      const monthMap = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
      const val = params?.row?.month || params?.value;
      return <span>{monthMap[val - 1] || val}</span>;
    },
  },
  { field: 'transactionCount', headerName: 'Transactions', width: 130 },
  { field: 'uniqueAssetsCount', headerName: 'Unique Assets', width: 130 },
];

const Dashboard = () => {
  const [activeTab, setActiveTab] = useState('summary');
  const [loading, setLoading] = useState(true);
  const [dashboardData_, setDashboardData_] = useState({
    stats: {},
    monthlyStats: [],
    categoryBreakdown: [],
    recentTransactions: [],
    expiredWarranty: [],
    scheduleStats: {},
  });
  const [drilldown, setDrilldown] = useState({ isOpen: false, title: '', endpoint: '', params: {} });
  const [expiredWarrantyPage, setExpiredWarrantyPage] = useState(1);
  const [recentPage, setRecentPage] = useState(1);
  const [monthlyStatsPage, setMonthlyStatsPage] = useState(1);
  const [categoryBreakdownPage, setCategoryBreakdownPage] = useState(1);
  const theme = useUIStore((s) => s.theme);
  const isDark = theme === 'dark';
  const textColor = isDark ? '#f9fafb' : '#111827';
  const gridColor = isDark ? '#374151' : '#e5e7eb';

  const { stats, monthlyStats, categoryBreakdown, recentTransactions, expiredWarranty, scheduleStats } = dashboardData_;

  const safeMonthlyStats = Array.isArray(monthlyStats) ? monthlyStats : [];
  const safeCategoryBreakdown = Array.isArray(categoryBreakdown) ? categoryBreakdown : [];
  const safeRecentTransactions = Array.isArray(recentTransactions) ? recentTransactions : [];
  const safeExpiredWarranty = Array.isArray(expiredWarranty) ? expiredWarranty : [];
  const safeScheduleStats = scheduleStats || {};

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    setLoading(true);
    const result = await dashboardData.fetchDashboardData();
    if (result.success) {
      setDashboardData_(result.data);
    } else {
      setDashboardData_({
        stats: {},
        monthlyStats: [],
        categoryBreakdown: [],
        recentTransactions: [],
        expiredWarranty: [],
        scheduleStats: {},
      });
    }
    setLoading(false);
  };

  const monthlyLabels = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  const monthlyDataMap = {};
  safeMonthlyStats.forEach(item => {
    monthlyDataMap[item.month - 1] = item.transactionCount;
  });
  const monthlyChartData = monthlyLabels.map((_, idx) => monthlyDataMap[idx] || 0);

  const categoryLabels = safeCategoryBreakdown.map(item => item.category || 'Uncategorized');
  const categoryValues = safeCategoryBreakdown.map(item => item.totalValue || 0);
  const categoryColors = ['#dc2626', '#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#06b6d4', '#ef4444', '#6b7280'];

  const statusLabels = ['Available', 'Assigned', 'On Loan', 'In Maintenance', 'Damaged'];
  const statusDataValues = [
    stats.availableAssets || 0,
    stats.assignedAssets || 0,
    stats.assetsOnLoan || 0,
    stats.assetsInMaintenance || 0,
    stats.damagedAssets || 0,
  ];
  const statusBackgroundColors = statusLabels.map(s => ASSET_STATUS_CHART_COLORS[s] || '#6b7280');

  const doughnutChartData = {
    labels: statusLabels,
    datasets: [{
      data: statusDataValues,
      backgroundColor: statusBackgroundColors,
      borderWidth: 0,
      hoverOffset: 10,
      borderRadius: 8,
      spacing: 2,
    }],
  };

  const doughnutChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '65%',
    plugins: {
      legend: {
        position: 'bottom',
        labels: { color: textColor, padding: 16, usePointStyle: true, pointStyleWidth: 12, pointStyleHeight: 12, font: { size: 12, family: "'Inter', sans-serif" }, boxWidth: 12, boxHeight: 12 },
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
            const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
            return `${label}: ${value} (${percentage}%)`;
          },
        },
      },
    },
  };

  const lineChartData = {
    labels: monthlyLabels,
    datasets: [{
      label: 'Transactions',
      data: monthlyChartData,
      borderColor: '#dc2626',
      backgroundColor: 'rgba(220, 38, 38, 0.1)',
      fill: true,
      tension: 0.4,
      pointBackgroundColor: '#dc2626',
      pointBorderColor: '#ffffff',
      pointBorderWidth: 2,
      pointRadius: 4,
      pointHoverRadius: 6,
      borderWidth: 2,
    }],
  };

  const lineChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: isDark ? '#1f2937' : '#ffffff',
        titleColor: textColor,
        bodyColor: textColor,
        borderColor: isDark ? '#4b5563' : '#d1d5db',
        borderWidth: 1,
        padding: 10,
        cornerRadius: 8,
        callbacks: { label: (context) => `Transactions: ${context.raw}` },
      },
    },
    scales: {
      y: { grid: { color: gridColor }, ticks: { color: textColor, stepSize: Math.max(1, Math.ceil(Math.max(...monthlyChartData, 10) / 5)) }, title: { display: true, text: 'Number of Transactions', color: textColor, font: { size: 12 } } },
      x: { grid: { display: false }, ticks: { color: textColor } },
    },
  };

  const barChartData = {
    labels: categoryLabels,
    datasets: [{
      label: 'Asset Value (IDR)',
      data: categoryValues,
      backgroundColor: categoryColors.slice(0, categoryLabels.length),
      borderRadius: 8,
      barPercentage: 0.7,
      categoryPercentage: 0.8,
      hoverBackgroundColor: '#dc2626',
    }],
  };

  const barChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: isDark ? '#1f2937' : '#ffffff',
        titleColor: textColor,
        bodyColor: textColor,
        borderColor: isDark ? '#4b5563' : '#d1d5db',
        borderWidth: 1,
        padding: 10,
        cornerRadius: 8,
        callbacks: { label: (context) => `Value: ${formatCurrency(context.raw)}` },
      },
    },
    scales: {
      y: { grid: { color: gridColor }, ticks: { color: textColor, callback: (value) => formatCurrency(value) }, title: { display: true, text: 'Total Value (IDR)', color: textColor, font: { size: 12 } } },
      x: { grid: { display: false }, ticks: { color: textColor, font: { size: 11 } } },
    },
  };

  const currentMonth = new Date().getMonth();
  const thisMonthTransactions = safeMonthlyStats.find(item => item.month === currentMonth + 1);
  const thisMonthCount = thisMonthTransactions?.transactionCount || 0;

  const assetUtilizationRate = stats.totalAssets > 0
    ? ((stats.assignedAssets + stats.assetsOnLoan) / stats.totalAssets * 100)
    : 0;

  const pendingApprovalsCount = stats.pendingApprovals || 0;
  const damagedCount = stats.damagedAssets || 0;
  const totalEmployees = stats.totalEmployees || 0;
  const activeEmployees = stats.activeEmployees || 0;

  const statCardsConfig = [
    { icon: FiPackage, label: 'Total Assets', value: stats.totalAssets || 0, endpoint: '/Asset/grid', params: {}, color: '#dc2626', bg: 'rgba(220, 38, 38, 0.1)' },
    { icon: FiCheckCircle, label: 'Available', value: stats.availableAssets || 0, endpoint: '/Asset/grid', params: { status: 'Available' }, color: '#10b981', bg: 'rgba(16, 185, 129, 0.1)' },
    { icon: FiUser, label: 'Assigned', value: stats.assignedAssets || 0, endpoint: '/Asset/grid', params: { status: 'Assigned' }, color: '#3b82f6', bg: 'rgba(59, 130, 246, 0.1)' },
    { icon: FiLayers, label: 'On Loan', value: stats.assetsOnLoan || 0, endpoint: '/Asset/grid', params: { status: 'On Loan' }, color: '#8b5cf6', bg: 'rgba(139, 92, 246, 0.1)' },
    { icon: FiTool, label: 'In Maintenance', value: stats.assetsInMaintenance || 0, endpoint: '/Asset/grid', params: { status: 'In Maintenance' }, color: '#f59e0b', bg: 'rgba(245, 158, 11, 0.1)' },
    { icon: FiShield, label: 'Damaged', value: damagedCount, endpoint: '/Asset/grid', params: { status: 'Damaged' }, color: '#ef4444', bg: 'rgba(239, 68, 68, 0.1)' },
    { icon: FiClock, label: 'Pending Approvals', value: pendingApprovalsCount, endpoint: '/AssetTransaction/grid-uncomplete', params: {}, color: '#f59e0b', bg: 'rgba(245, 158, 11, 0.1)' },
    { icon: FiAlertTriangle, label: 'Overdue Loans', value: stats.overdueLoanCount || 0, endpoint: '/AssetTransaction/overdue-loans', params: {}, color: '#ef4444', bg: 'rgba(239, 68, 68, 0.1)' },
    { icon: FiDollarSign, label: 'Total Value', value: formatCurrency(stats.totalAssetValue), noDrilldown: true, color: '#10b981', bg: 'rgba(16, 185, 129, 0.1)' },
    { icon: FiTrendingUp, label: 'Utilization', value: `${assetUtilizationRate.toFixed(1)}%`, noDrilldown: true, color: '#06b6d4', bg: 'rgba(6, 182, 212, 0.1)' },
    { icon: FiBarChart2, label: 'This Month', value: thisMonthCount, endpoint: '/AssetTransaction/grid-add', params: {}, color: '#6366f1', bg: 'rgba(99, 102, 241, 0.1)' },
    { icon: FiCalendar, label: 'Upcoming Maintenance', value: safeScheduleStats.upcomingMaintenance || 0, noDrilldown: true, color: '#f59e0b', bg: 'rgba(245, 158, 11, 0.1)' },
    { icon: FiUsers, label: 'Total Employees', value: totalEmployees, noDrilldown: true, color: '#8b5cf6', bg: 'rgba(139, 92, 246, 0.1)' },
    { icon: FiUser, label: 'Active Employees', value: activeEmployees, noDrilldown: true, color: '#10b981', bg: 'rgba(16, 185, 129, 0.1)' },
    { icon: FiBriefcase, label: 'Departments', value: stats.totalDepartments || 0, noDrilldown: true, color: '#f59e0b', bg: 'rgba(245, 158, 11, 0.1)' },
    { icon: FiMapPin, label: 'Offices', value: stats.totalOffices || 0, noDrilldown: true, color: '#3b82f6', bg: 'rgba(59, 130, 246, 0.1)' },
  ];

  const handleCardClick = (card) => {
    if (card.noDrilldown) return;
    setDrilldown({
      isOpen: true,
      title: `${card.label} Details`,
      endpoint: card.endpoint,
      params: card.params || {},
    });
  };

  if (loading) return <div className="page-loading"><Spinner size="lg" /></div>;

  const paginatedExpiredWarranty = safeExpiredWarranty.slice((expiredWarrantyPage - 1) * 10, expiredWarrantyPage * 10);
  const paginatedRecent = safeRecentTransactions.slice((recentPage - 1) * 10, recentPage * 10);
  const paginatedMonthlyStats = safeMonthlyStats.slice((monthlyStatsPage - 1) * 10, monthlyStatsPage * 10);
  const paginatedCategoryBreakdown = safeCategoryBreakdown.slice((categoryBreakdownPage - 1) * 10, categoryBreakdownPage * 10);

  return (
    <div className="dashboard fade-transition">
      <div className="page-header">
        <h1 className="page-title">Dashboard</h1>
        <p className="page-description">Overview of asset management system performance</p>
      </div>

      <Tabs tabs={DASHBOARD_TABS} activeTab={activeTab} onTabChange={setActiveTab} />

      {activeTab === 'summary' && (
        <div className="dashboard__tab-content">
          <div className="dashboard__stats">
            {statCardsConfig.map((card) => (
              <StatCard
                key={card.label}
                icon={card.icon}
                label={card.label}
                value={card.value}
                color={card.color}
                bgColor={card.bg}
                onClick={() => handleCardClick(card)}
                clickable={!card.noDrilldown}
              />
            ))}
          </div>

          {safeExpiredWarranty.length > 0 && (
            <div className="dashboard__alert-wrapper">
              <Card className="dashboard__alert-card">
                <div className="dashboard__alert-header">
                  <div className="dashboard__alert-title-group">
                    <FiAlertTriangle className="dashboard__alert-icon" />
                    <Typography variant="h6" fontWeight={600}>Expired Warranty</Typography>
                    <Chip label={`${safeExpiredWarranty.length} assets`} size="small" className="dashboard__alert-count" />
                  </div>
                </div>
                <div className="dashboard__alert-body u-min-h-200">
                  <DataTable
                    rows={paginatedExpiredWarranty}
                    columns={expiredWarrantyColumns}
                    pageSize={10}
                    page={expiredWarrantyPage}
                    onPageChange={setExpiredWarrantyPage}
                    getRowId={(row) => row?.assetId || `ew-${Math.random()}`}
                    hideFooter={false}
                    ariaLabel="Expired warranty table"
                    paginationMode="client"
                    autoHeight={false}
                  />
                </div>
              </Card>
            </div>
          )}

          <div className="dashboard__summary-tables">
            {safeMonthlyStats.length > 0 && (
              <Card title="Monthly Transaction Statistics" className="dashboard__summary-card">
                <DataTable
                  rows={paginatedMonthlyStats}
                  columns={monthlyStatsColumns}
                  pageSize={10}
                  page={monthlyStatsPage}
                  onPageChange={setMonthlyStatsPage}
                  getRowId={(row) => row?.month || `month-${Math.random()}`}
                  hideFooter={false}
                  ariaLabel="Monthly stats table"
                  paginationMode="client"
                  autoHeight={false}
                />
              </Card>
            )}

            {safeCategoryBreakdown.length > 0 && (
              <Card title="Asset Value by Category" className="dashboard__summary-card">
                <DataTable
                  rows={paginatedCategoryBreakdown}
                  columns={categoryBreakdownColumns}
                  pageSize={10}
                  page={categoryBreakdownPage}
                  onPageChange={setCategoryBreakdownPage}
                  getRowId={(row) => row?.category || `cat-${Math.random()}`}
                  hideFooter={false}
                  ariaLabel="Category breakdown table"
                  paginationMode="client"
                  autoHeight={false}
                />
              </Card>
            )}

            {safeScheduleStats && (
              <Card title="Schedule Statistics" className="dashboard__summary-card">
                <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(150px, 1fr))', gap: 2 }}>
                  <Box className="dashboard__stat-item">
                    <Typography variant="caption" color="text.secondary">Total Handover</Typography>
                    <Typography variant="h6" fontWeight={600}>{safeScheduleStats.totalHandover || 0}</Typography>
                  </Box>
                  <Box className="dashboard__stat-item">
                    <Typography variant="caption" color="text.secondary">Total Return</Typography>
                    <Typography variant="h6" fontWeight={600}>{safeScheduleStats.totalReturn || 0}</Typography>
                  </Box>
                  <Box className="dashboard__stat-item">
                    <Typography variant="caption" color="text.secondary">Total Maintenance</Typography>
                    <Typography variant="h6" fontWeight={600}>{safeScheduleStats.totalMaintenance || 0}</Typography>
                  </Box>
                  <Box className="dashboard__stat-item">
                    <Typography variant="caption" color="text.secondary">Upcoming Handover</Typography>
                    <Typography variant="h6" fontWeight={600} color="warning.main">{safeScheduleStats.upcomingHandover || 0}</Typography>
                  </Box>
                  <Box className="dashboard__stat-item">
                    <Typography variant="caption" color="text.secondary">Upcoming Return</Typography>
                    <Typography variant="h6" fontWeight={600} color="warning.main">{safeScheduleStats.upcomingReturn || 0}</Typography>
                  </Box>
                  <Box className="dashboard__stat-item">
                    <Typography variant="caption" color="text.secondary">Upcoming Maintenance</Typography>
                    <Typography variant="h6" fontWeight={600} color="warning.main">{safeScheduleStats.upcomingMaintenance || 0}</Typography>
                  </Box>
                </Box>
              </Card>
            )}
          </div>
        </div>
      )}

      {activeTab === 'charts' && (
        <div className="dashboard__tab-content">
          <div className="dashboard__charts-row">
            <Card title="Asset Distribution" className="dashboard__chart-card">
              <div className="dashboard__chart-container">
                <Doughnut data={doughnutChartData} options={doughnutChartOptions} />
              </div>
            </Card>

            <Card title="Transaction Trend" className="dashboard__chart-card">
              <div className="dashboard__chart-container">
                <Line data={lineChartData} options={lineChartOptions} />
              </div>
              <div className="dashboard__chart-insight">
                <Typography variant="caption" color="text.secondary">
                  <FiTrendingUp size={14} />
                  {monthlyChartData.length > 1 && monthlyChartData[monthlyChartData.length - 1] > monthlyChartData[monthlyChartData.length - 2]
                    ? '↑ Increasing trend compared to last month'
                    : '↓ Decreasing trend compared to last month'}
                </Typography>
              </div>
            </Card>
          </div>

          <div className="dashboard__full-width">
            <Card title="Asset Value by Category" className="dashboard__chart-card">
              <div className="dashboard__chart-container--large">
                <Bar data={barChartData} options={barChartOptions} />
              </div>
            </Card>
          </div>

          <div className="dashboard__summary-tables">
            {safeCategoryBreakdown.length > 0 && (
              <Card title="Category Breakdown Details" className="dashboard__summary-card">
                <DataTable
                  rows={paginatedCategoryBreakdown}
                  columns={categoryBreakdownColumns}
                  pageSize={10}
                  page={categoryBreakdownPage}
                  onPageChange={setCategoryBreakdownPage}
                  getRowId={(row) => row?.category || `cat-${Math.random()}`}
                  hideFooter={false}
                  ariaLabel="Category breakdown table"
                  paginationMode="client"
                  autoHeight={false}
                />
              </Card>
            )}
          </div>
        </div>
      )}

      {activeTab === 'recent' && (
        <div className="dashboard__tab-content">
          <Card title="Recent Transactions">
            <div className="dashboard__table-wrapper">
              <DataTable
                rows={paginatedRecent}
                columns={transactionColumns}
                pageSize={10}
                page={recentPage}
                onPageChange={setRecentPage}
                getRowId={(row) => row?.assetTransactionId || `txn-${Math.random()}`}
                hideFooter={false}
                ariaLabel="Recent transactions table"
                paginationMode="client"
                autoHeight={false}
              />
            </div>
          </Card>

          {safeExpiredWarranty.length > 0 && (
            <Card title="Expired Warranty Assets" className="dashboard__summary-card">
              <DataTable
                rows={paginatedExpiredWarranty}
                columns={expiredWarrantyColumns}
                pageSize={10}
                page={expiredWarrantyPage}
                onPageChange={setExpiredWarrantyPage}
                getRowId={(row) => row?.assetId || `ew-${Math.random()}`}
                hideFooter={false}
                ariaLabel="Expired warranty table"
                paginationMode="client"
                autoHeight={false}
              />
            </Card>
          )}
        </div>
      )}

      <DrilldownModal
        isOpen={drilldown.isOpen}
        onClose={() => setDrilldown(prev => ({ ...prev, isOpen: false }))}
        title={drilldown.title}
        endpoint={drilldown.endpoint}
        params={drilldown.params}
        size="xl"
      />
    </div>
  );
};

export default Dashboard;