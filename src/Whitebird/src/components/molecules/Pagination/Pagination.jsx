import React from "react";
import { FiChevronLeft, FiChevronRight, FiChevronsLeft, FiChevronsRight } from "react-icons/fi";
import "./Pagination.scss";

const Pagination = ({
  currentPage = 1,
  totalPages = 1,
  pageSize = 10,
  totalItems = 0,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = [10, 25, 50, 100],
  showPageSize = true,
  showTotal = true,
  className = "",
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
      if (start > 2) pages.push("...");
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    if (end < totalPages) {
      if (end < totalPages - 1) pages.push("...");
      pages.push(totalPages);
    }

    return pages;
  };

  const startItem = Math.min((currentPage - 1) * pageSize + 1, totalItems);
  const endItem = Math.min(currentPage * pageSize, totalItems);

  return (
    <div className={`pagination ${className}`}>
      <div className="pagination__info">
        {showTotal && totalItems > 0 && (
          <span className="pagination__total">
            Showing {startItem} - {endItem} of {totalItems} items
          </span>
        )}
      </div>

      <div className="pagination__controls">
        {showPageSize && (
          <div className="pagination__page-size">
            <select
              value={pageSize}
              onChange={(e) => onPageSizeChange?.(Number(e.target.value))}
              className="pagination__select"
            >
              {pageSizeOptions.map((size) => (
                <option key={size} value={size}>
                  {size} / page
                </option>
              ))}
            </select>
          </div>
        )}

        <div className="pagination__pages">
          <button
            className="pagination__btn"
            onClick={() => onPageChange(1)}
            disabled={currentPage === 1}
          >
            <FiChevronsLeft size={16} />
          </button>
          <button
            className="pagination__btn"
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage === 1}
          >
            <FiChevronLeft size={16} />
          </button>

          {getPageNumbers().map((page, index) => (
            <React.Fragment key={index}>
              {page === "..." ? (
                <span className="pagination__ellipsis">...</span>
              ) : (
                <button
                  className={`pagination__btn ${currentPage === page ? "pagination__btn--active" : ""}`}
                  onClick={() => onPageChange(page)}
                >
                  {page}
                </button>
              )}
            </React.Fragment>
          ))}

          <button
            className="pagination__btn"
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
          >
            <FiChevronRight size={16} />
          </button>
          <button
            className="pagination__btn"
            onClick={() => onPageChange(totalPages)}
            disabled={currentPage === totalPages}
          >
            <FiChevronsRight size={16} />
          </button>
        </div>
      </div>
    </div>
  );
};

export default Pagination;