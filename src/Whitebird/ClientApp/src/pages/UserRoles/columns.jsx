// src/pages/UserRoles/columns.jsx
import React from 'react';
import { formatDateTime } from '../../core/utils/date.utils';

/**
 * Renders the UserRoles grid.
 * The backend may or may not include display names on the grid response —
 * columns render defensively so the page works with either contract.
 */
export const getAssignmentColumns = (actionColumn) => [
  {
    field: 'userId',
    headerName: 'User ID',
    width: 130,
    renderCell: (p) => {
      const row = p?.row || {};
      const value = row.userId ?? p?.value;
      if (value === undefined || value === null) {
        return <span className="u-text-muted">-</span>;
      }
      return <span>{row.userName ? `${row.userName} (#${value})` : `#${value}`}</span>;
    },
  },
  {
    field: 'roleId',
    headerName: 'Role',
    flex: 1,
    minWidth: 220,
    renderCell: (p) => {
      const row = p?.row || {};
      const roleId = row.roleId ?? p?.value;
      if (roleId === undefined || roleId === null) {
        return <span className="u-text-muted">-</span>;
      }
      if (row.roleName) {
        return <span>{row.roleName}</span>;
      }
      if (row.roleCode) {
        return <span>{row.roleCode}</span>;
      }
      return <span>Role #{roleId}</span>;
    },
  },
  {
    field: 'assignedAt',
    headerName: 'Assigned At',
    width: 180,
    renderCell: (p) => {
      const value = p?.row?.assignedAt ?? p?.row?.createdDate ?? p?.value;
      if (!value) return <span className="u-text-muted">-</span>;
      return <span>{formatDateTime(value)}</span>;
    },
  },
  actionColumn,
];