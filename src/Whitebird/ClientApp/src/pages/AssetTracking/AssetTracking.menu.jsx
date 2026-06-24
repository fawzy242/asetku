import React, { useState, useEffect, useCallback, useRef, useMemo } from "react";
import { 
  FiSearch, FiBox, FiUser, FiMapPin, FiRefreshCw, 
  FiCalendar, FiClock, FiAlertTriangle, FiActivity,
  FiTool, FiMove
} from "react-icons/fi";
import { 
  Grid, Box, Typography, Avatar, Chip, Paper, 
  IconButton, Tooltip, Tabs as MuiTabs, Tab
} from "@mui/material";
import { useLocation } from "react-router-dom";
import AssetTrackingData from "./AssetTracking.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Card from "../../components/atoms/Card/Card";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { getTransactionTypeName } from "../../core/constants/transactionTypes";
import { useSweetAlert } from "../../hooks/useSweetAlert";
import utilsHelper from "../../core/utils/utils.helper";
import "./AssetTracking.scss";

const trackingData = new AssetTrackingData();

// ============================================================
// TIMELINE COLUMNS - Langsung mapping dari response
// ============================================================
const timelineColumns = [
  { 
    field: "date", 
    headerName: "Date & Time", 
    width: 180,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.date || params?.value;
      if (!value) return <span>-</span>;
      return <span>{utilsHelper.formatDateTime(value)}</span>;
    }
  },
  { 
    field: "activityType", 
    headerName: "Activity Type", 
    width: 160, 
    flex: 0.5,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.activityType || row.transactionTypeName || '-';
      return <span>{value}</span>;
    }
  },
  { 
    field: "previousHolder", 
    headerName: "From", 
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.previousHolder || row.fromEmployeeName || '-';
      return <span>{value}</span>;
    }
  },
  { 
    field: "newHolder", 
    headerName: "To", 
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.newHolder || row.toEmployeeName || '-';
      return <span>{value}</span>;
    }
  },
  { 
    field: "previousStatus", 
    headerName: "Previous Status", 
    width: 120,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.previousStatus || row.conditionBeforeName || '-';
      return <span>{value}</span>;
    }
  },
  { 
    field: "newStatus", 
    headerName: "New Status", 
    width: 120,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.newStatus || row.conditionAfterName || '-';
      return <span>{value}</span>;
    }
  },
  { 
    field: "notes", 
    headerName: "Notes", 
    flex: 1, 
    minWidth: 200,
    renderCell: (params) => {
      const row = params?.row || {};
      const value = row.notes || row.note || '-';
      return <span>{value}</span>;
    }
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
    return () => {
      isMountedRef.current = false;
    };
  }, []);

  const loadAssets = async () => {
    setLoading(true);
    const assetsRes = await trackingData.fetchAssets();
    if (isMountedRef.current && assetsRes.success) {
      setAssets(assetsRes.data);
      const params = new URLSearchParams(location.search);
      const assetIdParam = params.get('assetId');
      if (assetIdParam) {
        const assetId = parseInt(assetIdParam, 10);
        const assetExists = assetsRes.data.some(a => a.assetId === assetId);
        if (assetExists) {
          handleAssetSelect(assetId);
        }
      }
    }
    if (isMountedRef.current) {
      setLoading(false);
    }
  };

  // ============================================================
  // HANDLE ASSET SELECT - Direct mapping dari response
  // ============================================================
  const handleAssetSelect = useCallback(async (assetId) => {
    if (!assetId) { 
      setSelectedAssetId(null); 
      setTrackingData_(null); 
      return; 
    }
    setSelectedAssetId(assetId);
    setLoadingData(true);
    
    try {
      const trackingRes = await trackingData.fetchAssetTracking(assetId);
      
      if (isMountedRef.current) {
        if (trackingRes.success && trackingRes.data) {
          const data = trackingRes.data;
          
          setTrackingData_({
            assetId: data.assetId,
            assetCode: data.assetCode || '-',
            assetName: data.assetName || '-',
            currentStatus: data.currentStatus || 'Available',
            categoryName: data.categoryName || '-',
            currentHolderName: data.currentHolderName || '-',
            currentLocation: data.currentLocation || '-',
            condition: data.condition || '-',
            isOnLoan: data.isOnLoan || false,
            isInMaintenance: data.isInMaintenance || false,
            isOverdue: data.isOverdue || false,
            loanDueDate: data.loanDueDate,
            totalTransactions: data.totalTransactions || 0,
            timeline: data.timeline || []
          });
        } else {
          // Fallback: Fetch detail and transactions separately
          const [detailRes, transRes] = await Promise.all([
            trackingData.fetchAssetDetail(assetId),
            trackingData.fetchTransactions(assetId)
          ]);
          
          if (isMountedRef.current) {
            const transactions = transRes.data || [];
            const timeline = transactions.map(tx => ({
              id: tx.assetTransactionId || tx.id || Math.random().toString(),
              date: tx.transactionDate || tx.date || new Date().toISOString(),
              activityType: tx.transactionTypeName || tx.typeName || getTransactionTypeName(tx.transactionType) || 'Unknown',
              previousHolder: tx.fromEmployeeName || tx.fromEmployee || tx.fromName || '-',
              newHolder: tx.toEmployeeName || tx.toEmployee || tx.toName || '-',
              previousStatus: tx.conditionBeforeName || tx.conditionBefore || tx.prevStatus || '-',
              newStatus: tx.conditionAfterName || tx.conditionAfter || tx.newStatus || '-',
              notes: tx.notes || tx.note || '-'
            }));
            
            setTrackingData_({
              assetId: detailRes.data?.assetId,
              assetCode: detailRes.data?.assetCode || '-',
              assetName: detailRes.data?.assetName || '-',
              currentStatus: detailRes.data?.currentStatus || 'Available',
              categoryName: detailRes.data?.categoryName || '-',
              currentHolderName: detailRes.data?.currentHolderName || '-',
              currentLocation: detailRes.data?.officeName || '-',
              condition: detailRes.data?.assetConditionName || '-',
              isOnLoan: detailRes.data?.currentStatus === 'On Loan',
              isInMaintenance: detailRes.data?.currentStatus === 'In Maintenance',
              isOverdue: detailRes.data?.isOverdue || false,
              loanDueDate: detailRes.data?.expectedReturnDate,
              totalTransactions: transactions.length,
              timeline: timeline
            });
          }
        }
      }
    } catch (error) {
      console.error('Error fetching asset tracking data:', error);
      if (isMountedRef.current) {
        toast.error('Failed to load asset tracking data');
      }
    } finally {
      if (isMountedRef.current) {
        setLoadingData(false);
      }
    }
  }, [toast]);

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
        if (assetExists && selectedAssetId !== assetId) {
          handleAssetSelect(assetId);
        }
      }
    }
  }, [location.search, assets, selectedAssetId, handleAssetSelect]);

  const getStatusChip = (status) => (
    <Chip label={status || '-'} size="small" sx={getStatusChipStyles(status)} />
  );

  const assetOptions = useMemo(() => [
    { value: "", label: "Choose an asset to track..." },
    ...assets.map(a => ({ value: a.assetId, label: `${a.assetCode} - ${a.assetName}` }))
  ], [assets]);

  const timeline = trackingData_?.timeline || [];
  const currentAsset = trackingData_;

  // ============================================================
  // FILTER TIMELINE BY CATEGORY
  // ============================================================
  const filteredTimeline = useMemo(() => {
    if (historyFilter === 'all') return timeline;
    
    const filterMap = {
      'movement': ['HANDOVER', 'TRANSFER', 'RETURN'],
      'loan': ['LOAN', 'LOAN_RETURN'],
      'maintenance': ['MAINTENANCE', 'POST_MAINTENANCE'],
      'disposal': ['DISPOSAL']
    };
    
    const allowedTypes = filterMap[historyFilter] || [];
    return timeline.filter(t => {
      const type = t.activityType?.toUpperCase() || '';
      return allowedTypes.some(at => type.includes(at));
    });
  }, [timeline, historyFilter]);

  // ============================================================
  // STATISTICS PER CATEGORY
  // ============================================================
  const stats = useMemo(() => {
    const result = {
      totalMovements: 0,
      totalLoans: 0,
      totalMaintenance: 0,
      totalDisposals: 0
    };
    
    timeline.forEach(t => {
      const type = t.activityType?.toUpperCase() || '';
      if (['HANDOVER', 'TRANSFER', 'RETURN'].some(at => type.includes(at))) {
        result.totalMovements++;
      }
      if (['LOAN', 'LOAN_RETURN'].some(at => type.includes(at))) {
        result.totalLoans++;
      }
      if (['MAINTENANCE', 'POST_MAINTENANCE'].some(at => type.includes(at))) {
        result.totalMaintenance++;
      }
      if (type.includes('DISPOSAL')) {
        result.totalDisposals++;
      }
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
        {/* ============================================================ */}
        {/* ASSET SELECTION CARD - Refresh button DI DALAM CARD seperti Employee Summary */}
        {/* ============================================================ */}
        <Grid item xs={12}>
          <Card className="asset-tracking__selection-card">
            <div className="asset-tracking__selection-content">
              <div className="asset-tracking__selection-icon">
                <FiSearch size={24} />
              </div>
              <div className="asset-tracking__selection-select">
                <Select 
                  label="Select Asset" 
                  value={selectedAssetId || ""} 
                  onChange={e => handleAssetSelect(e.target.value)}
                  options={assetOptions} 
                />
              </div>
              {/* Refresh button DI DALAM CARD - SAMA seperti Employee Summary */}
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
        
        {/* ============================================================ */}
        {/* ASSET PROFILE - Data dari server */}
        {/* ============================================================ */}
        {selectedAssetId && !loadingData && currentAsset && (
          <>
            <Grid item xs={12}>
              <Card className="asset-tracking__profile-card">
                <Grid container spacing={3} alignItems="center">
                  <Grid item>
                    <Avatar className="asset-tracking__profile-avatar">
                      <FiBox size={36} />
                    </Avatar>
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
                    <div className="asset-tracking__info-row">
                      <FiUser size={16} className="asset-tracking__info-icon" />
                      <span className="asset-tracking__info-label">Holder:</span>
                      <span className="asset-tracking__info-value">{currentAsset.currentHolderName || '-'}</span>
                    </div>
                    <div className="asset-tracking__info-row">
                      <FiMapPin size={16} className="asset-tracking__info-icon" />
                      <span className="asset-tracking__info-label">Location:</span>
                      <span className="asset-tracking__info-value">{currentAsset.currentLocation || '-'}</span>
                    </div>
                    <div className="asset-tracking__info-row">
                      <FiActivity size={16} className="asset-tracking__info-icon" />
                      <span className="asset-tracking__info-label">Condition:</span>
                      <span className="asset-tracking__info-value">{currentAsset.condition || '-'}</span>
                    </div>
                    {currentAsset.loanDueDate && (
                      <div className="asset-tracking__info-row">
                        <FiCalendar size={16} className="asset-tracking__info-icon" />
                        <span className="asset-tracking__info-label">Due Date:</span>
                        <span className="asset-tracking__info-value" style={{ color: currentAsset.isOverdue ? '#ef4444' : 'inherit' }}>
                          {utilsHelper.formatDate(currentAsset.loanDueDate)}
                        </span>
                      </div>
                    )}
                    <div className="asset-tracking__info-row">
                      <FiRefreshCw size={16} className="asset-tracking__info-icon" />
                      <span className="asset-tracking__info-label">Total Transactions:</span>
                      <span className="asset-tracking__info-value">{currentAsset.totalTransactions || 0}</span>
                    </div>
                  </Grid>
                </Grid>
              </Card>
            </Grid>

            {/* ============================================================ */}
            {/* STATISTICS CARDS */}
            {/* ============================================================ */}
            <Grid item xs={12}>
              <Grid container spacing={2}>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiMove size={28} className="asset-tracking__stat-icon" style={{ color: '#3b82f6' }} />
                    <Typography variant="h4" fontWeight={700}>{stats.totalMovements}</Typography>
                    <Typography variant="caption" color="text.secondary">Movements</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiClock size={28} className="asset-tracking__stat-icon" style={{ color: '#f59e0b' }} />
                    <Typography variant="h4" fontWeight={700}>{stats.totalLoans}</Typography>
                    <Typography variant="caption" color="text.secondary">Loans</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiTool size={28} className="asset-tracking__stat-icon" style={{ color: '#8b5cf6' }} />
                    <Typography variant="h4" fontWeight={700}>{stats.totalMaintenance}</Typography>
                    <Typography variant="caption" color="text.secondary">Maintenance</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiClock size={28} className="asset-tracking__stat-icon" style={{ color: '#ef4444' }} />
                    <Typography variant="h4" fontWeight={700}>{stats.totalDisposals}</Typography>
                    <Typography variant="caption" color="text.secondary">Disposals</Typography>
                  </Paper>
                </Grid>
              </Grid>
            </Grid>

            {/* ============================================================ */}
            {/* TIMELINE - Dengan filter tabs */}
            {/* ============================================================ */}
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
                  getRowId={(row) => row.id || row.assetTransactionId || `timeline-${row.date}-${row.activityType}-${Math.random()}`}
                  hideFooter={false}
                  ariaLabel="Asset timeline table"
                />
              </Card>
            </Grid>
          </>
        )}
        
        {/* ============================================================ */}
        {/* LOADING STATE */}
        {/* ============================================================ */}
        {selectedAssetId && loadingData && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__loading-container">
                <Spinner size="lg" />
              </div>
            </Card>
          </Grid>
        )}

        {/* ============================================================ */}
        {/* EMPTY STATE */}
        {/* ============================================================ */}
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