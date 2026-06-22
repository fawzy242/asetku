import DashboardApi from './Dashboard.api';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class DashboardData extends BaseData {
  constructor() {
    super(DashboardApi);
  }

  async fetchDashboardData() {
    try {
      const currentYear = new Date().getFullYear();
      const [stats, monthlyStats, categoryBreakdown, expiredWarranty, upcomingMaintenance, pendingApprovals, recentTransactions] = await Promise.all([
        this.api.getStats(),
        this.api.getMonthlyStats(currentYear),
        this.api.getCategoryBreakdown(),
        this.api.getExpiredWarranty(),
        this.api.getUpcomingMaintenance(),
        this.api.getPendingApprovals(),
        this.api.getRecentTransactions(),
      ]);
      
      return {
        success: true,
        data: {
          stats: stats.data || {},
          monthlyStats: monthlyStats.data || [],
          categoryBreakdown: categoryBreakdown.data || [],
          expiredWarranty: expiredWarranty.data || [],
          upcomingMaintenance: upcomingMaintenance.data || [],
          pendingApprovals: pendingApprovals.data || [],
          recentTransactions: recentTransactions.data || {},
        }
      };
    } catch (error) {
      console.error('Dashboard data fetch error:', error);
      return { success: false, error: 'Failed to load dashboard data' };
    }
  }
}

export default DashboardData;