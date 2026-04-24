import AssetTrackingApi from './AssetTracking.api';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class AssetTrackingData {
  constructor() { this.api = AssetTrackingApi; }

  async fetchAssets() {
    try { const r = await this.api.getAssets(); return { success: true, data: r.data || [] }; }
    catch { return { success: false, data: [] }; }
  }

  async fetchAssetDetail(id) {
    try { const r = await this.api.getAssetById(id); return r.isSuccess ? { success: true, data: r.data } : { success: false }; }
    catch { return { success: false }; }
  }

  async fetchTransactions(assetId) {
    try { const r = await this.api.getTransactionsByAssetId(assetId); return { success: true, data: r.data || [] }; }
    catch { return { success: false, data: [] }; }
  }

  async fetchEmployees() {
    try { const r = await this.api.getEmployees(); return { success: true, data: r.data || [] }; }
    catch { return { success: false, data: [] }; }
  }

  async fetchLocations() {
    try { const r = await this.api.getLocations(); return { success: true, data: r.data || [] }; }
    catch { return { success: false, data: [] }; }
  }
}

export default AssetTrackingData;