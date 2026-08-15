import apiService from '../services/api.service';

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
  getImportTemplate: () => apiService.get('/Employee/import/template', { responseType: 'blob' }),
  dropdown: () => apiService.get('/Employee/dropdown'),
  officeList: () => apiService.get('/Employee/office-list'),
  departmentList: () => apiService.get('/Employee/department-list'),
  masterDataList: (referenceName) =>
    apiService.get(`/Employee/masterdata-list?referenceName=${referenceName}`),
  getPhoto: (id) => apiService.get(`/Employee/${id}/photo`, { responseType: 'blob' }),
  getAttachments: (id) => apiService.get(`/Employee/${id}/attachments`),
};