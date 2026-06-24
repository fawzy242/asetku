import React, { memo, useMemo, useState, useCallback, useEffect } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import './DataTable.scss';

const DataTable = memo(({
  rows = [],
  columns = [],
  loading = false,
  pageSize = 10,
  pageSizeOptions = [10, 25, 50, 100],
  onRowClick = null,
  checkboxSelection = false,
  onRowSelectionModelChange = null,
  rowHeight = 52,
  headerHeight = 56,
  autoHeight = false,
  getRowId = null,
  hideFooter = false,
  className = '',
  ariaLabel = 'Data table',
  disableColumnMenu = false,
  disableColumnFilter = false,
  disableColumnSelector = false,
  disableColumnResize = false,
  totalRowCount = 0,
  paginationMode = 'server',
  onPaginationModelChange = null,
  page = 1,
  onPageChange = null,
  onPageSizeChange = null,
}) => {
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: pageSize });

  // Sync with external pageSize changes
  useEffect(() => {
    setPaginationModel(prev => ({ ...prev, pageSize: pageSize }));
  }, [pageSize]);

  const safeRows = useMemo(() => {
    if (!rows) return [];
    if (Array.isArray(rows)) return rows;
    return [];
  }, [rows]);

  const safeGetRowId = useCallback((row) => {
    if (typeof getRowId === 'function') {
      const customId = getRowId(row);
      if (customId !== undefined && customId !== null) {
        return customId;
      }
    }
    if (row?.id != null) return row.id;
    if (row?.assetId != null) return row.assetId;
    if (row?.categoryId != null) return row.categoryId;
    if (row?.supplierId != null) return row.supplierId;
    if (row?.officeId != null) return row.officeId;
    if (row?.employeeId != null) return row.employeeId;
    if (row?.assetTransactionId != null) return row.assetTransactionId;
    return `row-${Math.random().toString(36).substr(2, 9)}`;
  }, [getRowId]);

  const handlePaginationModelChange = (newModel) => {
    setPaginationModel(newModel);
    
    if (onPaginationModelChange) {
      onPaginationModelChange(newModel);
    }
    
    if (onPageChange && newModel.page + 1 !== page) {
      onPageChange(newModel.page + 1);
    }
    if (onPageSizeChange && newModel.pageSize !== pageSize) {
      onPageSizeChange(newModel.pageSize);
    }
  };

  // For server-side pagination, use totalRowCount from API
  const rowCount = paginationMode === 'server' ? totalRowCount : safeRows.length;

  // Hapus totalRowCount untuk client pagination
  const effectiveTotalRowCount = paginationMode === 'server' ? totalRowCount : undefined;

  const dataGridSx = {
    border: 'none',
    backgroundColor: 'transparent',
    // Berikan height default jika autoHeight false
    height: autoHeight ? 'auto' : 400,
    minHeight: 300,
    '& .MuiDataGrid-main': { width: '100%' },
    '& .MuiDataGrid-virtualScroller': {
      overflowY: 'auto !important',
      overflowX: 'auto !important',
      minHeight: 200,
      '&::-webkit-scrollbar': {
        width: '8px',
        height: '8px',
      },
      '&::-webkit-scrollbar-track': {
        background: 'var(--scrollbar-track)',
        borderRadius: '4px',
      },
      '&::-webkit-scrollbar-thumb': {
        background: 'var(--scrollbar-thumb)',
        borderRadius: '4px',
      },
      '&::-webkit-scrollbar-thumb:hover': {
        background: 'var(--scrollbar-thumb-hover)',
      },
      '&::-webkit-scrollbar-corner': {
        background: 'transparent',
      },
    },
    '& .MuiDataGrid-columnHeaders': {
      backgroundColor: 'var(--table-header-bg)',
      color: 'var(--text-primary)',
      fontWeight: 600,
      fontSize: '0.875rem',
      borderBottom: '1px solid var(--border)',
      position: 'sticky',
      top: 0,
      zIndex: 2,
    },
    '& .MuiDataGrid-cell': {
      borderBottom: '1px solid var(--border)',
      color: 'var(--text-primary)',
      fontSize: '0.875rem',
    },
    '& .MuiDataGrid-row:hover': {
      backgroundColor: 'var(--table-row-hover)',
    },
    '& .MuiDataGrid-footerContainer': {
      display: hideFooter ? 'none' : 'flex',
      borderTop: '1px solid var(--border)',
      minHeight: '52px',
      '& .MuiTablePagination-root': {
        color: 'var(--text-primary)',
      },
      '& .MuiTablePagination-selectIcon': {
        color: 'var(--text-primary)',
      },
      '& .MuiIconButton-root': {
        color: 'var(--text-primary)',
      },
      '& .MuiTablePagination-displayedRows': {
        color: 'var(--text-primary)',
      },
    },
    '& .MuiDataGrid-overlay': {
      backgroundColor: 'var(--background)',
    },
    '& .MuiDataGrid-columnSeparator': {
      display: 'flex !important',
      visibility: 'visible !important',
    },
    '& .MuiDataGrid-iconSeparator': {
      cursor: 'col-resize !important',
      '&:hover': { color: 'var(--primary)' },
    },
    '& .MuiDataGrid-menuIcon': {
      opacity: 0.5,
      transition: 'opacity 0.2s',
      '&:hover': { opacity: 1 },
    },
    '& .MuiDataGrid-menuIconButton': {
      color: 'var(--text-secondary)',
    },
    '& .MuiCheckbox-root': {
      color: 'var(--text-secondary)',
      '&.Mui-checked': { color: 'var(--primary)' },
    },
    '& .MuiDataGrid-virtualScrollerContent--overflowed': {
      minWidth: '100%',
    },
  };

  return (
    <div className={`data-table ${className}`} style={{ width: '100%', minHeight: autoHeight ? 'auto' : '300px' }}>
      <DataGrid
        rows={safeRows}
        columns={columns}
        loading={loading}
        paginationModel={paginationModel}
        onPaginationModelChange={handlePaginationModelChange}
        pageSizeOptions={pageSizeOptions}
        onRowClick={onRowClick}
        checkboxSelection={checkboxSelection}
        onRowSelectionModelChange={onRowSelectionModelChange}
        rowHeight={rowHeight}
        columnHeaderHeight={headerHeight}
        autoHeight={autoHeight}
        getRowId={safeGetRowId}
        disableRowSelectionOnClick
        hideFooter={hideFooter}
        disableColumnMenu={disableColumnMenu}
        disableColumnFilter={disableColumnFilter}
        disableColumnSelector={disableColumnSelector}
        disableColumnResize={disableColumnResize}
        sx={dataGridSx}
        rowCount={effectiveTotalRowCount}
        paginationMode={paginationMode}
        slotProps={{
          loadingOverlay: {
            variant: 'skeleton',
            noRowsVariant: 'skeleton',
          },
        }}
      />
    </div>
  );
});

DataTable.displayName = 'DataTable';
export default DataTable;