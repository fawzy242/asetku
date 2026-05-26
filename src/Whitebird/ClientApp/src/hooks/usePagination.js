import { useState, useCallback, useEffect } from 'react';

export const usePagination = (options = {}) => {
  const {
    initialPage = 1,
    initialPageSize = 10,
    pageSizeOptions = [10, 25, 50, 100],
  } = options;

  const [page, setPage] = useState(initialPage);
  const [pageSize, setPageSize] = useState(initialPageSize);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  // Calculate total pages when totalCount or pageSize changes
  useEffect(() => {
    const calculatedTotalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    setTotalPages(calculatedTotalPages);
    
    // Reset to page 1 if current page exceeds total pages
    if (page > calculatedTotalPages) {
      setPage(1);
    }
  }, [totalCount, pageSize, page]);

  const goToPage = useCallback((newPage) => {
    if (newPage >= 1 && newPage <= totalPages) {
      setPage(newPage);
    }
  }, [totalPages]);

  const goToFirstPage = useCallback(() => {
    setPage(1);
  }, []);

  const goToLastPage = useCallback(() => {
    setPage(totalPages);
  }, [totalPages]);

  const goToNextPage = useCallback(() => {
    if (page < totalPages) {
      setPage(page + 1);
    }
  }, [page, totalPages]);

  const goToPreviousPage = useCallback(() => {
    if (page > 1) {
      setPage(page - 1);
    }
  }, [page]);

  const changePageSize = useCallback((newPageSize) => {
    setPageSize(newPageSize);
    setPage(1);
  }, []);

  const updateTotalCount = useCallback((newTotalCount) => {
    setTotalCount(newTotalCount);
  }, []);

  const getOffset = useCallback(() => {
    return (page - 1) * pageSize;
  }, [page, pageSize]);

  const getLimit = useCallback(() => {
    return pageSize;
  }, [pageSize]);

  const resetPagination = useCallback(() => {
    setPage(initialPage);
    setPageSize(initialPageSize);
    setTotalCount(0);
  }, [initialPage, initialPageSize]);

  return {
    // State
    page,
    pageSize,
    totalCount,
    totalPages,
    pageSizeOptions,
    
    // Actions
    goToPage,
    goToFirstPage,
    goToLastPage,
    goToNextPage,
    goToPreviousPage,
    changePageSize,
    updateTotalCount,
    getOffset,
    getLimit,
    resetPagination,
    
    // Convenience setters
    setPage,
    setPageSize,
  };
};

export default usePagination;