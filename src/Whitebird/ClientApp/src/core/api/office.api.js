import apiService from '../services/api.service';

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