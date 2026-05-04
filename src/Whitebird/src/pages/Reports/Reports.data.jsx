import ReportsApi from './Reports.api';
import * as XLSX from 'xlsx';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class ReportsData {
  constructor() { this.api = ReportsApi; }

  async fetchDashboardStats() {
    try { const r = await this.api.getDashboardStats(); return { success: true, data: r.data || {} }; }
    catch { return { success: false, error: 'Failed to load stats' }; }
  }

  async fetchFilterOptions() {
    try { const [categories, suppliers, employees] = await Promise.all([this.api.getCategories(), this.api.getSuppliers(), this.api.getEmployees()]); const departments = [...new Set((employees.data || []).map(e => e.department).filter(Boolean))]; return { success: true, data: { categories: categories.data || [], suppliers: suppliers.data || [], employees: employees.data || [], departments } }; }
    catch { return { success: false, error: 'Failed to load filter options' }; }
  }

  async fetchTransactionData(params) { try { const r = await this.api.getAssetTransactionData(params); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }
  async fetchInventoryData(params) { try { const r = await this.api.getAssetInventoryData(params); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }
  async fetchEmployeeAssetData(params) { try { const r = await this.api.getEmployeeAssetData(params); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }
  async fetchMaintenanceData(params) { try { const r = await this.api.getMaintenanceData(params); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }
  async fetchFinancialData(params) { try { const r = await this.api.getFinancialData(params); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }

  async exportToExcel(data, filename, sheetName = 'Report') {
    try {
      const ws = XLSX.utils.json_to_sheet(data);
      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, sheetName);
      XLSX.writeFile(wb, `${filename}_${new Date().toISOString().split('T')[0]}.xlsx`);
      ConfirmDialog.toast.success('Report exported successfully');
      return { success: true };
    } catch { ConfirmDialog.toast.error('Failed to export report'); return { success: false }; }
  }

  async exportTransaction(params) { const r = await this.fetchTransactionData(params); if (r.success) return this.exportToExcel(r.data, 'Transaction_Report'); return r; }
  async exportInventory(params) { const r = await this.fetchInventoryData(params); if (r.success) return this.exportToExcel(r.data, 'Inventory_Report'); return r; }
  async exportEmployee(params) { const r = await this.fetchEmployeeAssetData(params); if (r.success) return this.exportToExcel(r.data, 'Employee_Report'); return r; }
  async exportMaintenance(params) { const r = await this.fetchMaintenanceData(params); if (r.success) return this.exportToExcel(r.data, 'Maintenance_Report'); return r; }
  async exportFinancial(params) { const r = await this.fetchFinancialData(params); if (r.success) return this.exportToExcel(r.data, 'Financial_Report'); return r; }
}

export default ReportsData;