// src/pages/Dashboard/Dashboard.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';

class DashboardData extends BaseData {
  constructor() {
    super(apiClient.dashboard);
    this.setEntityName('dashboard');
  }

  async fetchDashboardData() {
    try {
      const currentYear = new Date().getFullYear();

      const results = await Promise.allSettled([
        this.api.getStats(),
        this.api.getMonthlyStats(currentYear),
        this.api.getCategoryBreakdown(),
        this.api.getRecentTransactions(10),
        this.api.getExpiredWarranty(),
        this.api.getScheduleStats(),
      ]);

      // Helper to extract data from ApiResponse
      const extractData = (result, fallback = {}) => {
        if (result.status === 'fulfilled' && result.value?.data) {
          const data = result.value.data;
          // If data is ApiResponse with isSuccess
          if (data.isSuccess !== undefined) {
            return data.isSuccess ? (data.data || fallback) : fallback;
          }
          // If data is already the response
          return data || fallback;
        }
        return fallback;
      };

      // Helper to extract array data
      const extractArray = (result, fallback = []) => {
        const data = extractData(result, fallback);
        return Array.isArray(data) ? data : fallback;
      };

      const stats = extractData(results[0], {});
      const monthlyStats = extractArray(results[1], []);
      const categoryBreakdown = extractArray(results[2], []);
      const recentTransactions = extractArray(results[3], []);
      const expiredWarranty = extractArray(results[4], []);
      const scheduleStats = extractData(results[5], {});

      return {
        success: true,
        data: {
          stats,
          monthlyStats,
          categoryBreakdown,
          recentTransactions,
          expiredWarranty,
          scheduleStats,
        },
      };
    } catch (error) {
      console.error('Dashboard fetch error:', error);
      return {
        success: false,
        data: {
          stats: {},
          monthlyStats: [],
          categoryBreakdown: [],
          recentTransactions: [],
          expiredWarranty: [],
          scheduleStats: {},
        },
      };
    }
  }
}

export default DashboardData;