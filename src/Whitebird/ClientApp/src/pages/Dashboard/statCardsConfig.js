// src/pages/Dashboard/statCardsConfig.js
import {
  FiPackage,
  FiCheckCircle,
  FiUser,
  FiTool,
  FiDollarSign,
  FiClock,
  FiAlertTriangle,
  FiCalendar,
  FiLayers,
  FiShield,
  FiTrendingUp,
  FiBarChart2,
  FiUsers,
  FiBriefcase,
  FiMapPin,
} from 'react-icons/fi';
import { formatCurrency } from '../../core/utils/number.utils';

/**
 * Builds the 16-card configuration used by the Summary tab.
 * All derivations (this-month count, utilization rate, damaged count, etc.)
 * happen here so the page stays focused on orchestration.
 */
export const buildStatCardsConfig = ({ stats = {}, scheduleStats = {}, monthlyStats = [] }) => {
  const currentMonth = new Date().getMonth();
  const thisMonthTransactions = monthlyStats.find(
    (item) => item.month === currentMonth + 1
  );
  const thisMonthCount = thisMonthTransactions?.transactionCount || 0;

  const assetUtilizationRate =
    stats.totalAssets > 0
      ? ((stats.assignedAssets + stats.assetsOnLoan) / stats.totalAssets) * 100
      : 0;

  const pendingApprovalsCount = stats.pendingApprovals || 0;
  const damagedCount = stats.damagedAssets || 0;
  const totalEmployees = stats.totalEmployees || 0;
  const activeEmployees = stats.activeEmployees || 0;

  return [
    {
      icon: FiPackage,
      label: 'Total Assets',
      value: stats.totalAssets || 0,
      endpoint: '/Asset/grid',
      params: {},
      color: '#dc2626',
      bg: 'rgba(220, 38, 38, 0.1)',
    },
    {
      icon: FiCheckCircle,
      label: 'Available',
      value: stats.availableAssets || 0,
      endpoint: '/Asset/grid',
      params: { status: 'Available' },
      color: '#10b981',
      bg: 'rgba(16, 185, 129, 0.1)',
    },
    {
      icon: FiUser,
      label: 'Assigned',
      value: stats.assignedAssets || 0,
      endpoint: '/Asset/grid',
      params: { status: 'Assigned' },
      color: '#3b82f6',
      bg: 'rgba(59, 130, 246, 0.1)',
    },
    {
      icon: FiLayers,
      label: 'On Loan',
      value: stats.assetsOnLoan || 0,
      endpoint: '/Asset/grid',
      params: { status: 'On Loan' },
      color: '#8b5cf6',
      bg: 'rgba(139, 92, 246, 0.1)',
    },
    {
      icon: FiTool,
      label: 'In Maintenance',
      value: stats.assetsInMaintenance || 0,
      endpoint: '/Asset/grid',
      params: { status: 'In Maintenance' },
      color: '#f59e0b',
      bg: 'rgba(245, 158, 11, 0.1)',
    },
    {
      icon: FiShield,
      label: 'Damaged',
      value: damagedCount,
      endpoint: '/Asset/grid',
      params: { status: 'Damaged' },
      color: '#ef4444',
      bg: 'rgba(239, 68, 68, 0.1)',
    },
    {
      icon: FiClock,
      label: 'Pending Approvals',
      value: pendingApprovalsCount,
      endpoint: '/AssetTransaction/grid-uncomplete',
      params: {},
      color: '#f59e0b',
      bg: 'rgba(245, 158, 11, 0.1)',
    },
    {
      icon: FiAlertTriangle,
      label: 'Overdue Loans',
      value: stats.overdueLoanCount || 0,
      endpoint: '/AssetTransaction/overdue-loans',
      params: {},
      color: '#ef4444',
      bg: 'rgba(239, 68, 68, 0.1)',
    },
    {
      icon: FiDollarSign,
      label: 'Total Value',
      value: formatCurrency(stats.totalAssetValue),
      noDrilldown: true,
      color: '#10b981',
      bg: 'rgba(16, 185, 129, 0.1)',
    },
    {
      icon: FiTrendingUp,
      label: 'Utilization',
      value: `${assetUtilizationRate.toFixed(1)}%`,
      noDrilldown: true,
      color: '#06b6d4',
      bg: 'rgba(6, 182, 212, 0.1)',
    },
    {
      icon: FiBarChart2,
      label: 'This Month',
      value: thisMonthCount,
      endpoint: '/AssetTransaction/grid-add',
      params: {},
      color: '#6366f1',
      bg: 'rgba(99, 102, 241, 0.1)',
    },
    {
      icon: FiCalendar,
      label: 'Upcoming Maintenance',
      value: scheduleStats.upcomingMaintenance || 0,
      noDrilldown: true,
      color: '#f59e0b',
      bg: 'rgba(245, 158, 11, 0.1)',
    },
    {
      icon: FiUsers,
      label: 'Total Employees',
      value: totalEmployees,
      noDrilldown: true,
      color: '#8b5cf6',
      bg: 'rgba(139, 92, 246, 0.1)',
    },
    {
      icon: FiUser,
      label: 'Active Employees',
      value: activeEmployees,
      noDrilldown: true,
      color: '#10b981',
      bg: 'rgba(16, 185, 129, 0.1)',
    },
    {
      icon: FiBriefcase,
      label: 'Departments',
      value: stats.totalDepartments || 0,
      noDrilldown: true,
      color: '#f59e0b',
      bg: 'rgba(245, 158, 11, 0.1)',
    },
    {
      icon: FiMapPin,
      label: 'Offices',
      value: stats.totalOffices || 0,
      noDrilldown: true,
      color: '#3b82f6',
      bg: 'rgba(59, 130, 246, 0.1)',
    },
  ];
};