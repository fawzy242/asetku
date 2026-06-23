import React, { useEffect, useState } from "react";
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ArcElement, PointElement, LineElement, Filler } from 'chart.js';
import { Bar, Doughnut, Line } from 'react-chartjs-2';
import { FiPackage, FiCheckCircle, FiUser, FiTool, FiDollarSign, FiClock, FiAlertTriangle, FiCalendar, FiLayers, FiShield, FiTrendingUp, FiBarChart2, FiGrid, FiPieChart, FiList } from "react-icons/fi";
import { Chip, Box, Typography } from "@mui/material";
import DashboardData from "./Dashboard.data";
import Card from "../../components/atoms/Card/Card";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Spinner from "../../components/atoms/Spinner/Spinner";
import Tabs from "../../components/molecules/Tabs/Tabs";
import DrilldownModal from "../../components/molecules/DrilldownModal/DrilldownModal";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { ASSET_STATUS_CHART_COLORS } from "../../core/constants/assetStatuses";
import { useUIStore } from "../../stores/uiStore";
import utilsHelper from "../../core/utils/utils.helper";
import "./Dashboard.scss";

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ArcElement, PointElement, LineElement, Filler);

const dashboardData = new DashboardData();

const DASHBOARD_TABS = [
  { id: "summary", label: "Summary", icon: <FiGrid size={16} /> },
  { id: "charts", label: "Charts & Analytics", icon: <FiPieChart size={16} /> },
  { id: "recent", label: "Recent Activity", icon: <FiList size={16} /> },
];

const StatCard = ({ icon: Icon, label, value, color, bgColor, onClick, clickable }) => (
  <div className={`dashboard__stat-card ${clickable ? 'dashboard__stat-card--clickable' : ''}`} onClick={onClick}>
    <div className="dashboard__stat-icon" style={{ backgroundColor: bgColor, color }}>
      <Icon size={24} />
    </div>
    <div className="dashboard__stat-info">
      <h3>{label}</h3>
      <p>{value}</p>
    </div>
  </div>
);

// Drilldown columns - Enhanced
const drilldownColumns = [
  { field: "assetCode", headerName: "Code", width: 120 },
  { field: "assetName", headerName: "Name", flex: 1, minWidth: 180 },
  { field: "categoryName", headerName: "Category", width: 150 },
  { field: "officeName", headerName: "Office", width: 150 },
  { field: "employeeName", headerName: "Employee", width: 150 },
  { field: "departmentName", headerName: "Department", width: 150 },
  { 
    field: "currentStatus", 
    headerName: "Status", 
    width: 140, 
    renderCell: (p) => {
      const status = p?.value || '-';
      return <Chip label={status} size="small" sx={getStatusChipStyles(status)} />;
    }
  },
  { 
    field: "lastTransactionDate", 
    headerName: "Last Transaction", 
    width: 180, 
    valueFormatter: (p) => p?.value ? utilsHelper.formatDateTime(p.value) : '-' 
  },
];

const transactionColumns = [
  { field: "assetCode", headerName: "Asset Code", width: 120 },
  { field: "assetName", headerName: "Asset Name", flex: 1, minWidth: 180 },
  { field: "transactionTypeName", headerName: "Type", width: 150 },
  { field: "fromEmployeeName", headerName: "From", width: 150 },
  { field: "toEmployeeName", headerName: "To", width: 150 },
  { field: "approved", headerName: "Status", width: 120, renderCell: (p) => { 
    let status = 'Pending'; 
    if (p?.value === true) status = 'Approved'; 
    if (p?.value === false) status = 'Rejected'; 
    return <Chip label={status} size="small" sx={getStatusChipStyles(status)} />;
  } },
  { field: "transactionDate", headerName: "Date", width: 160, valueFormatter: (p) => p?.value ? utilsHelper.formatDateTime(p.value) : '-' },
];

const DashboardMenu = () => {
  const [activeTab, setActiveTab] = useState("summary");
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState({});
  const [monthlyStats, setMonthlyStats] = useState([]);
  const [categoryBreakdown, setCategoryBreakdown] = useState([]);
  const [expiredWarranty, setExpiredWarranty] = useState([]);
  const [upcomingMaintenance, setUpcomingMaintenance] = useState([]);
  const [recentTransactions, setRecentTransactions] = useState({ data: [] });
  const [drilldown, setDrilldown] = useState({ isOpen: false, title: '', endpoint: '', params: {}, columns: null });
  const theme = useUIStore((s) => s.theme);
  const isDark = theme === 'dark';
  const textColor = isDark ? '#f9fafb' : '#111827';
  const gridColor = isDark ? '#374151' : '#e5e7eb';
  const borderColor = isDark ? '#4b5563' : '#d1d5db';

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    setLoading(true);
    const result = await dashboardData.fetchDashboardData();
    if (result.success) {
      setStats(result.data.stats);
      setMonthlyStats(result.data.monthlyStats || []);
      setCategoryBreakdown(result.data.categoryBreakdown || []);
      setExpiredWarranty(result.data.expiredWarranty || []);
      setUpcomingMaintenance(result.data.upcomingMaintenance || []);
      setRecentTransactions(result.data.recentTransactions || { data: [] });
    }
    setLoading(false);
  };

  // Monthly chart data
  const monthlyLabels = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  const monthlyDataMap = {};
  monthlyStats.forEach(item => {
    monthlyDataMap[item.month - 1] = item.transactionCount;
  });
  const monthlyChartData = monthlyLabels.map((_, idx) => monthlyDataMap[idx] || 0);

  // Category breakdown
  const categoryLabels = categoryBreakdown.map(item => item.category || 'Uncategorized');
  const categoryValues = categoryBreakdown.map(item => item.totalValue);
  const categoryColors = ['#dc2626', '#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#06b6d4', '#ef4444', '#6b7280'];

  // Status chart
  const statusLabels = ['Available', 'Assigned', 'On Loan', 'In Maintenance', 'Damaged', 'Retired'];
  const statusDataValues = [
    stats.availableAssets || 0,
    stats.assignedAssets || 0,
    stats.assetsOnLoan || 0,
    stats.assetsInMaintenance || 0,
    stats.damagedAssets || 0,
    stats.retiredAssets || 0,
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
      spacing: 2 
    }] 
  };
  
  const doughnutChartOptions = { 
    responsive: true, 
    maintainAspectRatio: false, 
    cutout: '65%', 
    plugins: { 
      legend: { 
        position: 'bottom', 
        labels: { color: textColor, padding: 16, usePointStyle: true, pointStyleWidth: 12, pointStyleHeight: 12, font: { size: 12, family: "'Inter', sans-serif" }, boxWidth: 12, boxHeight: 12 } 
      }, 
      tooltip: { 
        backgroundColor: isDark ? '#1f2937' : '#ffffff', 
        titleColor: textColor, 
        bodyColor: textColor, 
        borderColor, 
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
          } 
        } 
      } 
    } 
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
      borderWidth: 2 
    }] 
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
        borderColor, 
        borderWidth: 1, 
        padding: 10, 
        cornerRadius: 8, 
        callbacks: { label: (context) => `Transactions: ${context.raw}` } 
      } 
    }, 
    scales: { 
      y: { grid: { color: gridColor }, ticks: { color: textColor, stepSize: Math.max(1, Math.ceil(Math.max(...monthlyChartData, 10) / 5)) }, title: { display: true, text: 'Number of Transactions', color: textColor, font: { size: 12 } } }, 
      x: { grid: { display: false }, ticks: { color: textColor } } 
    } 
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
      hoverBackgroundColor: '#dc2626' 
    }] 
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
        borderColor, 
        borderWidth: 1, 
        padding: 10, 
        cornerRadius: 8, 
        callbacks: { label: (context) => `Value: ${utilsHelper.formatCurrency(context.raw)}` } 
      } 
    }, 
    scales: { 
      y: { grid: { color: gridColor }, ticks: { color: textColor, callback: (value) => utilsHelper.formatCurrency(value) }, title: { display: true, text: 'Total Value (IDR)', color: textColor, font: { size: 12 } } }, 
      x: { grid: { display: false }, ticks: { color: textColor, font: { size: 11 } } } 
    } 
  };

  const currentMonth = new Date().getMonth();
  const thisMonthTransactions = monthlyStats.find(item => item.month === currentMonth + 1);
  const thisMonthCount = thisMonthTransactions?.transactionCount || 0;

  // Calculate additional KPIs
  const assetUtilizationRate = stats.totalAssets > 0 
    ? ((stats.assignedAssets + stats.assetsOnLoan) / stats.totalAssets * 100) 
    : 0;
  
  const assetAvailabilityRate = stats.totalAssets > 0 
    ? (stats.availableAssets / stats.totalAssets * 100) 
    : 0;

  const statCardsConfig = [
    { icon: FiPackage, label: 'Total Assets', value: stats.totalAssets || 0, endpoint: '/Asset/grid', params: {}, color: '#dc2626', bg: 'rgba(220, 38, 38, 0.1)' },
    { icon: FiCheckCircle, label: 'Available', value: stats.availableAssets || 0, endpoint: '/Asset/grid', params: { status: 'Available' }, color: '#10b981', bg: 'rgba(16, 185, 129, 0.1)' },
    { icon: FiUser, label: 'Assigned', value: stats.assignedAssets || 0, endpoint: '/Asset/grid', params: { status: 'Assigned' }, color: '#3b82f6', bg: 'rgba(59, 130, 246, 0.1)' },
    { icon: FiLayers, label: 'On Loan', value: stats.assetsOnLoan || 0, endpoint: '/Asset/grid', params: { status: 'On Loan' }, color: '#8b5cf6', bg: 'rgba(139, 92, 246, 0.1)' },
    { icon: FiTool, label: 'In Maintenance', value: stats.assetsInMaintenance || 0, endpoint: '/Asset/grid', params: { status: 'In Maintenance' }, color: '#f59e0b', bg: 'rgba(245, 158, 11, 0.1)' },
    { icon: FiShield, label: 'Damaged', value: stats.damagedAssets || 0, endpoint: '/Asset/grid', params: { status: 'Damaged' }, color: '#ef4444', bg: 'rgba(239, 68, 68, 0.1)' },
    { icon: FiClock, label: 'Pending Approvals', value: stats.pendingApprovals || 0, endpoint: '/AssetTransaction/grid', params: { approved: null }, color: '#f59e0b', bg: 'rgba(245, 158, 11, 0.1)' },
    { icon: FiAlertTriangle, label: 'Overdue Loans', value: stats.overdueLoanCount || 0, endpoint: '/AssetTransaction/overdue-loans', params: {}, color: '#ef4444', bg: 'rgba(239, 68, 68, 0.1)' },
    { icon: FiDollarSign, label: 'Total Value', value: utilsHelper.formatCurrency(stats.totalAssetValue), noDrilldown: true, color: '#10b981', bg: 'rgba(16, 185, 129, 0.1)' },
    { icon: FiTrendingUp, label: 'Utilization', value: `${assetUtilizationRate.toFixed(1)}%`, noDrilldown: true, color: '#06b6d4', bg: 'rgba(6, 182, 212, 0.1)' },
    { icon: FiBarChart2, label: 'This Month', value: thisMonthCount, endpoint: '/AssetTransaction/grid', params: {}, color: '#6366f1', bg: 'rgba(99, 102, 241, 0.1)' },
    { icon: FiCalendar, label: 'Avg Maintenance', value: stats.upcomingMaintenanceCount || 0, noDrilldown: true, color: '#f59e0b', bg: 'rgba(245, 158, 11, 0.1)' },
  ];

  const handleCardClick = (card) => {
    if (card.noDrilldown) return;
    
    let columns = null;
    if (card.label.includes('Transaction') || card.label.includes('Pending') || card.label.includes('Approvals')) {
      columns = transactionColumns;
    } else {
      columns = drilldownColumns;
    }
    
    setDrilldown({ 
      isOpen: true, 
      title: `${card.label} Details`, 
      endpoint: card.endpoint, 
      params: card.params, 
      columns: columns 
    });
  };

  if (loading) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="dashboard fade-transition">
      <div className="page-header">
        <h1 className="page-title">Dashboard</h1>
        <p className="page-description">Overview of asset management system performance</p>
      </div>

      <Tabs tabs={DASHBOARD_TABS} activeTab={activeTab} onTabChange={setActiveTab} />

      {/* Summary Tab */}
      {activeTab === "summary" && (
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

          {/* Alerts Section - Full Width Cards */}
          <div className="dashboard__alerts">
            {expiredWarranty.length > 0 && (
              <div className="dashboard__alert-wrapper">
                <Card className="dashboard__alert-card">
                  <div className="dashboard__alert-header">
                    <div className="dashboard__alert-title-group">
                      <FiAlertTriangle className="dashboard__alert-icon" style={{ color: '#ef4444' }} />
                      <Typography variant="h6" fontWeight={600}>Expired Warranty</Typography>
                      <Chip label={`${expiredWarranty.length} assets`} size="small" className="dashboard__alert-count" />
                    </div>
                  </div>
                  <div className="dashboard__alert-body">
                    {expiredWarranty.slice(0, 8).map(item => (
                      <div key={item.assetId} className="dashboard__alert-item">
                        <div className="dashboard__alert-item-info">
                          <span className="dashboard__alert-code">{item.assetCode}</span>
                          <span className="dashboard__alert-name">{item.assetName}</span>
                          <span className="dashboard__alert-detail">{item.categoryName || ''}</span>
                          <span className="dashboard__alert-detail">{item.officeName || ''}</span>
                        </div>
                        <Chip label={`Expired: ${utilsHelper.formatDate(item.warrantyExpiryDate)}`} size="small" className="dashboard__alert-chip--expired" />
                      </div>
                    ))}
                    {expiredWarranty.length > 8 && (
                      <div className="dashboard__alert-more">
                        +{expiredWarranty.length - 8} more expired warranties
                      </div>
                    )}
                  </div>
                </Card>
              </div>
            )}
            
            {upcomingMaintenance.length > 0 && (
              <div className="dashboard__alert-wrapper">
                <Card className="dashboard__alert-card">
                  <div className="dashboard__alert-header">
                    <div className="dashboard__alert-title-group">
                      <FiCalendar className="dashboard__alert-icon" style={{ color: '#f59e0b' }} />
                      <Typography variant="h6" fontWeight={600}>Upcoming Maintenance</Typography>
                      <Chip label={`${upcomingMaintenance.length} assets`} size="small" className="dashboard__alert-count" />
                    </div>
                  </div>
                  <div className="dashboard__alert-body">
                    {upcomingMaintenance.slice(0, 8).map(item => (
                      <div key={item.assetId} className="dashboard__alert-item">
                        <div className="dashboard__alert-item-info">
                          <span className="dashboard__alert-code">{item.assetCode}</span>
                          <span className="dashboard__alert-name">{item.assetName}</span>
                          <span className="dashboard__alert-detail">{item.categoryName || ''}</span>
                          <span className="dashboard__alert-detail">{item.officeName || ''}</span>
                        </div>
                        <Chip label={utilsHelper.formatDate(item.nextMaintenanceDate)} size="small" className="dashboard__alert-chip--upcoming" />
                      </div>
                    ))}
                    {upcomingMaintenance.length > 8 && (
                      <div className="dashboard__alert-more">
                        +{upcomingMaintenance.length - 8} more upcoming maintenance
                      </div>
                    )}
                  </div>
                </Card>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Charts Tab */}
      {activeTab === "charts" && (
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
                  <FiTrendingUp size={14} style={{ marginRight: 4 }} />
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
        </div>
      )}

      {/* Recent Tab */}
      {activeTab === "recent" && (
        <div className="dashboard__tab-content">
          <Card title="Recent Transactions">
            <div className="dashboard__table-wrapper">
              <DataTable 
                rows={recentTransactions.data || []} 
                columns={transactionColumns} 
                pageSize={10} 
                getRowId={(row) => row?.assetTransactionId || `txn-${Math.random()}`} 
                hideFooter={false} 
                ariaLabel="Recent transactions table"
              />
            </div>
          </Card>
        </div>
      )}

      <DrilldownModal 
        isOpen={drilldown.isOpen} 
        onClose={() => setDrilldown(prev => ({ ...prev, isOpen: false }))} 
        title={drilldown.title} 
        endpoint={drilldown.endpoint} 
        params={drilldown.params} 
        columns={drilldown.columns} 
        size="xl"
      />
    </div>
  );
};

export default DashboardMenu;