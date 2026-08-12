// src/pages/AssetTracking/AssetTracking.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';

class AssetTrackingData extends BaseData {
  constructor() {
    super(apiClient.assetTracking);
    this.setEntityName('asset');
  }

  async fetchAssets() {
    try {
      // FIX: Gunakan getGrid untuk dropdown asset (tanpa filter transaction type)
      const r = await apiClient.asset.getGrid({ page: 1, pageSize: 1000 });
      const data = r.data?.data?.data || r.data?.data || r.data || [];
      return { success: true, data: Array.isArray(data) ? data : [] };
    } catch (error) {
      return {
        success: false,
        data: [],
        error: error.userMessage || error.message || 'Failed to load assets'
      };
    }
  }

  async fetchAssetDetail(id) {
    try {
      const r = await apiClient.asset.getById(id);
      if (r.data?.isSuccess !== undefined) {
        return r.data.isSuccess
          ? { success: true, data: r.data.data }
          : { success: false, error: r.data.message };
      }
      return { success: true, data: r.data };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load asset detail'
      };
    }
  }

  async fetchAssetTracking(id) {
    try {
      const r = await this.api.getDetail(id);
      if (r.data?.isSuccess !== undefined) {
        return r.data.isSuccess
          ? { success: true, data: r.data.data }
          : { success: false, error: r.data.message };
      }
      return { success: true, data: r.data };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load asset tracking'
      };
    }
  }

  async fetchCurrentStatus(id) {
    try {
      const r = await this.api.lookup(id);
      if (r.data?.isSuccess !== undefined) {
        return r.data.isSuccess
          ? { success: true, data: r.data.data }
          : { success: false, error: r.data.message };
      }
      return { success: true, data: r.data };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load current status'
      };
    }
  }

  async fetchEmployees() {
    try {
      const r = await apiClient.employee.dropdown();
      const data = r.data?.data || r.data || [];
      return { success: true, data: Array.isArray(data) ? data : [] };
    } catch (error) {
      return {
        success: false,
        data: [],
        error: error.userMessage || error.message || 'Failed to load employees'
      };
    }
  }

  async fetchOffices() {
    try {
      const r = await apiClient.office.dropdown();
      const data = r.data?.data || r.data || [];
      return { success: true, data: Array.isArray(data) ? data : [] };
    } catch (error) {
      return {
        success: false,
        data: [],
        error: error.userMessage || error.message || 'Failed to load offices'
      };
    }
  }
}

export default AssetTrackingData;