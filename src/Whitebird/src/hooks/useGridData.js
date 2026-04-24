import { useState, useCallback, useEffect, useRef } from "react";

export const useGridData = (fetchFn, initialPage = 1, initialPageSize = 10) => {
  const [data, setData] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(initialPage);
  const [pageSize, setPageSize] = useState(initialPageSize);
  const [filters, setFilters] = useState({});
  const isFirstRender = useRef(true);

  const loadData = useCallback(async () => {
    setLoading(true);
    const result = await fetchFn({ page, pageSize, ...filters });
    if (result.success) {
      setData(result.data.data || []);
      setTotalCount(result.data.totalCount || 0);
    }
    setLoading(false);
  }, [fetchFn, page, pageSize, filters]);

  useEffect(() => {
    if (isFirstRender.current) {
      isFirstRender.current = false;
    }
    loadData();
  }, [loadData]);

  const updateFilters = (newFilters) => {
    setFilters(prev => ({ ...prev, ...newFilters }));
    setPage(1);
  };

  return { data, totalCount, loading, page, setPage, pageSize, setPageSize, filters, updateFilters, reload: loadData };
};