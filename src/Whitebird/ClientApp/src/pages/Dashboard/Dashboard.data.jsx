import DashboardApi from './Dashboard.api';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class DashboardData extends BaseData {
  constructor() {
    super(DashboardApi);
  }

  async fetchDashboardData() {
    try {
      const [stats, expiredWarranty, upcomingMaintenance, pendingApprovals, recentTransactions, monthlyStats] = await Promise.all([
        this.api.getStats(),
        this.api.getExpiredWarranty(),
        this.api.getUpcomingMaintenance(),
        this.api.getPendingApprovals(),
        this.api.getRecentTransactions(),
        this.api.getMonthlyStats().catch(() => ({ data: null })),
      ]);
      return {
        success: true,
        data: {
          stats: stats.data || {},
          expiredWarranty: expiredWarranty.data || [],
          upcomingMaintenance: upcomingMaintenance.data || [],
          pendingApprovals: pendingApprovals.data || [],
          recentTransactions: recentTransactions.data || {},
          monthlyStats: monthlyStats.data || null,
        }
      };
    } catch {
      return { success: false, error: 'Failed to load dashboard data' };
    }
  }
}

export default DashboardData;