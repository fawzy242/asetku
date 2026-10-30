// src/pages/Reports/constants.js
import {
  FiFileText,
  FiBox,
  FiUsers,
  FiTool,
  FiTrendingUp,
} from 'react-icons/fi';

export const REPORT_TYPES = [
  {
    id: 'transactions',
    label: 'Transaction Report',
    icon: FiFileText,
    description: 'Export asset transaction history',
    hasDateRange: true,
    hasTransactionTypeFilter: true,
  },
  {
    id: 'inventory',
    label: 'Inventory Report',
    icon: FiBox,
    description: 'Export current asset inventory',
    hasStatusFilter: true,
    hasCategoryFilter: true,
    hasSupplierFilter: true,
  },
  {
    id: 'employee',
    label: 'Employee Asset Report',
    icon: FiUsers,
    description: 'Export assets assigned to employees',
    hasEmployeeFilter: true,
  },
  {
    id: 'maintenance',
    label: 'Maintenance Report',
    icon: FiTool,
    description: 'Export maintenance history',
    hasDateRange: true,
  },
  {
    id: 'financial',
    label: 'Financial Report',
    icon: FiTrendingUp,
    description: 'Export asset financial summary',
    hasDateRange: true,
  },
];

export const INVENTORY_STATUS_OPTIONS = [
  { value: '', label: 'All Statuses' },
  { value: 'Available', label: 'Available' },
  { value: 'Assigned', label: 'Assigned' },
  { value: 'Damaged', label: 'Damaged' },
];

export const TRANSACTION_TYPE_OPTIONS = [
  { value: '', label: 'All Types' },
  { value: 'HANDOVER', label: 'HANDOVER' },
  { value: 'TRANSFER', label: 'TRANSFER' },
  { value: 'LOAN', label: 'LOAN' },
  { value: 'RETURN', label: 'RETURN' },
  { value: 'LOAN_RETURN', label: 'LOAN_RETURN' },
  { value: 'MAINTENANCE', label: 'MAINTENANCE' },
  { value: 'POST_MAINTENANCE', label: 'POST_MAINTENANCE' },
  { value: 'DISPOSAL', label: 'DISPOSAL' },
  { value: 'OPERATIONAL_OFFICE', label: 'OPERATIONAL_OFFICE' },
];

export const REPORT_OPTIONS = REPORT_TYPES.map((r) => ({
  value: r.id,
  label: r.label,
}));