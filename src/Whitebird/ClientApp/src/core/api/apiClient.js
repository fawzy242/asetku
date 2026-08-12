// src/core/api/apiClient.js
import apiService from '../services/api.service';

/**
 * CENTRAL API CLIENT
 * Single source of truth untuk semua endpoint backend v2
 */
const apiClient = {
  // ============================================================
  // AUTH
  // ============================================================
  auth: {
    login: (data) => apiService.post('/Auth/login', data),
    logout: (data) => apiService.post('/Auth/logout', data),
    forgotPassword: (data) => apiService.post('/Auth/forgot-password', data),
    resetPassword: (data) => apiService.post('/Auth/reset-password', data),
    resetPasswordWithToken: (data) => apiService.post('/Auth/reset-password-with-token', data),
    validateSession: () => apiService.get('/Auth/validate-session'),
  },

  // ============================================================
  // USER
  // ============================================================
  user: {
    getMe: () => apiService.get('/User/me'),
    updateProfile: (data) => apiService.put('/User/profile', data),
    changePassword: (data) => apiService.post('/User/change-password', data),
    uploadPhoto: (data) =>
      apiService.post('/User/profile-photo', data, {
        headers: { 'Content-Type': 'multipart/form-data' },
      }),
    getPhoto: () => apiService.get('/User/profile-photo', { responseType: 'blob' }),
    deletePhoto: () => apiService.delete('/User/profile-photo'),
  },

  // ============================================================
  // ASSET
  // ============================================================
  asset: {
    getAll: () => apiService.get('/Asset'),
    getById: (id) => apiService.get(`/Asset/${id}`),
    getGrid: (params) => apiService.get('/Asset/grid', { params }),
    create: (data) => apiService.post('/Asset', data),
    update: (id, data) => apiService.put(`/Asset/${id}`, data),
    delete: (id) => apiService.delete(`/Asset/${id}`),
    softDelete: (id) => apiService.delete(`/Asset/${id}/soft`),
    // Bulk operations
    activate: (data) => apiService.post('/Asset/activate', data),
    deactivate: (data) => apiService.post('/Asset/deactivate', data),
    // Import
    import: (file) => {
      const fd = new FormData();
      fd.append('file', file);
      return apiService.post('/Asset/import', fd, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    getImportTemplate: () => apiService.get('/Asset/import/template', { responseType: 'blob' }),
    // Asset dropdown for transaction - BUTUH PARAMETER transactionType
    listByTransaction: (transactionType) => 
      apiService.get(`/Asset/list-by-transaction?transactionType=${transactionType}`),
    // Photo profile
    uploadPhoto: (id, file) => {
      const fd = new FormData();
      fd.append('file', file);
      return apiService.post(`/Asset/${id}/photo`, fd, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    getPhoto: (id) => apiService.get(`/Asset/${id}/photo`, { responseType: 'blob' }),
    deletePhoto: (id) => apiService.delete(`/Asset/${id}/photo`),
  },

  // ============================================================
  // EMPLOYEE
  // ============================================================
  employee: {
    getAll: () => apiService.get('/Employee'),
    getById: (id) => apiService.get(`/Employee/${id}`),
    getGrid: (params) => apiService.get('/Employee/grid', { params }),
    create: (data) => apiService.post('/Employee', data),
    update: (id, data) => apiService.put(`/Employee/${id}`, data),
    delete: (id) => apiService.delete(`/Employee/${id}`),
    softDelete: (id) => apiService.delete(`/Employee/${id}/soft`),
    // Bulk operations
    activate: (data) => apiService.post('/Employee/activate', data),
    deactivate: (data) => apiService.post('/Employee/deactivate', data),
    // Import
    import: (file) => {
      const fd = new FormData();
      fd.append('file', file);
      return apiService.post('/Employee/import', fd, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    getImportTemplate: () => apiService.get('/Employee/import/template', { responseType: 'blob' }),
    // Dropdowns
    dropdown: () => apiService.get('/Employee/dropdown'),
    officeList: () => apiService.get('/Employee/office-list'),
    departmentList: () => apiService.get('/Employee/department-list'),
    masterDataList: (refName) =>
      apiService.get(`/Employee/masterdata-list?referenceName=${refName}`),
    // Photo profile
    uploadPhoto: (id, file) => {
      const fd = new FormData();
      fd.append('file', file);
      return apiService.post(`/Employee/${id}/photo`, fd, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    getPhoto: (id) => apiService.get(`/Employee/${id}/photo`, { responseType: 'blob' }),
    deletePhoto: (id) => apiService.delete(`/Employee/${id}/photo`),
  },

  // ============================================================
  // OFFICE
  // ============================================================
  office: {
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
  },

  // ============================================================
  // SUPPLIER
  // ============================================================
  supplier: {
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
  },

  // ============================================================
  // CATEGORY
  // ============================================================
  category: {
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
  },

  // ============================================================
  // DEPARTMENT
  // ============================================================
  department: {
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
  },

  // ============================================================
  // ASSET TRANSACTION
  // ============================================================
  transaction: {
    getAll: () => apiService.get('/AssetTransaction'),
    getById: (id) => apiService.get(`/AssetTransaction/${id}`),
    create: (data) => apiService.post('/AssetTransaction', data),
    update: (id, data) => apiService.put(`/AssetTransaction/${id}`, data),
    delete: (id) => apiService.delete(`/AssetTransaction/${id}`),
    softDelete: (id) => apiService.delete(`/AssetTransaction/${id}/soft`),
    // Grids
    getGridAdd: (params) => apiService.get('/AssetTransaction/grid-add', { params }),
    getGridReturn: (params) => apiService.get('/AssetTransaction/grid-return', { params }),
    getGridPendingApproval: (params) =>
      apiService.get('/AssetTransaction/grid-pending-approval', { params }),
    // Outstanding
    getActiveLoans: (params) => apiService.get('/AssetTransaction/active-loans', { params }),
    getOverdueLoans: (params) => apiService.get('/AssetTransaction/overdue-loans', { params }),
    getUnderMaintenance: (params) =>
      apiService.get('/AssetTransaction/under-maintenance', { params }),
    // Bulk operations
    bulkApproved: (data) => apiService.post('/AssetTransaction/bulk-approved', data),
    bulkPending: (data) => apiService.post('/AssetTransaction/bulk-pending', data),
    bulkRejected: (data) => apiService.post('/AssetTransaction/bulk-rejected', data),
    // Complete/Uncomplete
    complete: (id) => apiService.post(`/AssetTransaction/${id}/complete`),
    uncomplete: (id) => apiService.post(`/AssetTransaction/${id}/uncomplete`),
    // Shortcuts
    createReturn: (id, data) => apiService.post(`/AssetTransaction/${id}/create-return`, data),
    createPostMaintenance: (id, data) =>
      apiService.post(`/AssetTransaction/${id}/create-post-maintenance`, data),
    // Attachment
    uploadAttachment: (id, file, fileCategory, description, order) => {
      const fd = new FormData();
      fd.append('file', file);
      if (fileCategory) fd.append('fileCategory', fileCategory);
      if (description) fd.append('description', description);
      if (order !== undefined) fd.append('order', order);
      return apiService.post(`/AssetTransaction/${id}/attachment`, fd, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    getAttachments: (id) => apiService.get(`/AssetTransaction/${id}/attachment`),
    deleteAttachment: (id, attId) =>
      apiService.delete(`/AssetTransaction/${id}/attachment/${attId}`),
    // Approve Attachment
    uploadApproveAttachment: (id, file, docType, description, order) => {
      const fd = new FormData();
      fd.append('file', file);
      fd.append('documentType', docType);
      if (description) fd.append('description', description);
      if (order !== undefined) fd.append('order', order);
      return apiService.post(`/AssetTransaction/${id}/approve-attachment`, fd, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    getApproveAttachments: (id) => apiService.get(`/AssetTransaction/${id}/approve-attachment`),
    deleteApproveAttachment: (id, attId) =>
      apiService.delete(`/AssetTransaction/${id}/approve-attachment/${attId}`),
    approveAttachmentAction: (id, attId, data) =>
      apiService.post(`/AssetTransaction/${id}/approve-attachment/${attId}/action`, data),
    getRequiredDocTypes: (id) =>
      apiService.get(`/AssetTransaction/${id}/approve-attachment/required`),
    // Dropdowns
    listTransactionTypeAdd: () => apiService.get('/AssetTransaction/list-transactiontype-add'),
    listTransactionTypeReturn: () => apiService.get('/AssetTransaction/list-transactiontype-return'),
    listMaintenanceType: () => apiService.get('/AssetTransaction/list-maintenance-type'),
    listCondition: () => apiService.get('/AssetTransaction/list-condition'),
    listApproveDocumentType: () => apiService.get('/AssetTransaction/list-approve-document-type'),
    listEmployee: () => apiService.get('/AssetTransaction/list-employee'),
    // Import
    import: (file) => {
      const fd = new FormData();
      fd.append('file', file);
      return apiService.post('/AssetTransaction/import', fd, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    getImportTemplate: () =>
      apiService.get('/AssetTransaction/import/template', { responseType: 'blob' }),
  },

  // ============================================================
  // SCHEDULE HANDOVER
  // ============================================================
  scheduleHandover: {
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
  },

  // ============================================================
  // SCHEDULE RETURN
  // ============================================================
  scheduleReturn: {
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
  },

  // ============================================================
  // SCHEDULE MAINTENANCE
  // ============================================================
  scheduleMaintenance: {
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
  },

  // ============================================================
  // DASHBOARD
  // ============================================================
  dashboard: {
    getStats: () => apiService.get('/Dashboard/stats'),
    getMonthlyStats: (year) =>
      apiService.get(`/Dashboard/monthly-stats?year=${year || new Date().getFullYear()}`),
    getCategoryBreakdown: () => apiService.get('/Dashboard/category-breakdown'),
    getRecentTransactions: (limit = 10) =>
      apiService.get(`/Dashboard/recent-transactions?limit=${limit}`),
    getExpiredWarranty: () => apiService.get('/Dashboard/expired-warranty'),
    getScheduleStats: () => apiService.get('/Dashboard/schedule-stats'),
    getDetailStats: (params) => apiService.get('/Dashboard/detail-stats', { params }),
  },

  // ============================================================
  // ASSET TRACKING
  // ============================================================
  assetTracking: {
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
  },

  // ============================================================
  // EMPLOYEE SUMMARY
  // ============================================================
  employeeSummary: {
    lookup: (employeeId) => apiService.get(`/EmployeeSummary/lookup/${employeeId}`),
    getDetail: (employeeId) => apiService.get(`/EmployeeSummary/${employeeId}`),
    getStats: (employeeId) => apiService.get(`/EmployeeSummary/${employeeId}/stats`),
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
  },

  // ============================================================
  // HOME (Global Search)
  // ============================================================
  home: {
    search: (keyword) => apiService.get(`/Home/search?keyword=${encodeURIComponent(keyword)}`),
  },

  // ============================================================
  // REPORTS
  // ============================================================
  reports: {
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
  },
};

export default apiClient;