// src/core/api/api.js
//
// Centralized API endpoint contracts.
//
// This is the single source of truth for every HTTP endpoint the frontend
// calls. It is intentionally split from the transport layer
// (core/services/api.service.js) which owns the axios instance, interceptors,
// cancellation, and error normalization.
//
// Shape convention: every export is a plain object mapping a semantic method
// name to a thin function that wraps `apiService.<verb>(url, …)`.
//
// All domains live here. The legacy per-domain `*.api.js` files and the
// barrel `index.js` have been removed. Consumers should import directly from
// this module.

import apiService from '../services/api.service';

// ============================================================
// AUTH
// ============================================================
export const authApi = {
  login: (data) => apiService.post('/Auth/login', data),
  logout: (data) => apiService.post('/Auth/logout', data),
  validateSession: () => apiService.get('/Auth/validate-session'),
};

// ============================================================
// USER (self-service)
// ============================================================
export const userApi = {
  getMe: () => apiService.get('/User/me'),
  updateProfile: (data) => apiService.put('/User/profile', data),
  changePassword: (data) => apiService.post('/User/change-password', data),
  uploadPhoto: (data) =>
    apiService.post('/User/profile-photo', data, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),
  getPhoto: () => apiService.get('/User/profile-photo', { responseType: 'blob' }),
  deletePhoto: () => apiService.delete('/User/profile-photo'),
};

// ============================================================
// HOME (global search)
// ============================================================
export const homeApi = {
  search: (keyword) =>
    apiService.get(`/Home/search?keyword=${encodeURIComponent(keyword)}`),
};

// ============================================================
// DASHBOARD
// ============================================================
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

// ============================================================
// REPORTS
// ============================================================
export const reportsApi = {
  listEmployees: () => apiService.get('/Reports/employee-list'),
  listCategories: () => apiService.get('/Reports/category-list'),
  listSuppliers: () => apiService.get('/Reports/supplier-list'),
  listTransactionTypes: () => apiService.get('/Reports/transaction-type-list'),
  listMaintenanceTypes: () => apiService.get('/Reports/maintenance-type-list'),
  listConditions: () => apiService.get('/Reports/condition-list'),
  listStatuses: () => apiService.get('/Reports/status-list'),
  exportTransaction: (params) =>
    apiService.get('/Reports/asset-transaction/export', {
      params,
      responseType: 'blob',
    }),
  exportInventory: (params) =>
    apiService.get('/Reports/asset-inventory/export', {
      params,
      responseType: 'blob',
    }),
  exportEmployeeAsset: (params) =>
    apiService.get('/Reports/employee-asset/export', {
      params,
      responseType: 'blob',
    }),
  exportMaintenance: (params) =>
    apiService.get('/Reports/maintenance/export', {
      params,
      responseType: 'blob',
    }),
  exportFinancial: (params) =>
    apiService.get('/Reports/financial/export', {
      params,
      responseType: 'blob',
    }),
};

// ============================================================
// ASSET
// ============================================================
export const assetApi = {
  getAll: () => apiService.get('/Asset'),
  getById: (id) => apiService.get(`/Asset/${id}`),
  getGrid: (params) => apiService.get('/Asset/grid', { params }),
  create: (data) => apiService.post('/Asset', data),
  update: (id, data) => apiService.put(`/Asset/${id}`, data),
  delete: (id) => apiService.delete(`/Asset/${id}`),
  softDelete: (id) => apiService.delete(`/Asset/${id}/soft`),
  activate: (data) => apiService.post('/Asset/activate', data),
  deactivate: (data) => apiService.post('/Asset/deactivate', data),
  import: (file) => {
    const fd = new FormData();
    fd.append('file', file);
    return apiService.post('/Asset/import', fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  getImportTemplate: () =>
    apiService.get('/Asset/import/template', { responseType: 'blob' }),
  listByTransaction: (transactionType) =>
    apiService.get(`/Asset/list-by-transaction?transactionType=${transactionType}`),
  getPhoto: (id) => apiService.get(`/Asset/${id}/photo`, { responseType: 'blob' }),
  getAttachments: (id) => apiService.get(`/Asset/${id}/attachments`),
};

// ============================================================
// ASSET TRACKING
// ============================================================
export const assetTrackingApi = {
  lookup: (assetId) => apiService.get(`/AssetTracking/lookup/${assetId}`),
  getDetail: (assetId) => apiService.get(`/AssetTracking/${assetId}`),
  getStats: (assetId) => apiService.get(`/AssetTracking/${assetId}/stats`),
  getTimeline: (assetId) => apiService.get(`/AssetTracking/${assetId}/timeline`),
  getScheduleStats: (assetId, params) =>
    apiService.get(`/AssetTracking/${assetId}/schedule-stats`, { params }),
  getTransactionTypeStats: (assetId) =>
    apiService.get(`/AssetTracking/${assetId}/transactiontype-stats`),
  getEmployeeHistory: (assetId, params) =>
    apiService.get(`/AssetTracking/${assetId}/employee-history`, { params }),
  getPhoto: (assetId) =>
    apiService.get(`/AssetTracking/${assetId}/photo`, { responseType: 'blob' }),
  getAttachments: (assetId) => apiService.get(`/AssetTracking/${assetId}/attachments`),
};

// ============================================================
// ASSET TRANSACTION
// ============================================================
export const transactionApi = {
  getAll: () => apiService.get('/AssetTransaction'),
  getById: (id) => apiService.get(`/AssetTransaction/${id}`),
  create: (data) => apiService.post('/AssetTransaction', data),
  update: (id, data) => apiService.put(`/AssetTransaction/${id}`, data),
  updateApproval: (id, data) =>
    apiService.put(`/AssetTransaction/${id}/approval`, data),
  delete: (id) => apiService.delete(`/AssetTransaction/${id}`),
  softDelete: (id) => apiService.delete(`/AssetTransaction/${id}/soft`),
  getGridAdd: (params) => apiService.get('/AssetTransaction/grid-add', { params }),
  getGridReturn: (params) =>
    apiService.get('/AssetTransaction/grid-return', { params }),
  getGridUncomplete: (params) =>
    apiService.get('/AssetTransaction/grid-uncomplete', { params }),
  getGridComplete: (params) =>
    apiService.get('/AssetTransaction/grid-complete', { params }),
  getActiveLoans: (params) =>
    apiService.get('/AssetTransaction/active-loans', { params }),
  getOverdueLoans: (params) =>
    apiService.get('/AssetTransaction/overdue-loans', { params }),
  getUnderMaintenance: (params) =>
    apiService.get('/AssetTransaction/under-maintenance', { params }),
  bulkApproved: (data) => apiService.post('/AssetTransaction/bulk-approved', data),
  bulkPending: (data) => apiService.post('/AssetTransaction/bulk-pending', data),
  bulkRejected: (data) => apiService.post('/AssetTransaction/bulk-rejected', data),
  complete: (id) => apiService.post(`/AssetTransaction/${id}/complete`),
  uncomplete: (id) => apiService.post(`/AssetTransaction/${id}/uncomplete`),
  createReturn: (id, data) =>
    apiService.post(`/AssetTransaction/${id}/create-return`, data),
  createPostMaintenance: (id, data) =>
    apiService.post(`/AssetTransaction/${id}/create-post-maintenance`, data),
  listTransactionTypeAdd: () =>
    apiService.get('/AssetTransaction/list-transactiontype-add'),
  listTransactionTypeReturn: () =>
    apiService.get('/AssetTransaction/list-transactiontype-return'),
  listMaintenanceType: () =>
    apiService.get('/AssetTransaction/list-maintenance-type'),
  listCondition: () => apiService.get('/AssetTransaction/list-condition'),
  listApproveDocumentType: () =>
    apiService.get('/AssetTransaction/list-approve-document-type'),
  listEmployee: () => apiService.get('/AssetTransaction/list-employee'),
  import: (file) => {
    const fd = new FormData();
    fd.append('file', file);
    return apiService.post('/AssetTransaction/import', fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  getImportTemplate: () =>
    apiService.get('/AssetTransaction/import/template', { responseType: 'blob' }),
  getAttachments: (id) => apiService.get(`/AssetTransaction/${id}/attachments`),
  getApproveAttachments: (id) =>
    apiService.get(`/AssetTransaction/${id}/approve-attachments`),
};

// ============================================================
// EMPLOYEE
// ============================================================
export const employeeApi = {
  getAll: () => apiService.get('/Employee'),
  getById: (id) => apiService.get(`/Employee/${id}`),
  getGrid: (params) => apiService.get('/Employee/grid', { params }),
  create: (data) => apiService.post('/Employee', data),
  update: (id, data) => apiService.put(`/Employee/${id}`, data),
  delete: (id) => apiService.delete(`/Employee/${id}`),
  softDelete: (id) => apiService.delete(`/Employee/${id}/soft`),
  activate: (data) => apiService.post('/Employee/activate', data),
  deactivate: (data) => apiService.post('/Employee/deactivate', data),
  import: (file) => {
    const fd = new FormData();
    fd.append('file', file);
    return apiService.post('/Employee/import', fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  getImportTemplate: () =>
    apiService.get('/Employee/import/template', { responseType: 'blob' }),
  dropdown: () => apiService.get('/Employee/dropdown'),
  officeList: () => apiService.get('/Employee/office-list'),
  departmentList: () => apiService.get('/Employee/department-list'),
  masterDataList: (referenceName) =>
    apiService.get(`/Employee/masterdata-list?referenceName=${referenceName}`),
  getPhoto: (id) => apiService.get(`/Employee/${id}/photo`, { responseType: 'blob' }),
  getAttachments: (id) => apiService.get(`/Employee/${id}/attachments`),
};

// ============================================================
// EMPLOYEE SUMMARY
// ============================================================
export const employeeSummaryApi = {
  lookup: (employeeId) =>
    apiService.get(`/EmployeeSummary/lookup/${employeeId}`),
  getDetail: (employeeId) => apiService.get(`/EmployeeSummary/${employeeId}`),
  getStats: (employeeId) =>
    apiService.get(`/EmployeeSummary/${employeeId}/stats`),
  getCurrentHolding: (employeeId, params) =>
    apiService.get(`/EmployeeSummary/${employeeId}/current-holding`, { params }),
  getAssetsHistory: (employeeId, params) =>
    apiService.get(`/EmployeeSummary/${employeeId}/assets-history`, { params }),
  getTransactionHistory: (employeeId, params) =>
    apiService.get(`/EmployeeSummary/${employeeId}/transaction-history`, { params }),
  getCurrentHoldingStats: (employeeId) =>
    apiService.get(`/EmployeeSummary/${employeeId}/current-holding-stats`),
  getTransactionHistoryStats: (employeeId) =>
    apiService.get(`/EmployeeSummary/${employeeId}/transaction-history-stats`),
  getScheduleStats: (employeeId) =>
    apiService.get(`/EmployeeSummary/${employeeId}/schedule-stats`),
  getPhoto: (employeeId) =>
    apiService.get(`/EmployeeSummary/${employeeId}/photo`, { responseType: 'blob' }),
  getAttachments: (employeeId) =>
    apiService.get(`/EmployeeSummary/${employeeId}/attachments`),
};

// ============================================================
// CATEGORY
// ============================================================
export const categoryApi = {
  getAll: () => apiService.get('/Category'),
  getById: (id) => apiService.get(`/Category/${id}`),
  getGrid: (params) => apiService.get('/Category/grid', { params }),
  create: (data) => apiService.post('/Category', data),
  update: (id, data) => apiService.put(`/Category/${id}`, data),
  delete: (id) => apiService.delete(`/Category/${id}`),
  softDelete: (id) => apiService.delete(`/Category/${id}/soft`),
  dropdown: () => apiService.get('/Category/dropdown'),
  activate: (data) => apiService.post('/Category/activate', data),
  deactivate: (data) => apiService.post('/Category/deactivate', data),
};

// ============================================================
// DEPARTMENT
// ============================================================
export const departmentApi = {
  getAll: () => apiService.get('/Department'),
  getById: (id) => apiService.get(`/Department/${id}`),
  getGrid: (params) => apiService.get('/Department/grid', { params }),
  create: (data) => apiService.post('/Department', data),
  update: (id, data) => apiService.put(`/Department/${id}`, data),
  delete: (id) => apiService.delete(`/Department/${id}`),
  softDelete: (id) => apiService.delete(`/Department/${id}/soft`),
  dropdown: () => apiService.get('/Department/dropdown'),
  activate: (data) => apiService.post('/Department/activate', data),
  deactivate: (data) => apiService.post('/Department/deactivate', data),
};

// ============================================================
// OFFICE
// ============================================================
export const officeApi = {
  getAll: () => apiService.get('/Office'),
  getById: (id) => apiService.get(`/Office/${id}`),
  getGrid: (params) => apiService.get('/Office/grid', { params }),
  create: (data) => apiService.post('/Office', data),
  update: (id, data) => apiService.put(`/Office/${id}`, data),
  delete: (id) => apiService.delete(`/Office/${id}`),
  softDelete: (id) => apiService.delete(`/Office/${id}/soft`),
  dropdown: () => apiService.get('/Office/dropdown'),
  activate: (data) => apiService.post('/Office/activate', data),
  deactivate: (data) => apiService.post('/Office/deactivate', data),
};

// ============================================================
// SUPPLIER
// ============================================================
export const supplierApi = {
  getAll: () => apiService.get('/Supplier'),
  getById: (id) => apiService.get(`/Supplier/${id}`),
  getGrid: (params) => apiService.get('/Supplier/grid', { params }),
  create: (data) => apiService.post('/Supplier', data),
  update: (id, data) => apiService.put(`/Supplier/${id}`, data),
  delete: (id) => apiService.delete(`/Supplier/${id}`),
  softDelete: (id) => apiService.delete(`/Supplier/${id}/soft`),
  dropdown: () => apiService.get('/Supplier/dropdown'),
  activate: (data) => apiService.post('/Supplier/activate', data),
  deactivate: (data) => apiService.post('/Supplier/deactivate', data),
};

// ============================================================
// SCHEDULE HANDOVER
// ============================================================
export const scheduleHandoverApi = {
  getAll: () => apiService.get('/ScheduleHandover'),
  getById: (id) => apiService.get(`/ScheduleHandover/${id}`),
  getGrid: (params) => apiService.get('/ScheduleHandover/grid', { params }),
  create: (data) => apiService.post('/ScheduleHandover', data),
  update: (id, data) => apiService.put(`/ScheduleHandover/${id}`, data),
  delete: (id) => apiService.delete(`/ScheduleHandover/${id}`),
  softDelete: (id) => apiService.delete(`/ScheduleHandover/${id}/soft`),
  activate: (data) => apiService.post('/ScheduleHandover/activate', data),
  deactivate: (data) => apiService.post('/ScheduleHandover/deactivate', data),
  generate: (data) => apiService.post('/ScheduleHandover/generate', data),
};

// ============================================================
// SCHEDULE RETURN
// ============================================================
export const scheduleReturnApi = {
  getAll: () => apiService.get('/ScheduleReturn'),
  getById: (id) => apiService.get(`/ScheduleReturn/${id}`),
  getGrid: (params) => apiService.get('/ScheduleReturn/grid', { params }),
  create: (data) => apiService.post('/ScheduleReturn', data),
  update: (id, data) => apiService.put(`/ScheduleReturn/${id}`, data),
  delete: (id) => apiService.delete(`/ScheduleReturn/${id}`),
  softDelete: (id) => apiService.delete(`/ScheduleReturn/${id}/soft`),
  activate: (data) => apiService.post('/ScheduleReturn/activate', data),
  deactivate: (data) => apiService.post('/ScheduleReturn/deactivate', data),
  generate: (data) => apiService.post('/ScheduleReturn/generate', data),
};

// ============================================================
// SCHEDULE MAINTENANCE
// ============================================================
export const scheduleMaintenanceApi = {
  getAll: () => apiService.get('/ScheduleMaintenance'),
  getById: (id) => apiService.get(`/ScheduleMaintenance/${id}`),
  getGrid: (params) => apiService.get('/ScheduleMaintenance/grid', { params }),
  create: (data) => apiService.post('/ScheduleMaintenance', data),
  update: (id, data) => apiService.put(`/ScheduleMaintenance/${id}`, data),
  delete: (id) => apiService.delete(`/ScheduleMaintenance/${id}`),
  softDelete: (id) => apiService.delete(`/ScheduleMaintenance/${id}/soft`),
  activate: (data) => apiService.post('/ScheduleMaintenance/activate', data),
  deactivate: (data) => apiService.post('/ScheduleMaintenance/deactivate', data),
  generate: (data) => apiService.post('/ScheduleMaintenance/generate', data),
};

// ============================================================
// ROLES
// ============================================================
export const rolesApi = {
  getAll: () => apiService.get('/Roles'),
  getById: (id) => apiService.get(`/Roles/${id}`),
  getGrid: (params) => apiService.get('/Roles/grid', { params }),
  create: (data) => apiService.post('/Roles', data),
  update: (id, data) => apiService.put(`/Roles/${id}`, data),
  delete: (id) => apiService.delete(`/Roles/${id}`),
  softDelete: (id) => apiService.delete(`/Roles/${id}/soft`),
  dropdown: () => apiService.get('/Roles/dropdown'),
  activate: (data) => apiService.post('/Roles/activate', data),
  deactivate: (data) => apiService.post('/Roles/deactivate', data),
};

// ============================================================
// USER ROLES (assignment)
// ============================================================
export const userRolesApi = {
  getGrid: (params) => apiService.get('/UserRoles/grid', { params }),
  getByUser: (userId) => apiService.get(`/UserRoles/by-user/${userId}`),
  getByRole: (roleId) => apiService.get(`/UserRoles/by-role/${roleId}`),
  assign: (data) => apiService.post('/UserRoles/assign', data),
  unassign: (userId, roleId) =>
    apiService.delete(`/UserRoles/user/${userId}/role/${roleId}`),
  replaceUserRoles: (userId, data) =>
    apiService.put(`/UserRoles/user/${userId}`, data),
};