import { useState, useCallback } from 'react';
import { useQuery } from '@tanstack/react-query';

/**
 * Custom hook untuk grid data dengan React Query caching.
 * 
 * PERBAIKAN KRITIS: queryKey sekarang bisa berupa array dengan identifier
 * yang berubah saat tab/filter berubah. fetchFn dipanggil ulang (refetch)
 * SETIAP KALI queryKey berubah.
 * 
 * @param {Array|string} queryKey - Unique query key (array dianjurkan)
 * @param {Function} fetchFn - Async function untuk fetch data
 * @param {Object} [options] - Additional options
 */
export const useGridData = (queryKey, fetchFn, options = {}) => {
  const [page, setPage] = useState(options.initialPage || 1);
  const [pageSize, setPageSize] = useState(options.initialPageSize || 10);
  const [filters, setFilters] = useState({});

  // Query key: gabungan base key + page + pageSize + filters
  const fullQueryKey = Array.isArray(queryKey)
    ? [...queryKey, page, pageSize, JSON.stringify(filters)]
    : [queryKey, page, pageSize, JSON.stringify(filters)];

  const queryResult = useQuery({
    queryKey: fullQueryKey,
    queryFn: () => fetchFn({ page, pageSize, ...filters }),
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