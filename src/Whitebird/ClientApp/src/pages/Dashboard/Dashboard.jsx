// src/pages/Dashboard/Dashboard.jsx
import React, { useState, useEffect, useMemo } from 'react';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  PointElement,
  LineElement,
  Filler,
} from 'chart.js';
import { Alert } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { DashboardData } from '../../core/services';
import Spinner from '../../components/atoms/Spinner/Spinner';
import Tabs from '../../components/molecules/Tabs/Tabs';
import DrilldownModal from '../../components/molecules/DrilldownModal/DrilldownModal';
import { useUIStore } from '../../stores/uiStore';
import SummaryTab from './components/SummaryTab';
import ChartsTab from './components/ChartsTab';
import RecentTab from './components/RecentTab';
import { DASHBOARD_TABS, EMPTY_DASHBOARD } from './constants';
import { buildStatCardsConfig } from './statCardsConfig';
import './Dashboard.scss';

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  PointElement,
  LineElement,
  Filler
);

const dashboardData = new DashboardData();

const Dashboard = () => {
  const [activeTab, setActiveTab] = useState('summary');
  const [drilldown, setDrilldown] = useState({
    isOpen: false,
    title: '',
    endpoint: '',
    params: {},
  });
  const [bannerDismissed, setBannerDismissed] = useState(false);

  // Pagination state persists across tab switches — preserved from the original page.
  const [expiredWarrantyPage, setExpiredWarrantyPage] = useState(1);
  const [recentPage, setRecentPage] = useState(1);
  const [monthlyStatsPage, setMonthlyStatsPage] = useState(1);
  const [categoryBreakdownPage, setCategoryBreakdownPage] = useState(1);

  const theme = useUIStore((s) => s.theme);

  // ============================================================
  // DATA — single combined fetch.
  // partialFailure + failedSections propagate from DashboardData.
  // ============================================================
  const { data: dashboardData_ = EMPTY_DASHBOARD, isLoading: loading } =
    useQuery({
      queryKey: ['dashboard'],
      queryFn: async () => {
        const result = await dashboardData.fetchDashboardData();
        const base = result.success ? result.data : EMPTY_DASHBOARD;
        return {
          ...base,
          partialFailure: result.partialFailure || false,
          failedSections: result.failedSections || [],
        };
      },
      staleTime: 5 * 60 * 1000,
    });

  const {
    stats,
    monthlyStats,
    categoryBreakdown,
    recentTransactions,
    expiredWarranty,
    scheduleStats,
    partialFailure = false,
    failedSections = [],
  } = dashboardData_;

  const safeStats = stats || {};
  const safeMonthlyStats = Array.isArray(monthlyStats) ? monthlyStats : [];
  const safeCategoryBreakdown = Array.isArray(categoryBreakdown)
    ? categoryBreakdown
    : [];
  const safeRecentTransactions = Array.isArray(recentTransactions)
    ? recentTransactions
    : [];
  const safeExpiredWarranty = Array.isArray(expiredWarranty)
    ? expiredWarranty
    : [];
  const safeScheduleStats = scheduleStats || {};

  // ============================================================
  // PARTIAL FAILURE BANNER
  // ============================================================
  const failedSectionsKey = useMemo(
    () => failedSections.join('|'),
    [failedSections]
  );

  // Reset dismissed state whenever the set of failures changes, so a new
  // failure (or a fixed-then-broken-again cycle) shows the banner again.
  useEffect(() => {
    setBannerDismissed(false);
  }, [failedSectionsKey]);

  const showBanner = partialFailure && !bannerDismissed;
  const allFailed = failedSections.length >= 6;

  const statCardsConfig = buildStatCardsConfig({
    stats: safeStats,
    scheduleStats: safeScheduleStats,
    monthlyStats: safeMonthlyStats,
  });

  const handleCardClick = (card) => {
    if (card.noDrilldown) return;
    setDrilldown({
      isOpen: true,
      title: `${card.label} Details`,
      endpoint: card.endpoint,
      params: card.params || {},
    });
  };

  if (loading) {
    return (
      <div className="page-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="dashboard fade-transition">
      <div className="page-header">
        <h1 className="page-title">Dashboard</h1>
        <p className="page-description">
          Overview of asset management system performance
        </p>
      </div>

      {showBanner && (
        <Alert
          severity={allFailed ? 'error' : 'warning'}
          onClose={() => setBannerDismissed(true)}
          sx={{ mb: 2 }}
        >
          {allFailed
            ? 'Dashboard data could not be loaded. Please check your connection and refresh the page.'
            : `Some dashboard data could not be loaded (${failedSections.join(
                ', '
              )}). Values for those sections may be incomplete.`}
        </Alert>
      )}

      <Tabs tabs={DASHBOARD_TABS} activeTab={activeTab} onTabChange={setActiveTab} />

      {activeTab === 'summary' && (
        <SummaryTab
          statCardsConfig={statCardsConfig}
          onCardClick={handleCardClick}
          monthlyStats={safeMonthlyStats}
          monthlyStatsPage={monthlyStatsPage}
          onMonthlyStatsPageChange={setMonthlyStatsPage}
          categoryBreakdown={safeCategoryBreakdown}
          categoryBreakdownPage={categoryBreakdownPage}
          onCategoryBreakdownPageChange={setCategoryBreakdownPage}
          expiredWarranty={safeExpiredWarranty}
          expiredWarrantyPage={expiredWarrantyPage}
          onExpiredWarrantyPageChange={setExpiredWarrantyPage}
          scheduleStats={safeScheduleStats}
        />
      )}

      {activeTab === 'charts' && (
        <ChartsTab
          stats={safeStats}
          monthlyStats={safeMonthlyStats}
          categoryBreakdown={safeCategoryBreakdown}
          categoryBreakdownPage={categoryBreakdownPage}
          onCategoryBreakdownPageChange={setCategoryBreakdownPage}
          theme={theme}
        />
      )}

      {activeTab === 'recent' && (
        <RecentTab
          recentTransactions={safeRecentTransactions}
          recentPage={recentPage}
          onRecentPageChange={setRecentPage}
          expiredWarranty={safeExpiredWarranty}
          expiredWarrantyPage={expiredWarrantyPage}
          onExpiredWarrantyPageChange={setExpiredWarrantyPage}
        />
      )}

      <DrilldownModal
        isOpen={drilldown.isOpen}
        onClose={() => setDrilldown((prev) => ({ ...prev, isOpen: false }))}
        title={drilldown.title}
        endpoint={drilldown.endpoint}
        params={drilldown.params}
        size="xl"
      />
    </div>
  );
};

export default Dashboard;