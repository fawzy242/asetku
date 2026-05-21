import React, { useEffect, useState } from "react";
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ArcElement, PointElement, LineElement } from 'chart.js';
import { Doughnut } from 'react-chartjs-2';
import { FiPackage, FiCheckCircle, FiUser, FiTool, FiDollarSign, FiClock, FiAlertTriangle, FiCalendar, FiLayers, FiShield } from "react-icons/fi";
import { Chip } from "@mui/material";
import DashboardData from "./Dashboard.data";
import Card from "../../components/atoms/Card/Card";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Spinner from "../../components/atoms/Spinner/Spinner";
import DrilldownModal from "../../components/molecules/DrilldownModal/DrilldownModal";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { ASSET_STATUS_CHART_COLORS } from "../../core/constants/assetStatuses";
import { useUIStore } from "../../stores/uiStore";
import utilsHelper from "../../core/utils/utils.helper";
import "./Dashboard.scss";

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ArcElement, PointElement, LineElement);

/**
 * ============================================================
 * FUTURE BACKEND ENDPOINTS NEEDED FOR DASHBOARD
 * ============================================================
 * 
 * 1. GET /api/Reports/dashboard/monthly-stats
 *    Returns monthly transaction counts for the dashboard chart.
 *    
 *    Response:
 *    {
 *      "isSuccess": true,
 *      "data": {
 *        "months": ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
 *        "counts": [65, 59, 80, 81, 56, 55, 40, 45, 60, 70, 75, 80]
 *      }
 *    }
 * 
 * Currently using /api/Reports/asset-transaction/data?groupBy=month as fallback
 * ============================================================
 */

const dashboardData = new DashboardData();

const DashboardMenu = () => {
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState({});
  const [expiredWarranty, setExpiredWarranty] = useState([]);
  const [upcomingMaintenance, setUpcomingMaintenance] = useState([]);
  const [recentTransactions, setRecentTransactions] = useState({ data: [] });
  const [drilldown, setDrilldown] = useState({
    isOpen: false,
    title: '',
    endpoint: '',
    params: {},
    columns: null,
  });

  const theme = useUIStore((s) => s.theme);
  const isDark = theme === 'dark';

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    setLoading(true);
    const result = await dashboardData.fetchDashboardData();
    if (result.success) {
      setStats(result.data.stats);
      setExpiredWarranty(result.data.expiredWarranty || []);
      setUpcomingMaintenance(result.data.upcomingMaintenance || []);
      setRecentTransactions(result.data.recentTransactions || { data: [] });
    }
    setLoading(false);
  };

  const statusLabels = ['Available', 'Assigned', 'On Loan', 'In Maintenance', 'Under Repair', 'Damaged', 'Retired', 'Disposed'];
  const statusDataValues = [
    stats.availableAssets || 0,
    stats.assignedAssets || 0,
    stats.assetsOnLoan || 0,
    stats.assetsInMaintenance || 0,
    stats.underRepairAssets || 0,
    stats.damagedAssets || 0,
    stats.retiredAssets || 0,
    0,
  ];
  const statusBackgroundColors = statusLabels.map(s => ASSET_STATUS_CHART_COLORS[s] || '#6b7280');

  const statusChartData = {
    labels: statusLabels,
    datasets: [{
      data: statusDataValues,
      backgroundColor: statusBackgroundColors,
      borderWidth: 0,
    }],
  };

  const transactionColumns = [
    { field: "assetCode", headerName: "Asset Code", width: 120 },
    { field: "assetName", headerName: "Asset Name", flex: 1, minWidth: 180 },
    { field: "transactionType", headerName: "Type", width: 150 },
    {
      field: "transactionStatus",
      headerName: "Status",
      width: 120,
      renderCell: (p) => (
        <Chip
          label={p?.value || '-'}
          size="small"
          sx={getStatusChipStyles(p?.value)}
        />
      ),
    },
    {
      field: "transactionDate",
      headerName: "Date",
      width: 160,
      valueFormatter: (p) => {
        if (!p || !p.value) return '-';
        return utilsHelper.formatDateTime(p.value);
      },
    },
  ];

  const statCardsConfig = [
    { icon: FiPackage, label: 'Total Assets', value: stats.totalAssets || 0, endpoint: '/Asset/grid', params: {} },
    { icon: FiCheckCircle, label: 'Available', value: stats.availableAssets || 0, endpoint: '/Asset/grid', params: { status: 'Available' } },
    { icon: FiUser, label: 'Assigned', value: stats.assignedAssets || 0, endpoint: '/Asset/grid', params: { status: 'Assigned' } },
    { icon: FiLayers, label: 'On Loan', value: stats.assetsOnLoan || 0, endpoint: '/Asset/grid', params: { status: 'On Loan' } },
    { icon: FiTool, label: 'In Maintenance', value: stats.assetsInMaintenance || 0, endpoint: '/Asset/grid', params: { status: 'In Maintenance' } },
    { icon: FiShield, label: 'Damaged', value: stats.damagedAssets || 0, endpoint: '/Asset/grid', params: { status: 'Damaged' } },
    { icon: FiClock, label: 'Pending Txns', value: stats.pendingTransactions || 0, endpoint: '/AssetTransaction/grid', params: { approved: null } },
    { icon: FiAlertTriangle, label: 'Overdue Loans', value: stats.overdueLoanCount || 0, endpoint: '/AssetTransaction/overdue-loans', params: {} },
    { icon: FiDollarSign, label: 'Total Value', value: utilsHelper.formatCurrency(stats.totalAssetValue), noDrilldown: true },
  ];

  const handleCardClick = (card) => {
    if (card.noDrilldown) return;
    
    setDrilldown({
      isOpen: true,
      title: `${card.label} Details`,
      endpoint: card.endpoint,
      params: card.params,
      columns: card.label.includes('Transaction') || card.label.includes('Txn') ? transactionColumns : null,
    });
  };

  if (loading) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="dashboard fade-transition">
      <div className="page-header">
        <h1 className="page-title">Dashboard</h1>
      </div>

      <div className="dashboard__stats">
        {statCardsConfig.map((card) => (
          <Card 
            key={card.label} 
            className={`dashboard__stat-card ${!card.noDrilldown ? 'dashboard__stat-card--clickable' : ''}`}
            onClick={() => handleCardClick(card)}
          >
            <card.icon aria-hidden="true" />
            <div>
              <h3>{card.label}</h3>
              <p>{card.value}</p>
            </div>
          </Card>
        ))}
      </div>

      <div className="dashboard__charts">
        <Card title="Asset Status Distribution" className="dashboard__chart-card">
          <div className="dashboard__chart-container">
            <Doughnut
              data={statusChartData}
              options={{
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                  legend: {
                    position: 'bottom',
                    labels: {
                      color: isDark ? '#f9fafb' : '#111827',
                      padding: 16,
                      usePointStyle: true,
                      pointStyleWidth: 12,
                      pointStyleHeight: 12,
                      font: {
                        size: 12,
                        family: "'Inter', sans-serif",
                      },
                    }
                  }
                },
              }}
            />
          </div>
        </Card>
        <Card title="Quick Summary" className="dashboard__chart-card">
          <div className="dashboard__summary-list">
            {[
              ['Under Repair', stats.underRepairAssets || 0],
              ['Retired', stats.retiredAssets || 0],
              ['Expired Warranty', stats.expiredWarrantyCount || 0],
              ['Upcoming Maintenance', stats.upcomingMaintenanceCount || 0],
              ['Last 30 Days Txns', stats.last30DaysTransactions || 0],
              ['Total Employees', stats.totalEmployees || 0],
            ].map(([label, value]) => (
              <div key={label} className="dashboard__summary-item">
                <span>{label}</span>
                <span className="dashboard__summary-value">{value}</span>
              </div>
            ))}
          </div>
        </Card>
      </div>

      <div className="dashboard__alerts">
        {expiredWarranty.length > 0 && (
          <Card title={<span><FiAlertTriangle aria-hidden="true" style={{ marginRight: 8 }} />Expired Warranty ({expiredWarranty.length})</span>}>
            <ul aria-label="Expired warranty list">
              {expiredWarranty.slice(0, 5).map(item => (
                <li key={item.assetId}>
                  <span>{item.assetCode}</span>
                  <span>{item.assetName}</span>
                  <Chip label="Expired" size="small" sx={{ bgcolor: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', fontWeight: 500, fontSize: '0.75rem', height: 24, borderRadius: '4px' }} />
                </li>
              ))}
            </ul>
          </Card>
        )}
        {upcomingMaintenance.length > 0 && (
          <Card title={<span><FiCalendar aria-hidden="true" style={{ marginRight: 8 }} />Upcoming Maintenance ({upcomingMaintenance.length})</span>}>
            <ul aria-label="Upcoming maintenance list">
              {upcomingMaintenance.slice(0, 5).map(item => (
                <li key={item.assetId}>
                  <span>{item.assetCode}</span>
                  <span>{item.assetName}</span>
                  <span>{utilsHelper.formatDate(item.nextMaintenanceDate)}</span>
                </li>
              ))}
            </ul>
          </Card>
        )}
      </div>

      <Card title="Recent Transactions">
        <DataTable
          rows={recentTransactions.data || []}
          columns={transactionColumns}
          pageSize={5}
          getRowId={(row) => row?.assetTransactionId || `txn-${Math.random()}`}
          hideFooter={true}
          ariaLabel="Recent transactions table"
        />
      </Card>

      {/* Drilldown Modal */}
      <DrilldownModal
        isOpen={drilldown.isOpen}
        onClose={() => setDrilldown(prev => ({ ...prev, isOpen: false }))}
        title={drilldown.title}
        endpoint={drilldown.endpoint}
        params={drilldown.params}
        columns={drilldown.columns}
      />
    </div>
  );
};

export default DashboardMenu;