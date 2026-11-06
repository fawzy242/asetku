// src/hooks/useGridData.js
import { useState, useCallback, useEffect, useRef } from 'react';
import { useQuery } from '@tanstack/react-query';

export const useGridData = (queryKey, fetchFn, options = {}) => {
  const [page, setPage] = useState(options.initialPage || 1);
  const [pageSize, setPageSize] = useState(options.initialPageSize || 10);
  const [sortModel, setSortModel] = useState(options.initialSortModel || []);
  const abortControllerRef = useRef(null);

  const sortKey = sortModel.map((s) => `${s.field}:${s.sort}`).join(',');
  const baseKey = Array.isArray(queryKey) ? queryKey : [queryKey];
  const fullQueryKey = [...baseKey, page, pageSize, sortKey];

  useEffect(() => {
    if (abortControllerRef.current) abortControllerRef.current.abort();
    abortControllerRef.current = new AbortController();
    return () => { if (abortControllerRef.current) abortControllerRef.current.abort(); };
  }, [fullQueryKey]);

  const queryResult = useQuery({
    queryKey: fullQueryKey,
    queryFn: () => {
      if (abortControllerRef.current) abortControllerRef.current.abort();
      abortControllerRef.current = new AbortController();

      const sortBy = sortModel[0]?.field;
      const sortDescending = sortModel[0]?.sort === 'desc';

      return fetchFn(
        {
          page,
          pageSize,
          ...(sortBy ? { sortBy } : {}),
          ...(sortBy ? { sortDescending } : {}),
        },
        { signal: abortControllerRef.current.signal }
      );
    },
    staleTime: options.staleTime !== undefined ? options.staleTime : 0,
    enabled: options.enabled !== undefined ? options.enabled : true,
    placeholderData: (previousData) => previousData,
  });

  const handlePageChange = useCallback((newPage) => setPage(newPage), []);
  const handlePageSizeChange = useCallback((newPageSize) => { setPageSize(newPageSize); setPage(1); }, []);
  const handleSortChange = useCallback((newSortModel) => { setSortModel(newSortModel || []); setPage(1); }, []);

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
    setPage: handlePageChange,
    pageSize,
    setPageSize: handlePageSizeChange,
    sortModel,
    setSortModel: handleSortChange,
    reload: queryResult.refetch,
  };
};