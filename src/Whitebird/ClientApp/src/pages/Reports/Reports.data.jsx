import ReportsApi from './Reports.api';
import * as XLSX from 'xlsx';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import utilsHelper from '../../core/utils/utils.helper';

class ReportsData {
  constructor() {
    this.api = ReportsApi;
  }

  async fetchDashboardStats() {
    try {
      const r = await this.api.getDashboardStats();
      return { success: true, data: r.data || {} };
    } catch {
      return { success: false, error: 'Failed to load stats' };
    }
  }

  async fetchFilterOptions() {
    try {
      const [categories, suppliers, employees] = await Promise.all([
        this.api.getCategories(),
        this.api.getSuppliers(),
        this.api.getEmployees()
      ]);
      const departments = [...new Set((employees.data || []).map(e => e.department).filter(Boolean))];
      return {
        success: true,
        data: {
          categories: categories.data || [],
          suppliers: suppliers.data || [],
          employees: employees.data || [],
          departments
        }
      };
    } catch {
      return { success: false, error: 'Failed to load filter options' };
    }
  }

  async fetchTransactionData(params) {
    try {
      const r = await this.api.getAssetTransactionData(params);
      return { success: true, data: r.data || [] };
    } catch {
      return { success: false, data: [] };
    }
  }

  async fetchInventoryData(params) {
    try {
      const r = await this.api.getAssetInventoryData(params);
      return { success: true, data: r.data || [] };
    } catch {
      return { success: false, data: [] };
    }
  }

  async fetchEmployeeAssetData(params) {
    try {
      const r = await this.api.getEmployeeAssetData(params);
      return { success: true, data: r.data || [] };
    } catch {
      return { success: false, data: [] };
    }
  }

  async fetchMaintenanceData(params) {
    try {
      const r = await this.api.getMaintenanceData(params);
      return { success: true, data: r.data || [] };
    } catch {
      return { success: false, data: [] };
    }
  }

  async fetchFinancialData(params) {
    try {
      const r = await this.api.getFinancialData(params);
      return { success: true, data: r.data || [] };
    } catch {
      return { success: false, data: [] };
    }
  }

  async exportToExcel(data, filename, sheetName = 'Report') {
    try {
      if (!data || data.length === 0) {
        ConfirmDialog.toast.error('No data to export');
        return { success: false };
      }
      const ws = XLSX.utils.json_to_sheet(data);
      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, sheetName);
      XLSX.writeFile(wb, `${filename}_${new Date().toISOString().split('T')[0]}.xlsx`);
      ConfirmDialog.toast.success('Report exported successfully');
      return { success: true };
    } catch {
      ConfirmDialog.toast.error('Failed to export report');
      return { success: false };
    }
  }

  async exportFromServer(endpointFn, params, filename) {
    try {
      const blob = await endpointFn(params);
      utilsHelper.downloadFile(blob, `${filename}_${new Date().toISOString().split('T')[0]}.xlsx`);
      ConfirmDialog.toast.success('Report exported successfully');
      return { success: true };
    } catch {
      ConfirmDialog.toast.info('Server export unavailable. Using client-side export.');
      return { success: false };
    }
  }

  async exportTransaction(params, data) {
    const serverResult = await this.exportFromServer(this.api.exportAssetTransactionExcel, params, 'Transaction_Report');
    if (!serverResult.success && data && data.length > 0) {
      return this.exportToExcel(data, 'Transaction_Report');
    }
    return serverResult;
  }

  async exportInventory(params, data) {
    const serverResult = await this.exportFromServer(this.api.exportAssetInventoryExcel, params, 'Inventory_Report');
    if (!serverResult.success && data && data.length > 0) {
      return this.exportToExcel(data, 'Inventory_Report');
    }
    return serverResult;
  }

  async exportEmployee(params, data) {
    const serverResult = await this.exportFromServer(this.api.exportEmployeeAssetExcel, params, 'Employee_Report');
    if (!serverResult.success && data && data.length > 0) {
      return this.exportToExcel(data, 'Employee_Report');
    }
    return serverResult;
  }

  async exportMaintenance(params, data) {
    const serverResult = await this.exportFromServer(this.api.exportMaintenanceExcel, params, 'Maintenance_Report');
    if (!serverResult.success && data && data.length > 0) {
      return this.exportToExcel(data, 'Maintenance_Report');
    }
    return serverResult;
  }

  async exportFinancial(params, data) {
    const serverResult = await this.exportFromServer(this.api.exportFinancialExcel, params, 'Financial_Report');
    if (!serverResult.success && data && data.length > 0) {
      return this.exportToExcel(data, 'Financial_Report');
    }
    return serverResult;
  }
}

export default ReportsData;