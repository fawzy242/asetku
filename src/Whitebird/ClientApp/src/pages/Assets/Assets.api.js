import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class AssetsApi {
  async getAll() { return (await apiService.get('/Asset')).data; }
  async getById(id) { return (await apiService.get(`/Asset/${id}`)).data; }
  async getGridData(params) { return (await apiService.get(`/Asset/grid${utilsHelper.buildQueryString(params)}`)).data; }
  async getByCategory(categoryId) { return (await apiService.get(`/Asset/category/${categoryId}`)).data; }
  async getByStatus(status) { return (await apiService.get(`/Asset/status/${status}`)).data; }
  async getByHolder(employeeId) { return (await apiService.get(`/Asset/holder/${employeeId}`)).data; }
  async create(data) { return (await apiService.post('/Asset', data)).data; }
  async update(id, data) { return (await apiService.put(`/Asset/${id}`, data)).data; }
  async delete(id) { return (await apiService.delete(`/Asset/${id}`)).data; }
  async search(keyword) { return (await apiService.get(`/Asset/search?keyword=${encodeURIComponent(keyword)}`)).data; }
  async getExpiredWarranty() { return (await apiService.get('/Asset/expired-warranty')).data; }
  async getUpcomingMaintenance(daysAhead = 30) { return (await apiService.get(`/Asset/upcoming-maintenance?daysAhead=${daysAhead}`)).data; }
  async getCategories() { return (await apiService.get('/Category/active')).data; }
  async getSuppliers() { return (await apiService.get('/Supplier/active')).data; }
  async getEmployees() { return (await apiService.get('/Employee')).data; }
}

export default new AssetsApi();