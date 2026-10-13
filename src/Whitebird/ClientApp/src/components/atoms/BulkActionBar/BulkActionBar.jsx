import React from 'react';
import Button from '../Button/Button';
import './BulkActionBar.scss';

/**
 * Renders a horizontal strip of bulk action buttons.
 * Renders nothing when visible is false or actions list is empty.
 *
 * @param {boolean} visible - Typically selectionCount > 0
 * @param {Array}   actions - [{ key, label, variant, size, icon, onClick, disabled, loading }]
 * @param {string}  className
 */
const BulkActionBar = ({ visible = true, actions = [], className = '' }) => {
  if (!visible || !actions || actions.length === 0) return null;

  return (
    <div className={`bulk-action-bar ${className}`}>
      {actions.map((action, idx) => (
        <Button
          key={action.key || `bulk-${idx}`}
          variant={action.variant || 'primary'}
          size={action.size || 'sm'}
          onClick={action.onClick}
          disabled={action.disabled}
          loading={action.loading}
          startIcon={action.icon}
        >
          {action.label}
        </Button>
      ))}
    </div>
  );
};

export default BulkActionBar;