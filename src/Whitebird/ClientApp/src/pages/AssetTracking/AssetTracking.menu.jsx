import React, { useState, useEffect, useCallback, useRef, useMemo } from "react";
import { 
  FiSearch, FiBox, FiUser, FiMapPin, FiRefreshCw, 
  FiCalendar, FiClock, FiAlertTriangle, FiActivity,
  FiArrowRight, FiCheckCircle, FiXCircle, FiTool,
  FiArrowUpCircle, FiArrowDownCircle
} from "react-icons/fi";
import { 
  Grid, Box, Typography, Avatar, Chip, Paper, 
  Card as MuiCard, CardContent, IconButton, Tooltip 
} from "@mui/material";
import { useLocation } from "react-router-dom";
import AssetTrackingData from "./AssetTracking.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Card from "../../components/atoms/Card/Card";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import Skeleton from "../../components/atoms/Skeleton/Skeleton";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { TRANSACTION_TYPES } from "../../core/constants/transactionTypes";
import { useSweetAlert } from "../../hooks/useSweetAlert";
import utilsHelper from "../../core/utils/utils.helper";
import "./AssetTracking.scss";

const trackingData = new AssetTrackingData();

const timelineColumns = [
  { 
    field: "date", 
    headerName: "Date & Time", 
    width: 180,
    valueFormatter: (p) => p?.value ? utilsHelper.formatDateTime(p.value) : '-'
  },
  { field: "transactionTypeName", headerName: "Activity", width: 160, flex: 0.5 },
  { field: "fromEmployeeName", headerName: "From", width: 150, valueFormatter: (p) => p?.value || '-' },
  { field: "toEmployeeName", headerName: "To", width: 150, valueFormatter: (p) => p?.value || '-' },
  { field: "toLocationName", headerName: "Location", width: 130, valueFormatter: (p) => p?.value || '-' },
  { field: "conditionBeforeName", headerName: "Before", width: 100, valueFormatter: (p) => p?.value || '-' },
  { field: "conditionAfterName", headerName: "After", width: 100, valueFormatter: (p) => p?.value || '-' },
  { field: "notes", headerName: "Notes", flex: 1, minWidth: 200, valueFormatter: (p) => p?.value || '-' },
];

const getTransactionIcon = (type) => {
  switch (type) {
    case TRANSACTION_TYPES.HANDOVER:
      return <FiArrowDownCircle size={18} color="#3b82f6" />;
    case TRANSACTION_TYPES.TRANSFER:
      return <FiArrowRight size={18} color="#8b5cf6" />;
    case TRANSACTION_TYPES.LOAN:
      return <FiArrowUpCircle size={18} color="#f59e0b" />;
    case TRANSACTION_TYPES.RETURN:
    case TRANSACTION_TYPES.LOAN_RETURN:
      return <FiCheckCircle size={18} color="#10b981" />;
    case TRANSACTION_TYPES.MAINTENANCE:
      return <FiTool size={18} color="#f59e0b" />;
    case TRANSACTION_TYPES.POST_MAINTENANCE:
      return <FiCheckCircle size={18} color="#10b981" />;
    case TRANSACTION_TYPES.DISPOSAL:
      return <FiXCircle size={18} color="#ef4444" />;
    default:
      return <FiActivity size={18} />;
  }
};

const getTransactionColor = (type) => {
  switch (type) {
    case TRANSACTION_TYPES.HANDOVER:
      return '#3b82f6';
    case TRANSACTION_TYPES.TRANSFER:
      return '#8b5cf6';
    case TRANSACTION_TYPES.LOAN:
      return '#f59e0b';
    case TRANSACTION_TYPES.RETURN:
    case TRANSACTION_TYPES.LOAN_RETURN:
      return '#10b981';
    case TRANSACTION_TYPES.MAINTENANCE:
      return '#f59e0b';
    case TRANSACTION_TYPES.POST_MAINTENANCE:
      return '#10b981';
    case TRANSACTION_TYPES.DISPOSAL:
      return '#ef4444';
    default:
      return '#6b7280';
  }
};

const AssetTrackingMenu = () => {
  const [loading, setLoading] = useState(true);
  const [assets, setAssets] = useState([]);
  const [selectedAssetId, setSelectedAssetId] = useState(null);
  const [trackingData_, setTrackingData_] = useState(null);
  const [loadingData, setLoadingData] = useState(false);
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

  const handleAssetSelect = useCallback(async (assetId) => {
    if (!assetId) { 
      setSelectedAssetId(null); 
      setTrackingData_(null); 
      return; 
    }
    setSelectedAssetId(assetId);
    setLoadingData(true);
    const trackingRes = await trackingData.fetchAssetTracking(assetId);
    if (isMountedRef.current) {
      if (trackingRes.success && trackingRes.data) {
        setTrackingData_(trackingRes.data);
      } else {
        const [detailRes, transRes] = await Promise.all([
          trackingData.fetchAssetDetail(assetId),
          trackingData.fetchTransactions(assetId)
        ]);
        if (isMountedRef.current) {
          const transactions = transRes.data || [];
          const timeline = transactions.map(tx => ({
            id: tx.assetTransactionId || `tx-${Date.now()}-${Math.random()}`,
            date: tx.transactionDate,
            transactionType: tx.transactionType,
            transactionTypeName: tx.transactionTypeName || tx.transactionType,
            fromEmployeeName: tx.fromEmployeeName,
            toEmployeeName: tx.toEmployeeName,
            toLocationName: tx.toLocationName,
            conditionBeforeName: tx.conditionBeforeName,
            conditionAfterName: tx.conditionAfterName,
            notes: tx.notes,
            icon: getTransactionIcon(tx.transactionType),
            color: getTransactionColor(tx.transactionType)
          }));
          setTrackingData_({
            assetId: detailRes.data?.assetId,
            assetCode: detailRes.data?.assetCode,
            assetName: detailRes.data?.assetName,
            currentStatus: detailRes.data?.currentStatus || 'Available',
            categoryName: detailRes.data?.categoryName,
            currentHolderName: detailRes.data?.currentHolderName,
            currentLocation: detailRes.data?.officeName,
            condition: detailRes.data?.assetConditionName,
            isOnLoan: detailRes.data?.currentStatus === 'On Loan',
            isInMaintenance: detailRes.data?.currentStatus === 'In Maintenance',
            isOverdue: detailRes.data?.isOverdue || false,
            loanDueDate: detailRes.data?.expectedReturnDate,
            totalTransactions: transactions.length,
            timeline: timeline
          });
        }
      }
      setLoadingData(false);
    }
  }, []);

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
        {/* Asset Selection Card */}
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
              {selectedAssetId && (
                <Tooltip title="Refresh">
                  <IconButton 
                    onClick={() => handleAssetSelect(selectedAssetId)} 
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
        
        {selectedAssetId && !loadingData && currentAsset && (
          <>
            {/* Asset Profile Card */}
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
                      <FiCalendar size={16} className="asset-tracking__info-icon" />
                      <span className="asset-tracking__info-label">Last Transaction:</span>
                      <span className="asset-tracking__info-value">
                        {timeline.length > 0 ? utilsHelper.formatDate(timeline[0]?.date) : '-'}
                      </span>
                    </div>
                    <div className="asset-tracking__info-row">
                      <FiActivity size={16} className="asset-tracking__info-icon" />
                      <span className="asset-tracking__info-label">Total Transactions:</span>
                      <span className="asset-tracking__info-value">{currentAsset.totalTransactions || 0}</span>
                    </div>
                  </Grid>
                </Grid>
              </Card>
            </Grid>

            {/* Statistics Cards Row */}
            <Grid item xs={12}>
              <Grid container spacing={2}>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiUser size={28} className="asset-tracking__stat-icon" style={{ color: '#8b5cf6' }} />
                    <Typography variant="body2" fontWeight={600} className="asset-tracking__stat-value">
                      {currentAsset.currentHolderName || 'None'}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">Current Holder</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiMapPin size={28} className="asset-tracking__stat-icon" style={{ color: '#3b82f6' }} />
                    <Typography variant="body2" fontWeight={600} className="asset-tracking__stat-value">
                      {currentAsset.currentLocation || '-'}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">Office Location</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiActivity size={28} className="asset-tracking__stat-icon" style={{ color: currentAsset.currentStatus === 'Available' ? '#10b981' : '#f59e0b' }} />
                    {getStatusChip(currentAsset.currentStatus)}
                    <Typography variant="caption" color="text.secondary">Current Status</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Paper elevation={0} className="asset-tracking__stat-card">
                    <FiRefreshCw size={28} className="asset-tracking__stat-icon" style={{ color: '#dc2626' }} />
                    <Typography variant="h4" fontWeight={700}>{currentAsset.totalTransactions || 0}</Typography>
                    <Typography variant="caption" color="text.secondary">Total Transactions</Typography>
                  </Paper>
                </Grid>
              </Grid>
            </Grid>

            {/* Timeline Section */}
            <Grid item xs={12}>
              <Card 
                title="Transaction Timeline"
                subtitle={`${timeline.length} record(s) found`}
              >
                {loadingData ? (
                  <Box sx={{ py: 4 }}>
                    <Skeleton variant="rect" height={400} />
                  </Box>
                ) : timeline.length === 0 ? (
                  <div className="asset-tracking__empty-state">
                    <FiRefreshCw size={48} />
                    <Typography variant="h6" gutterBottom>No Transaction History</Typography>
                    <Typography variant="body2">This asset has no transaction records yet.</Typography>
                  </div>
                ) : (
                  <>
                    {/* Timeline Visual */}
                    <Box className="asset-tracking__timeline-visual">
                      {timeline.slice(0, 10).map((item, index) => (
                        <div key={item.id || index} className="asset-tracking__timeline-item">
                          <div className="asset-tracking__timeline-icon" style={{ backgroundColor: item.color || '#6b7280' }}>
                            {item.icon || <FiActivity size={16} />}
                          </div>
                          <div className="asset-tracking__timeline-content">
                            <div className="asset-tracking__timeline-header">
                              <span className="asset-tracking__timeline-type">{item.transactionTypeName || 'Unknown'}</span>
                              <span className="asset-tracking__timeline-date">
                                {item.date ? utilsHelper.formatDateTime(item.date) : '-'}
                              </span>
                            </div>
                            <div className="asset-tracking__timeline-details">
                              {item.fromEmployeeName && (
                                <span className="asset-tracking__timeline-detail">
                                  From: {item.fromEmployeeName}
                                </span>
                              )}
                              {item.toEmployeeName && (
                                <span className="asset-tracking__timeline-detail">
                                  To: {item.toEmployeeName}
                                </span>
                              )}
                              {item.toLocationName && (
                                <span className="asset-tracking__timeline-detail">
                                  Location: {item.toLocationName}
                                </span>
                              )}
                              {item.notes && (
                                <span className="asset-tracking__timeline-detail asset-tracking__timeline-notes">
                                  {item.notes}
                                </span>
                              )}
                            </div>
                          </div>
                        </div>
                      ))}
                      {timeline.length > 10 && (
                        <div className="asset-tracking__timeline-more">
                          +{timeline.length - 10} more transactions
                        </div>
                      )}
                    </Box>

                    <DataTable 
                      rows={timeline} 
                      columns={timelineColumns} 
                      pageSize={10}
                      getRowId={(row, index) => row.id || row.assetTransactionId || `timeline-${row.date}-${row.transactionTypeName}-${index}`}
                      hideFooter={false}
                      ariaLabel="Asset timeline table"
                    />
                  </>
                )}
              </Card>
            </Grid>
          </>
        )}
        
        {selectedAssetId && loadingData && (
          <Grid item xs={12}>
            <Card>
              <div className="asset-tracking__loading-container">
                <Spinner size="lg" />
              </div>
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