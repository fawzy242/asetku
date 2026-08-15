import { assetTrackingApi } from '../api/assetTracking.api';
import { assetApi } from '../api/asset.api';
import BaseData from './BaseData';

class AssetTrackingData extends BaseData {
  constructor() {
    super(assetTrackingApi);
    this.setEntityName('asset');
  }

  async fetchAssets() {
    try {
      const r = await assetApi.getGrid({ page: 1, pageSize: 1000 });
      const data = r.data?.data?.data || r.data?.data || r.data || [];
      return { success: true, data: Array.isArray(data) ? data : [] };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, data: [], error: 'canceled' };
      }
      return {
        success: false,
        data: [],
        error: error.userMessage || error.message || 'Failed to load assets'
      };
    }
  }

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
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled' };
      }
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load asset data'
      };
    }
  }

  async fetchAssetDetail(id) {
    try {
      const r = await assetApi.getById(id);
      if (r.data?.isSuccess !== undefined) {
        return r.data.isSuccess
          ? { success: true, data: r.data.data }
          : { success: false, error: r.data.message };
      }
      return { success: true, data: r.data };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled' };
      }
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
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled' };
      }
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load asset tracking'
      };
    }
  }

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
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled' };
      }
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load asset stats'
      };
    }
  }

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
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled' };
      }
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load timeline'
      };
    }
  }

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
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled' };
      }
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load schedule stats'
      };
    }
  }

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
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled' };
      }
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load transaction stats'
      };
    }
  }

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
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled' };
      }
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load employee history'
      };
    }
  }
}

export default AssetTrackingData;