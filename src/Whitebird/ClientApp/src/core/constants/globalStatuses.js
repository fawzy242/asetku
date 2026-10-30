// src/core/constants/globalStatuses.js
//
// Centralized status constants used across the asset management menus.
// This file consolidates what were previously three separate files:
//   - assetStatuses.js        (asset lifecycle status + tab set + options)
//   - employmentStatuses.js   (employment status + codes + chip palette)
//   - statusColors.js         (mixed status -> chip style mapping + helper)
//
// Every named export from those files is preserved so existing imports keep
// working after the import paths are updated to point here.

// ============================================================
// ASSET LIFECYCLE STATUSES
// ============================================================
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

// ============================================================
// EMPLOYMENT STATUSES
// ============================================================
export const EMPLOYMENT_STATUSES = {
  INTERNSHIP: 'Internship',
  TRAINING: 'Training',
  CONTRACT: 'Contract',
  PERMANENT: 'Permanent Employee',
  RESIGNED: 'Resigned',
};

export const EMPLOYMENT_STATUS_CODES = {
  INTERNSHIP: 1,
  TRAINING: 2,
  CONTRACT: 3,
  PERMANENT: 4,
  RESIGNED: 5,
};

export const EMPLOYMENT_STATUS_COLORS = {
  [EMPLOYMENT_STATUSES.INTERNSHIP]: { bg: 'rgba(16, 185, 129, 0.15)', color: '#10b981' },
  [EMPLOYMENT_STATUSES.TRAINING]: { bg: 'rgba(59, 130, 246, 0.15)', color: '#3b82f6' },
  [EMPLOYMENT_STATUSES.CONTRACT]: { bg: 'rgba(245, 158, 11, 0.15)', color: '#f59e0b' },
  [EMPLOYMENT_STATUSES.PERMANENT]: { bg: 'rgba(139, 92, 246, 0.15)', color: '#8b5cf6' },
  [EMPLOYMENT_STATUSES.RESIGNED]: { bg: 'rgba(239, 68, 68, 0.15)', color: '#ef4444' },
};

export const getEmploymentStatusStyles = (statusName) => {
  if (!statusName) return { bg: 'rgba(107, 114, 128, 0.15)', color: '#6b7280' };
  return (
    EMPLOYMENT_STATUS_COLORS[statusName] || {
      bg: 'rgba(107, 114, 128, 0.15)',
      color: '#6b7280',
    }
  );
};

// ============================================================
// GENERAL STATUS COLOR PALETTE
// Used by StatusChip and any status-badge surfaces.
// ============================================================
export const STATUS_COLORS = {
  // Asset Status
  'Available':      { bg: 'rgba(16, 185, 129, 0.1)', color: '#10b981', label: 'success' },
  'Assigned':       { bg: 'rgba(59, 130, 246, 0.1)', color: '#3b82f6', label: 'primary' },
  'On Loan':        { bg: 'rgba(139, 92, 246, 0.1)', color: '#8b5cf6', label: 'purple' },
  'In Maintenance': { bg: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b', label: 'warning' },
  'Under Repair':   { bg: 'rgba(245, 158, 11, 0.15)', color: '#d97706', label: 'warning' },
  'Damaged':        { bg: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', label: 'error' },
  'Retired':        { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280', label: 'secondary' },
  'Disposed':       { bg: 'rgba(107, 114, 128, 0.15)', color: '#4b5563', label: 'secondary' },

  // Transaction Status
  'Pending':        { bg: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b', label: 'warning' },
  'Approved':       { bg: 'rgba(16, 185, 129, 0.1)', color: '#10b981', label: 'success' },
  'Rejected':       { bg: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', label: 'error' },
  'Completed':      { bg: 'rgba(59, 130, 246, 0.1)', color: '#3b82f6', label: 'info' },
  'Cancelled':      { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280', label: 'secondary' },

  // Employee Status
  'Active':         { bg: 'rgba(16, 185, 129, 0.1)', color: '#10b981', label: 'success' },
  'Inactive':       { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280', label: 'secondary' },
  'Resigned':       { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280', label: 'secondary' },
  'On Leave':       { bg: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b', label: 'warning' },

  // Default fallback
  'default':        { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280', label: 'secondary' },
};

export const getStatusChipStyles = (status) => {
  const colors = STATUS_COLORS[status] || STATUS_COLORS.default;
  return {
    bgcolor: colors.bg,
    color: colors.color,
    fontWeight: 500,
    fontSize: '0.75rem',
    height: '24px',
    borderRadius: '16px',
    px: 1,
    '& .MuiChip-label': {
      px: 1.5,
    },
  };
};

// ============================================================
// DEFAULT EXPORT
// Aggregates every symbol from this module for callers that prefer a
// namespace import. Existing named imports are unaffected.
// ============================================================
export default {
  ASSET_STATUSES,
  ASSET_STATUS_OPTIONS,
  ASSET_TABS,
  EMPLOYMENT_STATUSES,
  EMPLOYMENT_STATUS_CODES,
  EMPLOYMENT_STATUS_COLORS,
  getEmploymentStatusStyles,
  STATUS_COLORS,
  getStatusChipStyles,
};