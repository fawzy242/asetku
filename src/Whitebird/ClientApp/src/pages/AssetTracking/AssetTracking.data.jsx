import AssetTrackingApi from './AssetTracking.api';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class AssetTrackingData extends BaseData {
  constructor() {
    super(AssetTrackingApi);
  }

  async fetchAssets() {
    try {
      const r = await this.api.getAssets();
      return { success: true, data: r.data || [] };
    } catch {
      return { success: false, data: [] };
    }
  }

  async fetchAssetDetail(id) {
    try {
      const r = await this.api.getAssetById(id);
      return r.isSuccess ? { success: true, data: r.data } : { success: false };
    } catch {
      return { success: false };
    }
  }

  async fetchAssetTracking(id) {
    try {
      const r = await this.api.getAssetTracking(id);
      return r.isSuccess ? { success: true, data: r.data } : { success: false };
    } catch {
      return { success: false };
    }
  }

  async fetchCurrentStatus(id) {
    try {
      const r = await this.api.getCurrentStatus(id);
      return r.isSuccess ? { success: true, data: r.data } : { success: false };
    } catch {
      return { success: false };
    }
  }

  async fetchTransactions(assetId) {
    try {
      const r = await this.api.getTransactionsByAssetId(assetId);
      return { success: true, data: r.data || [] };
    } catch {
      return { success: false, data: [] };
    }
  }

  async fetchEmployees() {
    try {
      const r = await this.api.getEmployees();
      return { success: true, data: r.data || [] };
    } catch {
      return { success: false, data: [] };
    }
  }

  async fetchOffices() {
    try {
      const r = await this.api.getOffices();
      return { success: true, data: r.data || [] };
    } catch {
      return { success: false, data: [] };
    }
  }
}

export default AssetTrackingData;