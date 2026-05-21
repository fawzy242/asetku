import apiService from '../../core/services/api.service';

class AssetTrackingApi {
  // Get all assets for dropdown
  async getAssets() { 
    return (await apiService.get('/Asset')).data; 
  }
  
  // Get single asset detail
  async getAssetById(id) { 
    return (await apiService.get(`/Asset/${id}`)).data; 
  }
  
  // NEW: Get asset tracking data (timeline + current status)
  async getAssetTracking(id) { 
    return (await apiService.get(`/Asset/tracking/${id}`)).data; 
  }
  
  // Get asset current status (derived from active transaction)
  async getCurrentStatus(id) { 
    return (await apiService.get(`/Asset/status/${id}`)).data; 
  }
  
  // Get transaction history for asset
  async getTransactionsByAssetId(assetId) { 
    return (await apiService.get(`/AssetTransaction/asset/${assetId}`)).data; 
  }
  
  // Reference data
  async getEmployees() { 
    return (await apiService.get('/Employee')).data; 
  }
  
  // UPDATED: Use Office instead of Location
  async getOffices() { 
    return (await apiService.get('/Office/active')).data; 
  }
  
  // REMOVED: getLocations (replaced by getOffices)
}

export default new AssetTrackingApi();