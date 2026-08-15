import apiService from '../services/api.service';

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
  getImportTemplate: () => apiService.get('/Asset/import/template', { responseType: 'blob' }),
  listByTransaction: (transactionType) =>
    apiService.get(`/Asset/list-by-transaction?transactionType=${transactionType}`),
  getPhoto: (id) => apiService.get(`/Asset/${id}/photo`, { responseType: 'blob' }),
  getAttachments: (id) => apiService.get(`/Asset/${id}/attachments`),
};