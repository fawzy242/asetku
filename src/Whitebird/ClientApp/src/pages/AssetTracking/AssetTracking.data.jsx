// src/pages/AssetTracking/AssetTracking.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';

class AssetTrackingData extends BaseData {
  constructor() {
    super(apiClient.assetTracking);
    this.setEntityName('asset');
  }

  // ============================================================
  // FETCH ASSETS FOR DROPDOWN
  // ============================================================
  async fetchAssets() {
    try {
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

  // ============================================================
  // ASSET LOOKUP (for dropdown)
  // ============================================================
  async fetchAssetLookup(id) {
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
        error: error.userMessage || error.message || 'Failed to load asset data'
      };
    }
  }

  // ============================================================
  // ASSET DETAIL
  // ============================================================
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

  // ============================================================
  // ASSET TRACKING — COMPLETE DETAIL
  // ============================================================
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

  // ============================================================
  // ASSET STATS (header)
  // ============================================================
  async fetchAssetStats(id) {
    try {
      const r = await this.api.getStats(id);
      if (r.data?.isSuccess !== undefined) {
        return r.data.isSuccess
          ? { success: true, data: r.data.data }
          : { success: false, error: r.data.message };
      }
      return { success: true, data: r.data };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load asset stats'
      };
    }
  }

  // ============================================================
  // TIMELINE
  // ============================================================
  async fetchTimeline(id) {
    try {
      const r = await this.api.getTimeline(id);
      if (r.data?.isSuccess !== undefined) {
        return r.data.isSuccess
          ? { success: true, data: r.data.data }
          : { success: false, error: r.data.message };
      }
      return { success: true, data: r.data };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load timeline'
      };
    }
  }

  // ============================================================
  // SCHEDULE STATS
  // ============================================================
  async fetchScheduleStats(assetId, params) {
    try {
      const r = await this.api.getScheduleStats(assetId, params);
      if (r.data?.isSuccess !== undefined) {
        return r.data.isSuccess
          ? { success: true, data: r.data.data }
          : { success: false, error: r.data.message };
      }
      return { success: true, data: r.data };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load schedule stats'
      };
    }
  }

  // ============================================================
  // TRANSACTION TYPE STATS
  // ============================================================
  async fetchTransactionTypeStats(id) {
    try {
      const r = await this.api.getTransactionTypeStats(id);
      if (r.data?.isSuccess !== undefined) {
        return r.data.isSuccess
          ? { success: true, data: r.data.data }
          : { success: false, error: r.data.message };
      }
      return { success: true, data: r.data };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load transaction stats'
      };
    }
  }

  // ============================================================
  // EMPLOYEE HISTORY
  // ============================================================
  async fetchEmployeeHistory(assetId, params) {
    try {
      const r = await this.api.getEmployeeHistory(assetId, params);
      if (r.data?.isSuccess !== undefined) {
        return r.data.isSuccess
          ? { success: true, data: r.data.data }
          : { success: false, error: r.data.message };
      }
      return { success: true, data: r.data };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load employee history'
      };
    }
  }
}

export default AssetTrackingData;