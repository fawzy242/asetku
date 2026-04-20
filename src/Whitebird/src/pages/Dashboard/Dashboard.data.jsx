import DashboardApi from './Dashboard.api';

class DashboardData {
  constructor() {
    this.api = DashboardApi;
  }

  async loadDashboardData() {
    try {
      const [stats, expiredWarranty, upcomingMaintenance, pendingApprovals, recentTransactions] = 
        await Promise.all([
          this.api.getStats(),
          this.api.getExpiredWarranty(),
          this.api.getUpcomingMaintenance(),
          this.api.getPendingApprovals(),
          this.api.getRecentTransactions()
        ]);

      return {
        success: true,
        data: {
          stats: stats.data || {},
          expiredWarranty: expiredWarranty.data || [],
          upcomingMaintenance: upcomingMaintenance.data || [],
          pendingApprovals: pendingApprovals.data || [],
          recentTransactions: recentTransactions.data || {}
        }
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load dashboard data'
      };
    }
  }
}

export default DashboardData;