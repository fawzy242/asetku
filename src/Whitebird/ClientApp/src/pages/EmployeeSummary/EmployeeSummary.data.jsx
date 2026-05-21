import EmployeeSummaryApi from './EmployeeSummary.api';

class EmployeeSummaryData {
  constructor() {
    this.api = EmployeeSummaryApi;
  }

  async fetchEmployees() {
    try {
      const r = await this.api.getEmployees();
      return { success: true, data: r.data || [] };
    } catch {
      return { success: false, data: [] };
    }
  }

  async fetchEmployeeDetail(id) {
    try {
      const r = await this.api.getEmployeeById(id);
      return r.isSuccess ? { success: true, data: r.data } : { success: false };
    } catch {
      return { success: false };
    }
  }

  async fetchAssetSummary(id) {
    try {
      const r = await this.api.getAssetSummary(id);
      return r.isSuccess ? { success: true, data: r.data } : { success: false };
    } catch {
      return { success: false };
    }
  }
}

export default EmployeeSummaryData;