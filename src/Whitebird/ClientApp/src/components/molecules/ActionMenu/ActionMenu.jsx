import React from 'react';
import { Menu, MenuItem, ListItemIcon, ListItemText } from '@mui/material';
import { FiEye } from 'react-icons/fi';
import './ActionMenu.scss';

/**
 * Single MUI Menu rendered once per grid (not once per row).
 * The parent (page or hook) supplies:
 *  - anchorEl, open, onClose
 *  - actions: [{ type, label, icon: Component, color }]
 *  - row: the currently selected row
 *  - onAction(type, row)
 */
const ActionMenu = ({
  anchorEl,
  open,
  onClose,
  actions = [],
  onAction,
  row = null,
}) => {
  if (!actions || actions.length === 0) {
    // Still render the menu so MUI closes it cleanly when open->false
    return (
      <Menu
        anchorEl={anchorEl}
        open={open}
        onClose={onClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
        PaperProps={{ className: 'action-menu__paper' }}
      />
    );
  }

  return (
    <Menu
      anchorEl={anchorEl}
      open={open}
      onClose={onClose}
      anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      transformOrigin={{ vertical: 'top', horizontal: 'right' }}
      PaperProps={{ className: 'action-menu__paper' }}
    >
      {actions.map((action) => {
        const Icon = action.icon || FiEye;
        const colorStyle =
          action.color === 'error'
            ? 'var(--error)'
            : action.color === 'success'
              ? 'var(--success)'
              : action.color === 'warning'
                ? 'var(--warning)'
                : action.color === 'info'
                  ? 'var(--info)'
                  : 'var(--text-primary)';

        return (
          <MenuItem
            key={action.type}
            onClick={() => onAction(action.type, row)}
            sx={{ color: colorStyle }}
          >
            <ListItemIcon sx={{ minWidth: 36, color: 'inherit' }}>
              <Icon size={18} />
            </ListItemIcon>
            <ListItemText>{action.label}</ListItemText>
          </MenuItem>
        );
      })}
    </Menu>
  );
};

export default ActionMenu;