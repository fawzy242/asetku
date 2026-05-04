import React, { useEffect, useState } from "react";
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ArcElement, PointElement, LineElement } from 'chart.js';
import { Doughnut, Line } from 'react-chartjs-2';
import { FiPackage, FiCheckCircle, FiUser, FiTool, FiDollarSign, FiClock, FiAlertTriangle, FiCalendar } from "react-icons/fi";
import { Chip } from "@mui/material";
import DashboardData from "./Dashboard.data";
import Card from "../../components/atoms/Card/Card";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Spinner from "../../components/atoms/Spinner/Spinner";
import utilsHelper from "../../core/utils/utils.helper";
import "./Dashboard.scss";

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ArcElement, PointElement, LineElement);

const dashboardData = new DashboardData();

const STATUS_CHIP_COLORS = {
  'Pending': { bg: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b' },
  'Approved': { bg: 'rgba(16, 185, 129, 0.1)', color: '#10b981' },
  'Rejected': { bg: 'rgba(239, 68, 68, 0.1)', color: '#ef4444' },
  'Completed': { bg: 'rgba(59, 130, 246, 0.1)', color: '#3b82f6' },
  'Cancelled': { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280' },
};

const DashboardMenu = () => {
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState({});
  const [expiredWarranty, setExpiredWarranty] = useState([]);
  const [upcomingMaintenance, setUpcomingMaintenance] = useState([]);
  const [recentTransactions, setRecentTransactions] = useState({ data: [] });
  const [monthlyStats, setMonthlyStats] = useState(null);

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    setLoading(true);
    const result = await dashboardData.fetchDashboardData();
    if (result.success) {
      setStats(result.data.stats);
      setExpiredWarranty(result.data.expiredWarranty || []);
      setUpcomingMaintenance(result.data.upcomingMaintenance || []);
      setRecentTransactions(result.data.recentTransactions || { data: [] });
      setMonthlyStats(result.data.monthlyStats || null);
    }
    setLoading(false);
  };

  const statusChartData = {
    labels: ['Available', 'Assigned', 'Under Repair', 'Retired'],
    datasets: [{
      data: [
        stats.availableAssets || 0,
        stats.assignedAssets || 0,
        stats.underRepairAssets || 0,
        stats.retiredAssets || 0,
      ],
      backgroundColor: ['#10b981', '#3b82f6', '#f59e0b', '#6b7280'],
      borderWidth: 0,
    }],
  };

  const hasMonthlyData = monthlyStats && monthlyStats.counts && monthlyStats.counts.length > 0;

  const transactionChartData = hasMonthlyData ? {
    labels: monthlyStats.months || [],
    datasets: [{
      label: 'Transactions',
      data: monthlyStats.counts || [],
      borderColor: '#dc2626',
      backgroundColor: 'rgba(220, 38, 38, 0.1)',
      tension: 0.4,
    }],
  } : {
    labels: [],
    datasets: [{
      label: 'No data',
      data: [],
      borderColor: '#dc2626',
      backgroundColor: 'rgba(220, 38, 38, 0.1)',
      tension: 0.4,
    }],
  };

  const transactionColumns = [
    { field: "assetCode", headerName: "Asset Code", width: 120 },
    { field: "assetName", headerName: "Asset Name", flex: 1, minWidth: 180 },
    { field: "transactionType", headerName: "Type", width: 120 },
    {
      field: "transactionStatus",
      headerName: "Status",
      width: 120,
      renderCell: (p) => {
        const colors = STATUS_CHIP_COLORS[p.value] || { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280' };
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
      field: "transactionDate",
      headerName: "Date",
      width: 160,
      valueFormatter: (p) => utilsHelper.formatDateTime(p.value),
    },
  ];

  if (loading) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="dashboard fade-transition">
      <div className="dashboard__stats">
        {[
          [FiPackage, 'Total Assets', stats.totalAssets || 0],
          [FiCheckCircle, 'Available', stats.availableAssets || 0],
          [FiUser, 'Assigned', stats.assignedAssets || 0],
          [FiTool, 'Under Repair', stats.underRepairAssets || 0],
          [FiDollarSign, 'Total Value', utilsHelper.formatCurrency(stats.totalAssetValue)],
          [FiClock, 'Pending', stats.pendingTransactions || 0],
        ].map(([Icon, label, value]) => (
          <Card key={label} className="dashboard__stat-card">
            <Icon aria-hidden="true" />
            <div><h3>{label}</h3><p>{value}</p></div>
          </Card>
        ))}
      </div>

      <div className="dashboard__charts">
        <Card title="Asset Status">
          <Doughnut
            data={statusChartData}
            options={{ responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } }}
          />
        </Card>
        <Card title={hasMonthlyData ? "Monthly Transactions" : "Monthly Transactions (No Data)"}>
          {hasMonthlyData ? (
            <Line
              data={transactionChartData}
              options={{ responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } }}
            />
          ) : (
            <div className="dashboard__chart-empty" aria-label="Monthly transaction data not available yet">
              <FiCalendar size={40} aria-hidden="true" />
              <p>Monthly data will appear here once available</p>
            </div>
          )}
        </Card>
      </div>

      <div className="dashboard__alerts">
        {expiredWarranty.length > 0 && (
          <Card title={<span><FiAlertTriangle aria-hidden="true" /> Expired Warranty ({expiredWarranty.length})</span>}>
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
          <Card title={<span><FiCalendar aria-hidden="true" /> Upcoming Maintenance ({upcomingMaintenance.length})</span>}>
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
          getRowId={(row) => row.assetTransactionId}
          hideFooter={true}
          ariaLabel="Recent transactions table"
        />
      </Card>
    </div>
  );
};

export default DashboardMenu;