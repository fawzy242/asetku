import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class AssetsApi {
  async getAll() {
    const response = await apiService.get('/asset');
    return response.data;
  }

  async getById(id) {
    const response = await apiService.get(`/asset/${id}`);
    return response.data;
  }

  async getGridData(params) {
    const queryString = utilsHelper.buildQueryString(params);
    const response = await apiService.get(`/asset/grid${queryString}`);
    return response.data;
  }

  async create(data) {
    const response = await apiService.post('/asset', data);
    return response.data;
  }

  async update(id, data) {
    const response = await apiService.put(`/asset/${id}`, data);
    return response.data;
  }

  async delete(id) {
    const response = await apiService.delete(`/asset/${id}`);
    return response.data;
  }

  async softDelete(id) {
    const response = await apiService.delete(`/asset/${id}/soft`);
    return response.data;
  }

  async getByCategory(categoryId) {
    const response = await apiService.get(`/asset/category/${categoryId}`);
    return response.data;
  }

  async getByStatus(status) {
    const response = await apiService.get(`/asset/status/${status}`);
    return response.data;
  }

  async search(keyword) {
    const response = await apiService.get(`/asset/search?keyword=${encodeURIComponent(keyword)}`);
    return response.data;
  }

  async getCategories() {
    const response = await apiService.get('/category/active');
    return response.data;
  }

  async getSuppliers() {
    const response = await apiService.get('/supplier/active');
    return response.data;
  }

  async getEmployees() {
    const response = await apiService.get('/employee');
    return response.data;
  }
}

export default new AssetsApi();