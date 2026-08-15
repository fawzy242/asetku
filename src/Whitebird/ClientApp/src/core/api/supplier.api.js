import apiService from '../services/api.service';

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