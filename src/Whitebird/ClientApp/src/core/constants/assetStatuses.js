// src/core/constants/assetStatuses.js

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

export const ASSET_STATUS_OPTIONS = [
  { value: ASSET_STATUSES.AVAILABLE, label: 'Available' },
  { value: ASSET_STATUSES.ASSIGNED, label: 'Assigned' },
  { value: ASSET_STATUSES.ON_LOAN, label: 'On Loan' },
  { value: ASSET_STATUSES.IN_MAINTENANCE, label: 'In Maintenance' },
  { value: ASSET_STATUSES.DAMAGED, label: 'Damaged' },
  { value: ASSET_STATUSES.RETIRED, label: 'Retired' },
  { value: ASSET_STATUSES.DISPOSED, label: 'Disposed' },
];

export const ASSET_STATUS_FILTER_OPTIONS = [
  { value: '', label: 'All Statuses' },
  ...ASSET_STATUS_OPTIONS,
];

// FIX: Asset tabs - sesuai spesifikasi
export const ASSET_TABS = [
  { id: 'all', label: 'All' },
  { id: 'active', label: 'Active' },
  { id: 'available', label: 'Available' },
  { id: 'assigned', label: 'Assigned' },
  { id: 'onloan', label: 'On Loan' },
  { id: 'inmaintenance', label: 'In Maintenance' },
  { id: 'damaged', label: 'Damaged' },
  { id: 'inactive', label: 'Inactive' },
];

export const ASSET_GRID_TABS = ASSET_TABS;

export const ASSET_STATUS_TABS = ASSET_TABS;

export const ASSET_STATUS_CHART_COLORS = {
  'Available': '#10b981',
  'Assigned': '#3b82f6',
  'On Loan': '#8b5cf6',
  'In Maintenance': '#f59e0b',
  'Damaged': '#ef4444',
  'Retired': '#6b7280',
  'Disposed': '#4b5563',
};

export const ASSET_CONDITION_OPTIONS = [
  { value: '', label: 'Select Condition' },
  { value: '1', label: 'Good' },
  { value: '2', label: 'Normal' },
  { value: '3', label: 'Damaged' },
];

export const ASSET_CONDITION_PURCHASE_OPTIONS = [
  { value: '', label: 'Select' },
  { value: '1', label: 'New' },
  { value: '2', label: 'Second Hand' },
];

export const CONDITION_OPTIONS = ASSET_CONDITION_OPTIONS;

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