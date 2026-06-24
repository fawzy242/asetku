import React from 'react';
import { Chip } from '@mui/material';
import { getStatusChipStyles } from '../../../core/constants/statusColors';
import './StatusChip.scss';

/**
 * Centralized Status Chip component
 * Uses getStatusChipStyles from statusColors.js
 * 
 * @param {Object} props
 * @param {string} props.status - Status text
 * @param {string} props.size - Chip size (small, medium)
 * @param {string} props.className - Additional class
 */
const StatusChip = ({ status, size = 'small', className = '' }) => {
  const displayStatus = status || 'Available';
  const styles = getStatusChipStyles(displayStatus);
  
  return (
    <Chip 
      label={displayStatus} 
      size={size} 
      className={`status-chip ${className}`}
      sx={styles}
    />
  );
};

export default StatusChip;