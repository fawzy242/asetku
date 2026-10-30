// src/pages/AssetTracking/components/TrackingStatsGrid.jsx
import React, { useMemo } from 'react';
import { Box, Paper, Typography } from '@mui/material';
import {
  FiBox,
  FiMove,
  FiClock,
  FiTool,
  FiActivity,
} from 'react-icons/fi';

/**
 * Computes per-type counts from timeline entries and renders the 7-card grid.
 * Derivation lives here so the page stays focused on data fetching.
 */
const TrackingStatsGrid = ({ currentAsset, timelineData = [] }) => {
  const stats = useMemo(() => {
    const result = {
      totalMovements: 0,
      totalLoans: 0,
      totalMaintenance: 0,
      totalDisposals: 0,
      totalHandovers: 0,
      totalTransfers: 0,
    };
    timelineData.forEach((t) => {
      const type = t.activityType?.toUpperCase() || '';
      if (type.includes('HANDOVER')) result.totalHandovers++;
      if (type.includes('TRANSFER')) result.totalTransfers++;
      if (
        ['HANDOVER', 'TRANSFER', 'RETURN', 'LOAN_RETURN'].some((at) =>
          type.includes(at)
        )
      )
        result.totalMovements++;
      if (type.includes('LOAN')) result.totalLoans++;
      if (
        ['MAINTENANCE', 'POST_MAINTENANCE'].some((at) => type.includes(at))
      )
        result.totalMaintenance++;
      if (type.includes('DISPOSAL')) result.totalDisposals++;
    });
    return result;
  }, [timelineData]);

  const statCards = [
    {
      icon: FiBox,
      label: 'Total Transactions',
      value: currentAsset?.totalTransactions || 0,
      color: '#3b82f6',
    },
    {
      icon: FiMove,
      label: 'Movements',
      value: stats.totalMovements,
      color: '#3b82f6',
    },
    {
      icon: FiMove,
      label: 'Handovers',
      value: stats.totalHandovers,
      color: '#8b5cf6',
    },
    {
      icon: FiMove,
      label: 'Transfers',
      value: stats.totalTransfers,
      color: '#6366f1',
    },
    {
      icon: FiClock,
      label: 'Loans',
      value: stats.totalLoans,
      color: '#f59e0b',
    },
    {
      icon: FiTool,
      label: 'Maintenance',
      value: stats.totalMaintenance,
      color: '#f97316',
    },
    {
      icon: FiActivity,
      label: 'Disposals',
      value: stats.totalDisposals,
      color: '#ef4444',
    },
  ];

  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fill, minmax(140px, 1fr))',
        gap: 2,
      }}
    >
      {statCards.map((card, index) => {
        const Icon = card.icon;
        return (
          <Paper
            elevation={0}
            className="asset-tracking__stat-card"
            key={index}
          >
            <Icon
              size={24}
              className="asset-tracking__stat-icon"
              style={{ color: card.color }}
            />
            <Typography variant="h5" fontWeight={700}>
              {card.value}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {card.label}
            </Typography>
          </Paper>
        );
      })}
    </Box>
  );
};

export default TrackingStatsGrid;