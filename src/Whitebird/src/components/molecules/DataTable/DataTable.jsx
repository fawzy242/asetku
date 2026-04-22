import React, { useEffect, useState } from "react";
import { DataGrid } from "@mui/x-data-grid";
import { createTheme, ThemeProvider } from "@mui/material/styles";
import "./DataTable.scss";

const DataTable = ({
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
  autoHeight = true,
  getRowId = (row) => row.id,
  hideFooter = false,
  className = "",
}) => {
  const [isDark, setIsDark] = useState(() => {
    return document.documentElement.getAttribute("data-theme") === "dark";
  });

  useEffect(() => {
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.attributeName === "data-theme") {
          const newTheme = document.documentElement.getAttribute("data-theme");
          setIsDark(newTheme === "dark");
        }
      });
    });

    observer.observe(document.documentElement, { attributes: true });
    return () => observer.disconnect();
  }, []);

  const theme = createTheme({
    palette: {
      mode: isDark ? "dark" : "light",
      primary: { main: "#dc2626" },
      background: {
        default: isDark ? "#111827" : "#ffffff",
        paper: isDark ? "#1f2937" : "#ffffff",
      },
      text: {
        primary: isDark ? "#f9fafb" : "#111827",
        secondary: isDark ? "#9ca3af" : "#6b7280",
      },
    },
    typography: {
      fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
      fontSize: 14,
    },
    shape: { borderRadius: 8 },
    components: {
      MuiDataGrid: {
        styleOverrides: {
          root: {
            border: "none",
            backgroundColor: "transparent",
            width: "100%",
            "& .MuiDataGrid-main": { borderRadius: 0 },
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
            "& .MuiDataGrid-row": {
              "&:hover": { backgroundColor: isDark ? "#374151" : "#f9fafb" },
              "&.Mui-selected": {
                backgroundColor: isDark ? "rgba(220, 38, 38, 0.15)" : "rgba(220, 38, 38, 0.08)",
                "&:hover": {
                  backgroundColor: isDark ? "rgba(220, 38, 38, 0.25)" : "rgba(220, 38, 38, 0.12)",
                },
              },
            },
            "& .MuiDataGrid-footerContainer": {
              borderTop: `1px solid ${isDark ? "#4b5563" : "#e5e7eb"}`,
              backgroundColor: "transparent",
              minHeight: 52,
            },
            "& .MuiTablePagination-root": {
              color: isDark ? "#9ca3af" : "#6b7280",
              fontSize: "0.875rem",
            },
            "& .MuiTablePagination-selectLabel, & .MuiTablePagination-displayedRows": {
              fontSize: "0.875rem",
              margin: 0,
            },
            "& .MuiTablePagination-select": {
              paddingTop: 4,
              paddingBottom: 4,
            },
            "& .MuiSvgIcon-root": { color: isDark ? "#9ca3af" : "#6b7280" },
            "& .MuiDataGrid-overlay": {
              backgroundColor: "transparent",
              color: isDark ? "#9ca3af" : "#6b7280",
            },
            "& .MuiDataGrid-columnSeparator, & .MuiDataGrid-menuIcon": { display: "none" },
            "& .MuiDataGrid-sortIcon": { color: isDark ? "#9ca3af" : "#6b7280" },
            "& .MuiCheckbox-root": {
              color: isDark ? "#9ca3af" : "#6b7280",
              "&.Mui-checked": { color: "#dc2626" },
            },
          },
        },
      },
    },
  });

  return (
    <div className={`data-table ${className}`}>
      <ThemeProvider theme={theme}>
        <DataGrid
          rows={rows}
          columns={columns}
          loading={loading}
          initialState={{ pagination: { paginationModel: { pageSize, page: 0 } } }}
          pageSizeOptions={pageSizeOptions}
          onRowClick={onRowClick}
          checkboxSelection={checkboxSelection}
          onRowSelectionModelChange={onSelectionChange}
          rowHeight={rowHeight}
          columnHeaderHeight={headerHeight}
          autoHeight={autoHeight}
          getRowId={getRowId}
          disableRowSelectionOnClick
          disableColumnMenu
          hideFooter={hideFooter}
          sx={{
            "& .MuiDataGrid-virtualScroller": {
              minHeight: rows.length === 0 ? "200px" : "auto",
            },
          }}
        />
      </ThemeProvider>
    </div>
  );
};

export default DataTable;