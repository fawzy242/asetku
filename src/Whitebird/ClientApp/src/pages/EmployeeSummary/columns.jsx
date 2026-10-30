// src/pages/EmployeeSummary/columns.jsx
import React from 'react';
import { Chip } from '@mui/material';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import { formatDate, formatDateTime } from '../../core/utils/date.utils';

export const assetColumns = [
  { field: 'assetCode', headerName: 'Asset Code', width: 130 },
  { field: 'assetName', headerName: 'Asset Name', flex: 1, minWidth: 180 },
  { field: 'categoryName', headerName: 'Category', width: 150 },
  {
    field: 'status',
    headerName: 'Status',
    width: 140,
    renderCell: (p) => <StatusChip status={p?.value || '-'} />,
  },
  { field: 'associationType', headerName: 'Type', width: 110 },
  {
    field: 'sinceDate',
    headerName: 'Since',
    width: 120,
    renderCell: (params) => {
      const value = params?.row?.sinceDate || params?.value;
      if (!value) return <span className="u-text-muted">-</span>;
      return <span>{formatDate(value)}</span>;
    },
  },
  {
    field: 'expectedReturnDate',
    headerName: 'Expected Return',
    width: 130,
    renderCell: (params) => {
      const value = params?.row?.expectedReturnDate || params?.value;
      if (!value) return <span className="u-text-muted">-</span>;
      return <span>{formatDate(value)}</span>;
    },
  },
  {
    field: 'isOverdue',
    headerName: 'Overdue',
    width: 90,
    renderCell: (p) =>
      p?.value ? (
        <Chip label="Yes" size="small" className="employee-summary__chip-overdue" />
      ) : (
        <Chip label="No" size="small" className="employee-summary__chip-ontime" />
      ),
  },
  { field: 'conditionName', headerName: 'Condition', width: 100 },
];

export const historyColumns = [
  {
    field: 'transactionDate',
    headerName: 'Date',
    width: 180,
    renderCell: (params) => {
      const value = params?.row?.transactionDate || params?.value;
      if (!value) return <span className="u-text-muted">-</span>;
      return <span>{formatDateTime(value)}</span>;
    },
  },
  {
    field: 'transactionTypeName',
    headerName: 'Type',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      const value =
        row.transactionTypeName || row.typeName || row.transactionType || '-';
      return <span>{value}</span>;
    },
  },
  { field: 'assetCode', headerName: 'Asset', width: 130 },
  { field: 'assetName', headerName: 'Asset Name', flex: 1, minWidth: 180 },
  {
    field: 'employeeName',
    headerName: 'Employee',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      return <span>{row.employeeName || row.employee || '-'}</span>;
    },
  },
  {
    field: 'conditionAfterName',
    headerName: 'Condition',
    width: 100,
    renderCell: (params) => {
      const row = params?.row || {};
      return (
        <span>
          {row.conditionAfterName || row.conditionAfter || row.condition || '-'}
        </span>
      );
    },
  },
  {
    field: 'notes',
    headerName: 'Notes',
    width: 200,
    renderCell: (params) => {
      const row = params?.row || {};
      return <span>{row.notes || row.note || '-'}</span>;
    },
  },
];