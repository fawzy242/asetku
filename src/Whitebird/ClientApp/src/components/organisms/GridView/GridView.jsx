import React, { useState, useCallback, useEffect } from 'react';
import { Box } from '@mui/material';
import DataTable from '../../molecules/DataTable/DataTable';
import SearchToolbar from '../../molecules/SearchToolbar/SearchToolbar';
import Tabs from '../../molecules/Tabs/Tabs';
import Button from '../../atoms/Button/Button';
import Spinner from '../../atoms/Spinner/Spinner';
import { FiPlus } from 'react-icons/fi';

const GridView = ({
  title,
  tabs,
  activeTab,
  onTabChange,
  onCreate,
  columns,
  data,
  loading,
  totalCount,
  page,
  pageSize,
  onPageChange,
  onPageSizeChange,
  onSearch,
  showCheckbox = false,
  selectedRows = [],
  onSelectionChange,
  createButtonText = 'Add New',
  ariaLabel = 'Data table',
  extraActions = null,
  className = '',
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [isInitialLoad, setIsInitialLoad] = useState(true);

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

  const showSpinner = loading && (!data || data.length === 0) && isInitialLoad;

  useEffect(() => {
    if (!loading && data && data.length > 0) {
      setIsInitialLoad(false);
    }
  }, [loading, data]);

  useEffect(() => {
    setIsInitialLoad(true);
    const timer = setTimeout(() => {
      setIsInitialLoad(false);
    }, 500);
    return () => clearTimeout(timer);
  }, [activeTab]);

  if (showSpinner) {
    return (
      <Box className={`grid-view ${className}`}>
        <div className="page-header">
          <h1 className="page-title" style={{ margin: 0 }}>{title}</h1>
        </div>
        <div className="page-loading"><Spinner size="lg" /></div>
      </Box>
    );
  }

  return (
    <Box className={`grid-view ${className}`}>
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

      {tabs && tabs.length > 0 && (
        <Tabs tabs={tabs} activeTab={activeTab} onTabChange={onTabChange} />
      )}

      <SearchToolbar onSearch={handleSearch} placeholder="Search..." />

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
          loading={loading}
          pageSize={pageSize}
          page={page}
          totalRowCount={totalCount}
          onPageChange={onPageChange}
          onPageSizeChange={onPageSizeChange}
          getRowId={getRowId}
          hideFooter={false}
          autoHeight={false}
          checkboxSelection={showCheckbox}
          onRowSelectionModelChange={onSelectionChange}
          ariaLabel={ariaLabel}
          disableColumnFilter={false}
          disableColumnMenu={false}
          paginationMode="server"
        />
      </Box>
    </Box>
  );
};

export default GridView;