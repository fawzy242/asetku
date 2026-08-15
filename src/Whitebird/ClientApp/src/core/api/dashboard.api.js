import apiService from '../services/api.service';

export const dashboardApi = {
  getStats: () => apiService.get('/Dashboard/stats'),
  getMonthlyStats: (year) =>
    apiService.get(`/Dashboard/monthly-stats?year=${year || new Date().getFullYear()}`),
  getCategoryBreakdown: () => apiService.get('/Dashboard/category-breakdown'),
  getRecentTransactions: (limit = 10) =>
    apiService.get(`/Dashboard/recent-transactions?limit=${limit}`),
  getExpiredWarranty: () => apiService.get('/Dashboard/expired-warranty'),
  getScheduleStats: () => apiService.get('/Dashboard/schedule-stats'),
  getDetailStats: (params) => apiService.get('/Dashboard/detail-stats', { params }),
};