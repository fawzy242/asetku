import React from 'react';
import { Chip } from '@mui/material';
import { getEmploymentStatusStyles } from '../../../core/constants/employmentStatuses';
import './EmploymentStatusChip.scss';

/**
 * Centralized Employment Status Chip component
 * Uses getEmploymentStatusStyles from employmentStatuses.js
 * 
 * @param {Object} props
 * @param {string} props.status - Employment status text
 * @param {string} props.size - Chip size (small, medium)
 * @param {string} props.className - Additional class
 */
const EmploymentStatusChip = ({ status, size = 'small', className = '' }) => {
  const displayStatus = status || 'Unknown';
  const styles = getEmploymentStatusStyles(displayStatus);
  
  return (
    <Chip 
      label={displayStatus} 
      size={size} 
      className={`employment-status-chip ${className}`}
      sx={{
        bgcolor: styles.bg,
        color: styles.color,
        fontWeight: 500,
        fontSize: size === 'small' ? '0.75rem' : '0.875rem',
        height: size === 'small' ? '24px' : '32px',
        borderRadius: '16px',
        '& .MuiChip-label': {
          px: size === 'small' ? 1.5 : 2,
        }
      }}
    />
  );
};

export default EmploymentStatusChip;