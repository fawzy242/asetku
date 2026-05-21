import React, { memo, useMemo } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { useUIStore } from '../../../stores/uiStore';
import './DataTable.scss';

const DataTable = memo(({
  rows = [],
  columns = [],
  loading = false,
  pageSize = 10,
  pageSizeOptions = [10, 25, 50],
  onRowClick = null,
  checkboxSelection = false,
  onSelectionChange = null,
  rowHeight = 52,
  headerHeight = 56,
  autoHeight = false,
  getRowId = null,
  hideFooter = true,
  className = '',
  ariaLabel = 'Data table',
}) => {
  const theme = useUIStore((s) => s.theme);
  const isDark = theme === 'dark';

  // Memoized rows to prevent unnecessary re-renders
  const safeRows = useMemo(() => {
    if (!rows) return [];
    if (Array.isArray(rows)) return rows;
    return [];
  }, [rows]);

  // Memoized columns to prevent unnecessary re-renders
  const safeColumns = useMemo(() => {
    if (!columns || !Array.isArray(columns)) return [];
    return columns;
  }, [columns]);

  // Memoized getRowId function
  const safeGetRowId = useMemo(() => {
    return (row) => {
      if (typeof getRowId === 'function') {
        const customId = getRowId(row);
        if (customId !== undefined && customId !== null && customId !== '' && !Number.isNaN(customId)) {
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
      const fallback = row?.assetCode || row?.categoryName || row?.supplierName ||
        row?.officeName || row?.fullName || row?.employeeCode || '';
      if (fallback) return fallback;
      return `row-${Math.random().toString(36).substr(2, 9)}`;
    };
  }, [getRowId]);

  const effectiveAutoHeight = autoHeight || hideFooter;

  // Memoized MUI theme to prevent recreation
  const muiTheme = useMemo(() => createTheme({
    palette: {
      mode: isDark ? 'dark' : 'light',
      primary: { main: '#dc2626' },
      background: {
        default: isDark ? '#111827' : '#ffffff',
        paper: isDark ? '#1f2937' : '#ffffff',
      },
      text: {
        primary: isDark ? '#f9fafb' : '#111827',
        secondary: isDark ? '#9ca3af' : '#6b7280',
      },
    },
    typography: {
      fontFamily: "'Inter', sans-serif",
      fontSize: 14,
    },
    shape: { borderRadius: 8 },
    components: {
      MuiDataGrid: {
        styleOverrides: {
          root: {
            border: 'none',
            backgroundColor: 'transparent',
            width: '100%',
            minWidth: '100%',
            '& .MuiDataGrid-main': { width: '100%', minWidth: '100%' },
            '& .MuiDataGrid-columnHeaders': {
              backgroundColor: isDark ? '#374151' : '#f9fafb',
              color: isDark ? '#f9fafb' : '#374151',
              fontWeight: 600,
              fontSize: '0.875rem',
              borderBottom: `1px solid ${isDark ? '#4b5563' : '#e5e7eb'}`,
            },
            '& .MuiDataGrid-cell': {
              borderBottom: `1px solid ${isDark ? '#374151' : '#f3f4f6'}`,
              color: isDark ? '#f9fafb' : '#1f2937',
              fontSize: '0.875rem',
              '&:focus, &:focus-within': { outline: 'none' },
            },
            '& .MuiDataGrid-row:hover': {
              backgroundColor: isDark ? '#374151' : '#f9fafb',
            },
            '& .MuiDataGrid-footerContainer': {
              display: hideFooter ? 'none' : 'flex',
              backgroundColor: isDark ? '#374151' : '#f9fafb',
              color: isDark ? '#f9fafb' : '#374151',
              borderTop: `1px solid ${isDark ? '#4b5563' : '#e5e7eb'}`,
            },
            '& .MuiTablePagination-root': {
              color: isDark ? '#f9fafb' : '#374151',
            },
            '& .MuiTablePagination-selectIcon, & .MuiSvgIcon-root': {
              color: isDark ? '#9ca3af' : '#6b7280',
            },
            '& .MuiDataGrid-overlay': {
              backgroundColor: isDark ? 'rgba(17, 24, 39, 0.7)' : 'rgba(255, 255, 255, 0.7)',
              color: isDark ? '#9ca3af' : '#6b7280',
            },
            '& .MuiDataGrid-columnSeparator': {
              display: 'flex !important',
              visibility: 'visible !important',
              color: isDark ? '#4b5563' : '#e5e7eb',
            },
            '& .MuiDataGrid-menuIcon': {
              display: 'flex !important',
              visibility: 'visible !important',
              width: 'auto !important',
              minWidth: '32px !important',
            },
            '& .MuiDataGrid-menuIconButton': {
              color: isDark ? '#9ca3af' : '#6b7280',
              opacity: 0.5,
              transition: 'opacity 0.2s',
              '&:hover': { opacity: 1 },
            },
            '& .MuiDataGrid-columnHeader:hover .MuiDataGrid-menuIconButton': {
              opacity: 1,
            },
            '& .MuiDataGrid-iconSeparator': {
              color: isDark ? '#4b5563' : '#e5e7eb',
              cursor: 'col-resize',
              '&:hover': { color: '#dc2626' },
            },
          },
        },
      },
      MuiCheckbox: {
        styleOverrides: {
          root: {
            color: isDark ? '#9ca3af' : '#6b7280',
            '&.Mui-checked': { color: '#dc2626' },
          },
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: {
            '&::-webkit-scrollbar': { width: '6px', height: '6px' },
            '&::-webkit-scrollbar-track': { background: isDark ? '#1f2937' : '#f3f4f6' },
            '&::-webkit-scrollbar-thumb': { background: isDark ? '#4b5563' : '#d1d5db', borderRadius: '3px' },
          },
        },
      },
    },
  }), [isDark, hideFooter]);

  return (
    <div
      className={`data-table ${className}`}
      role="region"
      aria-label={ariaLabel}
      aria-busy={loading}
      style={{ width: '100%', minWidth: '100%', maxWidth: '100%', display: 'flex', flexDirection: 'column' }}
    >
      <ThemeProvider theme={muiTheme}>
        <DataGrid
          rows={safeRows}
          columns={safeColumns}
          loading={loading}
          initialState={{ pagination: { paginationModel: { pageSize, page: 0 } } }}
          pageSizeOptions={pageSizeOptions}
          onRowClick={onRowClick}
          checkboxSelection={checkboxSelection}
          onRowSelectionModelChange={onSelectionChange}
          rowHeight={rowHeight}
          columnHeaderHeight={headerHeight}
          autoHeight={effectiveAutoHeight}
          getRowId={safeGetRowId}
          disableRowSelectionOnClick
          hideFooter={hideFooter}
          disableColumnMenu={false}
          disableColumnFilter={false}
          disableColumnSelector={false}
          disableColumnResize={false}
          sx={{
            width: '100%',
            minWidth: '100%',
            maxWidth: '100%',
            flex: 1,
            minHeight: effectiveAutoHeight ? undefined : 400,
          }}
        />
      </ThemeProvider>
    </div>
  );
});

DataTable.displayName = 'DataTable';
export default DataTable;