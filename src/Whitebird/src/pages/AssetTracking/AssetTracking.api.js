import apiService from '../../core/services/api.service';

class AssetTrackingApi {
  async getAssets() { return (await apiService.get('/Asset')).data; }
  async getAssetById(id) { return (await apiService.get(`/Asset/${id}`)).data; }
  async getTransactionsByAssetId(assetId) { return (await apiService.get(`/AssetTransaction/asset/${assetId}`)).data; }
  async getEmployees() { return (await apiService.get('/Employee')).data; }
  async getLocations() { return (await apiService.get('/Location/active')).data; }
}

export default new AssetTrackingApi();