import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class DepartmentsApi {
  async getAll() {
    return (await apiService.get('/Department')).data;
  }

  async getById(id) {
    return (await apiService.get(`/Department/${id}`)).data;
  }

  async getGridData(params) {
    return (await apiService.get(`/Department/grid${utilsHelper.buildQueryString(params)}`)).data;
  }

  async getActiveOnly() {
    return (await apiService.get('/Department/active')).data;
  }

  async create(data) {
    return (await apiService.post('/Department', data)).data;
  }

  async update(id, data) {
    return (await apiService.put(`/Department/${id}`, data)).data;
  }

  async delete(id) {
    return (await apiService.delete(`/Department/${id}`)).data;
  }

  async softDelete(id) {
    return (await apiService.delete(`/Department/${id}/soft`)).data;
  }
}

export default new DepartmentsApi();