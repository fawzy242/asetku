// src/pages/Suppliers/columns.jsx
import StatusChip from '../../components/atoms/StatusChip/StatusChip';

export const getSupplierColumns = (actionColumn) => [
  { field: 'supplierName', headerName: 'Name', flex: 1, minWidth: 180 },
  { field: 'contactPerson', headerName: 'Contact', width: 160 },
  { field: 'phoneNumber', headerName: 'Phone', width: 140 },
  { field: 'email', headerName: 'Email', width: 200 },
  { field: 'assetCount', headerName: 'Assets', width: 90, sortable: false },
  {
    field: 'isActive',
    headerName: 'Status',
    width: 110,
    sortable: false,
    renderCell: (p) => {
      const status = p?.value ? 'Active' : 'Inactive';
      return <StatusChip status={status} />;
    },
  },
  actionColumn,
];