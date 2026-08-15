import apiService from '../services/api.service';

export const transactionApi = {
  getAll: () => apiService.get('/AssetTransaction'),
  getById: (id) => apiService.get(`/AssetTransaction/${id}`),
  create: (data) => apiService.post('/AssetTransaction', data),
  update: (id, data) => apiService.put(`/AssetTransaction/${id}`, data),
  updateApproval: (id, data) => apiService.put(`/AssetTransaction/${id}/approval`, data),
  delete: (id) => apiService.delete(`/AssetTransaction/${id}`),
  softDelete: (id) => apiService.delete(`/AssetTransaction/${id}/soft`),
  getGridAdd: (params) => apiService.get('/AssetTransaction/grid-add', { params }),
  getGridReturn: (params) => apiService.get('/AssetTransaction/grid-return', { params }),
  getGridUncomplete: (params) =>
    apiService.get('/AssetTransaction/grid-uncomplete', { params }),
  getGridComplete: (params) =>
    apiService.get('/AssetTransaction/grid-complete', { params }),
  getActiveLoans: (params) => apiService.get('/AssetTransaction/active-loans', { params }),
  getOverdueLoans: (params) => apiService.get('/AssetTransaction/overdue-loans', { params }),
  getUnderMaintenance: (params) =>
    apiService.get('/AssetTransaction/under-maintenance', { params }),
  bulkApproved: (data) => apiService.post('/AssetTransaction/bulk-approved', data),
  bulkPending: (data) => apiService.post('/AssetTransaction/bulk-pending', data),
  bulkRejected: (data) => apiService.post('/AssetTransaction/bulk-rejected', data),
  complete: (id) => apiService.post(`/AssetTransaction/${id}/complete`),
  uncomplete: (id) => apiService.post(`/AssetTransaction/${id}/uncomplete`),
  createReturn: (id, data) => apiService.post(`/AssetTransaction/${id}/create-return`, data),
  createPostMaintenance: (id, data) =>
    apiService.post(`/AssetTransaction/${id}/create-post-maintenance`, data),
  listTransactionTypeAdd: () => apiService.get('/AssetTransaction/list-transactiontype-add'),
  listTransactionTypeReturn: () => apiService.get('/AssetTransaction/list-transactiontype-return'),
  listMaintenanceType: () => apiService.get('/AssetTransaction/list-maintenance-type'),
  listCondition: () => apiService.get('/AssetTransaction/list-condition'),
  listApproveDocumentType: () => apiService.get('/AssetTransaction/list-approve-document-type'),
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
  getApproveAttachments: (id) => apiService.get(`/AssetTransaction/${id}/approve-attachments`),
};