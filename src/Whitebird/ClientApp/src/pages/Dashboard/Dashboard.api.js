import apiService from '../../core/services/api.service';

class DashboardApi {
  async getStats() { 
    return (await apiService.get('/Reports/dashboard/stats')).data; 
  }
  
  async getMonthlyStats(year) { 
    const yearParam = year || new Date().getFullYear();
    return (await apiService.get(`/Reports/dashboard/monthly-stats?year=${yearParam}`)).data; 
  }
  
  async getCategoryBreakdown() { 
    return (await apiService.get('/Reports/dashboard/category-breakdown')).data; 
  }
  
  async getRecentTransactions(limit = 10) { 
    return (await apiService.get(`/Reports/dashboard/recent-transactions?limit=${limit}`)).data; 
  }
  
  async getExpiredWarranty() { 
    return (await apiService.get('/Asset/expired-warranty')).data; 
  }
  
  async getUpcomingMaintenance(daysAhead = 30) { 
    return (await apiService.get(`/Asset/upcoming-maintenance?daysAhead=${daysAhead}`)).data; 
  }
  
  async getPendingApprovals() { 
    return (await apiService.get('/AssetTransaction/pending-approvals')).data; 
  }
}

export default new DashboardApi();