// src/pages/Offices/columns.jsx
import StatusChip from '../../components/atoms/StatusChip/StatusChip';

export const getOfficeColumns = (actionColumn) => [
  { field: 'officeCode', headerName: 'Code', width: 120 },
  { field: 'officeName', headerName: 'Name', flex: 1, minWidth: 180 },
  { field: 'officeTypeName', headerName: 'Type', width: 130 },
  { field: 'city', headerName: 'City', width: 140 },
  { field: 'parentOfficeName', headerName: 'Parent', width: 170 },
  {
    field: 'childCount',
    headerName: 'Sub-offices',
    width: 130,
    sortable: false,
  },
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