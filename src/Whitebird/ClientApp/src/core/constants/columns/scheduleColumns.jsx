// src/core/constants/columns/scheduleColumns.jsx
import StatusChip from '../../../components/atoms/StatusChip/StatusChip';
import { formatDateTime } from '../../utils/date.utils';

export const getScheduleColumns = (actionColumn, scheduleType = 'handover') => {
  const baseColumns = [
    {
      field: 'scheduleDate',
      headerName: 'Schedule Date',
      width: 180,
      renderCell: (p) => formatDateTime(p?.value),
    },
    { field: 'assetCode', headerName: 'Asset Code', width: 120 },
    { field: 'assetName', headerName: 'Asset Name', flex: 1, minWidth: 180 },
  ];

  if (scheduleType === 'maintenance') {
    baseColumns.push({ field: 'maintenanceTypeName', headerName: 'Maintenance Type', width: 180 });
  } else {
    baseColumns.push(
      { field: 'employeeCode', headerName: 'Employee Code', width: 120 },
      { field: 'employeeName', headerName: 'Employee Name', width: 180 }
    );
  }

  baseColumns.push(
    {
      field: 'isActive',
      headerName: 'Status',
      width: 110,
      renderCell: (p) => <StatusChip status={p.value ? 'Active' : 'Inactive'} />,
    },
    actionColumn
  );

  return baseColumns;
};