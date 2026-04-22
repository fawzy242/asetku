import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class SuppliersApi {
  async getAll() {
    const response = await apiService.get('/supplier');
    return response.data;
  }

  async getById(id) {
    const response = await apiService.get(`/supplier/${id}`);
    return response.data;
  }

  async getGridData(params) {
    const queryString = utilsHelper.buildQueryString(params);
    const response = await apiService.get(`/supplier/grid${queryString}`);
    return response.data;
  }

  async getActiveOnly() {
    const response = await apiService.get('/supplier/active');
    return response.data;
  }

  async create(data) {
    const response = await apiService.post('/supplier', data);
    return response.data;
  }

  async update(id, data) {
    const response = await apiService.put(`/supplier/${id}`, data);
    return response.data;
  }

  async delete(id) {
    const response = await apiService.delete(`/supplier/${id}`);
    return response.data;
  }

  async softDelete(id) {
    const response = await apiService.delete(`/supplier/${id}/soft`);
    return response.data;
  }
}

export default new SuppliersApi();