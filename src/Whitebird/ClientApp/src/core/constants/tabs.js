/**
 * Centralized tab definitions for all pages
 * Untuk menghilangkan duplikasi tab definitions
 */

// Common tabs for Active/Inactive/All pattern
export const STATUS_TABS = [
  { id: 'all', label: 'All' },
  { id: 'active', label: 'Active' },
  { id: 'inactive', label: 'Inactive' },
];

// Asset status tabs
export const ASSET_STATUS_TABS = [
  { id: 'all', label: 'All Assets' },
  { id: 'Available', label: 'Available' },
  { id: 'Assigned', label: 'Assigned' },
  { id: 'On Loan', label: 'On Loan' },
  { id: 'In Maintenance', label: 'In Maintenance' },
];

// Transaction status tabs
export const TRANSACTION_STATUS_TABS = [
  { id: 'all', label: 'All' },
  { id: 'pending', label: 'Pending' },
  { id: 'approved', label: 'Approved' },
  { id: 'rejected', label: 'Rejected' },
  { id: 'active-loans', label: 'Active Loans' },
  { id: 'overdue-loans', label: 'Overdue Loans' },
];

// Master Data tabs
export const MASTER_DATA_TABS = [
  { id: 'transaction-types', label: 'Transaction Types' },
  { id: 'asset-conditions', label: 'Asset Conditions' },
  { id: 'employee-positions', label: 'Employee Positions' },
  { id: 'employee-statuses', label: 'Employee Statuses' },
  { id: 'office-types', label: 'Office Types' },
  { id: 'maintenance-types', label: 'Maintenance Types' },
  { id: 'asset-condition-purchases', label: 'Purchase Conditions' },
];

export default {
  STATUS_TABS,
  ASSET_STATUS_TABS,
  TRANSACTION_STATUS_TABS,
  MASTER_DATA_TABS,
};