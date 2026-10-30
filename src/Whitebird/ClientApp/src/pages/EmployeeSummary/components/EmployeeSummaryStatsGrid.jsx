// src/pages/EmployeeSummary/components/EmployeeSummaryStatsGrid.jsx
import React from 'react';
import { Grid, Paper, Typography } from '@mui/material';
import {
  FiBox,
  FiLayers,
  FiTool,
  FiDollarSign,
  FiRefreshCw,
  FiAlertTriangle,
} from 'react-icons/fi';
import { formatCurrency } from '../../../core/utils/number.utils';

const EmployeeSummaryStatsGrid = ({
  assetSummary,
  currentAssets = [],
  assetHistory = [],
  totalAssetValue = 0,
}) => {
  return (
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
  );
};

export default EmployeeSummaryStatsGrid;