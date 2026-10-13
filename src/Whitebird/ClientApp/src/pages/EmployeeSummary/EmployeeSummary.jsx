// src/pages/EmployeeSummary/EmployeeSummary.jsx
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  FiSearch,
  FiUser,
  FiBox,
  FiDollarSign,
  FiCalendar,
  FiMail,
  FiPhone,
  FiBriefcase,
  FiRefreshCw,
  FiAlertTriangle,
  FiLayers,
  FiTool,
  FiPieChart,
} from 'react-icons/fi';
import {
  Grid,
  Box,
  Typography,
  Avatar,
  Chip,
  Paper,
  IconButton,
  Tooltip,
} from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useLocation } from 'react-router-dom';
import { EmployeeSummaryData } from '../../core/services';
import DataTable from '../../components/molecules/DataTable/DataTable';
import Card from '../../components/atoms/Card/Card';
import Select from '../../components/atoms/Select/Select';
import Spinner from '../../components/atoms/Spinner/Spinner';
import Skeleton from '../../components/atoms/Skeleton/Skeleton';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import InfoRow from '../../components/molecules/InfoRow/InfoRow';
import { useOptions } from '../../hooks/useOptions';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { formatDate, formatDateTime } from '../../core/utils/date.utils';
import { formatCurrency } from '../../core/utils/number.utils';
import './EmployeeSummary.scss';

const summaryData = new EmployeeSummaryData();

const assetColumns = [
  { field: 'assetCode', headerName: 'Asset Code', width: 130 },
  { field: 'assetName', headerName: 'Asset Name', flex: 1, minWidth: 180 },
  { field: 'categoryName', headerName: 'Category', width: 150 },
  {
    field: 'status',
    headerName: 'Status',
    width: 140,
    renderCell: (p) => <StatusChip status={p?.value || '-'} />,
  },
  { field: 'associationType', headerName: 'Type', width: 110 },
  {
    field: 'sinceDate',
    headerName: 'Since',
    width: 120,
    renderCell: (params) => {
      const value = params?.row?.sinceDate || params?.value;
      if (!value) return <span className="u-text-muted">-</span>;
      return <span>{formatDate(value)}</span>;
    },
  },
  {
    field: 'expectedReturnDate',
    headerName: 'Expected Return',
    width: 130,
    renderCell: (params) => {
      const value = params?.row?.expectedReturnDate || params?.value;
      if (!value) return <span className="u-text-muted">-</span>;
      return <span>{formatDate(value)}</span>;
    },
  },
  {
    field: 'isOverdue',
    headerName: 'Overdue',
    width: 90,
    renderCell: (p) =>
      p?.value ? (
        <Chip label="Yes" size="small" className="employee-summary__chip-overdue" />
      ) : (
        <Chip label="No" size="small" className="employee-summary__chip-ontime" />
      ),
  },
  { field: 'conditionName', headerName: 'Condition', width: 100 },
];

const historyColumns = [
  {
    field: 'transactionDate',
    headerName: 'Date',
    width: 180,
    renderCell: (params) => {
      const value = params?.row?.transactionDate || params?.value;
      if (!value) return <span className="u-text-muted">-</span>;
      return <span>{formatDateTime(value)}</span>;
    },
  },
  {
    field: 'transactionTypeName',
    headerName: 'Type',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      const value =
        row.transactionTypeName || row.typeName || row.transactionType || '-';
      return <span>{value}</span>;
    },
  },
  { field: 'assetCode', headerName: 'Asset', width: 130 },
  { field: 'assetName', headerName: 'Asset Name', flex: 1, minWidth: 180 },
  {
    field: 'employeeName',
    headerName: 'Employee',
    width: 150,
    renderCell: (params) => {
      const row = params?.row || {};
      return <span>{row.employeeName || row.employee || '-'}</span>;
    },
  },
  {
    field: 'conditionAfterName',
    headerName: 'Condition',
    width: 100,
    renderCell: (params) => {
      const row = params?.row || {};
      return (
        <span>
          {row.conditionAfterName || row.conditionAfter || row.condition || '-'}
        </span>
      );
    },
  },
  {
    field: 'notes',
    headerName: 'Notes',
    width: 200,
    renderCell: (params) => {
      const row = params?.row || {};
      return <span>{row.notes || row.note || '-'}</span>;
    },
  },
];

const EmployeeSummary = () => {
  const [selectedEmployeeId, setSelectedEmployeeId] = useState(null);
  const location = useLocation();
  const { toast } = useSweetAlert();
  const queryClient = useQueryClient();

  // Employee dropdown
  const { data: employees = [], isLoading: loading } = useQuery({
    queryKey: ['employee-summary-list'],
    queryFn: async () => {
      const r = await summaryData.fetchEmployees();
      return r.success ? r.data : [];
    },
    staleTime: 5 * 60 * 1000,
  });

  // Employee detail
  const { data: employeeDetail = null, isLoading: detailLoading } = useQuery({
    queryKey: ['employee-summary-detail', selectedEmployeeId],
    queryFn: async () => {
      const r = await summaryData.fetchEmployeeDetail(selectedEmployeeId);
      return r.success ? r.data : null;
    },
    enabled: !!selectedEmployeeId,
    staleTime: 60 * 1000,
  });

  // Asset summary
  const { data: assetSummary = null, isLoading: summaryLoading } = useQuery({
    queryKey: ['employee-summary-assets', selectedEmployeeId],
    queryFn: async () => {
      const r = await summaryData.fetchAssetSummary(selectedEmployeeId);
      if (!r.success) return null;
      const data = r.data;
      return {
        currentlyHeldAssets: data.currentlyHeldAssets || 0,
        assetsOnLoan: data.assetsOnLoan || 0,
        overdueLoans: data.overdueLoans || 0,
        totalHistoricalAssets: data.totalHistoricalAssets || 0,
        returnedAssets: data.returnedAssets || 0,
        damagedReturns: data.damagedReturns || 0,
        currentAssets: data.currentAssets || [],
        assetHistory: data.assetHistory || [],
      };
    },
    enabled: !!selectedEmployeeId,
    staleTime: 60 * 1000,
  });

  const loadingData = detailLoading || summaryLoading;

  // URL param preselect
  useEffect(() => {
    if (!employees || employees.length === 0) return;
    const params = new URLSearchParams(location.search);
    const employeeIdParam = params.get('employeeId');
    if (!employeeIdParam) return;
    const employeeId = parseInt(employeeIdParam, 10);
    if (isNaN(employeeId)) return;
    const exists = employees.some((e) => e.employeeId === employeeId);
    if (exists && selectedEmployeeId !== employeeId) {
      setSelectedEmployeeId(employeeId);
    }
  }, [employees, location.search, selectedEmployeeId]);

  const handleEmployeeSelect = useCallback((employeeId) => {
    if (!employeeId) {
      setSelectedEmployeeId(null);
      return;
    }
    setSelectedEmployeeId(employeeId);
  }, []);

  const handleRefresh = useCallback(() => {
    if (!selectedEmployeeId) return;
    queryClient.invalidateQueries({
      queryKey: ['employee-summary-detail', selectedEmployeeId],
    });
    queryClient.invalidateQueries({
      queryKey: ['employee-summary-assets', selectedEmployeeId],
    });
    toast.success('Data refreshed');
  }, [selectedEmployeeId, queryClient, toast]);

  const currentAssets = assetSummary?.currentAssets || [];
  const assetHistory = assetSummary?.assetHistory || [];

  const totalAssetValue = useMemo(
    () =>
      currentAssets.reduce(
        (sum, a) => sum + (a.purchasePrice || 0),
        0
      ),
    [currentAssets]
  );

  const categorySummary = useMemo(() => {
    if (currentAssets.length === 0) return [];
    const categoryMap = {};
    currentAssets.forEach((asset) => {
      const category = asset.categoryName || 'Uncategorized';
      categoryMap[category] = (categoryMap[category] || 0) + 1;
    });
    return Object.entries(categoryMap)
      .map(([category, count]) => ({ category, count }))
      .sort((a, b) => b.count - a.count);
  }, [currentAssets]);

  const employeeOptions = useOptions(
    Array.isArray(employees)
      ? employees.map((e) => ({
          value: e.employeeId,
          label: `${e.employeeCode} - ${e.fullName}`,
        }))
      : [],
    'Choose an employee...'
  );

  if (loading) {
    return (
      <div className="employee-summary">
        <div className="page-header">
          <h1 className="page-title">Employee Summary</h1>
          <p className="page-description">
            View asset assignments and transaction history by employee
          </p>
        </div>
        <div className="page-loading">
          <Spinner size="lg" />
        </div>
      </div>
    );
  }

  return (
    <div className="employee-summary">
      <div className="page-header">
        <h1 className="page-title">Employee Summary</h1>
        <p className="page-description">
          View asset assignments and transaction history by employee
        </p>
      </div>

      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Card className="employee-summary__selection-card">
            <div className="employee-summary__selection-content">
              <div className="employee-summary__selection-icon">
                <FiSearch size={24} />
              </div>
              <div className="employee-summary__selection-select">
                <Select
                  label="Select Employee"
                  value={selectedEmployeeId || ''}
                  onChange={(e) => handleEmployeeSelect(e.target.value)}
                  options={employeeOptions}
                />
              </div>
              {selectedEmployeeId && (
                <Tooltip title="Refresh">
                  <IconButton
                    onClick={handleRefresh}
                    size="small"
                    className="employee-summary__refresh-btn"
                  >
                    <FiRefreshCw size={18} />
                  </IconButton>
                </Tooltip>
              )}
            </div>
          </Card>
        </Grid>

        {selectedEmployeeId && !loadingData && employeeDetail && (
          <>
            <Grid item xs={12}>
              <Card className="employee-summary__profile-card">
                <Grid container spacing={3} alignItems="center">
                  <Grid item>
                    <Avatar className="employee-summary__profile-avatar">
                      {employeeDetail?.fullName?.charAt(0) || '?'}
                    </Avatar>
                  </Grid>
                  <Grid item xs>
                    <Typography
                      variant="h5"
                      fontWeight={700}
                      className="employee-summary__profile-name"
                    >
                      {employeeDetail?.fullName || '-'}
                    </Typography>
                    <div className="employee-summary__profile-tags">
                      <Chip
                        icon={<FiBriefcase />}
                        label={employeeDetail?.employeeCode || '-'}
                        size="small"
                        variant="outlined"
                      />
                      <Chip
                        icon={<FiBriefcase />}
                        label={employeeDetail?.positionName || '-'}
                        size="small"
                        variant="outlined"
                      />
                      <Chip
                        icon={<FiBriefcase />}
                        label={employeeDetail?.departmentName || '-'}
                        size="small"
                        variant="outlined"
                      />
                      <Chip
                        icon={<FiMail />}
                        label={employeeDetail?.email || '-'}
                        size="small"
                        variant="outlined"
                      />
                      <Chip
                        icon={<FiPhone />}
                        label={employeeDetail?.phoneNumber || '-'}
                        size="small"
                        variant="outlined"
                      />
                      <StatusChip status={employeeDetail?.employmentStatusName} />
                    </div>
                  </Grid>
                  <Grid item>
                    <InfoRow
                      icon={FiCalendar}
                      label="Join Date"
                      value={formatDate(employeeDetail?.joinDate)}
                    />
                    {employeeDetail?.resignDate && (
                      <InfoRow
                        icon={FiCalendar}
                        label="Resign Date"
                        value={formatDate(employeeDetail?.resignDate)}
                      />
                    )}
                  </Grid>
                </Grid>
              </Card>
            </Grid>

            <Grid item xs={12}>
              <Grid container spacing={2}>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiBox
                      size={28}
                      className="employee-summary__stat-icon employee-summary__stat-icon--red"
                    />
                    <Typography variant="h4" fontWeight={700}>
                      {assetSummary?.currentlyHeldAssets || currentAssets.length}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      Total Assets
                    </Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiLayers
                      size={28}
                      className="employee-summary__stat-icon employee-summary__stat-icon--purple"
                    />
                    <Typography variant="h4" fontWeight={700}>
                      {assetSummary?.assetsOnLoan || 0}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      On Loan
                    </Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiTool
                      size={28}
                      className="employee-summary__stat-icon employee-summary__stat-icon--yellow"
                    />
                    <Typography variant="h4" fontWeight={700}>
                      {assetSummary?.assetsInMaintenance || 0}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      In Maintenance
                    </Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiDollarSign
                      size={28}
                      className="employee-summary__stat-icon employee-summary__stat-icon--green"
                    />
                    <Typography variant="h4" fontWeight={700}>
                      {formatCurrency(totalAssetValue)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      Total Value
                    </Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiRefreshCw
                      size={28}
                      className="employee-summary__stat-icon employee-summary__stat-icon--blue"
                    />
                    <Typography variant="h4" fontWeight={700}>
                      {assetSummary?.returnedAssets ||
                        assetHistory.filter(
                          (t) =>
                            t.transactionTypeName === 'RETURN' ||
                            t.transactionTypeName === 'LOAN_RETURN'
                        ).length}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      Returns
                    </Typography>
                  </Paper>
                </Grid>
                <Grid item xs={6} sm={4} md={2}>
                  <Paper elevation={0} className="employee-summary__stat-card">
                    <FiAlertTriangle
                      size={28}
                      className="employee-summary__stat-icon employee-summary__stat-icon--red"
                    />
                    <Typography variant="h4" fontWeight={700}>
                      {assetSummary?.damagedReturns || 0}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      Damaged
                    </Typography>
                  </Paper>
                </Grid>
              </Grid>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card
                title="Asset Category Summary"
                subtitle="Distribution of assets by category"
              >
                {currentAssets.length === 0 ? (
                  <div className="employee-summary__empty-state u-p-24">
                    <FiPieChart size={32} />
                    <Typography variant="body2">No assets to summarize</Typography>
                  </div>
                ) : (
                  <div className="employee-summary__category-grid">
                    {categorySummary.map(({ category, count }) => (
                      <div
                        key={category}
                        className="employee-summary__category-item"
                      >
                        <span className="employee-summary__category-name">
                          {category}
                        </span>
                        <span className="employee-summary__category-count">
                          {count}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card title="Last Transaction" subtitle="Most recent activity">
                {assetHistory.length === 0 ? (
                  <div className="employee-summary__empty-state u-p-24">
                    <FiRefreshCw size={32} />
                    <Typography variant="body2">No transactions found</Typography>
                  </div>
                ) : (
                  <div className="employee-summary__last-transaction">
                    <InfoRow
                      icon={FiCalendar}
                      label="Date"
                      value={formatDateTime(assetHistory[0]?.transactionDate)}
                    />
                    <InfoRow
                      icon={FiRefreshCw}
                      label="Type"
                      value={
                        assetHistory[0]?.transactionTypeName ||
                        assetHistory[0]?.typeName
                      }
                    />
                    <InfoRow
                      icon={FiBox}
                      label="Asset"
                      value={`${assetHistory[0]?.assetCode || '-'} - ${assetHistory[0]?.assetName || '-'}`}
                    />
                    <InfoRow
                      icon={FiTool}
                      label="Condition"
                      value={
                        assetHistory[0]?.conditionAfterName ||
                        assetHistory[0]?.conditionAfter
                      }
                    />
                  </div>
                )}
              </Card>
            </Grid>

            <Grid item xs={12}>
              <Card
                title="Current Assets"
                subtitle={`${currentAssets.length} asset(s) currently assigned`}
              >
                {currentAssets.length === 0 ? (
                  <div className="employee-summary__empty-state">
                    <FiBox size={48} />
                    <Typography variant="h6" gutterBottom>
                      No Assets Assigned
                    </Typography>
                    <Typography variant="body2">
                      This employee has no assets currently assigned.
                    </Typography>
                  </div>
                ) : (
                  <DataTable
                    rows={currentAssets}
                    columns={assetColumns}
                    pageSize={10}
                    getRowId={(row) => row.assetId || `asset-${Math.random()}`}
                    hideFooter={false}
                    autoHeight={false}
                    ariaLabel="Employee current assets table"
                  />
                )}
              </Card>
            </Grid>

            <Grid item xs={12}>
              <Card
                title="Asset History"
                subtitle={`${assetHistory.length} transaction(s) found`}
              >
                {assetHistory.length === 0 ? (
                  <div className="employee-summary__empty-state">
                    <FiRefreshCw size={48} />
                    <Typography variant="h6" gutterBottom>
                      No Transaction History
                    </Typography>
                    <Typography variant="body2">
                      No asset transactions found for this employee.
                    </Typography>
                  </div>
                ) : (
                  <DataTable
                    rows={assetHistory}
                    columns={historyColumns}
                    pageSize={10}
                    getRowId={(row) =>
                      row.assetTransactionId || `hist-${Math.random()}`
                    }
                    hideFooter={false}
                    autoHeight={false}
                    ariaLabel="Employee asset history table"
                  />
                )}
              </Card>
            </Grid>
          </>
        )}

        {selectedEmployeeId && loadingData && (
          <Grid item xs={12}>
            <Card>
              <div className="employee-summary__loading-container">
                <Spinner size="lg" />
              </div>
            </Card>
          </Grid>
        )}

        {!selectedEmployeeId && !loading && (
          <Grid item xs={12}>
            <Card>
              <div className="employee-summary__empty-state">
                <FiUser size={64} />
                <Typography variant="h6" gutterBottom>
                  Select an Employee to View Summary
                </Typography>
                <Typography variant="body2">
                  Choose an employee from the dropdown above to view their asset
                  summary and transaction history.
                </Typography>
              </div>
            </Card>
          </Grid>
        )}
      </Grid>
    </div>
  );
};

export default EmployeeSummary;