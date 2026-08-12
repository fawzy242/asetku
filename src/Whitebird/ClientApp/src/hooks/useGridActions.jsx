// src/hooks/useGridActions.jsx
import React, { useState, useCallback, useMemo } from 'react';
import { IconButton, Menu, MenuItem, ListItemIcon, ListItemText } from '@mui/material';
import { FiMoreVertical, FiEye, FiEdit2, FiTrash2, FiCheck, FiX, FiRotateCcw, FiTool } from 'react-icons/fi';

export const ACTION_TYPES = {
  VIEW: 'view',
  EDIT: 'edit',
  DELETE: 'delete',
  APPROVE: 'approve',
  REJECT: 'reject',
  RETURN: 'return',
  POST_MAINTENANCE: 'postMaintenance',
};

export const getActionConfig = (type) => {
  const configs = {
    [ACTION_TYPES.VIEW]: { icon: FiEye, label: 'View', color: 'primary' },
    [ACTION_TYPES.EDIT]: { icon: FiEdit2, label: 'Edit', color: 'primary' },
    [ACTION_TYPES.DELETE]: { icon: FiTrash2, label: 'Delete', color: 'error' },
    [ACTION_TYPES.APPROVE]: { icon: FiCheck, label: 'Approve', color: 'success' },
    [ACTION_TYPES.REJECT]: { icon: FiX, label: 'Reject', color: 'error' },
    [ACTION_TYPES.RETURN]: { icon: FiRotateCcw, label: 'Return', color: 'warning' },
    [ACTION_TYPES.POST_MAINTENANCE]: { icon: FiTool, label: 'Post Maintenance', color: 'info' },
  };
  return configs[type] || configs[ACTION_TYPES.VIEW];
};

/**
 * Hook untuk standardisasi action column dengan kebab menu
 * 
 * @param {Object} options
 * @param {Array} options.actions - List of action types to show (fallback jika tidak ada row data)
 * @param {Function} options.onAction - Callback when action is clicked (actionType, row)
 * @param {Function} options.getConditionalActions - Optional function to filter actions per row
 * @param {string} options.rowIdField - Field name for row ID (default: 'id')
 * @param {Array} options.defaultActions - Default actions jika row tidak memiliki flag
 * 
 * PERBAIKAN: Prioritaskan row data (CanEdit, CanDelete, CanReturn, CanPost)
 *            dari backend daripada actions default.
 */
export const useGridActions = ({ 
  actions = [], 
  onAction, 
  getConditionalActions = null,
  rowIdField = 'id',
  defaultActions = [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE]
}) => {
  const [anchorEl, setAnchorEl] = useState(null);
  const [selectedRow, setSelectedRow] = useState(null);
  const [menuOpen, setMenuOpen] = useState(false);

  const handleMenuOpen = useCallback((event, row) => {
    event.stopPropagation();
    setAnchorEl(event.currentTarget);
    setSelectedRow(row);
    setMenuOpen(true);
  }, []);

  const handleMenuClose = useCallback(() => {
    setAnchorEl(null);
    setSelectedRow(null);
    setMenuOpen(false);
  }, []);

  const handleActionClick = useCallback((actionType, row) => {
    handleMenuClose();
    if (onAction) {
      onAction(actionType, row);
    }
  }, [onAction, handleMenuClose]);

  const actionColumn = useMemo(() => {
    return {
      field: 'actions',
      headerName: 'Actions',
      width: 80,
      sortable: false,
      renderCell: (params) => {
        const row = params.row;
        let visibleActions = [];
        
        // ============================================================
        // PRIORITAS: Gunakan row data dari backend terlebih dahulu
        // Backend mengembalikan CanEdit, CanDelete, CanReturn, CanPost
        // ============================================================
        if (row.canEdit !== undefined || row.canDelete !== undefined || 
            row.canReturn !== undefined || row.canPost !== undefined) {
          
          // Gunakan flag dari row data
          if (row.canEdit) visibleActions.push(ACTION_TYPES.EDIT);
          if (row.canDelete) visibleActions.push(ACTION_TYPES.DELETE);
          if (row.canReturn) visibleActions.push(ACTION_TYPES.RETURN);
          if (row.canPost) visibleActions.push(ACTION_TYPES.POST_MAINTENANCE);
          
          // Jika tidak ada action yang tersedia, tampilkan View saja sebagai fallback
          if (visibleActions.length === 0) {
            visibleActions.push(ACTION_TYPES.VIEW);
          }
        } 
        // ============================================================
        // FALLBACK: Gunakan getConditionalActions jika disediakan
        // ============================================================
        else if (getConditionalActions) {
          visibleActions = getConditionalActions(row);
        }
        // ============================================================
        // FALLBACK TERAKHIR: Gunakan actions default
        // ============================================================
        else {
          visibleActions = actions.length > 0 ? actions : defaultActions;
        }
        
        if (!visibleActions || visibleActions.length === 0) {
          return null;
        }

        const rowId = row[rowIdField] || row.id || row.assetId || row.employeeId || row.assetTransactionId;
        const isMenuOpen = menuOpen && selectedRow?.[rowIdField] === rowId;
        
        return (
          <div className="table-actions">
            <IconButton
              size="small"
              onClick={(e) => handleMenuOpen(e, row)}
              className="icon-btn"
              aria-label="Actions"
            >
              <FiMoreVertical size={18} />
            </IconButton>
            <Menu
              anchorEl={anchorEl}
              open={isMenuOpen}
              onClose={handleMenuClose}
              PaperProps={{
                sx: {
                  minWidth: 160,
                  borderRadius: '8px',
                  boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
                  backgroundColor: 'var(--card-bg)',
                }
              }}
            >
              {visibleActions.map((actionType) => {
                const config = getActionConfig(actionType);
                const Icon = config.icon;
                return (
                  <MenuItem
                    key={actionType}
                    onClick={() => handleActionClick(actionType, row)}
                    sx={{
                      color: config.color === 'error' ? 'var(--error)' : 
                             config.color === 'success' ? 'var(--success)' : 
                             config.color === 'warning' ? 'var(--warning)' : 
                             'var(--text-primary)',
                    }}
                  >
                    <ListItemIcon sx={{ minWidth: 36, color: 'inherit' }}>
                      <Icon size={18} />
                    </ListItemIcon>
                    <ListItemText>{config.label}</ListItemText>
                  </MenuItem>
                );
              })}
            </Menu>
          </div>
        );
      },
    };
  }, [actions, defaultActions, getConditionalActions, rowIdField, anchorEl, menuOpen, selectedRow, handleMenuOpen, handleMenuClose, handleActionClick]);

  return { actionColumn };
};

export default useGridActions;