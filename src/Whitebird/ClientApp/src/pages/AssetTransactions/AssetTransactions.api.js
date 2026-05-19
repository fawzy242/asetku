import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class AssetTransactionsApi {
  async getAll() { return (await apiService.get('/AssetTransaction')).data; }
  async getById(id) { return (await apiService.get(`/AssetTransaction/${id}`)).data; }
  async getGridData(params) { return (await apiService.get(`/AssetTransaction/grid${utilsHelper.buildQueryString(params)}`)).data; }
  async getByAssetId(assetId) { return (await apiService.get(`/AssetTransaction/asset/${assetId}`)).data; }
  async getByEmployeeId(employeeId) { return (await apiService.get(`/AssetTransaction/employee/${employeeId}`)).data; }
  async getByStatus(status) { return (await apiService.get(`/AssetTransaction/status/${status}`)).data; }
  async getPendingApprovals() { return (await apiService.get('/AssetTransaction/pending-approvals')).data; }
  async getActiveLoans() { return (await apiService.get('/AssetTransaction/active-loans')).data; }
  async getOverdueLoans() { return (await apiService.get('/AssetTransaction/overdue-loans')).data; }
  async create(data) { return (await apiService.post('/AssetTransaction', data)).data; }
  async update(id, data) { return (await apiService.put(`/AssetTransaction/${id}`, data)).data; }
  async approve(id, data) { return (await apiService.post(`/AssetTransaction/${id}/approve`, data)).data; }
  async returnAsset(data) { return (await apiService.post('/AssetTransaction/return', data)).data; }
  async cancel(id) { return (await apiService.post(`/AssetTransaction/${id}/cancel`)).data; }
  async getAssets() { return (await apiService.get('/Asset')).data; }
  async getEmployees() { return (await apiService.get('/Employee')).data; }
  async getLocations() { return (await apiService.get('/Location/active')).data; }
}

export default new AssetTransactionsApi();