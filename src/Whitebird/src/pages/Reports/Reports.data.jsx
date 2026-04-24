import ReportsApi from './Reports.api';
import utilsHelper from '../../core/utils/utils.helper';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class ReportsData {
  constructor() { this.api = ReportsApi; }

  async fetchDashboardStats() {
    try { const result = await this.api.getDashboardStats(); return { success: true, data: result.data || {} }; }
    catch { return { success: false, error: 'Failed to load stats' }; }
  }

  async fetchFilterOptions() {
    try {
      const [categories, suppliers, employees] = await Promise.all([this.api.getCategories(), this.api.getSuppliers(), this.api.getEmployees()]);
      const departments = [...new Set((employees.data || []).map(e => e.department).filter(Boolean))];
      return { success: true, data: { categories: categories.data || [], suppliers: suppliers.data || [], employees: employees.data || [], departments } };
    } catch { return { success: false, error: 'Failed to load filter options' }; }
  }

  async fetchTransactionData(params) { try { const r = await this.api.getAssetTransactionData(params); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }
  async fetchInventoryData(params) { try { const r = await this.api.getAssetInventoryData(params); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }
  async fetchEmployeeAssetData(params) { try { const r = await this.api.getEmployeeAssetData(params); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }
  async fetchMaintenanceData(params) { try { const r = await this.api.getMaintenanceData(params); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }
  async fetchFinancialData(params) { try { const r = await this.api.getFinancialData(params); return { success: true, data: r.data || [] }; } catch { return { success: false, data: [] }; } }

  async exportTransaction(params) { try { const b = await this.api.exportAssetTransactionExcel(params); utilsHelper.downloadFile(b, `Transaction_${new Date().toISOString().split('T')[0]}.xlsx`); await ConfirmDialog.showSuccess('Success', 'Exported'); return { success: true }; } catch { await ConfirmDialog.showError('Error', 'Export failed'); return { success: false }; } }
  async exportInventory(params) { try { const b = await this.api.exportAssetInventoryExcel(params); utilsHelper.downloadFile(b, `Inventory_${new Date().toISOString().split('T')[0]}.xlsx`); await ConfirmDialog.showSuccess('Success', 'Exported'); return { success: true }; } catch { await ConfirmDialog.showError('Error', 'Export failed'); return { success: false }; } }
  async exportEmployee(params) { try { const b = await this.api.exportEmployeeAssetExcel(params); utilsHelper.downloadFile(b, `Employee_${new Date().toISOString().split('T')[0]}.xlsx`); await ConfirmDialog.showSuccess('Success', 'Exported'); return { success: true }; } catch { await ConfirmDialog.showError('Error', 'Export failed'); return { success: false }; } }
  async exportMaintenance(params) { try { const b = await this.api.exportMaintenanceExcel(params); utilsHelper.downloadFile(b, `Maintenance_${new Date().toISOString().split('T')[0]}.xlsx`); await ConfirmDialog.showSuccess('Success', 'Exported'); return { success: true }; } catch { await ConfirmDialog.showError('Error', 'Export failed'); return { success: false }; } }
  async exportFinancial(params) { try { const b = await this.api.exportFinancialExcel(params); utilsHelper.downloadFile(b, `Financial_${new Date().toISOString().split('T')[0]}.xlsx`); await ConfirmDialog.showSuccess('Success', 'Exported'); return { success: true }; } catch { await ConfirmDialog.showError('Error', 'Export failed'); return { success: false }; } }
}

export default ReportsData;