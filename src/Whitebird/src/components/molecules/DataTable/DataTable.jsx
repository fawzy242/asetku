import React from 'react';
import { DataGrid } from '@mui/x-data-grid';
import './DataTable.scss';

const DataTable = ({
  rows = [],
  columns = [],
  loading = false,
  pageSize = 10,
  pageSizeOptions = [10, 25, 50, 100],
  onRowClick = null,
  checkboxSelection = false,
  onSelectionChange = null,
  rowHeight = 52,
  headerHeight = 56,
  autoHeight = false,
  getRowId = (row) => row.id,
  className = ''
}) => {
  return (
    <div className={`data-table ${className}`}>
      <DataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        initialState={{
          pagination: {
            paginationModel: { pageSize, page: 0 }
          }
        }}
        pageSizeOptions={pageSizeOptions}
        onRowClick={onRowClick}
        checkboxSelection={checkboxSelection}
        onRowSelectionModelChange={onSelectionChange}
        rowHeight={rowHeight}
        columnHeaderHeight={headerHeight}
        autoHeight={autoHeight}
        getRowId={getRowId}
        disableRowSelectionOnClick
        sx={{
          border: 'none',
          '& .MuiDataGrid-cell': {
            borderColor: 'var(--border)',
            color: 'var(--text-primary)'
          },
          '& .MuiDataGrid-columnHeaders': {
            backgroundColor: 'var(--surface)',
            borderColor: 'var(--border)',
            color: 'var(--text-secondary)'
          },
          '& .MuiDataGrid-footerContainer': {
            borderColor: 'var(--border)',
            color: 'var(--text-secondary)'
          },
          '& .MuiDataGrid-row:hover': {
            backgroundColor: 'var(--surface)'
          },
          '& .MuiTablePagination-root': {
            color: 'var(--text-secondary)'
          },
          '& .MuiSvgIcon-root': {
            color: 'var(--text-secondary)'
          },
          '& .MuiDataGrid-overlay': {
            backgroundColor: 'var(--card-bg)',
            color: 'var(--text-secondary)'
          }
        }}
      />
    </div>
  );
};

export default DataTable;