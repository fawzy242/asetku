import React, { useState, useCallback, useMemo } from 'react';
import { IconButton } from '@mui/material';
import {
  FiMoreVertical,
  FiEye,
  FiEdit2,
  FiTrash2,
  FiCheck,
  FiX,
  FiRotateCcw,
  FiTool,
} from 'react-icons/fi';

export const ACTION_TYPES = {
  VIEW: 'view',
  EDIT: 'edit',
  DELETE: 'delete',
  APPROVE: 'approve',
  REJECT: 'reject',
  RETURN: 'return',
  POST_MAINTENANCE: 'postMaintenance',
  COMPLETE: 'complete',
  UNCOMPLETE: 'uncomplete',
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
    [ACTION_TYPES.COMPLETE]: { icon: FiCheck, label: 'Complete', color: 'success' },
    [ACTION_TYPES.UNCOMPLETE]: { icon: FiRotateCcw, label: 'Uncomplete', color: 'warning' },
  };
  return configs[type] || configs[ACTION_TYPES.VIEW];
};

/**
 * Standardizes the row action column with a single Menu per grid.
 * Renders only the kebab button in the column; the Menu element is
 * the caller's responsibility (see `menuAnchorEl` / `menuActions`).
 */
export const useGridActions = ({
  actions = [],
  onAction,
  getConditionalActions = null,
  rowIdField = 'id',
  defaultActions = [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
}) => {
  const [anchorEl, setAnchorEl] = useState(null);
  const [selectedRow, setSelectedRow] = useState(null);

  const handleMenuOpen = useCallback((event, row) => {
    event.stopPropagation();
    setAnchorEl(event.currentTarget);
    setSelectedRow(row);
  }, []);

  const handleMenuClose = useCallback(() => {
    setAnchorEl(null);
    setSelectedRow(null);
  }, []);

  const handleActionClick = useCallback(
    (actionType, row) => {
      const targetRow = row || selectedRow;
      handleMenuClose();
      if (onAction) {
        onAction(actionType, targetRow);
      }
    },
    [onAction, selectedRow, handleMenuClose]
  );

  const actionColumn = useMemo(
    () => ({
      field: 'actions',
      headerName: 'Actions',
      width: 80,
      sortable: false,
      filterable: false,
      disableColumnMenu: true,
      disableReorder: true,
      renderCell: (params) => (
        <div className="table-actions">
          <IconButton
            size="small"
            onClick={(e) => handleMenuOpen(e, params.row)}
            className="icon-btn"
            aria-label="Row actions"
          >
            <FiMoreVertical size={18} />
          </IconButton>
        </div>
      ),
    }),
    [handleMenuOpen]
  );

  // Resolve the action list for the currently selected row.
  const menuActions = useMemo(() => {
    if (!selectedRow) return [];

    let visibleActions = [];

    if (
      selectedRow.canEdit !== undefined ||
      selectedRow.canDelete !== undefined ||
      selectedRow.canReturn !== undefined ||
      selectedRow.canPost !== undefined
    ) {
      if (selectedRow.canEdit) visibleActions.push(ACTION_TYPES.EDIT);
      if (selectedRow.canDelete) visibleActions.push(ACTION_TYPES.DELETE);
      if (selectedRow.canReturn) visibleActions.push(ACTION_TYPES.RETURN);
      if (selectedRow.canPost) visibleActions.push(ACTION_TYPES.POST_MAINTENANCE);

      if (visibleActions.length === 0) {
        visibleActions.push(ACTION_TYPES.VIEW);
      }
    } else if (getConditionalActions) {
      visibleActions = getConditionalActions(selectedRow);
      if (visibleActions.length === 0) {
        visibleActions.push(ACTION_TYPES.VIEW);
      }
    } else {
      visibleActions =
        actions.length > 0 ? actions : defaultActions;
      if (!visibleActions.includes(ACTION_TYPES.VIEW)) {
        visibleActions = [ACTION_TYPES.VIEW, ...visibleActions];
      }
    }

    visibleActions = [...new Set(visibleActions)];

    return visibleActions.map((type) => {
      const config = getActionConfig(type);
      return {
        type,
        label: config.label,
        icon: config.icon,
        color: config.color,
      };
    });
  }, [selectedRow, actions, defaultActions, getConditionalActions]);

  return {
    actionColumn,
    menuAnchorEl: anchorEl,
    menuOpen: Boolean(anchorEl),
    menuActions,
    selectedRow,
    handleMenuClose,
    handleActionClick,
    rowIdField,
  };
};

export default useGridActions;