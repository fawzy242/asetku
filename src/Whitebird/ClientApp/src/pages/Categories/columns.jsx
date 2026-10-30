// src/pages/Categories/columns.jsx
import StatusChip from '../../components/atoms/StatusChip/StatusChip';

export const getCategoryColumns = (actionColumn) => [
  { field: 'categoryName', headerName: 'Name', flex: 1, minWidth: 200 },
  { field: 'description', headerName: 'Description', flex: 1, minWidth: 200 },
  { field: 'parentCategoryName', headerName: 'Parent', width: 180 },
  {
    field: 'childCount',
    headerName: 'Subcategories',
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