// src/pages/Employees/columns.jsx
import EmploymentStatusChip from '../../components/atoms/EmploymentStatusChip/EmploymentStatusChip';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';

export const getEmployeeColumns = (actionColumn) => [
  { field: 'employeeCode', headerName: 'Code', width: 120 },
  { field: 'fullName', headerName: 'Full Name', flex: 1, minWidth: 180 },
  {
    field: 'employmentStatusName',
    headerName: 'Employment Status',
    width: 160,
    renderCell: (p) => {
      const status = p?.value || 'Unknown';
      return <EmploymentStatusChip status={status} />;
    },
  },
  { field: 'departmentName', headerName: 'Department', width: 150 },
  { field: 'positionName', headerName: 'Position', width: 150 },
  { field: 'email', headerName: 'Email', width: 200 },
  { field: 'officeName', headerName: 'Office', width: 150 },
  {
    field: 'isActive',
    headerName: 'Status',
    width: 120,
    renderCell: (p) => {
      const status = p?.value ? 'Active' : 'Inactive';
      return <StatusChip status={status} />;
    },
  },
  actionColumn,
];