// src/pages/Departments/columns.jsx
import StatusChip from '../../components/atoms/StatusChip/StatusChip';

export const getDepartmentColumns = (actionColumn) => [
  { field: 'departmentCode', headerName: 'Code', width: 150 },
  { field: 'departmentName', headerName: 'Name', flex: 1, minWidth: 200 },
  { field: 'description', headerName: 'Description', flex: 1, minWidth: 200 },
  {
    field: 'employeeCount',
    headerName: 'Employees',
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