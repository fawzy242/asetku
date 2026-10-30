// src/pages/AssetTracking/components/TrackingTimelineCard.jsx
import React, { useState, useMemo } from 'react';
import { Box, Tabs as MuiTabs, Tab } from '@mui/material';
import Card from '../../../components/atoms/Card/Card';
import AssetTimeline from './AssetTimeline';
import { historyFilterTabs } from '../constants';

const TrackingTimelineCard = ({ timelineData = [] }) => {
  const [historyFilter, setHistoryFilter] = useState('all');

  const filteredTimeline = useMemo(() => {
    if (historyFilter === 'all') return timelineData;
    const filterMap = {
      movement: ['HANDOVER', 'TRANSFER', 'RETURN', 'LOAN_RETURN'],
      loan: ['LOAN'],
      maintenance: ['MAINTENANCE', 'POST_MAINTENANCE'],
      disposal: ['DISPOSAL'],
      handover: ['HANDOVER'],
      transfer: ['TRANSFER'],
    };
    const allowedTypes = filterMap[historyFilter] || [];
    return timelineData.filter((t) => {
      const type = t.activityType?.toUpperCase() || '';
      return allowedTypes.some((at) => type.includes(at));
    });
  }, [timelineData, historyFilter]);

  return (
    <Card title="Timeline" subtitle="Transaction history">
      <Box
        sx={{
          borderBottom: 1,
          borderColor: 'var(--border)',
          mb: 2,
        }}
      >
        <MuiTabs
          value={historyFilter}
          onChange={(e, v) => setHistoryFilter(v)}
          variant="scrollable"
          scrollButtons="auto"
        >
          {historyFilterTabs.map((tab) => (
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
      <AssetTimeline items={filteredTimeline} />
    </Card>
  );
};

export default TrackingTimelineCard;