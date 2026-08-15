import apiService from '../services/api.service';

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