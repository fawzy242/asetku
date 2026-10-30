// src/pages/Dashboard/constants.jsx
import React from 'react';
import {
  FiGrid,
  FiPieChart,
  FiList,
} from 'react-icons/fi';

export const ASSET_STATUS_CHART_COLORS = {
  Available: '#10b981',
  Assigned: '#3b82f6',
  'On Loan': '#8b5cf6',
  'In Maintenance': '#f59e0b',
  Damaged: '#ef4444',
  Retired: '#6b7280',
  Disposed: '#4b5563',
};

export const EMPTY_DASHBOARD = {
  stats: {},
  monthlyStats: [],
  categoryBreakdown: [],
  recentTransactions: [],
  expiredWarranty: [],
  scheduleStats: {},
};

export const DASHBOARD_TABS = [
  { id: 'summary', label: 'Summary', icon: <FiGrid size={16} /> },
  { id: 'charts', label: 'Charts & Analytics', icon: <FiPieChart size={16} /> },
  { id: 'recent', label: 'Recent Activity', icon: <FiList size={16} /> },
];

export const MONTHLY_LABELS = [
  'Jan',
  'Feb',
  'Mar',
  'Apr',
  'May',
  'Jun',
  'Jul',
  'Aug',
  'Sep',
  'Oct',
  'Nov',
  'Dec',
];