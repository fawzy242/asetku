// src/pages/Assets/columns.jsx
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import { formatCurrency } from '../../core/utils/number.utils';

export const getAssetColumns = (actionColumn) => [
  { field: 'assetCode', headerName: 'Code', width: 120 },
  { field: 'assetName', headerName: 'Name', flex: 1, minWidth: 180 },
  { field: 'categoryName', headerName: 'Category', width: 150 },
  { field: 'brand', headerName: 'Brand', width: 100 },
  { field: 'model', headerName: 'Model', width: 100 },
  {
    field: 'currentStatus',
    headerName: 'Status',
    width: 140,
    renderCell: (p) => {
      const status = p?.value || 'Available';
      return <StatusChip status={status} />;
    },
  },
  {
    field: 'purchasePrice',
    headerName: 'Price',
    width: 130,
    renderCell: (params) => {
      const value = params?.row?.purchasePrice ?? params?.value;
      if (value === null || value === undefined) {
        return <span className="u-text-muted">-</span>;
      }
      const numValue = typeof value === 'string' ? parseFloat(value) : value;
      if (isNaN(numValue)) return <span className="u-text-muted">-</span>;
      return <span>{formatCurrency(numValue)}</span>;
    },
  },
  actionColumn,
];