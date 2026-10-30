// src/pages/AssetTracking/AssetTracking.jsx
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  FiSearch,
  FiRefreshCw,
  FiUsers,
} from 'react-icons/fi';
import { Grid, Box, Typography, IconButton, Tooltip } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useLocation } from 'react-router-dom';
import { AssetTrackingData } from '../../core/services';
import DataTable from '../../components/molecules/DataTable/DataTable';
import Card from '../../components/atoms/Card/Card';
import Select from '../../components/atoms/Select/Select';
import Spinner from '../../components/atoms/Spinner/Spinner';
import { useOptions } from '../../hooks/useOptions';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import {
  employeeHistoryColumns,
  scheduleStatsColumns,
} from './columns';
import { TRANSACTION_TYPE_COLORS } from './constants';
import TrackingProfileCard from './components/TrackingProfileCard';
import TrackingStatsGrid from './components/TrackingStatsGrid';
import TrackingChartCard from './components/TrackingChartCard';
import TrackingTimelineCard from './components/TrackingTimelineCard';
import './AssetTracking.scss';

const trackingData = new AssetTrackingData();

const AssetTracking = () => {
  const location = useLocation();
  const { toast } = useSweetAlert();
  const queryClient = useQueryClient();

  const [selectedAssetId, setSelectedAssetId] = useState(null);
  const [schedulePage, setSchedulePage] = useState(1);
  const [schedulePageSize, setSchedulePageSize] = useState(10);
  const [employeePage, setEmployeePage] = useState(1);
  const [employeePageSize, setEmployeePageSize] = useState(10);

  // ============================================================
  // QUERIES
  // ============================================================

  const { data: assets = [], isLoading: loading } = useQuery({
    queryKey: ['asset-tracking-assets'],
    queryFn: async () => {
      const res = await trackingData.fetchAssets();
      return res.success ? res.data : [];
    },
    staleTime: 5 * 60 * 1000,
  });

  const { data: currentAsset = null, isLoading: loadingData } = useQuery({
    queryKey: ['asset-tracking-detail', selectedAssetId],
    queryFn: async () => {
      const res = await trackingData.fetchAssetTracking(selectedAssetId);
      if (res.success && res.data) return res.data;
      const detailRes = await trackingData.fetchAssetDetail(selectedAssetId);
      return detailRes.success ? detailRes.data : null;
    },
    enabled: !!selectedAssetId,
    staleTime: 60 * 1000,
  });

  const { data: transactionTypeStats = null } = useQuery({
    queryKey: ['asset-tracking-tx-stats', selectedAssetId],
    queryFn: async () => {
      const res = await trackingData.fetchTransactionTypeStats(selectedAssetId);
      return res.success ? res.data : null;
    },
    enabled: !!selectedAssetId,
    staleTime: 5 * 60 * 1000,
  });

  const {
    data: scheduleStatsData = { data: [], totalCount: 0 },
    isLoading: scheduleLoading,
  } = useQuery({
    queryKey: [
      'asset-tracking-schedule',
      selectedAssetId,
      schedulePage,
      schedulePageSize,
    ],
    queryFn: async () => {
      const res = await trackingData.fetchScheduleStats(selectedAssetId, {
        page: schedulePage,
        pageSize: schedulePageSize,
      });
      return res.success ? res.data : { data: [], totalCount: 0 };
    },
    enabled: !!selectedAssetId,
    staleTime: 2 * 60 * 1000,
  });

  const {
    data: employeeHistoryData = { data: [], totalCount: 0 },
    isLoading: employeeLoading,
  } = useQuery({
    queryKey: [
      'asset-tracking-employees',
      selectedAssetId,
      employeePage,
      employeePageSize,
    ],
    queryFn: async () => {
      const res = await trackingData.fetchEmployeeHistory(selectedAssetId, {
        page: employeePage,
        pageSize: employeePageSize,
      });
      return res.success ? res.data : { data: [], totalCount: 0 };
    },
    enabled: !!selectedAssetId,
    staleTime: 2 * 60 * 1000,
  });

  // ============================================================
  // URL PARAM PRESELECTION
  // ============================================================
  useEffect(() => {
    if (!assets || assets.length === 0) return;
    const params = new URLSearchParams(location.search);
    const assetIdParam = params.get('assetId');
    if (!assetIdParam) return;
    const assetId = parseInt(assetIdParam, 10);
    if (isNaN(assetId)) return;
    const exists = assets.some((a) => a.assetId === assetId);
    if (exists && selectedAssetId !== assetId) {
      setSelectedAssetId(assetId);
      setSchedulePage(1);
      setEmployeePage(1);
    }
  }, [assets, location.search, selectedAssetId]);

  // ============================================================
  // DERIVED DATA
  // ============================================================
  const timelineData = useMemo(() => {
    if (!currentAsset?.timeline) return [];
    return currentAsset.timeline.map((item) => ({
      ...item,
      employeeName: item.employeeName || item.newHolder || '-',
      status: item.status || item.newStatus || '-',
    }));
  }, [currentAsset]);

  const scheduleStats = scheduleStatsData?.data || [];
  const scheduleTotal = scheduleStatsData?.totalCount || 0;
  const employeeHistory = employeeHistoryData?.data || [];
  const employeeTotal = employeeHistoryData?.totalCount || 0;

  const transactionTypeChartData = useMemo(() => {
    const list = transactionTypeStats?.data || [];
    return {
      labels: list.map((item) => item.transactionType || 'Unknown'),
      values: list.map((item) => item.count || 0),
      colors: list.map(
        (item) => TRANSACTION_TYPE_COLORS[item.transactionType] || '#6b7280'
      ),
      details: list.map((item) => ({
        label: item.transactionType || 'Unknown',
        count: item.count || 0,
        percentage: item.percentage || 0,
        color: TRANSACTION_TYPE_COLORS[item.transactionType] || '#6b7280',
      })),
    };
  }, [transactionTypeStats]);

  // ============================================================
  // HANDLERS
  // ============================================================
  const handleAssetSelect = useCallback((assetId) => {
    if (!assetId) {
      setSelectedAssetId(null);
      return;
    }
    setSelectedAssetId(assetId);
    setSchedulePage(1);
    setEmployeePage(1);
  }, []);

  const handleRefresh = useCallback(() => {
    if (!selectedAssetId) return;
    queryClient.invalidateQueries({
      queryKey: ['asset-tracking-detail', selectedAssetId],
    });
    queryClient.invalidateQueries({
      queryKey: ['asset-tracking-tx-stats', selectedAssetId],
    });
    queryClient.invalidateQueries({
      queryKey: ['asset-tracking-schedule', selectedAssetId],
    });
    queryClient.invalidateQueries({
      queryKey: ['asset-tracking-employees', selectedAssetId],
    });
    toast.success('Data refreshed');
  }, [selectedAssetId, queryClient, toast]);

  const handleSchedulePageChange = (newPage) => {
    setSchedulePage(newPage);
  };
  const handleSchedulePageSizeChange = (newSize) => {
    setSchedulePageSize(newSize);
    setSchedulePage(1);
  };
  const handleEmployeePageChange = (newPage) => {
    setEmployeePage(newPage);
  };
  const handleEmployeePageSizeChange = (newSize) => {
    setEmployeePageSize(newSize);
    setEmployeePage(1);
  };

  const assetOptions = useOptions(
    Array.isArray(assets)
      ? assets.map((a) => ({
          value: a.assetId,
          label: `${a.assetCode} - ${a.assetName}`,
        }))
      : [],
    'Choose an asset to track...'
  );

  // ============================================================
  // RENDER — LOADING
  // ============================================================
  if (loading) {
    return (
      <div className="asset-tracking">
        <div className="page-header">
          <h1 className="page-title">Asset Tracking</h1>
          <p className="page-description">
            Track asset location, status, and transaction history
          </p>
        </div>
        <div className="page-loading">
          <Spinner size="lg" />
        </div>
      </div>
    );
  }

  return (
    <div className="asset-tracking">
      <div className="page-header">
        <h1 className="page-title">Asset Tracking</h1>
        <p className="page-description">
          Track asset location, status, and transaction history
        </p>
      </div>

      <Grid container spacing={3}>
        {/* Asset selector */}
        <Grid item xs={12}>
          <Card className="asset-tracking__selection-card">
            <div className="asset-tracking__selection-content">
              <div className="asset-tracking__selection-icon">
                <FiSearch size={24} />
              </div>
              <div className="asset-tracking__selection-select">
                <Select
                  label="Select Asset"
                  value={selectedAssetId || ''}
                  onChange={(e) => handleAssetSelect(e.target.value)}
                  options={assetOptions}
                />
              </div>
              {selectedAssetId && (
                <Tooltip title="Refresh Data">
                  <IconButton
                    onClick={handleRefresh}
                    size="small"
                    className="asset-tracking__refresh-btn"
                  >
                    <FiRefreshCw size={18} />
                  </IconButton>
                </Tooltip>
              )}
            </div>
          </Card>
        </Grid>

        {/* Loading skeleton while detail loads */}
        {selectedAssetId && loadingData && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__loading-container">
                <Spinner size="lg" />
              </div>
            </Card>
          </Grid>
        )}

        {/* Detail content */}
        {selectedAssetId && !loadingData && currentAsset && (
          <>
            <Grid item xs={12}>
              <TrackingProfileCard currentAsset={currentAsset} />
            </Grid>

            <Grid item xs={12}>
              <TrackingStatsGrid
                currentAsset={currentAsset}
                timelineData={timelineData}
              />
            </Grid>

            <Grid item xs={12} md={4}>
              <TrackingChartCard
                transactionTypeChartData={transactionTypeChartData}
              />
            </Grid>

            <Grid item xs={12} md={8}>
              <Card
                title="Schedule Stats"
                subtitle="Upcoming and completed schedules"
              >
                <DataTable
                  rows={scheduleStats}
                  columns={scheduleStatsColumns}
                  loading={scheduleLoading}
                  pageSize={schedulePageSize}
                  page={schedulePage}
                  totalRowCount={scheduleTotal}
                  onPageChange={handleSchedulePageChange}
                  onPageSizeChange={handleSchedulePageSizeChange}
                  getRowId={(row) =>
                    row.scheduleId || `schedule-${Math.random()}`
                  }
                  hideFooter={false}
                  ariaLabel="Schedule stats table"
                  paginationMode="server"
                  autoHeight={false}
                />
              </Card>
            </Grid>

            <Grid item xs={12}>
              <Card
                title="Employee History"
                subtitle={`${employeeTotal} employee(s) have handled this asset`}
              >
                <DataTable
                  rows={employeeHistory}
                  columns={employeeHistoryColumns}
                  loading={employeeLoading}
                  pageSize={employeePageSize}
                  page={employeePage}
                  totalRowCount={employeeTotal}
                  onPageChange={handleEmployeePageChange}
                  onPageSizeChange={handleEmployeePageSizeChange}
                  getRowId={(row) => row.employeeId || `emp-${Math.random()}`}
                  hideFooter={false}
                  ariaLabel="Employee history table"
                  paginationMode="server"
                  autoHeight={false}
                />
              </Card>
            </Grid>

            <Grid item xs={12}>
              <TrackingTimelineCard timelineData={timelineData} />
            </Grid>
          </>
        )}

        {/* Empty state — no asset selected */}
        {!selectedAssetId && !loading && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__empty-state">
                <FiSearch size={64} />
                <Typography variant="h6" gutterBottom>
                  Select an Asset to Track
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Choose an asset from the dropdown above to view its tracking
                  history, current status, and transaction timeline.
                </Typography>
              </div>
            </Card>
          </Grid>
        )}

        {/* Empty state — asset selected but no data */}
        {selectedAssetId && !loadingData && !currentAsset && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__empty-state">
                <FiUsers size={64} />
                <Typography variant="h6" gutterBottom>
                  No Data Available
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  No tracking data was found for the selected asset.
                </Typography>
              </div>
            </Card>
          </Grid>
        )}
      </Grid>
    </div>
  );
};

export default AssetTracking;