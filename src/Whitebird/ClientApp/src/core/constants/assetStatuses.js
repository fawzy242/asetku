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
  DAMAGED: 'Damaged',
  RETIRED: 'Retired',
  DISPOSED: 'Disposed',
  INACTIVE: 'Inactive',
};

/**
 * Full asset status options untuk form create/edit.
 */
export const ASSET_STATUS_OPTIONS = [
  { value: ASSET_STATUSES.AVAILABLE, label: 'Available' },
  { value: ASSET_STATUSES.ASSIGNED, label: 'Assigned' },
  { value: ASSET_STATUSES.ON_LOAN, label: 'On Loan' },
  { value: ASSET_STATUSES.IN_MAINTENANCE, label: 'In Maintenance' },
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
 * UPDATED: Asset status tabs untuk navigation
 * REMOVED: Under Repair (not in backend)
 */
export const ASSET_STATUS_TABS = [
  { id: 'all', label: 'All Assets' },
  { id: 'Available', label: 'Available' },
  { id: 'Assigned', label: 'Assigned' },
  { id: 'On Loan', label: 'On Loan' },
  { id: 'In Maintenance', label: 'In Maintenance' },
  { id: 'Damaged', label: 'Damaged' },
];

/**
 * Asset status options untuk tab navigation (untuk grid filter)
 */
export const ASSET_GRID_TABS = [
  { id: 'all', label: 'All Assets' },
  { id: 'Available', label: 'Available' },
  { id: 'Assigned', label: 'Assigned' },
  { id: 'On Loan', label: 'On Loan' },
  { id: 'In Maintenance', label: 'In Maintenance' },
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
 * UPDATED: Transaction tabs
 */
export const TRANSACTION_TABS = [
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

/**
 * Get asset status from transaction type
 */
export const getAssetStatusFromTransactionType = (transactionType) => {
  switch (transactionType) {
    case 1: // HANDOVER
    case 2: // TRANSFER
      return ASSET_STATUSES.ASSIGNED;
    case 3: // LOAN
      return ASSET_STATUSES.ON_LOAN;
    case 6: // MAINTENANCE
      return ASSET_STATUSES.IN_MAINTENANCE;
    case 8: // DISPOSAL
      return ASSET_STATUSES.DISPOSED;
    default:
      return ASSET_STATUSES.AVAILABLE;
  }
};

export default ASSET_STATUSES;