/**
 * CENTRALIZED ASSET STATUS CONSTANTS
 * Single source of truth untuk semua asset status options.
 * 
 * NOTE: Status ini untuk DISPLAY SAJA. Backend mengirim currentStatus dari active transaction.
 * 
 * Usage:
 *   import { ASSET_STATUS_OPTIONS, ASSET_STATUS_FILTER_OPTIONS } from '../../core/constants/assetStatuses';
 */

export const ASSET_STATUSES = {
  AVAILABLE: 'Available',
  ASSIGNED: 'Assigned',
  ON_LOAN: 'On Loan',
  IN_MAINTENANCE: 'In Maintenance',
  UNDER_REPAIR: 'Under Repair',
  DAMAGED: 'Damaged',
  RETIRED: 'Retired',
  DISPOSED: 'Disposed',
};

/**
 * Full asset status options untuk form create/edit.
 */
export const ASSET_STATUS_OPTIONS = [
  { value: ASSET_STATUSES.AVAILABLE, label: 'Available' },
  { value: ASSET_STATUSES.ASSIGNED, label: 'Assigned' },
  { value: ASSET_STATUSES.ON_LOAN, label: 'On Loan' },
  { value: ASSET_STATUSES.IN_MAINTENANCE, label: 'In Maintenance' },
  { value: ASSET_STATUSES.UNDER_REPAIR, label: 'Under Repair' },
  { value: ASSET_STATUSES.DAMAGED, label: 'Damaged' },
  { value: ASSET_STATUSES.RETIRED, label: 'Retired' },
  { value: ASSET_STATUSES.DISPOSED, label: 'Disposed' },
];

/**
 * Asset status options untuk filter dropdown (include "All").
 */
export const ASSET_STATUS_FILTER_OPTIONS = [
  { value: '', label: 'All Statuses' },
  ...ASSET_STATUS_OPTIONS,
];

/**
 * Asset status options untuk tab navigation.
 */
export const ASSET_STATUS_TABS = [
  { id: 'all', label: 'All Assets' },
  { id: 'Available', label: 'Available' },
  { id: 'Assigned', label: 'Assigned' },
  { id: 'On Loan', label: 'On Loan' },
  { id: 'In Maintenance', label: 'In Maintenance' },
  { id: 'Under Repair', label: 'Under Repair' },
  { id: 'Damaged', label: 'Damaged' },
];

/**
 * Transaction status filter options.
 */
export const TRANSACTION_STATUS_FILTER_OPTIONS = [
  { value: '', label: 'All Statuses' },
  { value: 'Pending', label: 'Pending' },
  { value: 'Approved', label: 'Approved' },
  { value: 'Rejected', label: 'Rejected' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Cancelled', label: 'Cancelled' },
];

/**
 * Transaction tabs untuk halaman AssetTransaction.
 */
export const TRANSACTION_TABS = [
  { id: 'all', label: 'All' },
  { id: 'pending', label: 'Pending' },
  { id: 'approved', label: 'Approved' },
  { id: 'rejected', label: 'Rejected' },
  { id: 'active-loans', label: 'Active Loans' },
  { id: 'overdue-loans', label: 'Overdue Loans' },
];

/**
 * Dashboard chart color mapping untuk asset status.
 */
export const ASSET_STATUS_CHART_COLORS = {
  'Available': '#10b981',
  'Assigned': '#3b82f6',
  'On Loan': '#8b5cf6',
  'In Maintenance': '#f59e0b',
  'Under Repair': '#d97706',
  'Damaged': '#ef4444',
  'Retired': '#6b7280',
  'Disposed': '#4b5563',
};

/**
 * Asset Condition Mapping (from MasterData)
 * Untuk form create/edit asset
 */
export const ASSET_CONDITION_OPTIONS = [
  { value: '', label: 'Select Condition' },
  { value: '1', label: 'Good' },
  { value: '2', label: 'Normal' },
  { value: '3', label: 'Damaged' },
];

/**
 * Asset Condition Purchase Mapping
 */
export const ASSET_CONDITION_PURCHASE_OPTIONS = [
  { value: '', label: 'Select' },
  { value: '1', label: 'New' },
  { value: '2', label: 'Second Hand' },
];

/**
 * Condition options untuk dropdown (deprecated, use ASSET_CONDITION_OPTIONS)
 */
export const CONDITION_OPTIONS = ASSET_CONDITION_OPTIONS;

export default ASSET_STATUSES;