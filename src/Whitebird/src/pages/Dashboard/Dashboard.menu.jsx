import React, { useEffect, useState } from 'react';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  PointElement,
  LineElement
} from 'chart.js';
import { Bar, Doughnut, Line } from 'react-chartjs-2';
import DashboardData from './Dashboard.data';
import Card from '../../components/atoms/Card/Card';
import DataTable from '../../components/molecules/DataTable/DataTable';
import Badge from '../../components/atoms/Badge/Badge';
import Spinner from '../../components/atoms/Spinner/Spinner';
import utilsHelper from '../../core/utils/utils.helper';

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  PointElement,
  LineElement
);

const DashboardMenu = () => {
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState({});
  const [expiredWarranty, setExpiredWarranty] = useState([]);
  const [upcomingMaintenance, setUpcomingMaintenance] = useState([]);
  const [pendingApprovals, setPendingApprovals] = useState([]);
  const [recentTransactions, setRecentTransactions] = useState({ data: [] });

  const dashboardData = new DashboardData();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    const result = await dashboardData.loadDashboardData();
    
    if (result.success) {
      setStats(result.data.stats);
      setExpiredWarranty(result.data.expiredWarranty);
      setUpcomingMaintenance(result.data.upcomingMaintenance);
      setPendingApprovals(result.data.pendingApprovals);
      setRecentTransactions(result.data.recentTransactions);
    }
    
    setLoading(false);
  };

  const assetStatusChartData = {
    labels: ['Available', 'Assigned', 'Under Repair', 'Retired'],
    datasets: [{
      data: [
        stats.availableAssets || 0,
        stats.assignedAssets || 0,
        stats.underRepairAssets || 0,
        stats.retiredAssets || 0
      ],
      backgroundColor: ['#10b981', '#3b82f6', '#f59e0b', '#6b7280'],
      borderWidth: 0
    }]
  };

  const monthlyTransactionData = {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
    datasets: [{
      label: 'Transactions',
      data: [65, 59, 80, 81, 56, 55, 40, 45, 60, 70, 75, 80],
      borderColor: '#dc2626',
      backgroundColor: 'rgba(220, 38, 38, 0.1)',
      tension: 0.4
    }]
  };

  const transactionColumns = [
    { field: 'assetCode', headerName: 'Asset Code', width: 120 },
    { field: 'assetName', headerName: 'Asset Name', width: 200 },
    { field: 'transactionType', headerName: 'Type', width: 120 },
    {
      field: 'transactionStatus',
      headerName: 'Status',
      width: 120,
      renderCell: (params) => (
        <Badge variant={utilsHelper.getStatusColor(params.value)}>
          {params.value}
        </Badge>
      )
    },
    {
      field: 'transactionDate',
      headerName: 'Date',
      width: 160,
      valueFormatter: (params) => utilsHelper.formatDateTime(params.value)
    }
  ];

  if (loading) {
    return (
      <div className="dashboard-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="dashboard">
      {/* Stats Cards */}
      <div className="dashboard__stats">
        <Card className="dashboard__stat-card">
          <div className="dashboard__stat-icon">📦</div>
          <div className="dashboard__stat-content">
            <h3>Total Assets</h3>
            <p className="dashboard__stat-value">{stats.totalAssets || 0}</p>
          </div>
        </Card>
        
        <Card className="dashboard__stat-card">
          <div className="dashboard__stat-icon">✅</div>
          <div className="dashboard__stat-content">
            <h3>Available</h3>
            <p className="dashboard__stat-value">{stats.availableAssets || 0}</p>
          </div>
        </Card>
        
        <Card className="dashboard__stat-card">
          <div className="dashboard__stat-icon">👤</div>
          <div className="dashboard__stat-content">
            <h3>Assigned</h3>
            <p className="dashboard__stat-value">{stats.assignedAssets || 0}</p>
          </div>
        </Card>
        
        <Card className="dashboard__stat-card">
          <div className="dashboard__stat-icon">🔧</div>
          <div className="dashboard__stat-content">
            <h3>Under Repair</h3>
            <p className="dashboard__stat-value">{stats.underRepairAssets || 0}</p>
          </div>
        </Card>
        
        <Card className="dashboard__stat-card">
          <div className="dashboard__stat-icon">💰</div>
          <div className="dashboard__stat-content">
            <h3>Total Value</h3>
            <p className="dashboard__stat-value">
              {utilsHelper.formatCurrency(stats.totalAssetValue)}
            </p>
          </div>
        </Card>
        
        <Card className="dashboard__stat-card">
          <div className="dashboard__stat-icon">⏳</div>
          <div className="dashboard__stat-content">
            <h3>Pending Approvals</h3>
            <p className="dashboard__stat-value">{stats.pendingTransactions || 0}</p>
          </div>
        </Card>
      </div>

      {/* Charts */}
      <div className="dashboard__charts">
        <Card title="Asset Status Distribution" className="dashboard__chart-card">
          <Doughnut 
            data={assetStatusChartData}
            options={{
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                legend: {
                  position: 'bottom'
                }
              }
            }}
          />
        </Card>
        
        <Card title="Monthly Transactions" className="dashboard__chart-card">
          <Line
            data={monthlyTransactionData}
            options={{
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                legend: {
                  display: false
                }
              }
            }}
          />
        </Card>
      </div>

      {/* Alerts Section */}
      <div className="dashboard__alerts">
        {expiredWarranty.length > 0 && (
          <Card 
            title={`Expired Warranty (${expiredWarranty.length})`}
            className="dashboard__alert-card"
          >
            <ul className="dashboard__alert-list">
              {expiredWarranty.slice(0, 5).map(item => (
                <li key={item.assetId} className="dashboard__alert-item">
                  <span className="dashboard__alert-code">{item.assetCode}</span>
                  <span className="dashboard__alert-name">{item.assetName}</span>
                  <Badge variant="error">Expired</Badge>
                </li>
              ))}
            </ul>
          </Card>
        )}
        
        {upcomingMaintenance.length > 0 && (
          <Card 
            title={`Upcoming Maintenance (${upcomingMaintenance.length})`}
            className="dashboard__alert-card"
          >
            <ul className="dashboard__alert-list">
              {upcomingMaintenance.slice(0, 5).map(item => (
                <li key={item.assetId} className="dashboard__alert-item">
                  <span className="dashboard__alert-code">{item.assetCode}</span>
                  <span className="dashboard__alert-name">{item.assetName}</span>
                  <span className="dashboard__alert-date">
                    {utilsHelper.formatDate(item.nextMaintenanceDate)}
                  </span>
                </li>
              ))}
            </ul>
          </Card>
        )}
      </div>

      {/* Recent Transactions */}
      <Card title="Recent Transactions" className="dashboard__transactions">
        <DataTable
          rows={recentTransactions.data || []}
          columns={transactionColumns}
          pageSize={5}
          autoHeight
          getRowId={(row) => row.assetTransactionId}
        />
      </Card>
    </div>
  );
};

export default DashboardMenu;