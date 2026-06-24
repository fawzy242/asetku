import DashboardApi from './Dashboard.api';
import BaseData from '../../core/services/BaseData';

class DashboardData extends BaseData {
  constructor() {
    super(DashboardApi);
  }

  async fetchDashboardData() {
    try {
      const currentYear = new Date().getFullYear();
      
      // Gunakan Promise.all dengan error handling per request
      const results = await Promise.allSettled([
        this.api.getStats(),
        this.api.getMonthlyStats(currentYear),
        this.api.getCategoryBreakdown(),
        this.api.getRecentTransactions(10),
        this.api.getExpiredWarranty(),
        this.api.getUpcomingMaintenance(),
        this.api.getPendingApprovals(),
      ]);
      
      // Extract data dari results
      const extractData = (result, defaultValue = []) => {
        if (result.status === 'fulfilled' && result.value?.data) {
          return result.value.data;
        }
        return defaultValue;
      };
      
      const stats = extractData(results[0], {});
      const monthlyStats = extractData(results[1], []);
      const categoryBreakdown = extractData(results[2], []);
      const recentTransactions = extractData(results[3], []);
      const expiredWarranty = extractData(results[4], []);
      const upcomingMaintenance = extractData(results[5], []);
      const pendingApprovals = extractData(results[6], []);
      
      return {
        success: true,
        data: {
          stats,
          monthlyStats,
          categoryBreakdown,
          recentTransactions,
          expiredWarranty,
          upcomingMaintenance,
          pendingApprovals,
        }
      };
    } catch (error) {
      console.error('Dashboard data fetch error:', error);
      return { 
        success: false, 
        error: 'Failed to load dashboard data',
        data: {
          stats: {},
          monthlyStats: [],
          categoryBreakdown: [],
          recentTransactions: [],
          expiredWarranty: [],
          upcomingMaintenance: [],
          pendingApprovals: [],
        }
      };
    }
  }
}

export default DashboardData;