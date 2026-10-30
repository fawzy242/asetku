// src/pages/Dashboard/components/SummaryTab.jsx
import React from 'react';
import { Box, Typography, Chip } from '@mui/material';
import { FiAlertTriangle } from 'react-icons/fi';
import Card from '../../../components/atoms/Card/Card';
import DataTable from '../../../components/molecules/DataTable/DataTable';
import StatCard from '../../../components/molecules/StatCard/StatCard';
import {
  expiredWarrantyColumns,
  categoryBreakdownColumns,
  monthlyStatsColumns,
} from '../columns';

const SummaryTab = ({
  statCardsConfig,
  onCardClick,
  monthlyStats,
  monthlyStatsPage,
  onMonthlyStatsPageChange,
  categoryBreakdown,
  categoryBreakdownPage,
  onCategoryBreakdownPageChange,
  expiredWarranty,
  expiredWarrantyPage,
  onExpiredWarrantyPageChange,
  scheduleStats,
}) => {
  const paginatedExpiredWarranty = (expiredWarranty || []).slice(
    (expiredWarrantyPage - 1) * 10,
    expiredWarrantyPage * 10
  );
  const paginatedMonthlyStats = (monthlyStats || []).slice(
    (monthlyStatsPage - 1) * 10,
    monthlyStatsPage * 10
  );
  const paginatedCategoryBreakdown = (categoryBreakdown || []).slice(
    (categoryBreakdownPage - 1) * 10,
    categoryBreakdownPage * 10
  );

  return (
    <div className="dashboard__tab-content">
      <div className="dashboard__stats">
        {statCardsConfig.map((card) => (
          <StatCard
            key={card.label}
            icon={card.icon}
            label={card.label}
            value={card.value}
            color={card.color}
            bgColor={card.bg}
            onClick={() => onCardClick(card)}
            clickable={!card.noDrilldown}
          />
        ))}
      </div>

      {expiredWarranty.length > 0 && (
        <div className="dashboard__alert-wrapper">
          <Card className="dashboard__alert-card">
            <div className="dashboard__alert-header">
              <div className="dashboard__alert-title-group">
                <FiAlertTriangle className="dashboard__alert-icon" />
                <Typography variant="h6" fontWeight={600}>
                  Expired Warranty
                </Typography>
                <Chip
                  label={`${expiredWarranty.length} assets`}
                  size="small"
                  className="dashboard__alert-count"
                />
              </div>
            </div>
            <div className="dashboard__alert-body u-min-h-200">
              <DataTable
                rows={paginatedExpiredWarranty}
                columns={expiredWarrantyColumns}
                pageSize={10}
                page={expiredWarrantyPage}
                onPageChange={onExpiredWarrantyPageChange}
                getRowId={(row) => row?.assetId || `ew-${Math.random()}`}
                hideFooter={false}
                ariaLabel="Expired warranty table"
                paginationMode="client"
                autoHeight={false}
              />
            </div>
          </Card>
        </div>
      )}

      <div className="dashboard__summary-tables">
        {monthlyStats.length > 0 && (
          <Card
            title="Monthly Transaction Statistics"
            className="dashboard__summary-card"
          >
            <DataTable
              rows={paginatedMonthlyStats}
              columns={monthlyStatsColumns}
              pageSize={10}
              page={monthlyStatsPage}
              onPageChange={onMonthlyStatsPageChange}
              getRowId={(row) => row?.month || `month-${Math.random()}`}
              hideFooter={false}
              ariaLabel="Monthly stats table"
              paginationMode="client"
              autoHeight={false}
            />
          </Card>
        )}

        {categoryBreakdown.length > 0 && (
          <Card
            title="Asset Value by Category"
            className="dashboard__summary-card"
          >
            <DataTable
              rows={paginatedCategoryBreakdown}
              columns={categoryBreakdownColumns}
              pageSize={10}
              page={categoryBreakdownPage}
              onPageChange={onCategoryBreakdownPageChange}
              getRowId={(row) => row?.category || `cat-${Math.random()}`}
              hideFooter={false}
              ariaLabel="Category breakdown table"
              paginationMode="client"
              autoHeight={false}
            />
          </Card>
        )}

        {scheduleStats && (
          <Card title="Schedule Statistics" className="dashboard__summary-card">
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fill, minmax(150px, 1fr))',
                gap: 2,
              }}
            >
              <Box className="dashboard__stat-item">
                <Typography variant="caption" color="text.secondary">
                  Total Handover
                </Typography>
                <Typography variant="h6" fontWeight={600}>
                  {scheduleStats.totalHandover || 0}
                </Typography>
              </Box>
              <Box className="dashboard__stat-item">
                <Typography variant="caption" color="text.secondary">
                  Total Return
                </Typography>
                <Typography variant="h6" fontWeight={600}>
                  {scheduleStats.totalReturn || 0}
                </Typography>
              </Box>
              <Box className="dashboard__stat-item">
                <Typography variant="caption" color="text.secondary">
                  Total Maintenance
                </Typography>
                <Typography variant="h6" fontWeight={600}>
                  {scheduleStats.totalMaintenance || 0}
                </Typography>
              </Box>
              <Box className="dashboard__stat-item">
                <Typography variant="caption" color="text.secondary">
                  Upcoming Handover
                </Typography>
                <Typography variant="h6" fontWeight={600} color="warning.main">
                  {scheduleStats.upcomingHandover || 0}
                </Typography>
              </Box>
              <Box className="dashboard__stat-item">
                <Typography variant="caption" color="text.secondary">
                  Upcoming Return
                </Typography>
                <Typography variant="h6" fontWeight={600} color="warning.main">
                  {scheduleStats.upcomingReturn || 0}
                </Typography>
              </Box>
              <Box className="dashboard__stat-item">
                <Typography variant="caption" color="text.secondary">
                  Upcoming Maintenance
                </Typography>
                <Typography variant="h6" fontWeight={600} color="warning.main">
                  {scheduleStats.upcomingMaintenance || 0}
                </Typography>
              </Box>
            </Box>
          </Card>
        )}
      </div>
    </div>
  );
};

export default SummaryTab;