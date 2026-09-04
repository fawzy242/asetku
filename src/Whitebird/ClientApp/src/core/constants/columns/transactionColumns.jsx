// src/core/constants/columns/transactionColumns.jsx
import { Chip } from '@mui/material';
import StatusChip from '../../../components/atoms/StatusChip/StatusChip';
import { getTransactionTypeColor } from '../../constants/transactionTypes';
import { formatDate, formatDateTime } from '../../utils/date.utils';

export const baseTransactionColumns = [
  {
    field: 'transactionDate',
    headerName: 'Date',
    width: 180,
    renderCell: (p) => {
      const val = p?.row?.transactionDate ?? p?.value;
      return val ? <span>{formatDateTime(val)}</span> : <span className="u-text-muted">-</span>;
    },
  },
  {
    field: 'assetCode',
    headerName: 'Asset Code',
    width: 120,
    renderCell: (p) => <span>{p?.row?.assetCode ?? p?.value ?? '-'}</span>,
  },
  {
    field: 'assetName',
    headerName: 'Asset Name',
    flex: 1,
    minWidth: 160,
    renderCell: (p) => <span>{p?.row?.assetName ?? p?.value ?? '-'}</span>,
  },
  {
    field: 'transactionTypeName',
    headerName: 'Type',
    width: 150,
    renderCell: (p) => {
      const name = p?.row?.transactionTypeName ?? p?.value ?? '';
      const colors = getTransactionTypeColor(name);
      return <Chip label={colors.label} size="small" sx={{ bgcolor: colors.bg, color: colors.color, fontWeight: 500 }} />;
    },
  },
  {
    field: 'employeeName',
    headerName: 'Employee',
    width: 150,
    renderCell: (p) => <span>{p?.row?.employeeName ?? p?.value ?? '-'}</span>,
  },
];

export const getMonitoringColumns = (actionColumn, activeSubTab) => {
  const statusColumn = {
    field: 'approved',
    headerName: 'Status',
    width: 120,
    renderCell: (p) => {
      const row = p?.row || {};
      let status = 'Pending';
      if (row.approved === true) status = 'Approved';
      if (row.approved === false) status = 'Rejected';
      if (row.completed === true) status = 'Completed';
      return <StatusChip status={status} />;
    },
  };

  const conditionColumn = {
    field: 'conditionName',
    headerName: 'Condition',
    width: 120,
    renderCell: (p) => <span>{p?.row?.conditionName ?? p?.value ?? '-'}</span>,
  };

  if (activeSubTab === 'uncomplete' || activeSubTab === 'complete') {
    const extraColumns = [];
    if (activeSubTab === 'uncomplete') {
      extraColumns.push(
        {
          field: 'hasNeedToChange',
          headerName: 'Need Revision',
          width: 130,
          renderCell: (p) => {
            const hasNeedToChange = p?.row?.hasNeedToChange || false;
            return hasNeedToChange ? <Chip label="⚠️ Need Revision" size="small" color="warning" /> : <Chip label="✓ OK" size="small" color="success" />;
          },
        },
        {
          field: 'allAttachmentsApproved',
          headerName: 'Docs Approved',
          width: 130,
          renderCell: (p) => {
            const allApproved = p?.row?.allAttachmentsApproved || false;
            return allApproved ? <Chip label="✓ All Approved" size="small" color="success" /> : <Chip label="⏳ Pending" size="small" color="warning" />;
          },
        }
      );
    }
    return [...baseTransactionColumns, conditionColumn, statusColumn, ...extraColumns, actionColumn];
  }

  return [
    ...baseTransactionColumns,
    conditionColumn,
    statusColumn,
    {
      field: 'expectedLoanReturnDate',
      headerName: 'Exp. Return',
      width: 130,
      renderCell: (p) => {
        const val = p?.row?.expectedLoanReturnDate ?? p?.value;
        return val ? <span>{formatDate(val)}</span> : <span className="u-text-muted">-</span>;
      },
    },
  ];
};

export const getManagementColumns = (baseColumns, actionColumn) => [
  ...baseColumns,
  {
    field: 'officeName',
    headerName: 'Office',
    width: 150,
    renderCell: (p) => <span>{p?.row?.officeName ?? p?.value ?? '-'}</span>,
  },
  {
    field: 'approved',
    headerName: 'Status',
    width: 120,
    renderCell: (p) => {
      const row = p?.row || {};
      let status = 'Pending';
      if (row.approved === true) status = 'Approved';
      if (row.approved === false) status = 'Rejected';
      return <StatusChip status={status} />;
    },
  },
  {
    field: 'expectedLoanReturnDate',
    headerName: 'Exp. Return',
    width: 130,
    renderCell: (p) => {
      const val = p?.row?.expectedLoanReturnDate ?? p?.value;
      return val ? <span>{formatDate(val)}</span> : <span className="u-text-muted">-</span>;
    },
  },
  actionColumn,
];

export const getManagementReturnColumns = (baseColumns, actionColumn) => [
  ...baseColumns,
  {
    field: 'officeName',
    headerName: 'Office',
    width: 150,
    renderCell: (p) => <span>{p?.row?.officeName ?? p?.value ?? '-'}</span>,
  },
  {
    field: 'approved',
    headerName: 'Status',
    width: 120,
    renderCell: (p) => {
      const row = p?.row || {};
      let status = 'Pending';
      if (row.approved === true) status = 'Approved';
      if (row.approved === false) status = 'Rejected';
      return <StatusChip status={status} />;
    },
  },
  actionColumn,
];