import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class LocationsApi {
  async getAll() {
    const response = await apiService.get('/location');
    return response.data;
  }

  async getById(id) {
    const response = await apiService.get(`/location/${id}`);
    return response.data;
  }

  async getGridData(params) {
    const queryString = utilsHelper.buildQueryString(params);
    const response = await apiService.get(`/location/grid${queryString}`);
    return response.data;
  }

  async getActiveOnly() {
    const response = await apiService.get('/location/active');
    return response.data;
  }

  async getSubLocations(parentId) {
    const response = await apiService.get(`/location/sublocations/${parentId}`);
    return response.data;
  }

  async create(data) {
    const response = await apiService.post('/location', data);
    return response.data;
  }

  async update(id, data) {
    const response = await apiService.put(`/location/${id}`, data);
    return response.data;
  }

  async delete(id) {
    const response = await apiService.delete(`/location/${id}`);
    return response.data;
  }

  async softDelete(id) {
    const response = await apiService.delete(`/location/${id}/soft`);
    return response.data;
  }
}

export default new LocationsApi();