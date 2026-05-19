/**
 * CENTRALIZED STATUS COLOR CONSTANTS
 * Single source of truth untuk semua status colors di seluruh aplikasi.
 * 
 * Usage:
 *   import { STATUS_COLORS } from '../../core/constants/statusColors';
 *   const colors = STATUS_COLORS[status] || STATUS_COLORS.default;
 */

export const STATUS_COLORS = {
  // Asset Status
  'Available':       { bg: 'rgba(16, 185, 129, 0.1)', color: '#10b981', label: 'success' },
  'Assigned':        { bg: 'rgba(59, 130, 246, 0.1)', color: '#3b82f6', label: 'primary' },
  'On Loan':         { bg: 'rgba(139, 92, 246, 0.1)', color: '#8b5cf6', label: 'purple' },
  'In Maintenance':  { bg: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b', label: 'warning' },
  'Under Repair':    { bg: 'rgba(245, 158, 11, 0.15)', color: '#d97706', label: 'warning' },
  'Damaged':         { bg: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', label: 'error' },
  'Retired':         { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280', label: 'secondary' },
  'Disposed':        { bg: 'rgba(107, 114, 128, 0.15)', color: '#4b5563', label: 'secondary' },

  // Transaction Status
  'Pending':         { bg: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b', label: 'warning' },
  'Approved':        { bg: 'rgba(16, 185, 129, 0.1)', color: '#10b981', label: 'success' },
  'Rejected':        { bg: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', label: 'error' },
  'Completed':       { bg: 'rgba(59, 130, 246, 0.1)', color: '#3b82f6', label: 'info' },
  'Cancelled':       { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280', label: 'secondary' },

  // Employee Status
  'Active':          { bg: 'rgba(16, 185, 129, 0.1)', color: '#10b981', label: 'success' },
  'Resigned':        { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280', label: 'secondary' },
  'On Leave':        { bg: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b', label: 'warning' },

  // Default fallback
  'default':         { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280', label: 'secondary' },
};

/**
 * Get status chip styles for MUI Chip component
 * @param {string} status - Status text
 * @returns {{ bg: string, color: string }}
 */
export const getStatusChipStyles = (status) => {
  const colors = STATUS_COLORS[status] || STATUS_COLORS.default;
  return {
    bgcolor: colors.bg,
    color: colors.color,
    fontWeight: 500,
    fontSize: '0.75rem',
    height: 24,
    borderRadius: '4px',
  };
};

export default STATUS_COLORS;