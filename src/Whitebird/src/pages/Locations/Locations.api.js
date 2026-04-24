import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class LocationsApi {
  async getAll() { return (await apiService.get('/Location')).data; }
  async getById(id) { return (await apiService.get(`/Location/${id}`)).data; }
  async getGridData(params) { return (await apiService.get(`/Location/grid${utilsHelper.buildQueryString(params)}`)).data; }
  async getActiveOnly() { return (await apiService.get('/Location/active')).data; }
  async getSubLocations(parentId) { return (await apiService.get(`/Location/sublocations/${parentId}`)).data; }
  async create(data) { return (await apiService.post('/Location', data)).data; }
  async update(id, data) { return (await apiService.put(`/Location/${id}`, data)).data; }
  async delete(id) { return (await apiService.delete(`/Location/${id}`)).data; }
}

export default new LocationsApi();