import { useState, useCallback } from 'react';

/**
 * Standardizes bulk selection for MUI DataGrid.
 * @param {Object} options
 * @param {string} options.idField - ID field name (default: 'id')
 */
export const useBulkSelection = (options = {}) => {
  const { idField = 'id' } = options;
  const [selectedRowIds, setSelectedRowIds] = useState([]);

  const handleSelectionChange = useCallback((selectionModel) => {
    setSelectedRowIds(selectionModel || []);
  }, []);

  const clearSelection = useCallback(() => {
    setSelectedRowIds([]);
  }, []);

  const getSelectedIds = useCallback(
    (rows) => {
      if (!rows || rows.length === 0) return [];
      return selectedRowIds.map((rowId) => {
        const row = rows.find((r) => r[idField] === rowId);
        return row?.[idField] || rowId;
      });
    },
    [selectedRowIds, idField]
  );

  const getSelectedRows = useCallback(
    (rows) => {
      if (!rows || rows.length === 0) return [];
      return selectedRowIds
        .map((rowId) => rows.find((r) => r[idField] === rowId))
        .filter(Boolean);
    },
    [selectedRowIds, idField]
  );

  const hasSelection = selectedRowIds.length > 0;
  const selectionCount = selectedRowIds.length;

  return {
    selectedRowIds,
    selectionCount,
    hasSelection,
    handleSelectionChange,
    clearSelection,
    getSelectedIds,
    getSelectedRows,
  };
};

export default useBulkSelection;