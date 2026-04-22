import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class AssetsApi {
  async getAll() {
    const response = await apiService.get('/Asset');
    return response.data;
  }

  async getById(id) {
    const response = await apiService.get(`/Asset/${id}`);
    return response.data;
  }

  async getGridData(params) {
    const queryString = utilsHelper.buildQueryString(params);
    const response = await apiService.get(`/Asset/grid${queryString}`);
    return response.data;
  }

  async getByCategory(categoryId) {
    const response = await apiService.get(`/Asset/category/${categoryId}`);
    return response.data;
  }

  async getByStatus(status) {
    const response = await apiService.get(`/Asset/status/${status}`);
    return response.data;
  }

  async getByHolder(employeeId) {
    const response = await apiService.get(`/Asset/holder/${employeeId}`);
    return response.data;
  }

  async create(data) {
    const response = await apiService.post('/Asset', data);
    return response.data;
  }

  async update(id, data) {
    const response = await apiService.put(`/Asset/${id}`, data);
    return response.data;
  }

  async delete(id) {
    const response = await apiService.delete(`/Asset/${id}`);
    return response.data;
  }

  async softDelete(id) {
    const response = await apiService.delete(`/Asset/${id}/soft`);
    return response.data;
  }

  async search(keyword) {
    const response = await apiService.get(`/Asset/search?keyword=${encodeURIComponent(keyword)}`);
    return response.data;
  }

  async getExpiredWarranty() {
    const response = await apiService.get('/Asset/expired-warranty');
    return response.data;
  }

  async getUpcomingMaintenance(daysAhead = 30) {
    const response = await apiService.get(`/Asset/upcoming-maintenance?daysAhead=${daysAhead}`);
    return response.data;
  }

  async getDashboardStats() {
    const response = await apiService.get('/Asset/dashboard-stats');
    return response.data;
  }

  async getCategories() {
    const response = await apiService.get('/Category/active');
    return response.data;
  }

  async getSuppliers() {
    const response = await apiService.get('/Supplier/active');
    return response.data;
  }

  async getEmployees() {
    const response = await apiService.get('/Employee');
    return response.data;
  }
}

export default new AssetsApi();