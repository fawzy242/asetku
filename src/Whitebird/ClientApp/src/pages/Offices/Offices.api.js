import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class OfficesApi {
  async getAll() {
    return (await apiService.get('/Office')).data;
  }

  async getById(id) {
    return (await apiService.get(`/Office/${id}`)).data;
  }

  async getGridData(params) {
    return (await apiService.get(`/Office/grid${utilsHelper.buildQueryString(params)}`)).data;
  }

  async getActiveOnly() {
    return (await apiService.get('/Office/active')).data;
  }

  async getSubOffices(parentId) {
    return (await apiService.get(`/Office/suboffices/${parentId}`)).data;
  }

  async create(data) {
    return (await apiService.post('/Office', data)).data;
  }

  async update(id, data) {
    return (await apiService.put(`/Office/${id}`, data)).data;
  }

  async delete(id) {
    return (await apiService.delete(`/Office/${id}`)).data;
  }

  async softDelete(id) {
    return (await apiService.delete(`/Office/${id}/soft`)).data;
  }
}

export default new OfficesApi();