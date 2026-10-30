// src/pages/Roles/columns.jsx
import StatusChip from '../../components/atoms/StatusChip/StatusChip';

export const getRoleColumns = (actionColumn) => [
  { field: 'roleCode', headerName: 'Code', width: 140 },
  { field: 'roleName', headerName: 'Name', flex: 1, minWidth: 200 },
  { field: 'description', headerName: 'Description', flex: 1, minWidth: 240 },
  {
    field: 'isActive',
    headerName: 'Status',
    width: 110,
    sortable: false,
    renderCell: (p) => {
      const status = p.value ? 'Active' : 'Inactive';
      return <StatusChip status={status} />;
    },
  },
  actionColumn,
];