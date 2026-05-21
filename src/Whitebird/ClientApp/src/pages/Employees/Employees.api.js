import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class EmployeesApi {
  // Basic CRUD
  async getAll() { 
    return (await apiService.get('/Employee')).data; 
  }
  
  async getById(id) { 
    return (await apiService.get(`/Employee/${id}`)).data; 
  }
  
  async getGridData(params) { 
    return (await apiService.get(`/Employee/grid${utilsHelper.buildQueryString(params)}`)).data; 
  }
  
  // UPDATED: Now uses departmentId instead of department string
  async getByDepartmentId(departmentId) { 
    return (await apiService.get(`/Employee/department/${departmentId}`)).data; 
  }
  
  // UPDATED: Now uses employmentStatus (int) instead of status string
  async getByEmploymentStatus(employmentStatus) { 
    return (await apiService.get(`/Employee/status/${employmentStatus}`)).data; 
  }
  
  async create(data) { 
    return (await apiService.post('/Employee', data)).data; 
  }
  
  async update(id, data) { 
    return (await apiService.put(`/Employee/${id}`, data)).data; 
  }
  
  async delete(id) { 
    return (await apiService.delete(`/Employee/${id}`)).data; 
  }
  
  async softDelete(id) { 
    return (await apiService.delete(`/Employee/${id}/soft`)).data; 
  }
  
  // NEW: Bulk Operations
  async bulkActivate(ids, activate) { 
    return (await apiService.post('/Employee/activate', { ids, activate })).data; 
  }
  
  // NEW: Asset Summary
  async getAssetSummary(id) { 
    return (await apiService.get(`/Employee/${id}/asset-summary`)).data; 
  }
  
  // NEW: Import
  async import(file) {
    const formData = new FormData();
    formData.append('file', file);
    return (await apiService.post('/Employee/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })).data;
  }
  
  async downloadImportTemplate() {
    const response = await apiService.get('/Employee/import/template', { responseType: 'blob' });
    return response.data;
  }
  
  // REMOVED: getByDepartment (string), getByStatus (string) - replaced by getByDepartmentId and getByEmploymentStatus
}

export default new EmployeesApi();