import apiService from '../services/api.service';

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