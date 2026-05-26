import apiService from '../../core/services/api.service';

class ReportsApi {
  async getDashboardStats() { 
    return (await apiService.get('/Reports/dashboard/stats')).data; 
  }
  
  async getAssetTransactionData(params, options = {}) { 
    return (await apiService.get(`/Reports/asset-transaction/data?${new URLSearchParams(params)}`, options)).data; 
  }
  
  async getAssetInventoryData(params, options = {}) { 
    return (await apiService.get(`/Reports/asset-inventory/data?${new URLSearchParams(params)}`, options)).data; 
  }
  
  async getEmployeeAssetData(params, options = {}) { 
    return (await apiService.get(`/Reports/employee-asset/data?${new URLSearchParams(params)}`, options)).data; 
  }
  
  async getMaintenanceData(params, options = {}) { 
    return (await apiService.get(`/Reports/maintenance/data?${new URLSearchParams(params)}`, options)).data; 
  }
  
  async getFinancialData(params, options = {}) { 
    return (await apiService.get(`/Reports/financial/data?${new URLSearchParams(params)}`, options)).data; 
  }

  // Excel export endpoints
  async exportAssetTransactionExcel(params, options = {}) {
    const response = await apiService.get(`/Reports/asset-transaction/excel?${new URLSearchParams(params)}`, { ...options, responseType: 'blob' });
    return response.data;
  }
  
  async exportAssetInventoryExcel(params, options = {}) {
    const response = await apiService.get(`/Reports/asset-inventory/excel?${new URLSearchParams(params)}`, { ...options, responseType: 'blob' });
    return response.data;
  }
  
  async exportEmployeeAssetExcel(params, options = {}) {
    const response = await apiService.get(`/Reports/employee-asset/excel?${new URLSearchParams(params)}`, { ...options, responseType: 'blob' });
    return response.data;
  }
  
  async exportMaintenanceExcel(params, options = {}) {
    const response = await apiService.get(`/Reports/maintenance/excel?${new URLSearchParams(params)}`, { ...options, responseType: 'blob' });
    return response.data;
  }
  
  async exportFinancialExcel(params, options = {}) {
    const response = await apiService.get(`/Reports/financial/excel?${new URLSearchParams(params)}`, { ...options, responseType: 'blob' });
    return response.data;
  }

  async getCategories(options = {}) { 
    return (await apiService.get('/Category/active', options)).data; 
  }
  
  async getSuppliers(options = {}) { 
    return (await apiService.get('/Supplier/active', options)).data; 
  }
  
  async getEmployees(options = {}) { 
    return (await apiService.get('/Employee', options)).data; 
  }
}

export default new ReportsApi();