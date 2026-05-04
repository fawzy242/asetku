import React, { useState, useEffect, useCallback } from "react";
import { FiSearch, FiBox, FiUser, FiMapPin, FiRefreshCw, FiArrowRight, FiInfo, FiCalendar, FiClock } from "react-icons/fi";
import { Grid, Box, Typography, Avatar, Chip } from "@mui/material";
import { Chrono } from "react-chrono";
import AssetTrackingData from "./AssetTracking.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Card from "../../components/atoms/Card/Card";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import { useUIStore } from "../../stores/uiStore";
import utilsHelper from "../../core/utils/utils.helper";
import "./AssetTracking.scss";

const trackingData = new AssetTrackingData();

const STATUS_CHIP_COLORS = {
  'Available': { bg: 'rgba(16, 185, 129, 0.1)', color: '#10b981' },
  'Assigned': { bg: 'rgba(59, 130, 246, 0.1)', color: '#3b82f6' },
  'Under Repair': { bg: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b' },
  'Retired': { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280' },
  'Disposed': { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280' },
};

const AssetTrackingMenu = () => {
  const [loading, setLoading] = useState(true);
  const [assets, setAssets] = useState([]);
  const [selectedAsset, setSelectedAsset] = useState(null);
  const [assetDetail, setAssetDetail] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [loadingTransactions, setLoadingTransactions] = useState(false);

  const theme = useUIStore((s) => s.theme);
  const isDark = theme === 'dark';

  useEffect(() => {
    loadInitialData();
  }, []);

  const loadInitialData = async () => {
    setLoading(true);
    const [assetsRes] = await Promise.all([trackingData.fetchAssets()]);
    if (assetsRes.success) setAssets(assetsRes.data);
    setLoading(false);
  };

  const handleAssetSelect = useCallback(async (assetId) => {
    if (!assetId) {
      setSelectedAsset(null);
      setAssetDetail(null);
      setTransactions([]);
      return;
    }
    setSelectedAsset(assetId);
    setLoadingTransactions(true);
    const [detailRes, transRes] = await Promise.all([
      trackingData.fetchAssetDetail(assetId),
      trackingData.fetchTransactions(assetId),
    ]);
    if (detailRes.success) setAssetDetail(detailRes.data);
    if (transRes.success) setTransactions(transRes.data);
    setLoadingTransactions(false);
  }, []);

  const getStatusChip = (status) => {
    const colors = STATUS_CHIP_COLORS[status] || { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280' };
    return (
      <Chip
        label={status || '-'}
        size="small"
        sx={{
          bgcolor: colors.bg,
          color: colors.color,
          fontWeight: 500,
          fontSize: '0.75rem',
          height: 24,
          borderRadius: '4px',
        }}
      />
    );
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

  const timelineItems = transactions.map((tx) => ({
    title: utilsHelper.formatDateTime(tx.transactionDate),
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
          <Typography variant="body2" component="span">
            <strong>Status:</strong>{' '}
          </Typography>
          {getStatusChip(tx.transactionStatus)}
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

  const chronologicalItems = [...timelineItems].reverse();

  const assetOptions = useCallback(() => [
    { value: "", label: "Choose an asset to track..." },
    ...assets.map(a => ({ value: a.assetId, label: `${a.assetCode} - ${a.assetName}` })),
  ], [assets]);

  const transactionColumns = [
    {
      field: "transactionDate",
      headerName: "Date",
      width: 160,
      valueFormatter: (p) => utilsHelper.formatDateTime(p.value),
    },
    { field: "transactionType", headerName: "Type", width: 130 },
    { field: "fromEmployeeName", headerName: "From", width: 160 },
    { field: "toEmployeeName", headerName: "To", width: 160 },
    {
      field: "transactionStatus",
      headerName: "Status",
      width: 120,
      renderCell: (p) => getStatusChip(p.value),
    },
    { field: "notes", headerName: "Notes", width: 200 },
  ];

  if (loading) return <div className="page-loading"><Spinner size="lg" /></div>;

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
                  options={[
                    { value: "", label: "Choose an asset to track..." },
                    ...assets.map(a => ({ value: a.assetId, label: `${a.assetCode} - ${a.assetName}` })),
                  ]}
                />
              </Box>
            </Box>
          </Card>
        </Grid>

        {selectedAsset && (
          <>
            <Grid item xs={12}>
              <Card>
                <Grid container spacing={2} alignItems="center">
                  <Grid item>
                    <Avatar sx={{ width: 64, height: 64, bgcolor: 'var(--primary)', fontSize: 28 }}>
                      <FiBox size={32} color="white" />
                    </Avatar>
                  </Grid>
                  <Grid item xs>
                    <Typography variant="h5" fontWeight={700}>{assetDetail?.assetName || '-'}</Typography>
                    <Box sx={{ display: 'flex', gap: 2, mt: 1, flexWrap: 'wrap' }}>
                      <Chip icon={<FiInfo />} label={`Code: ${assetDetail?.assetCode || '-'}`} size="small" variant="outlined" />
                      <Chip icon={<FiBox />} label={`Category: ${assetDetail?.categoryName || '-'}`} size="small" variant="outlined" />
                      <Chip icon={<FiUser />} label={`Holder: ${assetDetail?.currentHolderName || 'None'}`} size="small" variant="outlined" />
                      <Chip icon={<FiMapPin />} label={`Location: ${assetDetail?.location || '-'}`} size="small" variant="outlined" />
                      {getStatusChip(assetDetail?.status)}
                    </Box>
                  </Grid>
                  <Grid item>
                    <Box sx={{ textAlign: 'right' }}>
                      <Typography variant="body2" color="text.secondary">Purchase Date</Typography>
                      <Typography variant="body1" fontWeight={600}>
                        {utilsHelper.formatDate(assetDetail?.purchaseDate) || '-'}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>Purchase Price</Typography>
                      <Typography variant="body1" fontWeight={600} color="primary">
                        {utilsHelper.formatCurrency(assetDetail?.purchasePrice)}
                      </Typography>
                    </Box>
                  </Grid>
                </Grid>
              </Card>
            </Grid>

            <Grid item xs={12} md={4}>
              <Card title="Specifications">
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                  {[
                    ['Brand', assetDetail?.brand],
                    ['Model', assetDetail?.model],
                    ['Serial Number', assetDetail?.serialNumber],
                    ['Condition', assetDetail?.condition],
                    ['Warranty', assetDetail?.warrantyPeriod ? `${assetDetail.warrantyPeriod} months` : '-'],
                  ].map(([label, value]) => (
                    <Box key={label} sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">{label}</Typography>
                      <Typography variant="body2" fontWeight={500}>{value || '-'}</Typography>
                    </Box>
                  ))}
                </Box>
              </Card>
            </Grid>

            <Grid item xs={12} md={4}>
              <Card title="Maintenance">
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                  {[
                    ['Last Maintenance', utilsHelper.formatDate(assetDetail?.lastMaintenanceDate)],
                    ['Next Maintenance', utilsHelper.formatDate(assetDetail?.nextMaintenanceDate)],
                    ['Residual Value', utilsHelper.formatCurrency(assetDetail?.residualValue)],
                    ['Useful Life', assetDetail?.usefulLife ? `${assetDetail.usefulLife} years` : '-'],
                  ].map(([label, value]) => (
                    <Box key={label} sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">{label}</Typography>
                      <Typography variant="body2" fontWeight={500}>{value || '-'}</Typography>
                    </Box>
                  ))}
                </Box>
              </Card>
            </Grid>

            <Grid item xs={12} md={4}>
              <Card title="Depreciation">
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                  {[
                    ['Start Date', utilsHelper.formatDate(assetDetail?.depreciationStartDate)],
                    ['Purchase Price', utilsHelper.formatCurrency(assetDetail?.purchasePrice)],
                    ['Residual Value', utilsHelper.formatCurrency(assetDetail?.residualValue)],
                    ['Supplier', assetDetail?.supplierName],
                  ].map(([label, value]) => (
                    <Box key={label} sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" color="text.secondary">{label}</Typography>
                      <Typography variant="body2" fontWeight={500}>{value || '-'}</Typography>
                    </Box>
                  ))}
                </Box>
              </Card>
            </Grid>

            <Grid item xs={12}>
              <Card title={`Transaction History (${transactions.length})`}>
                {loadingTransactions ? (
                  <div className="page-loading"><Spinner size="lg" /></div>
                ) : transactions.length === 0 ? (
                  <Box sx={{ textAlign: 'center', py: 4, color: 'var(--text-secondary)' }}>
                    <FiRefreshCw size={40} style={{ marginBottom: 8 }} />
                    <Typography>No transaction history found</Typography>
                  </Box>
                ) : (
                  <Box>
                    <Box sx={{ display: { xs: 'none', md: 'block' } }}>
                      <Chrono
                        items={chronologicalItems}
                        mode="VERTICAL_ALTERNATING"
                        scrollable={true}
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
                        borderlessCards={false}
                        lineWidth={3}
                        disableClickOnCircle={false}
                        allowDynamicUpdate={true}
                      />
                    </Box>
                    <Box sx={{ display: { xs: 'block', md: 'none' } }}>
                      <DataTable
                        rows={transactions}
                        columns={transactionColumns}
                        pageSize={10}
                        getRowId={(_, i) => `tx-${i}`}
                        hideFooter={true}
                        ariaLabel="Transaction history table"
                      />
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