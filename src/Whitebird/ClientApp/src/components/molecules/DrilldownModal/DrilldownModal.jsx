import React, { useState, useEffect, useCallback, useRef } from 'react';
import { Box, Typography, Chip, Pagination as MuiPagination, Stack } from '@mui/material';
import Modal from '../Modal/Modal';
import DataTable from '../DataTable/DataTable';
import Spinner from '../../atoms/Spinner/Spinner';
import apiService from '../../../core/services/api.service';
import { getStatusChipStyles } from '../../../core/constants/statusColors';

const DrilldownModal = ({ isOpen, onClose, title, endpoint, params = {}, columns }) => {
  const [loading, setLoading] = useState(false);
  const [data, setData] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const isMountedRef = useRef(true);

  useEffect(() => {
    isMountedRef.current = true;
    return () => {
      isMountedRef.current = false;
    };
  }, []);

  const fetchData = useCallback(async () => {
    if (!endpoint) {
      console.warn('DrilldownModal: No endpoint provided');
      return;
    }
    
    setLoading(true);
    try {
      // Build query string from params
      const queryParams = new URLSearchParams();
      
      // Add pagination params
      queryParams.append('page', page);
      queryParams.append('pageSize', pageSize);
      
      Object.entries(params).forEach(([key, value]) => {
        if (value !== null && value !== undefined && value !== '') {
          if (value === null) {
            return;
          }
          queryParams.append(key, value);
        }
      });
      
      const url = `${endpoint}${queryParams.toString() ? `?${queryParams.toString()}` : ''}`;
      console.log('DrilldownModal fetching:', url);
      
      const response = await apiService.get(url);
      const responseData = response.data;
      
      let items = [];
      let total = 0;
      let totalPagesCount = 1;
      
      // Handle different response structures
      if (responseData?.data?.data && Array.isArray(responseData.data.data)) {
        items = responseData.data.data;
        total = responseData.data.totalCount || items.length;
        totalPagesCount = responseData.data.totalPages || Math.ceil(total / pageSize);
      } 
      else if (responseData?.data && Array.isArray(responseData.data)) {
        items = responseData.data;
        total = items.length;
        totalPagesCount = Math.ceil(total / pageSize);
      } 
      else if (Array.isArray(responseData)) {
        items = responseData;
        total = items.length;
        totalPagesCount = Math.ceil(total / pageSize);
      } 
      else if (responseData?.data?.items && Array.isArray(responseData.data.items)) {
        items = responseData.data.items;
        total = responseData.data.totalCount || items.length;
        totalPagesCount = responseData.data.totalPages || Math.ceil(total / pageSize);
      } 
      else if (responseData?.isSuccess && responseData?.data && Array.isArray(responseData.data)) {
        items = responseData.data;
        total = items.length;
        totalPagesCount = Math.ceil(total / pageSize);
      }
      
      if (isMountedRef.current) {
        setData(items);
        setTotalCount(total);
        setTotalPages(totalPagesCount);
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
    }
    if (isMountedRef.current) {
      setLoading(false);
    }
  }, [endpoint, params, page, pageSize]);

  useEffect(() => {
    if (isOpen && endpoint) {
      setPage(1); // Reset page when modal opens
      fetchData();
    }
  }, [isOpen, endpoint, fetchData]);

  // Refetch when page changes
  useEffect(() => {
    if (isOpen && endpoint && page > 0) {
      fetchData();
    }
  }, [page, isOpen, endpoint, fetchData]);

  const handlePageChange = (event, newPage) => {
    setPage(newPage);
  };

  const defaultColumns = columns || [
    { field: "assetCode", headerName: "Code", width: 120 },
    { field: "assetName", headerName: "Name", flex: 1, minWidth: 180 },
    { 
      field: "currentStatus", 
      headerName: "Status", 
      width: 140, 
      renderCell: (p) => <Chip label={p?.value || '-'} size="small" sx={getStatusChipStyles(p?.value)} /> 
    },
  ];

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title} size="lg">
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
            columns={defaultColumns}
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