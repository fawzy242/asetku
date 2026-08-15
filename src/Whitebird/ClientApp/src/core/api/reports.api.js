import apiService from '../services/api.service';

export const reportsApi = {
  listEmployees: () => apiService.get('/Reports/employee-list'),
  listCategories: () => apiService.get('/Reports/category-list'),
  listSuppliers: () => apiService.get('/Reports/supplier-list'),
  listTransactionTypes: () => apiService.get('/Reports/transaction-type-list'),
  listMaintenanceTypes: () => apiService.get('/Reports/maintenance-type-list'),
  listConditions: () => apiService.get('/Reports/condition-list'),
  listStatuses: () => apiService.get('/Reports/status-list'),
  exportTransaction: (params) =>
    apiService.get('/Reports/asset-transaction/export', { params, responseType: 'blob' }),
  exportInventory: (params) =>
    apiService.get('/Reports/asset-inventory/export', { params, responseType: 'blob' }),
  exportEmployeeAsset: (params) =>
    apiService.get('/Reports/employee-asset/export', { params, responseType: 'blob' }),
  exportMaintenance: (params) =>
    apiService.get('/Reports/maintenance/export', { params, responseType: 'blob' }),
  exportFinancial: (params) =>
    apiService.get('/Reports/financial/export', { params, responseType: 'blob' }),
};