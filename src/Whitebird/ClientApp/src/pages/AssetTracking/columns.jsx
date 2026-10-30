// src/pages/AssetTracking/columns.jsx
import React from 'react';
import { Chip } from '@mui/material';
import { formatDateTime } from '../../core/utils/date.utils';

export const employeeHistoryColumns = [
  { field: 'employeeCode', headerName: 'Code', width: 120 },
  { field: 'fullName', headerName: 'Full Name', flex: 1, minWidth: 180 },
  { field: 'departmentName', headerName: 'Department', width: 150 },
  { field: 'positionName', headerName: 'Position', width: 150 },
  {
    field: 'lastTransactionDate',
    headerName: 'Last Transaction',
    width: 180,
    renderCell: (params) => {
      const val = params?.row?.lastTransactionDate ?? params?.value;
      return val ? <span>{formatDateTime(val)}</span> : <span className="u-text-muted">-</span>;
    },
  },
  { field: 'totalTransactions', headerName: 'Total Txns', width: 120 },
  { field: 'assetsHeld', headerName: 'Assets Held', width: 120 },
];

export const scheduleStatsColumns = [
  {
    field: 'scheduleDate',
    headerName: 'Schedule Date',
    width: 180,
    renderCell: (params) => {
      const val = params?.row?.scheduleDate ?? params?.value;
      return val ? <span>{formatDateTime(val)}</span> : <span className="u-text-muted">-</span>;
    },
  },
  { field: 'assetCode', headerName: 'Asset Code', width: 120 },
  { field: 'assetName', headerName: 'Asset Name', flex: 1, minWidth: 180 },
  { field: 'employeeName', headerName: 'Employee', width: 150 },
  { field: 'type', headerName: 'Type', width: 120 },
  {
    field: 'status',
    headerName: 'Status',
    width: 120,
    renderCell: (params) => {
      const val = params?.row?.status ?? 'Pending';
      const isCompleted = val === 'Completed';
      return isCompleted ? (
        <Chip label="Completed" size="small" color="success" />
      ) : (
        <Chip label="Pending" size="small" color="warning" />
      );
    },
  },
];