import { useState, useCallback, useEffect, useRef } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { DEBOUNCE_CONFIG } from '../core/constants/appConstants';
import { useDebounce } from './useDebounce';
import apiService from '../core/services/api.service';

/**
 * Custom hook untuk grid data dengan React Query caching.
 * 
 * PERBAIKAN:
 * - queryKey menggunakan array of values instead of JSON.stringify
 * - debounce untuk filter changes
 * - abort controller untuk cancel in-flight requests
 * 
 * @param {Array|string} queryKey - Unique query key (array dianjurkan)
 * @param {Function} fetchFn - Async function untuk fetch data
 * @param {Object} [options] - Additional options
 */
export const useGridData = (queryKey, fetchFn, options = {}) => {
  const [page, setPage] = useState(options.initialPage || 1);
  const [pageSize, setPageSize] = useState(options.initialPageSize || 10);
  const [filters, setFilters] = useState({});
  const abortControllerRef = useRef(null);
  
  // Debounce filter changes to prevent excessive API calls
  const debouncedFilters = useDebounce(filters, DEBOUNCE_CONFIG.SEARCH_DELAY);

  // Build query key as array (more efficient than JSON.stringify)
  const fullQueryKey = Array.isArray(queryKey)
    ? [...queryKey, page, pageSize, ...Object.entries(debouncedFilters).flat()]
    : [queryKey, page, pageSize, ...Object.entries(debouncedFilters).flat()];

  // Cancel previous request when query key changes
  useEffect(() => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    abortControllerRef.current = new AbortController();
    
    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fullQueryKey]);

  const queryResult = useQuery({
    queryKey: fullQueryKey,
    queryFn: () => {
      // Cancel previous request
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
      abortControllerRef.current = new AbortController();
      
      return fetchFn({ page, pageSize, ...debouncedFilters }, {
        signal: abortControllerRef.current.signal
      });
    },
    staleTime: options.staleTime || 2 * 60 * 1000,
    placeholderData: (previousData) => previousData,
  });

  const updateFilters = useCallback((newFilters) => {
    setFilters(prev => ({ ...prev, ...newFilters }));
    setPage(1);
  }, []);

  const extractData = (source) => {
    if (!source) return [];
    if (Array.isArray(source)) return source;
    if (source.data && Array.isArray(source.data)) return source.data;
    if (source.data?.data && Array.isArray(source.data.data)) return source.data.data;
    if (source.data?.data?.data && Array.isArray(source.data.data.data)) return source.data.data.data;
    return [];
  };

  const extractTotalCount = (source) => {
    if (!source) return 0;
    if (typeof source.totalCount === 'number') return source.totalCount;
    if (source.data && typeof source.data.totalCount === 'number') return source.data.totalCount;
    if (source.data?.data && typeof source.data.data.totalCount === 'number') return source.data.data.totalCount;
    if (source.data?.data?.data && typeof source.data.data.data.totalCount === 'number') return source.data.data.data.totalCount;
    return 0;
  };

  const rawData = queryResult.data;
  const data = extractData(rawData);
  const totalCount = extractTotalCount(rawData) || data.length;

  return {
    data,
    totalCount,
    loading: queryResult.isLoading || queryResult.isFetching,
    error: queryResult.error,
    page,
    setPage,
    pageSize,
    setPageSize,
    filters,
    updateFilters,
    reload: queryResult.refetch,
  };
};