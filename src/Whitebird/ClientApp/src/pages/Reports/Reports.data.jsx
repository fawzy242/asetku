import ReportsApi from './Reports.api';
import BaseData from '../../core/services/BaseData';
import utilsHelper from '../../core/utils/utils.helper';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class ReportsData extends BaseData {
  constructor() {
    super(ReportsApi);
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
      const departments = [...new Set((employees.data || []).map(e => e.departmentName || e.department).filter(Boolean))];
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

  // Excel Export Methods - Using server-side export with blob response
  async exportTransaction(params) {
    try {
      const blob = await this.api.exportAssetTransactionExcel(params);
      // Ensure blob is actually a Blob
      if (blob instanceof Blob) {
        utilsHelper.downloadFile(blob, `Transaction_Report_${new Date().toISOString().split('T')[0]}.xlsx`);
        ConfirmDialog.toast.success('Report exported successfully');
        return { success: true };
      } else {
        throw new Error('Invalid response format');
      }
    } catch (error) {
      console.error('Export failed:', error);
      ConfirmDialog.toast.error('Failed to export report');
      return { success: false };
    }
  }

  async exportInventory(params) {
    try {
      const blob = await this.api.exportAssetInventoryExcel(params);
      if (blob instanceof Blob) {
        utilsHelper.downloadFile(blob, `Inventory_Report_${new Date().toISOString().split('T')[0]}.xlsx`);
        ConfirmDialog.toast.success('Report exported successfully');
        return { success: true };
      } else {
        throw new Error('Invalid response format');
      }
    } catch (error) {
      console.error('Export failed:', error);
      ConfirmDialog.toast.error('Failed to export report');
      return { success: false };
    }
  }

  async exportEmployee(params) {
    try {
      const blob = await this.api.exportEmployeeAssetExcel(params);
      if (blob instanceof Blob) {
        utilsHelper.downloadFile(blob, `Employee_Report_${new Date().toISOString().split('T')[0]}.xlsx`);
        ConfirmDialog.toast.success('Report exported successfully');
        return { success: true };
      } else {
        throw new Error('Invalid response format');
      }
    } catch (error) {
      console.error('Export failed:', error);
      ConfirmDialog.toast.error('Failed to export report');
      return { success: false };
    }
  }

  async exportMaintenance(params) {
    try {
      const blob = await this.api.exportMaintenanceExcel(params);
      if (blob instanceof Blob) {
        const reportType = params.isUpcoming ? 'Upcoming_Maintenance' : 'Maintenance_History';
        utilsHelper.downloadFile(blob, `${reportType}_${new Date().toISOString().split('T')[0]}.xlsx`);
        ConfirmDialog.toast.success('Report exported successfully');
        return { success: true };
      } else {
        throw new Error('Invalid response format');
      }
    } catch (error) {
      console.error('Export failed:', error);
      ConfirmDialog.toast.error('Failed to export report');
      return { success: false };
    }
  }

  async exportFinancial(params) {
    try {
      const blob = await this.api.exportFinancialExcel(params);
      if (blob instanceof Blob) {
        utilsHelper.downloadFile(blob, `Financial_Report_${new Date().toISOString().split('T')[0]}.xlsx`);
        ConfirmDialog.toast.success('Report exported successfully');
        return { success: true };
      } else {
        throw new Error('Invalid response format');
      }
    } catch (error) {
      console.error('Export failed:', error);
      ConfirmDialog.toast.error('Failed to export report');
      return { success: false };
    }
  }
}

export default ReportsData;