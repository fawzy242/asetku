import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class SuppliersApi {
  async getAll() { return (await apiService.get('/Supplier')).data; }
  async getById(id) { return (await apiService.get(`/Supplier/${id}`)).data; }
  async getGridData(params) { return (await apiService.get(`/Supplier/grid${utilsHelper.buildQueryString(params)}`)).data; }
  async getActiveOnly() { return (await apiService.get('/Supplier/active')).data; }
  async create(data) { return (await apiService.post('/Supplier', data)).data; }
  async update(id, data) { return (await apiService.put(`/Supplier/${id}`, data)).data; }
  async delete(id) { return (await apiService.delete(`/Supplier/${id}`)).data; }
}

export default new SuppliersApi();