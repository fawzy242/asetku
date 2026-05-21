import React, { useState, useEffect, useCallback } from "react";
import { FiSearch, FiBox, FiUser, FiMapPin, FiRefreshCw, FiInfo, FiCalendar, FiClock, FiAlertTriangle, FiDollarSign } from "react-icons/fi";
import { Grid, Box, Typography, Avatar, Chip } from "@mui/material";
import AssetTrackingData from "./AssetTracking.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Card from "../../components/atoms/Card/Card";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import utilsHelper from "../../core/utils/utils.helper";
import "./AssetTracking.scss";

/**
 * ============================================================
 * FUTURE BACKEND ENDPOINTS NEEDED FOR ASSET TRACKING
 * ============================================================
 * 
 * 1. GET /api/Asset/{id}/timeline
 *    Returns complete timeline/history for a single asset.
 *    Query: ?startDate=&endDate=&transactionType=
 *    Response: Array of timeline events with date, activityType, description,
 *              previousHolder, newHolder, previousStatus, newStatus, notes
 * 
 * 2. GET /api/Asset/tracking-list
 *    Returns lightweight tracking list for asset selector.
 *    Query: ?search=&status=&categoryId=&holderId=&page=&pageSize=
 *    Response: Paginated list with assetId, assetCode, assetName, currentStatus
 * 
 * Currently using fallback: /api/Asset/tracking/{id} + /api/AssetTransaction/asset/{id}
 * ============================================================
 */

const trackingData = new AssetTrackingData();

const AssetTrackingMenu = () => {
  const [loading, setLoading] = useState(true);
  const [assets, setAssets] = useState([]);
  const [selectedAsset, setSelectedAsset] = useState(null);
  const [trackingData_, setTrackingData_] = useState(null);
  const [loadingData, setLoadingData] = useState(false);

  useEffect(() => { loadInitialData(); }, []);

  const loadInitialData = async () => {
    setLoading(true);
    const [assetsRes] = await Promise.all([trackingData.fetchAssets()]);
    if (assetsRes.success) setAssets(assetsRes.data);
    setLoading(false);
  };

  const handleAssetSelect = useCallback(async (assetId) => {
    if (!assetId) { 
      setSelectedAsset(null); 
      setTrackingData_(null); 
      return; 
    }
    setSelectedAsset(assetId);
    setLoadingData(true);
    
    // Try dedicated tracking endpoint first
    const trackingRes = await trackingData.fetchAssetTracking(assetId);
    if (trackingRes.success) {
      setTrackingData_(trackingRes.data);
    } else {
      // Fallback: build tracking data from detail + transactions
      const [detailRes, transRes] = await Promise.all([
        trackingData.fetchAssetDetail(assetId),
        trackingData.fetchTransactions(assetId)
      ]);
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
        totalTransactions: (transRes.data || []).length,
        timeline: (transRes.data || []).map(tx => ({
          date: tx.transactionDate,
          activityType: tx.transactionTypeName,
          description: tx.notes || `Transaction: ${tx.transactionTypeName}`,
          previousHolder: tx.fromEmployeeName,
          newHolder: tx.toEmployeeName,
          previousStatus: tx.conditionBeforeName,
          newStatus: tx.conditionAfterName,
          notes: tx.notes
        }))
      });
    }
    setLoadingData(false);
  }, []);

  const getStatusChip = (status) => (
    <Chip 
      label={status || '-'} 
      size="small" 
      sx={getStatusChipStyles(status)} 
    />
  );

  const timelineColumns = [
    { field: "date", headerName: "Date", width: 160, valueFormatter: (p) => p?.value ? utilsHelper.formatDateTime(p.value) : '-' },
    { field: "activityType", headerName: "Activity", width: 160 },
    { field: "description", headerName: "Description", flex: 1, minWidth: 200 },
    { field: "previousHolder", headerName: "Previous Holder", width: 150 },
    { field: "newHolder", headerName: "New Holder", width: 150 },
    { field: "newStatus", headerName: "Status", width: 120 },
  ];

  if (loading) return <div className="page-loading"><Spinner size="lg" /></div>;

  const timeline = trackingData_?.timeline || [];
  const assetOptions = assets.map(a => ({ 
    value: a.assetId, 
    label: `${a.assetCode} - ${a.assetName}` 
  }));

  return (
    <div className="asset-tracking">
      <div className="page-header">
        <h1 className="page-title">Asset Tracking</h1>
      </div>
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Card>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <FiSearch size={20} style={{ color: 'var(--primary)' }} />
              <Box sx={{ flex: 1, maxWidth: 500 }}>
                <Select 
                  label="Select Asset" 
                  value={selectedAsset || ""} 
                  onChange={e => handleAssetSelect(e.target.value)}
                  options={[{ value: "", label: "Choose an asset to track..." }, ...assetOptions]} 
                />
              </Box>
            </Box>
          </Card>
        </Grid>
        
        {selectedAsset && (
          <>
            {/* Asset Summary Card */}
            <Grid item xs={12}>
              <Card>
                <Grid container spacing={2} alignItems="center">
                  <Grid item>
                    <Avatar sx={{ width: 64, height: 64, bgcolor: 'var(--primary)', fontSize: 28 }}>
                      <FiBox size={32} color="white" />
                    </Avatar>
                  </Grid>
                  <Grid item xs>
                    <Typography variant="h5" fontWeight={700}>
                      {trackingData_?.assetName || '-'}
                    </Typography>
                    <Box sx={{ display: 'flex', gap: 2, mt: 1, flexWrap: 'wrap' }}>
                      <Chip icon={<FiInfo />} label={`Code: ${trackingData_?.assetCode || '-'}`} size="small" variant="outlined" />
                      <Chip icon={<FiBox />} label={`Category: ${trackingData_?.categoryName || '-'}`} size="small" variant="outlined" />
                      <Chip icon={<FiUser />} label={`Holder: ${trackingData_?.currentHolderName || 'None'}`} size="small" variant="outlined" />
                      <Chip icon={<FiMapPin />} label={`Office: ${trackingData_?.currentLocation || '-'}`} size="small" variant="outlined" />
                      {getStatusChip(trackingData_?.currentStatus)}
                      {trackingData_?.isOnLoan && (
                        <Chip icon={<FiClock />} label="On Loan" size="small" color="warning" variant="outlined" />
                      )}
                      {trackingData_?.isInMaintenance && (
                        <Chip icon={<FiClock />} label="In Maintenance" size="small" color="info" variant="outlined" />
                      )}
                      {trackingData_?.isOverdue && (
                        <Chip icon={<FiAlertTriangle />} label="Overdue" size="small" color="error" />
                      )}
                    </Box>
                  </Grid>
                  <Grid item>
                    <Box sx={{ textAlign: 'right' }}>
                      <Typography variant="body2" color="text.secondary">Condition</Typography>
                      <Typography variant="body1" fontWeight={600}>
                        {trackingData_?.condition || '-'}
                      </Typography>
                      {trackingData_?.loanDueDate && (
                        <>
                          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>Loan Due</Typography>
                          <Typography 
                            variant="body1" 
                            fontWeight={600} 
                            color={trackingData_?.isOverdue ? 'var(--error)' : 'var(--text-primary)'}
                          >
                            {utilsHelper.formatDate(trackingData_?.loanDueDate)}
                          </Typography>
                        </>
                      )}
                    </Box>
                  </Grid>
                </Grid>
              </Card>
            </Grid>

            {/* Statistics Cards */}
            <Grid item xs={12} sm={6} md={3}>
              <Card className="summary-stat-card">
                <FiRefreshCw size={28} style={{ color: 'var(--primary)' }} />
                <Typography variant="h4" fontWeight={700}>{trackingData_?.totalTransactions || 0}</Typography>
                <Typography variant="body2" color="text.secondary">Transactions</Typography>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card className="summary-stat-card">
                <FiUser size={28} style={{ color: trackingData_?.isOnLoan ? '#8b5cf6' : 'var(--success)' }} />
                <Typography variant="h4" fontWeight={700}>{trackingData_?.currentHolderName || 'None'}</Typography>
                <Typography variant="body2" color="text.secondary">Holder</Typography>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card className="summary-stat-card">
                <FiMapPin size={28} style={{ color: 'var(--info)' }} />
                <Typography variant="h4" fontWeight={700}>{trackingData_?.currentLocation || '-'}</Typography>
                <Typography variant="body2" color="text.secondary">Office</Typography>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card className="summary-stat-card">
                <FiBox size={28} style={{ color: 'var(--warning)' }} />
                {getStatusChip(trackingData_?.currentStatus)}
                <Typography variant="body2" color="text.secondary">Status</Typography>
              </Card>
            </Grid>

            {/* Timeline Table */}
            <Grid item xs={12}>
              <Card title={`Transaction Timeline (${timeline.length})`}>
                {loadingData ? (
                  <div className="page-loading"><Spinner size="lg" /></div>
                ) : timeline.length === 0 ? (
                  <Box sx={{ textAlign: 'center', py: 4, color: 'var(--text-secondary)' }}>
                    <FiRefreshCw size={40} style={{ marginBottom: 8 }} />
                    <Typography>No transaction history found</Typography>
                  </Box>
                ) : (
                  <DataTable 
                    rows={timeline} 
                    columns={timelineColumns} 
                    pageSize={15} 
                    getRowId={(_, i) => `timeline-${i}`} 
                    hideFooter={true} 
                    ariaLabel="Asset timeline table" 
                  />
                )}
              </Card>
            </Grid>
          </>
        )}
      </Grid>
    </div>
  );
};

export default AssetTrackingMenu;