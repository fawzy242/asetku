import React from 'react';
import { FiRefreshCw } from 'react-icons/fi';
import { Tooltip } from '@mui/material';
import './RefreshButton.scss';

/**
 * Icon button that visually indicates loading via a spinning icon.
 * Renders as a compact square button by default; pass `label` to render text too.
 */
const RefreshButton = ({
  onClick,
  loading = false,
  label = null,
  size = 'md',
  tooltip = 'Refresh',
  disabled = false,
  className = '',
}) => {
  const iconSize = size === 'sm' ? 16 : 18;

  return (
    <Tooltip title={tooltip}>
      <span className={`refresh-button-wrapper ${className}`}>
        <button
          type="button"
          className={`refresh-button refresh-button--${size} ${label ? 'refresh-button--with-label' : ''}`}
          onClick={onClick}
          disabled={disabled || loading}
          aria-label={tooltip}
        >
          <FiRefreshCw
            size={iconSize}
            className={`refresh-button__icon ${loading ? 'refresh-button__icon--spinning' : ''}`}
          />
          {label && <span className="refresh-button__label">{label}</span>}
        </button>
      </span>
    </Tooltip>
  );
};

export default RefreshButton;