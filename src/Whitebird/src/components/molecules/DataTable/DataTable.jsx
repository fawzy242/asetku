import React, { memo, useMemo } from "react";
import { DataGrid } from "@mui/x-data-grid";
import { createTheme, ThemeProvider } from "@mui/material/styles";
import { useTheme } from "../../../context/ThemeContext";
import "./DataTable.scss";

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
  getRowId = (row) => row.id,
  hideFooter = true,
  className = "",
}) => {
  const { theme: appTheme } = useTheme();
  const isDark = appTheme === "dark";

  // Calculate dynamic height based on pageSize
  const gridHeight = useMemo(() => {
    if (autoHeight) return undefined;
    const headerFooterHeight = 120;
    const calculatedHeight = Math.min(rows.length, pageSize) * rowHeight + headerFooterHeight;
    return Math.max(400, Math.min(800, calculatedHeight));
  }, [rows.length, pageSize, rowHeight, autoHeight]);

  const muiTheme = React.useMemo(() => createTheme({
    palette: {
      mode: isDark ? "dark" : "light",
      primary: { main: "#dc2626" },
      background: { 
        default: isDark ? "#111827" : "#ffffff", 
        paper: isDark ? "#1f2937" : "#ffffff" 
      },
      text: { 
        primary: isDark ? "#f9fafb" : "#111827", 
        secondary: isDark ? "#9ca3af" : "#6b7280" 
      },
    },
    typography: { 
      fontFamily: "'Inter', sans-serif", 
      fontSize: 14,
    },
    shape: { borderRadius: 8 },
    components: {
      MuiDataGrid: {
        defaultProps: {
          disableColumnResize: false,
        },
        styleOverrides: {
          root: {
            border: "none",
            backgroundColor: "transparent",
            height: gridHeight,
            minHeight: 400,
            '& .MuiDataGrid-columnHeader--resizable': {
              cursor: 'col-resize',
            },
            "& .MuiDataGrid-columnHeaders": {
              backgroundColor: isDark ? "#374151" : "#f9fafb",
              color: isDark ? "#f9fafb" : "#374151",
              fontWeight: 600,
              fontSize: "0.875rem",
              borderBottom: `1px solid ${isDark ? "#4b5563" : "#e5e7eb"}`,
            },
            "& .MuiDataGrid-columnHeader": {
              padding: "0 16px",
              "&:focus, &:focus-within": { outline: "none" },
            },
            "& .MuiDataGrid-columnHeaderTitle": {
              fontWeight: 600,
              fontSize: "0.875rem",
            },
            "& .MuiDataGrid-cell": {
              borderBottom: `1px solid ${isDark ? "#374151" : "#f3f4f6"}`,
              color: isDark ? "#f9fafb" : "#1f2937",
              fontSize: "0.875rem",
              padding: "0 16px",
              "&:focus, &:focus-within": { outline: "none" },
            },
            "& .MuiDataGrid-row:hover": {
              backgroundColor: isDark ? "#374151" : "#f9fafb",
            },
            "& .MuiDataGrid-footerContainer": {
              display: hideFooter ? "none" : "flex",
              borderTop: `1px solid ${isDark ? "#4b5563" : "#e5e7eb"}`,
            },
            "& .MuiDataGrid-overlay": {
              backgroundColor: "transparent",
              color: isDark ? "#9ca3af" : "#6b7280",
            },
            "& .MuiDataGrid-columnSeparator": {
              display: "none",
            },
            "& .MuiDataGrid-menuIcon": {
              display: "none",
            },
            "& .MuiCheckbox-root": {
              color: isDark ? "#9ca3af" : "#6b7280",
              "&.Mui-checked": { color: "#dc2626" },
            },
          },
          columnHeader: {
            '& .MuiDataGrid-iconButtonContainer': {
              visibility: 'visible',
              width: 'auto',
            },
            '& .MuiDataGrid-columnHeaderTitleContainer': {
              overflow: 'visible',
            },
          },
        },
      },
    },
  }), [isDark, gridHeight, hideFooter, rows.length, pageSize]);

  return (
    <div className={`data-table ${className}`} style={{ height: gridHeight, minHeight: 400 }}>
      <ThemeProvider theme={muiTheme}>
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
          hideFooter={hideFooter}
          resizable={true}
          sx={{
            "& .MuiDataGrid-virtualScroller": {
              overflow: "auto",
              minHeight: rows.length === 0 ? "200px" : "auto",
            },
          }}
        />
      </ThemeProvider>
    </div>
  );
});

DataTable.displayName = "DataTable";
export default DataTable;