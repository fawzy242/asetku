// src/pages/Dashboard/Dashboard.jsx
import React, { useState } from 'react';
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

  // Pagination state persists across tab switches — preserved from the original page.
  const [expiredWarrantyPage, setExpiredWarrantyPage] = useState(1);
  const [recentPage, setRecentPage] = useState(1);
  const [monthlyStatsPage, setMonthlyStatsPage] = useState(1);
  const [categoryBreakdownPage, setCategoryBreakdownPage] = useState(1);

  const theme = useUIStore((s) => s.theme);

  // ============================================================
  // DATA — single combined fetch (matches the previous Promise.allSettled
  // semantics inside DashboardData.fetchDashboardData).
  // ============================================================
  const { data: dashboardData_ = EMPTY_DASHBOARD, isLoading: loading } = useQuery({
    queryKey: ['dashboard'],
    queryFn: async () => {
      const result = await dashboardData.fetchDashboardData();
      return result.success ? result.data : EMPTY_DASHBOARD;
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