import React from 'react';
import './StatusBadge.scss';

const STATUS_COLORS = {
  // Asset Status
  'Available': 'success',
  'Assigned': 'primary',
  'Under Repair': 'warning',
  'Maintenance': 'warning',
  'Retired': 'secondary',
  'Disposed': 'secondary',
  // Transaction Status
  'Pending': 'warning',
  'Approved': 'success',
  'Rejected': 'error',
  'Completed': 'success',
  'Cancelled': 'secondary',
  // Employee Status
  'Active': 'success',
  'Resigned': 'secondary',
  'On Leave': 'warning',
};

/**
 * Reusable status badge component
 * @param {Object} props
 * @param {string} props.status - Status text to display
 * @param {string} [props.className] - Additional CSS class
 */
const StatusBadge = ({ status, className = '' }) => {
  if (!status && status !== 0) {
    return <span className={`status-badge status-badge--secondary ${className}`}>-</span>;
  }

  const variant = STATUS_COLORS[status] || 'secondary';

  return (
    <span className={`status-badge status-badge--${variant} ${className}`}>
      {status}
    </span>
  );
};

export default StatusBadge;