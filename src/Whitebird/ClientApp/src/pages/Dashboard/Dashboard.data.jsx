import DashboardApi from './Dashboard.api';
import BaseData from '../../core/services/BaseData';

class DashboardData extends BaseData {
  constructor() {
    super(DashboardApi);
  }

  async fetchDashboardData() {
    try {
      const currentYear = new Date().getFullYear();
      const [stats, monthlyStats, categoryBreakdown, recentTransactions, expiredWarranty, upcomingMaintenance, pendingApprovals] = await Promise.all([
        this.api.getStats(),
        this.api.getMonthlyStats(currentYear),
        this.api.getCategoryBreakdown(),
        this.api.getRecentTransactions(10),
        this.api.getExpiredWarranty(),
        this.api.getUpcomingMaintenance(),
        this.api.getPendingApprovals(),
      ]);
      
      return {
        success: true,
        data: {
          stats: stats.data || {},
          monthlyStats: monthlyStats.data || [],
          categoryBreakdown: categoryBreakdown.data || [],
          recentTransactions: recentTransactions.data || [],
          expiredWarranty: expiredWarranty.data || [],
          upcomingMaintenance: upcomingMaintenance.data || [],
          pendingApprovals: pendingApprovals.data || [],
        }
      };
    } catch (error) {
      console.error('Dashboard data fetch error:', error);
      return { success: false, error: 'Failed to load dashboard data' };
    }
  }
}

export default DashboardData;