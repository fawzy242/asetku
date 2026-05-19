import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class CategoriesApi {
  async getAll() { return (await apiService.get('/Category')).data; }
  async getById(id) { return (await apiService.get(`/Category/${id}`)).data; }
  async getGridData(params) { return (await apiService.get(`/Category/grid${utilsHelper.buildQueryString(params)}`)).data; }
  async getActiveOnly() { return (await apiService.get('/Category/active')).data; }
  async getSubCategories(parentId) { return (await apiService.get(`/Category/subcategories/${parentId}`)).data; }
  async create(data) { return (await apiService.post('/Category', data)).data; }
  async update(id, data) { return (await apiService.put(`/Category/${id}`, data)).data; }
  async delete(id) { return (await apiService.delete(`/Category/${id}`)).data; }
}

export default new CategoriesApi();