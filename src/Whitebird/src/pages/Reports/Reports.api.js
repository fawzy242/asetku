import apiService from '../../core/services/api.service';

class ReportsApi {
  async getDashboardStats() { return (await apiService.get('/Reports/dashboard/stats')).data; }
  async getAssetTransactionData(params) { return (await apiService.get(`/Reports/asset-transaction/data?${new URLSearchParams(params)}`)).data; }
  async getAssetInventoryData(params) { return (await apiService.get(`/Reports/asset-inventory/data?${new URLSearchParams(params)}`)).data; }
  async getEmployeeAssetData(params) { return (await apiService.get(`/Reports/employee-asset/data?${new URLSearchParams(params)}`)).data; }
  async getMaintenanceData(params) { return (await apiService.get(`/Reports/maintenance/data?${new URLSearchParams(params)}`)).data; }
  async getFinancialData(params) { return (await apiService.get(`/Reports/financial/data?${new URLSearchParams(params)}`)).data; }

  // Excel export endpoints from backend (EPPlus)
  async exportAssetTransactionExcel(params) {
    const response = await apiService.get(`/Reports/asset-transaction/excel?${new URLSearchParams(params)}`, { responseType: 'blob' });
    return response.data;
  }
  async exportAssetInventoryExcel(params) {
    const response = await apiService.get(`/Reports/asset-inventory/excel?${new URLSearchParams(params)}`, { responseType: 'blob' });
    return response.data;
  }
  async exportEmployeeAssetExcel(params) {
    const response = await apiService.get(`/Reports/employee-asset/excel?${new URLSearchParams(params)}`, { responseType: 'blob' });
    return response.data;
  }
  async exportMaintenanceExcel(params) {
    const response = await apiService.get(`/Reports/maintenance/excel?${new URLSearchParams(params)}`, { responseType: 'blob' });
    return response.data;
  }
  async exportFinancialExcel(params) {
    const response = await apiService.get(`/Reports/financial/excel?${new URLSearchParams(params)}`, { responseType: 'blob' });
    return response.data;
  }

  async getCategories() { return (await apiService.get('/Category/active')).data; }
  async getSuppliers() { return (await apiService.get('/Supplier/active')).data; }
  async getEmployees() { return (await apiService.get('/Employee')).data; }
}

export default new ReportsApi();