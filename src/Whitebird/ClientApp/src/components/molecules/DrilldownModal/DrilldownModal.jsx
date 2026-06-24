import React, { useState, useEffect, useCallback, useRef } from 'react';
import { Box, Typography, Chip, Pagination as MuiPagination, Stack } from '@mui/material';
import Modal from '../Modal/Modal';
import DataTable from '../DataTable/DataTable';
import Spinner from '../../atoms/Spinner/Spinner';
import apiService from '../../../core/services/api.service';
import { getStatusChipStyles } from '../../../core/constants/statusColors';

/**
 * DrilldownModal - Generic modal for showing detailed data from dashboard cards
 * @param {Object} props
 * @param {boolean} props.isOpen - Modal visibility
 * @param {Function} props.onClose - Close handler
 * @param {string} props.title - Modal title
 * @param {string} props.endpoint - API endpoint to fetch data
 * @param {Object} props.params - Query parameters
 * @param {Array} props.columns - Grid columns (optional, uses default if not provided)
 * @param {string} props.size - Modal size (sm, md, lg, xl)
 * @param {string} props.dataPath - Path to data array in response (e.g., 'data.data')
 */
const DrilldownModal = ({ 
  isOpen, 
  onClose, 
  title, 
  endpoint, 
  params = {}, 
  columns = null,
  size = 'xl',
  dataPath = 'data.data'
}) => {
  const [loading, setLoading] = useState(false);
  const [data, setData] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const isMountedRef = useRef(true);
  const abortControllerRef = useRef(null);

  useEffect(() => {
    isMountedRef.current = true;
    return () => {
      isMountedRef.current = false;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, []);

  useEffect(() => {
    if (isOpen) {
      setPage(1);
    }
  }, [isOpen, endpoint, JSON.stringify(params)]);

  const fetchData = useCallback(async () => {
    if (!endpoint) {
      console.warn('DrilldownModal: No endpoint provided');
      return;
    }
    
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    abortControllerRef.current = new AbortController();
    
    setLoading(true);
    try {
      const queryParams = new URLSearchParams();
      queryParams.append('page', page);
      queryParams.append('pageSize', pageSize);
      
      // Add all params
      Object.entries(params).forEach(([key, value]) => {
        if (value !== null && value !== undefined && value !== '') {
          queryParams.append(key, value);
        }
      });
      
      const url = `${endpoint}${queryParams.toString() ? `?${queryParams.toString()}` : ''}`;
      
      const response = await apiService.get(url, {
        signal: abortControllerRef.current.signal
      });
      
      const responseData = response.data;
      
      // Extract data using dataPath
      let items = [];
      let total = 0;
      
      // Parse dataPath (e.g., 'data.data' -> responseData.data.data)
      const pathParts = dataPath.split('.');
      let current = responseData;
      for (const part of pathParts) {
        if (current && current[part] !== undefined) {
          current = current[part];
        } else {
          current = null;
          break;
        }
      }
      
      if (Array.isArray(current)) {
        items = current;
        total = items.length;
      } else if (responseData?.data?.data && Array.isArray(responseData.data.data)) {
        items = responseData.data.data;
        total = responseData.data.totalCount || items.length;
      } else if (responseData?.data && Array.isArray(responseData.data)) {
        items = responseData.data;
        total = items.length;
      } else if (Array.isArray(responseData)) {
        items = responseData;
        total = items.length;
      } else if (responseData?.data?.items && Array.isArray(responseData.data.items)) {
        items = responseData.data.items;
        total = responseData.data.totalCount || items.length;
      }
      
      const calculatedTotalPages = Math.max(1, Math.ceil(total / pageSize));
      
      if (isMountedRef.current) {
        setData(items);
        setTotalCount(total);
        setTotalPages(calculatedTotalPages);
      }
    } catch (error) {
      if (error?.name !== 'CanceledError' && error?.message !== 'canceled') {
        console.error('Failed to fetch drilldown data:', error);
      }
      if (isMountedRef.current) {
        setData([]);
        setTotalCount(0);
        setTotalPages(1);
      }
    } finally {
      if (isMountedRef.current) {
        setLoading(false);
      }
    }
  }, [endpoint, params, page, pageSize, dataPath]);

  useEffect(() => {
    if (isOpen && endpoint) {
      fetchData();
    }
  }, [isOpen, endpoint, page, fetchData]);

  const handlePageChange = (event, newPage) => {
    setPage(newPage);
  };

  // Default columns - detect if data has 'approved' field for status
  const defaultColumns = useCallback(() => {
    const firstItem = data[0] || {};
    const hasApprovedField = 'approved' in firstItem;
    
    return [
      { field: "assetCode", headerName: "Code", width: 120 },
      { field: "assetName", headerName: "Name", flex: 1, minWidth: 180 },
      { 
        field: hasApprovedField ? "approved" : "currentStatus",
        headerName: "Status", 
        width: 140,
        renderCell: (p) => {
          let status = 'Pending';
          if (hasApprovedField) {
            // Transaction status
            if (p?.value === true) status = 'Approved';
            if (p?.value === false) status = 'Rejected';
          } else {
            // Asset status
            status = p?.value || 'Available';
          }
          return <Chip label={status} size="small" sx={getStatusChipStyles(status)} />;
        }
      },
    ];
  }, [data]);

  const gridColumns = columns || defaultColumns();

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title} size={size}>
      {loading ? (
        <div className="page-loading"><Spinner size="lg" /></div>
      ) : data.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 4, color: 'var(--text-secondary)' }}>
          <Typography>No data found</Typography>
        </Box>
      ) : (
        <>
          <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="body2" color="text.secondary">
              Showing page {page} of {totalPages} | Total {totalCount} item(s)
            </Typography>
          </Box>
          <DataTable
            rows={data}
            columns={gridColumns}
            pageSize={pageSize}
            getRowId={(row) => row.assetId || row.assetTransactionId || row.id || Math.random().toString()}
            hideFooter={true}
            autoHeight={false}
            ariaLabel={`${title} details`}
          />
          {totalPages > 1 && (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
              <Stack spacing={2}>
                <MuiPagination
                  count={totalPages}
                  page={page}
                  onChange={handlePageChange}
                  color="primary"
                  size="medium"
                  showFirstButton
                  showLastButton
                />
              </Stack>
            </Box>
          )}
        </>
      )}
    </Modal>
  );
};

export default DrilldownModal;