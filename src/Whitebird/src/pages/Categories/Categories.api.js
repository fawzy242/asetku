import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class CategoriesApi {
  async getAll() {
    const response = await apiService.get('/category');
    return response.data;
  }

  async getById(id) {
    const response = await apiService.get(`/category/${id}`);
    return response.data;
  }

  async getGridData(params) {
    const queryString = utilsHelper.buildQueryString(params);
    const response = await apiService.get(`/category/grid${queryString}`);
    return response.data;
  }

  async getActiveOnly() {
    const response = await apiService.get('/category/active');
    return response.data;
  }

  async getSubCategories(parentId) {
    const response = await apiService.get(`/category/subcategories/${parentId}`);
    return response.data;
  }

  async create(data) {
    const response = await apiService.post('/category', data);
    return response.data;
  }

  async update(id, data) {
    const response = await apiService.put(`/category/${id}`, data);
    return response.data;
  }

  async delete(id) {
    const response = await apiService.delete(`/category/${id}`);
    return response.data;
  }

  async softDelete(id) {
    const response = await apiService.delete(`/category/${id}/soft`);
    return response.data;
  }
}

export default new CategoriesApi();