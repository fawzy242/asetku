// src/pages/ScheduleReturn/columns.jsx
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import { formatDateTime } from '../../core/utils/date.utils';

export const getScheduleColumns = (actionColumn) => [
  {
    field: 'scheduleDate',
    headerName: 'Schedule Date',
    width: 180,
    renderCell: (p) => formatDateTime(p?.value),
  },
  { field: 'assetCode', headerName: 'Asset Code', width: 120 },
  { field: 'assetName', headerName: 'Asset Name', flex: 1, minWidth: 180 },
  { field: 'employeeCode', headerName: 'Employee Code', width: 120 },
  { field: 'employeeName', headerName: 'Employee Name', width: 180 },
  {
    field: 'isActive',
    headerName: 'Status',
    width: 110,
    renderCell: (p) => <StatusChip status={p.value ? 'Active' : 'Inactive'} />,
  },
  actionColumn,
];