import apiService from '../../core/services/api.service';

class ReportsApi {
  // Dashboard
  async getDashboardStats() { 
    return (await apiService.get('/Reports/dashboard/stats')).data; 
  }
  
  // ============================================================
  // ASSET TRANSACTION REPORT
  // ============================================================
  
  async getAssetTransactionData(params, options = {}) { 
    const queryParams = new URLSearchParams();
    if (params.startDate) queryParams.append('startDate', params.startDate);
    if (params.endDate) queryParams.append('endDate', params.endDate);
    if (params.transactionType) queryParams.append('transactionType', params.transactionType);
    
    return (await apiService.get(`/Reports/asset-transaction/data?${queryParams.toString()}`, options)).data; 
  }
  
  async exportAssetTransactionExcel(params) {
    const queryParams = new URLSearchParams();
    if (params.startDate) queryParams.append('startDate', params.startDate);
    if (params.endDate) queryParams.append('endDate', params.endDate);
    if (params.transactionType) queryParams.append('transactionType', params.transactionType);
    
    const response = await apiService.get(`/Reports/asset-transaction/excel?${queryParams.toString()}`, { 
      responseType: 'blob' 
    });
    return response.data;
  }
  
  // ============================================================
  // ASSET INVENTORY REPORT - UPDATED PARAMETERS
  // ============================================================
  
  async getAssetInventoryData(params, options = {}) { 
    const queryParams = new URLSearchParams();
    // Status filter: Available, Assigned, Damaged
    if (params.status) queryParams.append('status', params.status);
    if (params.categoryId) queryParams.append('categoryId', params.categoryId);
    if (params.supplierId) queryParams.append('supplierId', params.supplierId);
    
    return (await apiService.get(`/Reports/asset-inventory/data?${queryParams.toString()}`, options)).data; 
  }
  
  async exportAssetInventoryExcel(params) {
    const queryParams = new URLSearchParams();
    if (params.status) queryParams.append('status', params.status);
    if (params.categoryId) queryParams.append('categoryId', params.categoryId);
    if (params.supplierId) queryParams.append('supplierId', params.supplierId);
    
    const response = await apiService.get(`/Reports/asset-inventory/excel?${queryParams.toString()}`, { 
      responseType: 'blob' 
    });
    return response.data;
  }
  
  // ============================================================
  // EMPLOYEE ASSET REPORT - ONLY employeeId parameter
  // ============================================================
  
  async getEmployeeAssetData(params, options = {}) { 
    const queryParams = new URLSearchParams();
    // ONLY employeeId parameter, NO department
    if (params.employeeId) queryParams.append('employeeId', params.employeeId);
    
    return (await apiService.get(`/Reports/employee-asset/data?${queryParams.toString()}`, options)).data; 
  }
  
  async exportEmployeeAssetExcel(params) {
    const queryParams = new URLSearchParams();
    if (params.employeeId) queryParams.append('employeeId', params.employeeId);
    
    const response = await apiService.get(`/Reports/employee-asset/excel?${queryParams.toString()}`, { 
      responseType: 'blob' 
    });
    return response.data;
  }
  
  // ============================================================
  // MAINTENANCE REPORT
  // ============================================================
  
  async getMaintenanceData(params, options = {}) { 
    const queryParams = new URLSearchParams();
    if (params.startDate) queryParams.append('startDate', params.startDate);
    if (params.endDate) queryParams.append('endDate', params.endDate);
    if (params.isUpcoming !== undefined && params.isUpcoming !== null) {
      queryParams.append('isUpcoming', params.isUpcoming);
    }
    
    return (await apiService.get(`/Reports/maintenance/data?${queryParams.toString()}`, options)).data; 
  }
  
  async exportMaintenanceExcel(params) {
    const queryParams = new URLSearchParams();
    if (params.startDate) queryParams.append('startDate', params.startDate);
    if (params.endDate) queryParams.append('endDate', params.endDate);
    if (params.isUpcoming !== undefined && params.isUpcoming !== null) {
      queryParams.append('isUpcoming', params.isUpcoming);
    }
    
    const response = await apiService.get(`/Reports/maintenance/excel?${queryParams.toString()}`, { 
      responseType: 'blob' 
    });
    return response.data;
  }
  
  // ============================================================
  // FINANCIAL REPORT
  // ============================================================
  
  async getFinancialData(params, options = {}) { 
    const queryParams = new URLSearchParams();
    if (params.startDate) queryParams.append('startDate', params.startDate);
    if (params.endDate) queryParams.append('endDate', params.endDate);
    
    return (await apiService.get(`/Reports/financial/data?${queryParams.toString()}`, options)).data; 
  }
  
  async exportFinancialExcel(params) {
    const queryParams = new URLSearchParams();
    if (params.startDate) queryParams.append('startDate', params.startDate);
    if (params.endDate) queryParams.append('endDate', params.endDate);
    
    const response = await apiService.get(`/Reports/financial/excel?${queryParams.toString()}`, { 
      responseType: 'blob' 
    });
    return response.data;
  }

  // ============================================================
  // REFERENCE DATA FOR FILTERS
  // ============================================================
  
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