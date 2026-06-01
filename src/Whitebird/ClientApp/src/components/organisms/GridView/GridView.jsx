import React, { useState, useCallback } from 'react';
import { Box } from '@mui/material';
import DataTable from '../../molecules/DataTable/DataTable';
import Pagination from '../../molecules/Pagination/Pagination';
import SearchToolbar from '../../molecules/SearchToolbar/SearchToolbar';
import Tabs from '../../molecules/Tabs/Tabs';
import GridFilterPanel from '../../molecules/GridFilterPanel/GridFilterPanel';
import Spinner from '../../atoms/Spinner/Spinner';
import Button from '../../atoms/Button/Button';
import { FiPlus } from 'react-icons/fi';

/**
 * Standardized Grid View component
 */
const GridView = ({
  title,
  tabs,
  activeTab,
  onTabChange,
  onCreate,
  columns,
  data,
  loading,
  page,
  totalPages,
  pageSize,
  totalItems,
  onPageChange,
  onPageSizeChange,
  onSearch,
  showCheckbox = false,
  selectedRows = [],
  onSelectionChange,
  createButtonText = 'Add New',
  ariaLabel = 'Data table',
  extraActions = null,
  filterChildren,
  showFilters = false,
  onFilterToggle,
  onResetFilters,
}) => {
  const [searchTerm, setSearchTerm] = useState('');

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    if (onSearch) onSearch(search);
  }, [onSearch]);

  const getRowId = useCallback((row) => {
    if (row.id) return row.id;
    const possibleIdFields = Object.keys(row).filter(k => k.toLowerCase().includes('id'));
    if (possibleIdFields.length > 0) return row[possibleIdFields[0]];
    return `row-${Math.random().toString(36).substr(2, 9)}`;
  }, []);

  const displayLoading = loading && (!data || data.length === 0);

  return (
    <Box className="grid-view">
      {/* Header with Title and Actions */}
      <Box sx={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        mb: 3,
        flexWrap: 'wrap',
        gap: 2
      }}>
        <h1 className="page-title" style={{ margin: 0 }}>{title}</h1>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          {extraActions}
          {onCreate && (
            <Button variant="primary" onClick={onCreate} startIcon={<FiPlus />}>
              {createButtonText}
            </Button>
          )}
        </Box>
      </Box>

      {/* Tabs */}
      {tabs && tabs.length > 0 && (
        <Tabs tabs={tabs} activeTab={activeTab} onTabChange={onTabChange} />
      )}

      {/* Search Toolbar */}
      <SearchToolbar
        onSearch={handleSearch}
        onFilterToggle={onFilterToggle}
        showFilters={showFilters}
        placeholder="Search..."
      />

      {/* Filter Panel */}
      {filterChildren && (
        <GridFilterPanel
          visible={showFilters}
          onToggle={onFilterToggle}
          onReset={onResetFilters}
        >
          {filterChildren}
        </GridFilterPanel>
      )}

      {/* Data Table */}
      <Box
        sx={{
          backgroundColor: 'var(--card-bg)',
          borderRadius: '12px',
          overflow: 'hidden',
          mb: 2,
          border: '1px solid var(--border)',
          width: '100%',
          minWidth: 0,
        }}
      >
        <DataTable
          rows={data || []}
          columns={columns}
          loading={displayLoading}
          pageSize={pageSize}
          getRowId={getRowId}
          hideFooter={true}
          autoHeight={false}
          checkboxSelection={showCheckbox}
          onSelectionChange={onSelectionChange}
          ariaLabel={ariaLabel}
        />
      </Box>

      {/* Pagination */}
      <Pagination
        currentPage={page}
        totalPages={totalPages || 1}
        pageSize={pageSize}
        totalItems={totalItems || 0}
        onPageChange={onPageChange}
        onPageSizeChange={onPageSizeChange}
      />
    </Box>
  );
};

export default GridView;