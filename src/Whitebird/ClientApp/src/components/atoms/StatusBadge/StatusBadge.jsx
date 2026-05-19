import React from 'react';
import { STATUS_COLORS } from '../../../core/constants/statusColors';
import './StatusBadge.scss';

const STATUS_VARIANT_MAP = {
  'success': 'success',
  'primary': 'primary',
  'purple': 'purple',
  'warning': 'warning',
  'error': 'error',
  'secondary': 'secondary',
  'info': 'info',
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

  const colorConfig = STATUS_COLORS[status] || STATUS_COLORS.default;
  const variant = STATUS_VARIANT_MAP[colorConfig.label] || 'secondary';

  return (
    <span className={`status-badge status-badge--${variant} ${className}`}>
      {status}
    </span>
  );
};

export default StatusBadge;