import { useCallback } from 'react';
import { useGridData } from './useGridData';

/**
 * Enhanced useGridData with tab-based filtering
 * Untuk menghilangkan duplikasi fetchGridData dengan tab filtering
 * 
 * @param {string|Array} queryKey - Unique query key
 * @param {Function} baseFetchFn - Base fetch function
 * @param {Object} options - Options
 * @param {string} options.activeTab - Current active tab ID
 * @param {Object} options.tabFilters - Mapping of tab ID to filter object
 * @param {Object} options.extraFilters - Additional filters
 * @returns {Object} Grid data state and controls
 */
export const useTabFilteredGrid = (queryKey, baseFetchFn, options = {}) => {
  const { activeTab, tabFilters = {}, extraFilters = {} } = options;

  const fetchWithTabFilter = useCallback(async (params) => {
    const tabFilter = tabFilters[activeTab] || {};
    const mergedParams = { ...params, ...extraFilters, ...tabFilter };
    return baseFetchFn(mergedParams);
  }, [activeTab, tabFilters, extraFilters, baseFetchFn]);

  const fullQueryKey = Array.isArray(queryKey)
    ? [...queryKey, activeTab, ...Object.values(extraFilters)]
    : [queryKey, activeTab, ...Object.values(extraFilters)];

  return useGridData(fullQueryKey, fetchWithTabFilter);
};

export default useTabFilteredGrid;