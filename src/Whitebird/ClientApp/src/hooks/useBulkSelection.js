import { useState, useCallback } from 'react';

/**
 * Hook untuk standardisasi bulk selection di grid
 * @param {Object} options
 * @param {string} options.idField - ID field name (default: 'id')
 */
export const useBulkSelection = (options = {}) => {
  const { idField = 'id' } = options;
  const [selectedRowIds, setSelectedRowIds] = useState([]);

  const handleSelectionChange = useCallback((selectionModel) => {
    setSelectedRowIds(selectionModel);
  }, []);

  const clearSelection = useCallback(() => {
    setSelectedRowIds([]);
  }, []);

  const getSelectedIds = useCallback((rows) => {
    if (!rows || rows.length === 0) return [];
    return selectedRowIds.map(rowId => {
      const row = rows.find(r => r[idField] === rowId);
      return row?.[idField] || rowId;
    });
  }, [selectedRowIds, idField]);

  const hasSelection = selectedRowIds.length > 0;
  const selectionCount = selectedRowIds.length;

  return {
    selectedRowIds,
    selectionCount,
    hasSelection,
    handleSelectionChange,
    clearSelection,
    getSelectedIds,
  };
};

export default useBulkSelection;