import React, { useState, useEffect, useCallback } from 'react';
import { Box, Typography, Chip } from '@mui/material';
import Modal from '../Modal/Modal';
import DataTable from '../DataTable/DataTable';
import Spinner from '../../atoms/Spinner/Spinner';
import apiService from '../../../core/services/api.service';
import { getStatusChipStyles } from '../../../core/constants/statusColors';
import utilsHelper from '../../../core/utils/utils.helper';

const DrilldownModal = ({ isOpen, onClose, title, endpoint, params = {}, columns }) => {
  const [loading, setLoading] = useState(false);
  const [data, setData] = useState([]);
  const [totalCount, setTotalCount] = useState(0);

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const response = await apiService.get(endpoint, { params });
      const responseData = response.data;
      let items = [];
      
      if (responseData?.data?.data && Array.isArray(responseData.data.data)) {
        items = responseData.data.data;
      } else if (responseData?.data && Array.isArray(responseData.data)) {
        items = responseData.data;
      } else if (Array.isArray(responseData)) {
        items = responseData;
      }
      
      setData(items);
      setTotalCount(items.length);
    } catch (error) {
      console.error('Failed to fetch drilldown data:', error);
      setData([]);
      setTotalCount(0);
    }
    setLoading(false);
  }, [endpoint, params]);

  useEffect(() => {
    if (isOpen) {
      fetchData();
    }
  }, [isOpen, fetchData]);

  const defaultColumns = columns || [
    { field: "assetCode", headerName: "Code", width: 120 },
    { field: "assetName", headerName: "Name", flex: 1, minWidth: 180 },
    { field: "status", headerName: "Status", width: 140, renderCell: (p) => <Chip label={p?.value || '-'} size="small" sx={getStatusChipStyles(p?.value)} /> },
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
          <Box sx={{ mb: 2 }}>
            <Typography variant="body2" color="text.secondary">
              Showing {totalCount} item(s)
            </Typography>
          </Box>
          <DataTable
            rows={data}
            columns={defaultColumns}
            pageSize={10}
            getRowId={(row) => row.assetId || row.assetTransactionId || Math.random()}
            hideFooter={true}
            ariaLabel={`${title} details`}
          />
        </>
      )}
    </Modal>
  );
};

export default DrilldownModal;