import React, { memo } from 'react';
import { FiChevronLeft, FiChevronRight, FiChevronsLeft, FiChevronsRight } from 'react-icons/fi';
import './Pagination.scss';

const Pagination = memo(({
  currentPage,
  totalPages,
  pageSize,
  totalItems,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = [10, 25, 50, 100],
  showPageSize = true,
  showTotal = true,
  className = '',
}) => {
  const getPageNumbers = () => {
    const pages = [];
    const maxVisible = 5;
    let start = Math.max(1, currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(totalPages, start + maxVisible - 1);
    
    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }
    
    if (start > 1) {
      pages.push(1);
      if (start > 2) pages.push('...');
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    if (end < totalPages) {
      if (end < totalPages - 1) pages.push('...');
      pages.push(totalPages);
    }
    
    return pages;
  };

  const startItem = totalItems === 0 ? 0 : Math.min((currentPage - 1) * pageSize + 1, totalItems);
  const endItem = Math.min(currentPage * pageSize, totalItems);

  const handlePageSizeChange = (e) => {
    const newSize = parseInt(e.target.value, 10);
    onPageSizeChange(newSize);
  };

  const handlePageChange = (page) => {
    if (page !== currentPage && page >= 1 && page <= totalPages) {
      onPageChange(page);
    }
  };

  return (
    <nav className={`pagination ${className}`} aria-label="Pagination navigation">
      <div className="pagination__info">
        {showTotal && totalItems > 0 && (
          <span className="pagination__total" aria-live="polite">
            Showing {startItem} - {endItem} of {totalItems} items
          </span>
        )}
        {showTotal && totalItems === 0 && (
          <span className="pagination__total">No items to display</span>
        )}
      </div>

      <div className="pagination__controls">
        {showPageSize && (
          <div className="pagination__page-size">
            <label htmlFor="page-size-select" className="sr-only">Items per page</label>
            <select
              id="page-size-select"
              value={pageSize}
              onChange={handlePageSizeChange}
              className="pagination__select"
              aria-label="Items per page"
            >
              {pageSizeOptions.map((size) => (
                <option key={size} value={size}>{size} / page</option>
              ))}
            </select>
          </div>
        )}

        <div className="pagination__pages">
          <button
            className="pagination__btn"
            onClick={() => handlePageChange(1)}
            disabled={currentPage === 1 || totalPages === 0}
            aria-label="First page"
          >
            <FiChevronsLeft size={16} aria-hidden="true" />
          </button>
          <button
            className="pagination__btn"
            onClick={() => handlePageChange(currentPage - 1)}
            disabled={currentPage === 1 || totalPages === 0}
            aria-label="Previous page"
          >
            <FiChevronLeft size={16} aria-hidden="true" />
          </button>

          {getPageNumbers().map((page, index) => (
            <React.Fragment key={index}>
              {page === '...' ? (
                <span className="pagination__ellipsis" aria-hidden="true">...</span>
              ) : (
                <button
                  className={`pagination__btn ${currentPage === page ? 'pagination__btn--active' : ''}`}
                  onClick={() => handlePageChange(page)}
                  aria-label={`Page ${page}`}
                  aria-current={currentPage === page ? 'page' : undefined}
                >
                  {page}
                </button>
              )}
            </React.Fragment>
          ))}

          <button
            className="pagination__btn"
            onClick={() => handlePageChange(currentPage + 1)}
            disabled={currentPage === totalPages || totalPages === 0}
            aria-label="Next page"
          >
            <FiChevronRight size={16} aria-hidden="true" />
          </button>
          <button
            className="pagination__btn"
            onClick={() => handlePageChange(totalPages)}
            disabled={currentPage === totalPages || totalPages === 0}
            aria-label="Last page"
          >
            <FiChevronsRight size={16} aria-hidden="true" />
          </button>
        </div>
      </div>
    </nav>
  );
});

Pagination.displayName = 'Pagination';
export default Pagination;