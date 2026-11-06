// src/core/services/DashboardData.js
import { dashboardApi } from '../api/api';
import BaseData from './BaseData';

// Section labels surfaced in the UI when a piece of the combined fetch fails.
const SECTION_LABELS = [
  'Overview Stats',
  'Monthly Statistics',
  'Category Breakdown',
  'Recent Transactions',
  'Expired Warranty',
  'Scheduler Information',
];

class DashboardData extends BaseData {
  constructor() {
    super({
      getGrid: () => Promise.reject('Not implemented'),
      getById: () => Promise.reject('Not implemented'),
      create: () => Promise.reject('Not implemented'),
      update: () => Promise.reject('Not implemented'),
      delete: () => Promise.reject('Not implemented'),
      softDelete: () => Promise.reject('Not implemented'),
    });
    this.setEntityName('dashboard');
    this.api = dashboardApi;
  }

  async fetchDashboardData() {
    try {
      const currentYear = new Date().getFullYear();

      const results = await Promise.allSettled([
        this.api.getStats(),                      // 0
        this.api.getMonthlyStats(currentYear),    // 1
        this.api.getCategoryBreakdown(),          // 2
        this.api.getRecentTransactions(10),       // 3
        this.api.getExpiredWarranty(),            // 4
        this.api.getScheduleStats(),              // 5
      ]);

      // Detect which sections failed (rejected or isSuccess === false).
      const failedSections = [];
      results.forEach((result, idx) => {
        const failed =
          result.status === 'rejected' ||
          result.value?.data?.isSuccess === false;
        if (failed) failedSections.push(SECTION_LABELS[idx]);
      });

      const extractData = (result, fallback = {}) => {
        if (result.status === 'rejected') return fallback;
        const payload = result.value?.data;
        if (!payload) return fallback;
        if (payload.isSuccess === false) return fallback;
        if (payload.isSuccess === true) return payload.data ?? fallback;
        return payload;
      };

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
        partialFailure: failedSections.length > 0,
        failedSections,
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
      // Defensive branch — allSettled does not reject, but a synchronous
      // throw from an api call would land here.
      console.error('Dashboard fetch error:', error);
      return {
        success: false,
        partialFailure: true,
        failedSections: [...SECTION_LABELS],
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