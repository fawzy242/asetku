import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class AssetsApi {
  // Basic CRUD
  async getAll() { 
    return (await apiService.get('/Asset')).data; 
  }
  
  async getById(id) { 
    return (await apiService.get(`/Asset/${id}`)).data; 
  }
  
  async getGridData(params) { 
    return (await apiService.get(`/Asset/grid${utilsHelper.buildQueryString(params)}`)).data; 
  }
  
  async getByCategory(categoryId) { 
    return (await apiService.get(`/Asset/category/${categoryId}`)).data; 
  }
  
  async getByOffice(officeId) { 
    return (await apiService.get(`/Asset/office/${officeId}`)).data; 
  }
  
  async create(data) { 
    return (await apiService.post('/Asset', data)).data; 
  }
  
  async update(id, data) { 
    return (await apiService.put(`/Asset/${id}`, data)).data; 
  }
  
  async delete(id) { 
    return (await apiService.delete(`/Asset/${id}`)).data; 
  }
  
  async softDelete(id) { 
    return (await apiService.delete(`/Asset/${id}/soft`)).data; 
  }
  
  // Search & Special Queries
  async search(keyword) { 
    return (await apiService.get(`/Asset/search?keyword=${encodeURIComponent(keyword)}`)).data; 
  }
  
  async getExpiredWarranty() { 
    return (await apiService.get('/Asset/expired-warranty')).data; 
  }
  
  async getUpcomingMaintenance(daysAhead = 30) { 
    return (await apiService.get(`/Asset/upcoming-maintenance?daysAhead=${daysAhead}`)).data; 
  }
  
  // NEW: Asset Tracking
  async getAssetTracking(id) { 
    return (await apiService.get(`/Asset/tracking/${id}`)).data; 
  }
  
  async getCurrentStatus(id) { 
    return (await apiService.get(`/Asset/status/${id}`)).data; 
  }
  
  async getTransactionHistory(id) { 
    return (await apiService.get(`/Asset/history/${id}`)).data; 
  }
  
  // NEW: Bulk Operations
  async bulkActivate(ids, activate) { 
    return (await apiService.post('/Asset/activate', { ids, activate })).data; 
  }
  
  // NEW: Import
  async import(file) {
    const formData = new FormData();
    formData.append('file', file);
    return (await apiService.post('/Asset/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })).data;
  }
  
  async downloadImportTemplate() {
    const response = await apiService.get('/Asset/import/template', { responseType: 'blob' });
    return response.data;
  }
  
  // Reference Data (kept for backward compatibility)
  async getCategories() { 
    return (await apiService.get('/Category/active')).data; 
  }
  
  async getSuppliers() { 
    return (await apiService.get('/Supplier/active')).data; 
  }
  
  async getEmployees() { 
    return (await apiService.get('/Employee')).data; 
  }
  
  async getOffices() { 
    return (await apiService.get('/Office/active')).data; 
  }
  
  // REMOVED: getByStatus, getByHolder (obsolete)
}

export default new AssetsApi();