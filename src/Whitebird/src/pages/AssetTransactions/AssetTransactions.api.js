import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class AssetTransactionsApi {
  async getAll() {
    const response = await apiService.get('/assetTransaction');
    return response.data;
  }

  async getById(id) {
    const response = await apiService.get(`/assetTransaction/${id}`);
    return response.data;
  }

  async getGridData(params) {
    const queryString = utilsHelper.buildQueryString(params);
    const response = await apiService.get(`/assetTransaction/grid${queryString}`);
    return response.data;
  }

  async getByAssetId(assetId) {
    const response = await apiService.get(`/assetTransaction/asset/${assetId}`);
    return response.data;
  }

  async getByEmployeeId(employeeId) {
    const response = await apiService.get(`/assetTransaction/employee/${employeeId}`);
    return response.data;
  }

  async getByStatus(status) {
    const response = await apiService.get(`/assetTransaction/status/${status}`);
    return response.data;
  }

  async getPendingApprovals() {
    const response = await apiService.get('/assetTransaction/pending-approvals');
    return response.data;
  }

  async create(data) {
    const response = await apiService.post('/assetTransaction', data);
    return response.data;
  }

  async update(id, data) {
    const response = await apiService.put(`/assetTransaction/${id}`, data);
    return response.data;
  }

  async approve(id, data) {
    const response = await apiService.post(`/assetTransaction/${id}/approve`, data);
    return response.data;
  }

  async returnAsset(data) {
    const response = await apiService.post('/assetTransaction/return', data);
    return response.data;
  }

  async cancel(id) {
    const response = await apiService.post(`/assetTransaction/${id}/cancel`);
    return response.data;
  }

  async getAssets() {
    const response = await apiService.get('/asset');
    return response.data;
  }

  async getEmployees() {
    const response = await apiService.get('/employee');
    return response.data;
  }

  async getLocations() {
    const response = await apiService.get('/location/active');
    return response.data;
  }
}

export default new AssetTransactionsApi();