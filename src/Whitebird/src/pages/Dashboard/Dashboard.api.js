import apiService from '../../core/services/api.service';

class DashboardApi {
  async getStats() {
    const response = await apiService.get('/reports/dashboard/stats');
    return response.data;
  }

  async getExpiredWarranty() {
    const response = await apiService.get('/asset/expired-warranty');
    return response.data;
  }

  async getUpcomingMaintenance(daysAhead = 30) {
    const response = await apiService.get(`/asset/upcoming-maintenance?daysAhead=${daysAhead}`);
    return response.data;
  }

  async getPendingApprovals() {
    const response = await apiService.get('/assetTransaction/pending-approvals');
    return response.data;
  }

  async getRecentTransactions() {
    const response = await apiService.get('/assetTransaction/grid?page=1&pageSize=5');
    return response.data;
  }
}

export default new DashboardApi();