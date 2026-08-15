import apiService from '../services/api.service';

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