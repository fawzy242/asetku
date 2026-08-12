// src/pages/AssetTracking/AssetTracking.menu.jsx
import React, { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { FiSearch, FiBox, FiUser, FiMapPin, FiRefreshCw, FiCalendar, FiClock, FiAlertTriangle, FiActivity, FiTool, FiMove } from 'react-icons/fi';
import { Grid, Box, Typography, Avatar, Chip, Paper, IconButton, Tooltip, Tabs as MuiTabs, Tab } from '@mui/material';
import { useLocation } from 'react-router-dom';
import AssetTrackingData from './AssetTracking.data';
import DataTable from '../../components/molecules/DataTable/DataTable';
import Card from '../../components/atoms/Card/Card';
import Select from '../../components/atoms/Select/Select';
import Spinner from '../../components/atoms/Spinner/Spinner';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import InfoRow from '../../components/molecules/InfoRow/InfoRow';
import { useOptions } from '../../hooks/useOptions';
import { getTransactionTypeName } from '../../core/constants/transactionTypes';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import utilsHelper from '../../core/utils/utils.helper';
import './AssetTracking.scss';

const trackingData = new AssetTrackingData();

const timelineColumns = [
  {
    field: 'transactionDate',
    headerName: 'Date & Time',
    width: 180,
    renderCell: (params) => {
      const val = params?.row?.transactionDate ?? params?.value;
      return val ? <span>{utilsHelper.formatDateTime(val)}</span> : <span className="u-text-muted">-</span>;
    },
  },
  {
    field: 'transactionTypeName',
    headerName: 'Activity Type',
    width: 160,
    renderCell: (params) => {
      const val = params?.row?.transactionTypeName ?? params?.value ?? '-';
      return <span>{val}</span>;
    },
  },
  {
    field: 'employeeName',
    headerName: 'Employee',
    width: 150,
    renderCell: (params) => {
      const val = params?.row?.employeeName ?? params?.value ?? '-';
      return <span>{val}</span>;
    },
  },
  // FIX: Hapus fromEmployeeName dan toEmployeeName - backend hanya punya employeeName
  {
    field: 'conditionName',
    headerName: 'Condition',
    width: 120,
    renderCell: (params) => {
      const val = params?.row?.conditionName ?? params?.value ?? '-';
      return <span>{val}</span>;
    },
  },
  {
    field: 'notes',
    headerName: 'Notes',
    flex: 1,
    minWidth: 200,
    renderCell: (params) => {
      const val = params?.row?.notes ?? params?.value ?? '-';
      return <span>{val}</span>;
    },
  },
];

const AssetTrackingMenu = () => {
  const [loading, setLoading] = useState(true);
  const [assets, setAssets] = useState([]);
  const [selectedAssetId, setSelectedAssetId] = useState(null);
  const [trackingData_, setTrackingData_] = useState(null);
  const [loadingData, setLoadingData] = useState(false);
  const [historyFilter, setHistoryFilter] = useState('all');
  const isMountedRef = useRef(true);
  const location = useLocation();
  const { toast } = useSweetAlert();

  useEffect(() => {
    isMountedRef.current = true;
    loadAssets();
    return () => { isMountedRef.current = false; };
  }, []);

  const loadAssets = async () => {
    setLoading(true);
    const assetsRes = await trackingData.fetchAssets();
    if (isMountedRef.current && assetsRes.success) {
      const assetsData = Array.isArray(assetsRes.data) ? assetsRes.data : [];
      setAssets(assetsData);
      
      const params = new URLSearchParams(location.search);
      const assetIdParam = params.get('assetId');
      if (assetIdParam) {
        const assetId = parseInt(assetIdParam, 10);
        const assetExists = assetsData.some(a => a.assetId === assetId);
        if (assetExists) handleAssetSelect(assetId);
      }
    }
    if (isMountedRef.current) setLoading(false);
  };

  const handleAssetSelect = useCallback(
    async (assetId) => {
      if (!assetId) { setSelectedAssetId(null); setTrackingData_(null); return; }
      setSelectedAssetId(assetId);
      setLoadingData(true);

      try {
        const trackingRes = await trackingData.fetchAssetTracking(assetId);

        if (isMountedRef.current && trackingRes.success && trackingRes.data) {
          const data = trackingRes.data;
          // FIX: Backend mengembalikan timeline dengan employeeName, conditionName
          // Tidak ada fromEmployeeName, toEmployeeName, conditionBeforeName, conditionAfterName
          const timeline = (data.timeline || []).map(item => ({
            ...item,
            // Pastikan field yang digunakan sesuai dengan backend
            employeeName: item.employeeName || item.newHolder || item.currentHolderName || '-',
            conditionName: item.conditionName || item.newStatus || item.condition || '-',
          }));
          
          setTrackingData_({
            assetId: data.assetId,
            assetCode: data.assetCode || '-',
            assetName: data.assetName || '-',
            currentStatus: data.currentStatus || 'Available',
            categoryName: data.categoryName || '-',
            currentHolderName: data.currentHolderName || '-',
            currentLocation: data.currentLocation || '-',
            conditionName: data.conditionName || '-',
            isOnLoan: data.isOnLoan || false,
            isInMaintenance: data.isInMaintenance || false,
            isOverdue: data.isOverdue || false,
            loanDueDate: data.loanDueDate,
            totalTransactions: data.totalTransactions || 0,
            timeline: timeline,
            photoProfileUrl: data.photoProfileUrl || null,
            softwareInstallation: data.softwareInstallation || null,
          });
        } else {
          // Fallback: fetch dari asset detail
          const detailRes = await trackingData.fetchAssetDetail(assetId);
          if (isMountedRef.current && detailRes.success && detailRes.data) {
            const data = detailRes.data;
            setTrackingData_({
              assetId: data.assetId,
              assetCode: data.assetCode || '-',
              assetName: data.assetName || '-',
              currentStatus: data.currentStatus || 'Available',
              categoryName: data.categoryName || '-',
              currentHolderName: data.currentHolderName || '-',
              currentLocation: data.currentLocation || '-',
              conditionName: data.assetConditionName || '-',
              isOnLoan: data.currentStatus === 'On Loan',
              isInMaintenance: data.currentStatus === 'In Maintenance',
              isOverdue: false,
              loanDueDate: null,
              totalTransactions: 0,
              timeline: [],
              photoProfileUrl: null,
              softwareInstallation: data.softwareInstallation || null,
            });
          }
        }
      } catch (error) {
        console.error('Error fetching asset tracking:', error);
        if (isMountedRef.current) toast.error('Failed to load asset tracking data');
      } finally {
        if (isMountedRef.current) setLoadingData(false);
      }
    },
    [toast]
  );

  const handleRefresh = useCallback(async () => {
    if (!selectedAssetId) return;
    setLoadingData(true);
    await handleAssetSelect(selectedAssetId);
    setLoadingData(false);
    toast.success('Data refreshed');
  }, [selectedAssetId, handleAssetSelect, toast]);

  useEffect(() => {
    if (assets.length > 0) {
      const params = new URLSearchParams(location.search);
      const assetIdParam = params.get('assetId');
      if (assetIdParam) {
        const assetId = parseInt(assetIdParam, 10);
        const assetExists = assets.some(a => a.assetId === assetId);
        if (assetExists && selectedAssetId !== assetId) handleAssetSelect(assetId);
      }
    }
  }, [location.search, assets, selectedAssetId, handleAssetSelect]);

  const getStatusChip = (status) => <StatusChip status={status} />;

  const assetOptions = useOptions(
    Array.isArray(assets) ? assets.map(a => ({ value: a.assetId, label: `${a.assetCode} - ${a.assetName}` })) : [],
    'Choose an asset to track...'
  );

  const timeline = trackingData_?.timeline || [];
  const currentAsset = trackingData_;

  const filteredTimeline = useMemo(() => {
    if (historyFilter === 'all') return timeline;
    const filterMap = {
      movement: ['HANDOVER', 'TRANSFER', 'RETURN'],
      loan: ['LOAN', 'LOAN_RETURN'],
      maintenance: ['MAINTENANCE', 'POST_MAINTENANCE'],
      disposal: ['DISPOSAL'],
    };
    const allowedTypes = filterMap[historyFilter] || [];
    return timeline.filter(t => {
      const type = t.transactionTypeName?.toUpperCase() || '';
      return allowedTypes.some(at => type.includes(at));
    });
  }, [timeline, historyFilter]);

  const stats = useMemo(() => {
    const result = { totalMovements: 0, totalLoans: 0, totalMaintenance: 0, totalDisposals: 0 };
    timeline.forEach(t => {
      const type = t.transactionTypeName?.toUpperCase() || '';
      if (['HANDOVER', 'TRANSFER', 'RETURN'].some(at => type.includes(at))) result.totalMovements++;
      if (['LOAN', 'LOAN_RETURN'].some(at => type.includes(at))) result.totalLoans++;
      if (['MAINTENANCE', 'POST_MAINTENANCE'].some(at => type.includes(at))) result.totalMaintenance++;
      if (type.includes('DISPOSAL')) result.totalDisposals++;
    });
    return result;
  }, [timeline]);

  const historyFilterTabs = [
    { id: 'all', label: 'All History' },
    { id: 'movement', label: 'Movements' },
    { id: 'loan', label: 'Loans' },
    { id: 'maintenance', label: 'Maintenance' },
    { id: 'disposal', label: 'Disposals' },
  ];

  if (loading) {
    return (
      <div className="asset-tracking">
        <div className="page-header">
          <h1 className="page-title">Asset Tracking</h1>
          <p className="page-description">Track asset location, status, and transaction history</p>
        </div>
        <div className="page-loading"><Spinner size="lg" /></div>
      </div>
    );
  }

  return (
    <div className="asset-tracking">
      <div className="page-header">
        <h1 className="page-title">Asset Tracking</h1>
        <p className="page-description">Track asset location, status, and transaction history</p>
      </div>

      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Card className="asset-tracking__selection-card">
            <div className="asset-tracking__selection-content">
              <div className="asset-tracking__selection-icon"><FiSearch size={24} /></div>
              <div className="asset-tracking__selection-select">
                <Select
                  label="Select Asset"
                  value={selectedAssetId || ''}
                  onChange={e => handleAssetSelect(e.target.value)}
                  options={assetOptions}
                />
              </div>
              {selectedAssetId && (
                <Tooltip title="Refresh Data">
                  <IconButton onClick={handleRefresh} size="small" className="asset-tracking__refresh-btn">
                    <FiRefreshCw size={18} />
                  </IconButton>
                </Tooltip>
              )}
            </div>
          </Card>
        </Grid>

        {selectedAssetId && !loadingData && currentAsset && (
          <>
            <Grid item xs={12}>
              <Card className="asset-tracking__profile-card">
                <Grid container spacing={3} alignItems="center">
                  <Grid item>
                    <Avatar className="asset-tracking__profile-avatar"><FiBox size={36} /></Avatar>
                  </Grid>
                  <Grid item xs>
                    <Typography variant="h5" fontWeight={700} className="asset-tracking__profile-name">
                      {currentAsset.assetName || '-'}
                    </Typography>
                    <div className="asset-tracking__profile-tags">
                      <Chip label={`Code: ${currentAsset.assetCode || '-'}`} size="small" variant="outlined" />
                      <Chip label={`Category: ${currentAsset.categoryName || '-'}`} size="small" variant="outlined" />
                      {getStatusChip(currentAsset.currentStatus)}
                      {currentAsset.isOnLoan && <Chip icon={<FiClock />} label="On Loan" size="small" color="warning" variant="outlined" />}
                      {currentAsset.isInMaintenance && <Chip icon={<FiActivity />} label="In Maintenance" size="small" color="info" variant="outlined" />}
                      {currentAsset.isOverdue && <Chip icon={<FiAlertTriangle />} label="Overdue" size="small" color="error" />}
                    </div>
                  </Grid>
                  <Grid item>
                    <InfoRow icon={FiUser} label="Holder" value={currentAsset.currentHolderName} />
                    <InfoRow icon={FiMapPin} label="Location" value={currentAsset.currentLocation} />
                    <InfoRow icon={FiActivity} label="Condition" value={currentAsset.conditionName} />
                    {currentAsset.loanDueDate && (
                      <InfoRow
                        icon={FiCalendar}
                        label="Due Date"
                        value={utilsHelper.formatDate(currentAsset.loanDueDate)}
                        className={currentAsset.isOverdue ? 'u-text-error' : ''}
                      />
                    )}
                    <InfoRow icon={FiRefreshCw} label="Total Transactions" value={currentAsset.totalTransactions || 0} />
                  </Grid>
                </Grid>
              </Card>
            </Grid>

            <Grid item xs={12}>
              <Grid container spacing={2}>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiMove size={28} className="asset-tracking__stat-icon asset-tracking__stat-icon--blue" />
                    <Typography variant="h4" fontWeight={700}>{stats.totalMovements}</Typography>
                    <Typography variant="caption" color="text.secondary">Movements</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiClock size={28} className="asset-tracking__stat-icon asset-tracking__stat-icon--yellow" />
                    <Typography variant="h4" fontWeight={700}>{stats.totalLoans}</Typography>
                    <Typography variant="caption" color="text.secondary">Loans</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiTool size={28} className="asset-tracking__stat-icon asset-tracking__stat-icon--purple" />
                    <Typography variant="h4" fontWeight={700}>{stats.totalMaintenance}</Typography>
                    <Typography variant="caption" color="text.secondary">Maintenance</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiClock size={28} className="asset-tracking__stat-icon asset-tracking__stat-icon--red" />
                    <Typography variant="h4" fontWeight={700}>{stats.totalDisposals}</Typography>
                    <Typography variant="caption" color="text.secondary">Disposals</Typography>
                  </Paper>
                </Grid>
              </Grid>
            </Grid>

            <Grid item xs={12}>
              <Card>
                <Box sx={{ borderBottom: 1, borderColor: 'var(--border)', mb: 2 }}>
                  <MuiTabs
                    value={historyFilter}
                    onChange={(e, v) => setHistoryFilter(v)}
                    variant="scrollable"
                    scrollButtons="auto"
                  >
                    {historyFilterTabs.map(tab => (
                      <Tab
                        key={tab.id}
                        value={tab.id}
                        label={tab.label}
                        sx={{
                          textTransform: 'none',
                          fontWeight: 500,
                          color: 'var(--text-secondary)',
                          '&.Mui-selected': { color: 'var(--primary)' },
                        }}
                      />
                    ))}
                  </MuiTabs>
                </Box>
                <DataTable
                  rows={filteredTimeline}
                  columns={timelineColumns}
                  pageSize={10}
                  getRowId={(row) => row.id || row.assetTransactionId || `timeline-${row.transactionDate}-${row.transactionTypeName}-${Math.random()}`}
                  hideFooter={false}
                  ariaLabel="Asset timeline table"
                />
              </Card>
            </Grid>
          </>
        )}

        {selectedAssetId && loadingData && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__loading-container"><Spinner size="lg" /></div>
            </Card>
          </Grid>
        )}

        {!selectedAssetId && !loading && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__empty-state">
                <FiSearch size={64} />
                <Typography variant="h6" gutterBottom>Select an Asset to Track</Typography>
                <Typography variant="body2">
                  Choose an asset from the dropdown above to view its tracking history and current status.
                </Typography>
              </div>
            </Card>
          </Grid>
        )}
      </Grid>
    </div>
  );
};

export default AssetTrackingMenu;