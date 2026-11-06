// src/core/services/EmployeeSummaryData.js
import { employeeSummaryApi, employeeApi } from '../api/api';
import BaseData from './BaseData';

class EmployeeSummaryData extends BaseData {
  constructor() {
    super(employeeSummaryApi);
    this.setEntityName('employee');
  }

  async fetchEmployees() {
    try {
      const r = await employeeApi.dropdown();
      const data = r.data?.data || r.data || [];
      return { success: true, data: Array.isArray(data) ? data : [] };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, data: [], error: 'canceled' };
      }
      return {
        success: false,
        data: [],
        error: error.userMessage || error.message || 'Failed to load employees',
      };
    }
  }

  async fetchEmployeeDetail(id) {
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
        error: error.userMessage || error.message || 'Failed to load employee detail',
      };
    }
  }

  async fetchAssetSummary(id) {
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
        error: error.userMessage || error.message || 'Failed to load asset summary',
      };
    }
  }

  async fetchCurrentHolding(id, params) {
    try {
      const r = await this.api.getCurrentHolding(id, params);
      if (r.data?.isSuccess !== undefined) {
        return r.data.isSuccess
          ? { success: true, data: r.data.data || { data: [], totalCount: 0 } }
          : { success: false, error: r.data.message };
      }
      return { success: true, data: r.data || { data: [], totalCount: 0 } };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, data: { data: [], totalCount: 0 }, error: 'canceled' };
      }
      return {
        success: false,
        data: { data: [], totalCount: 0 },
        error: error.userMessage || error.message || 'Failed to load current holding',
      };
    }
  }

  async fetchTransactionHistory(id, params) {
    try {
      const r = await this.api.getTransactionHistory(id, params);
      if (r.data?.isSuccess !== undefined) {
        return r.data.isSuccess
          ? { success: true, data: r.data.data || { data: [], totalCount: 0 } }
          : { success: false, error: r.data.message };
      }
      return { success: true, data: r.data || { data: [], totalCount: 0 } };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, data: { data: [], totalCount: 0 }, error: 'canceled' };
      }
      return {
        success: false,
        data: { data: [], totalCount: 0 },
        error: error.userMessage || error.message || 'Failed to load transaction history',
      };
    }
  }
}

export default EmployeeSummaryData;