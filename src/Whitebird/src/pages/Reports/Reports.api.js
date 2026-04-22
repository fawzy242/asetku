import apiService from '../../core/services/api.service';

class ReportsApi {
  async getDashboardStats() {
    const response = await apiService.get('/Asset/dashboard-stats');
    return response.data;
  }

  async getAssetTransactionData(params) {
    const queryString = new URLSearchParams(params).toString();
    const response = await apiService.get(`/Reports/asset-transaction/data?${queryString}`);
    return response.data;
  }

  async exportAssetTransactionExcel(params) {
    const queryString = new URLSearchParams(params).toString();
    const response = await apiService.get(`/Reports/asset-transaction/excel?${queryString}`, {
      responseType: 'blob'
    });
    return response.data;
  }

  async getAssetInventoryData(params) {
    const queryString = new URLSearchParams(params).toString();
    const response = await apiService.get(`/Reports/asset-inventory/data?${queryString}`);
    return response.data;
  }

  async exportAssetInventoryExcel(params) {
    const queryString = new URLSearchParams(params).toString();
    const response = await apiService.get(`/Reports/asset-inventory/excel?${queryString}`, {
      responseType: 'blob'
    });
    return response.data;
  }

  async getEmployeeAssetData(params) {
    const queryString = new URLSearchParams(params).toString();
    const response = await apiService.get(`/Reports/employee-asset/data?${queryString}`);
    return response.data;
  }

  async exportEmployeeAssetExcel(params) {
    const queryString = new URLSearchParams(params).toString();
    const response = await apiService.get(`/Reports/employee-asset/excel?${queryString}`, {
      responseType: 'blob'
    });
    return response.data;
  }

  async getMaintenanceData(params) {
    const queryString = new URLSearchParams(params).toString();
    const response = await apiService.get(`/Reports/maintenance/data?${queryString}`);
    return response.data;
  }

  async exportMaintenanceExcel(params) {
    const queryString = new URLSearchParams(params).toString();
    const response = await apiService.get(`/Reports/maintenance/excel?${queryString}`, {
      responseType: 'blob'
    });
    return response.data;
  }

  async getFinancialData(params) {
    const queryString = new URLSearchParams(params).toString();
    const response = await apiService.get(`/Reports/financial/data?${queryString}`);
    return response.data;
  }

  async exportFinancialExcel(params) {
    const queryString = new URLSearchParams(params).toString();
    const response = await apiService.get(`/Reports/financial/excel?${queryString}`, {
      responseType: 'blob'
    });
    return response.data;
  }

  async getCategories() {
    const response = await apiService.get('/Category/active');
    return response.data;
  }

  async getSuppliers() {
    const response = await apiService.get('/Supplier/active');
    return response.data;
  }

  async getEmployees() {
    const response = await apiService.get('/Employee');
    return response.data;
  }
}

export default new ReportsApi();