// src/pages/EmployeeSummary/EmployeeSummary.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';

class EmployeeSummaryData extends BaseData {
  constructor() {
    super(apiClient.employeeSummary);
    this.setEntityName('employee');
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
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load employee detail'
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
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load asset summary'
      };
    }
  }
}

export default EmployeeSummaryData;