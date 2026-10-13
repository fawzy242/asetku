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

export default ASSET_STATUSES;