import React, { useState, useEffect, useCallback, useRef } from "react";
import { FiSearch, FiBox, FiUser, FiMapPin, FiRefreshCw, FiInfo, FiCalendar, FiClock, FiAlertTriangle, FiDollarSign, FiActivity } from "react-icons/fi";
import { Grid, Box, Typography, Avatar, Chip, Paper } from "@mui/material";
import { useLocation } from "react-router-dom";
import AssetTrackingData from "./AssetTracking.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Card from "../../components/atoms/Card/Card";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import utilsHelper from "../../core/utils/utils.helper";
import "./AssetTracking.scss";

const trackingData = new AssetTrackingData();

const AssetTrackingMenu = () => {
  const [loading, setLoading] = useState(true);
  const [assets, setAssets] = useState([]);
  const [selectedAsset, setSelectedAsset] = useState(null);
  const [trackingData_, setTrackingData_] = useState(null);
  const [loadingData, setLoadingData] = useState(false);
  const isMountedRef = useRef(true);
  const location = useLocation();

  useEffect(() => {
    isMountedRef.current = true;
    loadInitialData();
    return () => {
      isMountedRef.current = false;
    };
  }, []);

  const loadInitialData = async () => {
    setLoading(true);
    const assetsRes = await trackingData.fetchAssets();
    if (isMountedRef.current && assetsRes.success) {
      setAssets(assetsRes.data);
      
      // Check URL parameter for assetId after assets are loaded
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
      setSelectedAsset(null); 
      setTrackingData_(null); 
      return; 
    }
    setSelectedAsset(assetId);
    setLoadingData(true);
    
    const trackingRes = await trackingData.fetchAssetTracking(assetId);
    
    if (isMountedRef.current) {
      if (trackingRes.success) {
        setTrackingData_(trackingRes.data);
      } else {
        // Fallback: build tracking data from detail + transactions
        const [detailRes, transRes] = await Promise.all([
          trackingData.fetchAssetDetail(assetId),
          trackingData.fetchTransactions(assetId)
        ]);
        
        if (isMountedRef.current) {
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
              id: tx.assetTransactionId || `tx-${Date.now()}-${Math.random()}`,
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
      }
      setLoadingData(false);
    }
  }, []);

  // Handle URL parameter changes after component is mounted
  useEffect(() => {
    if (assets.length > 0) {
      const params = new URLSearchParams(location.search);
      const assetIdParam = params.get('assetId');
      if (assetIdParam) {
        const assetId = parseInt(assetIdParam, 10);
        const assetExists = assets.some(a => a.assetId === assetId);
        if (assetExists && selectedAsset !== assetId) {
          handleAssetSelect(assetId);
        }
      }
    }
  }, [location.search, assets, selectedAsset, handleAssetSelect]);

  const getStatusChip = (status) => (
    <Chip 
      label={status || '-'} 
      size="small" 
      sx={getStatusChipStyles(status)} 
    />
  );

  const timelineColumns = [
    { 
      field: "date", 
      headerName: "Date", 
      width: 180, 
      valueFormatter: (p) => p?.value ? utilsHelper.formatDateTime(p.value) : '-' 
    },
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
        {/* Asset Selection Card */}
        <Grid item xs={12}>
          <Card>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
              <FiSearch size={20} style={{ color: 'var(--primary)' }} />
              <Box sx={{ flex: 1, minWidth: 250 }}>
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
        
        {selectedAsset && !loadingData && trackingData_ && (
          <>
            {/* Asset Summary Card */}
            <Grid item xs={12}>
              <Card>
                <Grid container spacing={3} alignItems="center">
                  <Grid item>
                    <Avatar sx={{ width: 64, height: 64, bgcolor: 'var(--primary)', fontSize: 28 }}>
                      <FiBox size={32} color="white" />
                    </Avatar>
                  </Grid>
                  <Grid item xs>
                    <Typography variant="h5" fontWeight={700}>
                      {trackingData_?.assetName || '-'}
                    </Typography>
                    <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>
                      <Chip label={`Code: ${trackingData_?.assetCode || '-'}`} size="small" variant="outlined" />
                      <Chip label={`Category: ${trackingData_?.categoryName || '-'}`} size="small" variant="outlined" />
                      <Chip 
                        icon={<FiUser />} 
                        label={`Holder: ${trackingData_?.currentHolderName || 'None'}`} 
                        size="small" 
                        variant="outlined" 
                      />
                      <Chip 
                        icon={<FiMapPin />} 
                        label={`Office: ${trackingData_?.currentLocation || '-'}`} 
                        size="small" 
                        variant="outlined" 
                      />
                      {getStatusChip(trackingData_?.currentStatus)}
                      {trackingData_?.isOnLoan && (
                        <Chip icon={<FiClock />} label="On Loan" size="small" color="warning" variant="outlined" />
                      )}
                      {trackingData_?.isInMaintenance && (
                        <Chip icon={<FiActivity />} label="In Maintenance" size="small" color="info" variant="outlined" />
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
            <Grid item xs={12}>
              <Grid container spacing={2}>
                <Grid item xs={6} sm={3} md={3}>
                  <Paper elevation={0} sx={{ p: 2, textAlign: 'center', bgcolor: 'var(--card-bg)', borderRadius: 2, border: '1px solid var(--border)', height: '100%' }}>
                    <FiRefreshCw size={24} style={{ color: 'var(--primary)', marginBottom: 8 }} />
                    <Typography variant="h4" fontWeight={700}>{trackingData_?.totalTransactions || 0}</Typography>
                    <Typography variant="caption" color="text.secondary">Transactions</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3} md={3}>
                  <Paper elevation={0} sx={{ p: 2, textAlign: 'center', bgcolor: 'var(--card-bg)', borderRadius: 2, border: '1px solid var(--border)', height: '100%' }}>
                    <FiUser size={24} style={{ color: trackingData_?.isOnLoan ? '#8b5cf6' : 'var(--success)', marginBottom: 8 }} />
                    <Typography variant="body2" fontWeight={600} sx={{ wordBreak: 'break-word' }}>
                      {trackingData_?.currentHolderName || 'None'}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">Holder</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3} md={3}>
                  <Paper elevation={0} sx={{ p: 2, textAlign: 'center', bgcolor: 'var(--card-bg)', borderRadius: 2, border: '1px solid var(--border)', height: '100%' }}>
                    <FiMapPin size={24} style={{ color: 'var(--info)', marginBottom: 8 }} />
                    <Typography variant="body2" fontWeight={600} sx={{ wordBreak: 'break-word' }}>
                      {trackingData_?.currentLocation || '-'}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">Office</Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={3} md={3}>
                  <Paper elevation={0} sx={{ p: 2, textAlign: 'center', bgcolor: 'var(--card-bg)', borderRadius: 2, border: '1px solid var(--border)', height: '100%' }}>
                    <FiActivity size={24} style={{ color: 'var(--warning)', marginBottom: 8 }} />
                    {getStatusChip(trackingData_?.currentStatus)}
                    <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>Status</Typography>
                  </Paper>
                </Grid>
              </Grid>
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
                  <div style={{ width: '100%', minWidth: 0 }}>
                    <DataTable 
                      rows={timeline} 
                      columns={timelineColumns} 
                      pageSize={15} 
                      getRowId={(row, index) => {
                        if (row.id) return `timeline-${row.id}`;
                        if (row.assetTransactionId) return `timeline-${row.assetTransactionId}`;
                        return `timeline-${row.date}-${row.activityType}-${index}`;
                      }} 
                      hideFooter={true} 
                      autoHeight={true}
                      ariaLabel="Asset timeline table" 
                    />
                  </div>
                )}
              </Card>
            </Grid>
          </>
        )}
        
        {selectedAsset && loadingData && (
          <Grid item xs={12}>
            <Card>
              <div className="page-loading"><Spinner size="lg" /></div>
            </Card>
          </Grid>
        )}
      </Grid>
    </div>
  );
};

export default AssetTrackingMenu;