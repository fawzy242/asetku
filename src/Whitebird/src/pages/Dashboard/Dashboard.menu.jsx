import React, { useEffect, useState } from "react";
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ArcElement, PointElement, LineElement } from 'chart.js';
import { Doughnut, Line } from 'react-chartjs-2';
import { FiPackage, FiCheckCircle, FiUser, FiTool, FiDollarSign, FiClock, FiAlertTriangle, FiCalendar } from "react-icons/fi";
import DashboardData from "./Dashboard.data";
import Card from "../../components/atoms/Card/Card";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Spinner from "../../components/atoms/Spinner/Spinner";
import utilsHelper from "../../core/utils/utils.helper";
import "./Dashboard.scss";

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ArcElement, PointElement, LineElement);

const dashboardData = new DashboardData();

const DashboardMenu = () => {
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState({});
  const [expiredWarranty, setExpiredWarranty] = useState([]);
  const [upcomingMaintenance, setUpcomingMaintenance] = useState([]);
  const [recentTransactions, setRecentTransactions] = useState({ data: [] });

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    setLoading(true);
    const result = await dashboardData.fetchDashboardData();
    if (result.success) { setStats(result.data.stats); setExpiredWarranty(result.data.expiredWarranty); setUpcomingMaintenance(result.data.upcomingMaintenance); setRecentTransactions(result.data.recentTransactions); }
    setLoading(false);
  };

  const statusChartData = { labels: ['Available', 'Assigned', 'Under Repair', 'Retired'], datasets: [{ data: [stats.availableAssets || 0, stats.assignedAssets || 0, stats.underRepairAssets || 0, stats.retiredAssets || 0], backgroundColor: ['#10b981', '#3b82f6', '#f59e0b', '#6b7280'], borderWidth: 0 }] };
  const transactionChartData = { labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'], datasets: [{ label: 'Transactions', data: [65, 59, 80, 81, 56, 55, 40, 45, 60, 70, 75, 80], borderColor: '#dc2626', backgroundColor: 'rgba(220, 38, 38, 0.1)', tension: 0.4 }] };

  const transactionColumns = [
    { field: "assetCode", headerName: "Asset Code", width: 120 },
    { field: "assetName", headerName: "Asset Name", width: 200 },
    { field: "transactionType", headerName: "Type", width: 120 },
    { field: "transactionStatus", headerName: "Status", width: 120, renderCell: (p) => <span className={`status-badge status-badge--${utilsHelper.getStatusColor(p.value)}`}>{p.value}</span> },
    { field: "transactionDate", headerName: "Date", width: 160, valueFormatter: (p) => utilsHelper.formatDateTime(p.value) },
  ];

  if (loading) return <div className="dashboard-loading"><Spinner size="lg" /></div>;

  return (
    <div className="dashboard fade-transition">
      <div className="dashboard__stats">
        <Card className="dashboard__stat-card"><FiPackage /><div><h3>Total Assets</h3><p>{stats.totalAssets || 0}</p></div></Card>
        <Card className="dashboard__stat-card"><FiCheckCircle /><div><h3>Available</h3><p>{stats.availableAssets || 0}</p></div></Card>
        <Card className="dashboard__stat-card"><FiUser /><div><h3>Assigned</h3><p>{stats.assignedAssets || 0}</p></div></Card>
        <Card className="dashboard__stat-card"><FiTool /><div><h3>Under Repair</h3><p>{stats.underRepairAssets || 0}</p></div></Card>
        <Card className="dashboard__stat-card"><FiDollarSign /><div><h3>Total Value</h3><p>{utilsHelper.formatCurrency(stats.totalAssetValue)}</p></div></Card>
        <Card className="dashboard__stat-card"><FiClock /><div><h3>Pending</h3><p>{stats.pendingTransactions || 0}</p></div></Card>
      </div>

      <div className="dashboard__charts">
        <Card title="Asset Status"><Doughnut data={statusChartData} options={{ responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } }} /></Card>
        <Card title="Monthly Transactions"><Line data={transactionChartData} options={{ responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } }} /></Card>
      </div>

      <div className="dashboard__alerts">
        {expiredWarranty.length > 0 && <Card title={<><FiAlertTriangle /> Expired Warranty ({expiredWarranty.length})</>}><ul>{expiredWarranty.slice(0, 5).map(item => <li key={item.assetId}><span>{item.assetCode}</span><span>{item.assetName}</span><span className="badge badge--error">Expired</span></li>)}</ul></Card>}
        {upcomingMaintenance.length > 0 && <Card title={<><FiCalendar /> Upcoming Maintenance ({upcomingMaintenance.length})</>}><ul>{upcomingMaintenance.slice(0, 5).map(item => <li key={item.assetId}><span>{item.assetCode}</span><span>{item.assetName}</span><span>{utilsHelper.formatDate(item.nextMaintenanceDate)}</span></li>)}</ul></Card>}
      </div>

      <Card title="Recent Transactions"><DataTable rows={recentTransactions.data || []} columns={transactionColumns} pageSize={5} getRowId={(row) => row.assetTransactionId} hideFooter={true} /></Card>
    </div>
  );
};

export default DashboardMenu;