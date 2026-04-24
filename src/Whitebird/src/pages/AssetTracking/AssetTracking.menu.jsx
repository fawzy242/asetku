import React, { useState, useEffect } from "react";
import { FiSearch, FiBox, FiUser, FiMapPin, FiCalendar, FiRefreshCw, FiClock, FiArrowRight, FiInfo } from "react-icons/fi";
import { Grid, Box, Typography, Chip } from "@mui/material";
import { Chrono } from "react-chrono";
import AssetTrackingData from "./AssetTracking.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Card from "../../components/atoms/Card/Card";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import utilsHelper from "../../core/utils/utils.helper";
import "./AssetTracking.scss";

const trackingData = new AssetTrackingData();

const AssetTrackingMenu = () => {
  const [loading, setLoading] = useState(true);
  const [assets, setAssets] = useState([]);
  const [selectedAsset, setSelectedAsset] = useState(null);
  const [assetDetail, setAssetDetail] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [loadingTransactions, setLoadingTransactions] = useState(false);
  const [isDark, setIsDark] = useState(() => document.documentElement.getAttribute("data-theme") === "dark");

  useEffect(() => {
    loadInitialData();
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.attributeName === "data-theme") {
          setIsDark(document.documentElement.getAttribute("data-theme") === "dark");
        }
      });
    });
    observer.observe(document.documentElement, { attributes: true });
    return () => observer.disconnect();
  }, []);

  const loadInitialData = async () => {
    setLoading(true);
    const [assetsRes] = await Promise.all([trackingData.fetchAssets()]);
    if (assetsRes.success) setAssets(assetsRes.data);
    setLoading(false);
  };

  const handleAssetSelect = async (assetId) => {
    if (!assetId) { setSelectedAsset(null); setAssetDetail(null); setTransactions([]); return; }
    setSelectedAsset(assetId);
    setLoadingTransactions(true);
    const [detailRes, transRes] = await Promise.all([
      trackingData.fetchAssetDetail(assetId),
      trackingData.fetchTransactions(assetId)
    ]);
    if (detailRes.success) setAssetDetail(detailRes.data);
    if (transRes.success) setTransactions(transRes.data);
    setLoadingTransactions(false);
  };

  const getStatusColor = (status) => {
    const colors = { Pending: '#f59e0b', Approved: '#10b981', Rejected: '#ef4444', Completed: '#3b82f6', Cancelled: '#6b7280' };
    return colors[status] || '#6b7280';
  };

  const getTransactionIcon = (type) => {
    switch (type) {
      case 'Assignment': return '👤';
      case 'Return': return '🔄';
      case 'Maintenance': return '🔧';
      case 'Transfer': return '➡️';
      default: return '📋';
    }
  };

  // Build timeline items
  const timelineItems = transactions.map((tx) => ({
    title: `${utilsHelper.formatDateTime(tx.transactionDate)}`,
    cardTitle: `${getTransactionIcon(tx.transactionType)} ${tx.transactionType}`,
    cardSubtitle: (
      <Box sx={{ mt: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
          <FiUser size={14} />
          <Typography variant="body2">
            <strong>From:</strong> {tx.fromEmployeeName || 'System'}
            {tx.fromLocationName ? ` (${tx.fromLocationName})` : ''}
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
          <FiArrowRight size={14} />
          <Typography variant="body2">
            <strong>To:</strong> {tx.toEmployeeName || 'None'}
            {tx.toLocationName ? ` (${tx.toLocationName})` : ''}
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
          <FiInfo size={14} />
          <Typography variant="body2">
            <strong>Status:</strong> {' '}
            <Chip 
              label={tx.transactionStatus} 
              size="small" 
              sx={{ 
                bgcolor: getStatusColor(tx.transactionStatus) + '20', 
                color: getStatusColor(tx.transactionStatus), 
                fontWeight: 500, 
                fontSize: 11,
                height: 20
              }} 
            />
          </Typography>
        </Box>
        {tx.expectedReturnDate && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
            <FiClock size={14} />
            <Typography variant="caption" color="text.secondary">
              Expected Return: {utilsHelper.formatDate(tx.expectedReturnDate)}
            </Typography>
          </Box>
        )}
        {tx.actualReturnDate && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
            <FiClock size={14} />
            <Typography variant="caption" color="text.secondary">
              Actual Return: {utilsHelper.formatDate(tx.actualReturnDate)}
            </Typography>
          </Box>
        )}
        {tx.conditionBefore && tx.conditionAfter && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
            <FiBox size={14} />
            <Typography variant="caption" color="text.secondary">
              Condition: {tx.conditionBefore} → {tx.conditionAfter}
            </Typography>
          </Box>
        )}
        {tx.notes && (
          <Box sx={{ mt: 1, p: 1, bgcolor: 'var(--surface)', borderRadius: 1 }}>
            <Typography variant="caption" color="text.secondary">📝 {tx.notes}</Typography>
          </Box>
        )}
      </Box>
    ),
  }));

  // Reverse untuk tampilan kronologis (terbaru di atas)
  const chronologicalItems = [...timelineItems].reverse();

  const assetOptions = assets.map(a => ({ value: a.assetId, label: `${a.assetCode} - ${a.assetName}` }));

  const transactionColumns = [
    { field: "transactionDate", headerName: "Date", width: 160, valueFormatter: (p) => utilsHelper.formatDateTime(p.value) },
    { field: "transactionType", headerName: "Type", width: 130 },
    { field: "fromEmployeeName", headerName: "From", width: 160 },
    { field: "toEmployeeName", headerName: "To", width: 160 },
    { field: "transactionStatus", headerName: "Status", width: 110, renderCell: (p) => <Chip label={p.value} size="small" sx={{ bgcolor: getStatusColor(p.value) + '20', color: getStatusColor(p.value), fontWeight: 500 }} /> },
    { field: "notes", headerName: "Notes", width: 200 },
  ];

  if (loading) return <div className="tracking-loading"><Spinner size="lg" /></div>;

  return (
    <div className="asset-tracking">
      <div className="page-header">
        <h1 className="page-title">Asset Tracking</h1>
      </div>

      <Grid container spacing={3}>
        {/* Asset Selector */}
        <Grid item xs={12}>
          <Card>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <FiSearch size={20} style={{ color: 'var(--primary)' }} />
              <Box sx={{ flex: 1, maxWidth: 500 }}>
                <Select
                  label="Select Asset"
                  value={selectedAsset || ""}
                  onChange={e => handleAssetSelect(e.target.value)}
                  options={[
                    { value: "", label: "Choose an asset to track..." },
                    ...assetOptions
                  ]}
                  placeholder="Choose an asset to track..."
                />
              </Box>
            </Box>
          </Card>
        </Grid>

        {selectedAsset && (
          <>
            {/* Asset Info Card */}
            <Grid item xs={12}>
              <Card>
                <Grid container spacing={2} alignItems="center">
                  <Grid item>
                    <Box sx={{ width: 64, height: 64, borderRadius: 2, bgcolor: 'var(--primary)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                      <FiBox size={32} color="white" />
                    </Box>
                  </Grid>
                  <Grid item xs>
                    <Typography variant="h5" fontWeight={700}>{assetDetail?.assetName || '-'}</Typography>
                    <Box sx={{ display: 'flex', gap: 2, mt: 1, flexWrap: 'wrap' }}>
                      <Chip icon={<FiInfo />} label={`Code: ${assetDetail?.assetCode || '-'}`} size="small" variant="outlined" />
                      <Chip icon={<FiBox />} label={`Category: ${assetDetail?.categoryName || '-'}`} size="small" variant="outlined" />
                      <Chip icon={<FiUser />} label={`Holder: ${assetDetail?.currentHolderName || 'None'}`} size="small" variant="outlined" />
                      <Chip icon={<FiMapPin />} label={`Location: ${assetDetail?.location || '-'}`} size="small" variant="outlined" />
                      <Chip 
                        label={assetDetail?.status || '-'} 
                        size="small" 
                        sx={{ bgcolor: utilsHelper.getStatusColor(assetDetail?.status) === 'success' ? '#10b98120' : '#f59e0b20', color: utilsHelper.getStatusColor(assetDetail?.status) === 'success' ? '#10b981' : '#f59e0b' }} 
                      />
                    </Box>
                  </Grid>
                  <Grid item>
                    <Box sx={{ textAlign: 'right' }}>
                      <Typography variant="body2" color="text.secondary">Purchase Date</Typography>
                      <Typography variant="body1" fontWeight={600}>{utilsHelper.formatDate(assetDetail?.purchaseDate) || '-'}</Typography>
                      <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>Purchase Price</Typography>
                      <Typography variant="body1" fontWeight={600} color="primary">{utilsHelper.formatCurrency(assetDetail?.purchasePrice)}</Typography>
                    </Box>
                  </Grid>
                </Grid>
              </Card>
            </Grid>

            {/* Asset Specifications */}
            <Grid item xs={12} md={4}>
              <Card title="Specifications">
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Brand</Typography><Typography variant="body2" fontWeight={500}>{assetDetail?.brand || '-'}</Typography></Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Model</Typography><Typography variant="body2" fontWeight={500}>{assetDetail?.model || '-'}</Typography></Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Serial Number</Typography><Typography variant="body2" fontWeight={500}>{assetDetail?.serialNumber || '-'}</Typography></Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Condition</Typography><Typography variant="body2" fontWeight={500}>{assetDetail?.condition || '-'}</Typography></Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Warranty</Typography><Typography variant="body2" fontWeight={500}>{assetDetail?.warrantyPeriod ? `${assetDetail.warrantyPeriod} months` : '-'}</Typography></Box>
                </Box>
              </Card>
            </Grid>

            {/* Maintenance Info */}
            <Grid item xs={12} md={4}>
              <Card title="Maintenance">
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Last Maintenance</Typography><Typography variant="body2" fontWeight={500}>{utilsHelper.formatDate(assetDetail?.lastMaintenanceDate) || '-'}</Typography></Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Next Maintenance</Typography><Typography variant="body2" fontWeight={500} color={assetDetail?.nextMaintenanceDate && new Date(assetDetail.nextMaintenanceDate) < new Date() ? 'error' : 'text.primary'}>{utilsHelper.formatDate(assetDetail?.nextMaintenanceDate) || '-'}</Typography></Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Residual Value</Typography><Typography variant="body2" fontWeight={500}>{utilsHelper.formatCurrency(assetDetail?.residualValue)}</Typography></Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Useful Life</Typography><Typography variant="body2" fontWeight={500}>{assetDetail?.usefulLife ? `${assetDetail.usefulLife} years` : '-'}</Typography></Box>
                </Box>
              </Card>
            </Grid>

            {/* Depreciation Info */}
            <Grid item xs={12} md={4}>
              <Card title="Depreciation">
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Start Date</Typography><Typography variant="body2" fontWeight={500}>{utilsHelper.formatDate(assetDetail?.depreciationStartDate) || '-'}</Typography></Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Purchase Price</Typography><Typography variant="body2" fontWeight={500}>{utilsHelper.formatCurrency(assetDetail?.purchasePrice)}</Typography></Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Residual Value</Typography><Typography variant="body2" fontWeight={500}>{utilsHelper.formatCurrency(assetDetail?.residualValue)}</Typography></Box>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="body2" color="text.secondary">Supplier</Typography><Typography variant="body2" fontWeight={500}>{assetDetail?.supplierName || '-'}</Typography></Box>
                </Box>
              </Card>
            </Grid>

            {/* Transaction History - TIMELINE */}
            <Grid item xs={12}>
              <Card title={`Transaction History (${transactions.length})`}>
                {loadingTransactions ? (
                  <div className="tracking-loading"><Spinner size="lg" /></div>
                ) : transactions.length === 0 ? (
                  <Box sx={{ textAlign: 'center', py: 4, color: 'var(--text-secondary)' }}>
                    <FiRefreshCw size={40} style={{ marginBottom: 8 }} />
                    <Typography>No transaction history found</Typography>
                  </Box>
                ) : (
                  <Box>
                    {/* Desktop: Chrono Timeline */}
                    <Box sx={{ display: { xs: 'none', md: 'block' } }}>
                      <Chrono
                        items={chronologicalItems}
                        mode="VERTICAL_ALTERNATING"
                        scrollable={{ scrollbar: true }}
                        cardHeight={50}
                        cardWidth={450}
                        timelinePointShape="circle"
                        theme={{
                          primary: '#dc2626',
                          secondary: isDark ? '#1f2937' : '#ffffff',
                          cardBgColor: isDark ? '#1f2937' : '#ffffff',
                          cardForeColor: isDark ? '#f9fafb' : '#111827',
                          titleColor: isDark ? '#f9fafb' : '#111827',
                          titleColorActive: '#dc2626',
                        }}
                        fontSizes={{
                          cardSubtitle: '0.8rem',
                          cardText: '0.75rem',
                          cardTitle: '0.85rem',
                          title: '0.75rem',
                        }}
                        classNames={{
                          card: 'chrono-card',
                          cardMedia: 'chrono-card-media',
                          cardSubTitle: 'chrono-card-subtitle',
                          cardText: 'chrono-card-text',
                          cardTitle: 'chrono-card-title',
                          controls: 'chrono-controls',
                          title: 'chrono-title',
                        }}
                        slideShow
                        slideItemDuration={3000}
                        slideShowType="reveal"
                        hideControls={false}
                        disableToolbar={false}
                        borderLessCards={false}
                        lineWidth={3}
                        disableClickOnCircle={false}
                        enableDarkToggle={false}
                        allowDynamicUpdate={true}
                      />
                    </Box>

                    {/* Mobile: Table View */}
                    <Box sx={{ display: { xs: 'block', md: 'none' } }}>
                      <DataTable rows={transactions} columns={transactionColumns} pageSize={10} getRowId={(row, i) => i} hideFooter={true} />
                    </Box>
                  </Box>
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