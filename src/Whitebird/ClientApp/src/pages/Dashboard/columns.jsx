// src/pages/Dashboard/columns.jsx
import React from 'react';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import { formatDate, formatDateTime } from '../../core/utils/date.utils';
import { formatCurrency } from '../../core/utils/number.utils';

export const transactionColumns = [
  {
    field: 'assetCode',
    headerName: 'Code',
    width: 120,
    renderCell: (params) => {
      const row = params?.row || {};
      return <span>{row.assetCode || params?.value || '-'}</span>;
    },
  },
  {
    field: 'assetName',
    headerName: 'Asset Name',
    flex: 1,
    minWidth: 180,
    renderCell: (params) => {
      const row = params?.row || {};
      return <span>{row.assetName || params?.value || '-'}</span>;
    },
  },
  {
    field: 'transactionTypeName',
    headerName: 'Type',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      return <span>{row.transactionTypeName || params?.value || '-'}</span>;
    },
  },
  {
    field: 'fromEmployeeName',
    headerName: 'From',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      return <span>{row.fromEmployeeName || params?.value || '-'}</span>;
    },
  },
  {
    field: 'toEmployeeName',
    headerName: 'To',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      return <span>{row.toEmployeeName || params?.value || '-'}</span>;
    },
  },
  {
    field: 'approved',
    headerName: 'Status',
    width: 120,
    renderCell: (params) => {
      const row = params?.row || {};
      let status = 'Pending';
      if (row.approved === true || params?.value === true) status = 'Approved';
      if (row.approved === false || params?.value === false) status = 'Rejected';
      return <StatusChip status={status} />;
    },
  },
  {
    field: 'transactionDate',
    headerName: 'Date',
    width: 180,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.transactionDate || params?.value;
      if (!value) return <span>-</span>;
      return <span>{formatDateTime(value)}</span>;
    },
  },
];

export const expiredWarrantyColumns = [
  { field: 'assetCode', headerName: 'Code', width: 120 },
  { field: 'assetName', headerName: 'Name', flex: 1, minWidth: 180 },
  { field: 'categoryName', headerName: 'Category', width: 150 },
  { field: 'supplierName', headerName: 'Supplier', width: 150 },
  {
    field: 'warrantyExpiryDate',
    headerName: 'Expiry Date',
    width: 150,
    renderCell: (params) => {
      const value = params?.row?.warrantyExpiryDate || params?.value;
      if (!value) return <span>-</span>;
      return <span>{formatDate(value)}</span>;
    },
  },
  {
    field: 'purchasePrice',
    headerName: 'Price',
    width: 130,
    renderCell: (params) => {
      const value = params?.row?.purchasePrice || params?.value;
      if (!value) return <span>-</span>;
      return <span>{formatCurrency(value)}</span>;
    },
  },
];

export const categoryBreakdownColumns = [
  { field: 'category', headerName: 'Category', flex: 1, minWidth: 200 },
  { field: 'assetCount', headerName: 'Asset Count', width: 130 },
  {
    field: 'totalValue',
    headerName: 'Total Value',
    width: 180,
    renderCell: (params) => {
      const value = params?.row?.totalValue || params?.value;
      if (!value) return <span className="u-text-muted">-</span>;
      return <span>{formatCurrency(value)}</span>;
    },
  },
];

export const monthlyStatsColumns = [
  {
    field: 'month',
    headerName: 'Month',
    width: 120,
    renderCell: (params) => {
      const monthMap = [
        'Jan',
        'Feb',
        'Mar',
        'Apr',
        'May',
        'Jun',
        'Jul',
        'Aug',
        'Sep',
        'Oct',
        'Nov',
        'Dec',
      ];
      const val = params?.row?.month || params?.value;
      return <span>{monthMap[val - 1] || val}</span>;
    },
  },
  { field: 'transactionCount', headerName: 'Transactions', width: 130 },
  { field: 'uniqueAssetsCount', headerName: 'Unique Assets', width: 130 },
];