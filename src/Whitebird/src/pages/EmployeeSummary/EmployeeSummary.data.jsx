import EmployeeSummaryApi from './EmployeeSummary.api';

class EmployeeSummaryData {
  constructor() { this.api = EmployeeSummaryApi; }

  async fetchEmployees() { try { const r = await this.api.getEmployees(); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }
  async fetchEmployeeDetail(id) { try { const r = await this.api.getEmployeeById(id); return r.isSuccess ? { success: true, data: r.data } : { success: false }; } catch { return { success: false }; } }
  async fetchAssets(employeeId) { try { const r = await this.api.getAssetsByHolder(employeeId); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }
  async fetchTransactions(employeeId) { try { const r = await this.api.getTransactionsByEmployee(employeeId); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }
}

export default EmployeeSummaryData;