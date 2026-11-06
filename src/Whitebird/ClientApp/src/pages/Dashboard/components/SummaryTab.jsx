// src/pages/Dashboard/components/SummaryTab.jsx
import React from 'react';
import { Box, Typography, Chip } from '@mui/material';
import {
  FiAlertTriangle,
  FiArrowRight,
  FiArrowLeft,
  FiTool,
  FiClock,
} from 'react-icons/fi';
import Card from '../../../components/atoms/Card/Card';
import DataTable from '../../../components/molecules/DataTable/DataTable';
import StatCard from '../../../components/molecules/StatCard/StatCard';
import {
  expiredWarrantyColumns,
  categoryBreakdownColumns,
  monthlyStatsColumns,
} from '../columns';

// ============================================================
// SCHEDULER OVERVIEW — rendered at the top of the tab
// ============================================================
const SchedulerOverview = ({ scheduleStats }) => {
  const items = [
    {
      icon: FiArrowRight,
      label: 'Total Handover',
      value: scheduleStats.totalHandover || 0,
      variant: 'total',
    },
    {
      icon: FiArrowLeft,
      label: 'Total Return',
      value: scheduleStats.totalReturn || 0,
      variant: 'total',
    },
    {
      icon: FiTool,
      label: 'Total Maintenance',
      value: scheduleStats.totalMaintenance || 0,
      variant: 'total',
    },
    {
      icon: FiClock,
      label: 'Upcoming Handover',
      value: scheduleStats.upcomingHandover || 0,
      variant: 'upcoming',
    },
    {
      icon: FiClock,
      label: 'Upcoming Return',
      value: scheduleStats.upcomingReturn || 0,
      variant: 'upcoming',
    },
    {
      icon: FiClock,
      label: 'Upcoming Maintenance',
      value: scheduleStats.upcomingMaintenance || 0,
      variant: 'upcoming',
    },
  ];

  return (
    <div className="dashboard__scheduler-section">
      <Card
        title="Scheduler Information"
        subtitle="Overview of scheduled asset operations"
        className="dashboard__scheduler-card"
      >
        <div className="dashboard__scheduler-grid">
          {items.map((item) => {
            const Icon = item.icon;
            return (
              <div
                key={item.label}
                className={`dashboard__scheduler-item dashboard__scheduler-item--${item.variant}`}
              >
                <div className="dashboard__scheduler-icon">
                  <Icon size={20} />
                </div>
                <div className="dashboard__scheduler-body">
                  <Typography
                    variant="caption"
                    className="dashboard__scheduler-label"
                  >
                    {item.label}
                  </Typography>
                  <Typography
                    variant="h6"
                    className="dashboard__scheduler-value"
                  >
                    {item.value}
                  </Typography>
                </div>
              </div>
            );
          })}
        </div>
      </Card>
    </div>
  );
};

// ============================================================
// SUMMARY TAB
// ============================================================
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

  const hasScheduleStats =
    scheduleStats &&
    typeof scheduleStats === 'object' &&
    Object.keys(scheduleStats).length > 0;

  return (
    <div className="dashboard__tab-content">
      {/* 1. Scheduler Information — TOP (prominent) */}
      {hasScheduleStats && <SchedulerOverview scheduleStats={scheduleStats} />}

      {/* 2. Stats grid */}
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

      {/* 3. Expired Warranty alert */}
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

      {/* 4. Summary tables (monthly + category — scheduler removed from here) */}
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
      </div>
    </div>
  );
};

export default SummaryTab;