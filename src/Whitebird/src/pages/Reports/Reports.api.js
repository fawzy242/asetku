import apiService from '../../core/services/api.service';

class ReportsApi {
  async getDashboardStats() { return (await apiService.get('/Reports/dashboard/stats')).data; }
  async getAssetTransactionData(params) { return (await apiService.get(`/Reports/asset-transaction/data?${new URLSearchParams(params)}`)).data; }
  async exportAssetTransactionExcel(params) { return (await apiService.get(`/Reports/asset-transaction/excel?${new URLSearchParams(params)}`, { responseType: 'blob' })).data; }
  async getAssetInventoryData(params) { return (await apiService.get(`/Reports/asset-inventory/data?${new URLSearchParams(params)}`)).data; }
  async exportAssetInventoryExcel(params) { return (await apiService.get(`/Reports/asset-inventory/excel?${new URLSearchParams(params)}`, { responseType: 'blob' })).data; }
  async getEmployeeAssetData(params) { return (await apiService.get(`/Reports/employee-asset/data?${new URLSearchParams(params)}`)).data; }
  async exportEmployeeAssetExcel(params) { return (await apiService.get(`/Reports/employee-asset/excel?${new URLSearchParams(params)}`, { responseType: 'blob' })).data; }
  async getMaintenanceData(params) { return (await apiService.get(`/Reports/maintenance/data?${new URLSearchParams(params)}`)).data; }
  async exportMaintenanceExcel(params) { return (await apiService.get(`/Reports/maintenance/excel?${new URLSearchParams(params)}`, { responseType: 'blob' })).data; }
  async getFinancialData(params) { return (await apiService.get(`/Reports/financial/data?${new URLSearchParams(params)}`)).data; }
  async exportFinancialExcel(params) { return (await apiService.get(`/Reports/financial/excel?${new URLSearchParams(params)}`, { responseType: 'blob' })).data; }
  async getCategories() { return (await apiService.get('/Category/active')).data; }
  async getSuppliers() { return (await apiService.get('/Supplier/active')).data; }
  async getEmployees() { return (await apiService.get('/Employee')).data; }
}

export default new ReportsApi();