import ReportsApi from './Reports.api';
import utilsHelper from '../../core/utils/utils.helper';
import Swal from 'sweetalert2';

class ReportsData {
  constructor() {
    this.api = ReportsApi;
  }

  async loadDashboardStats() {
    try {
      const result = await this.api.getDashboardStats();
      return {
        success: true,
        data: result.data || {}
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load dashboard stats'
      };
    }
  }

  async loadAssetTransactionData(filters) {
    try {
      const result = await this.api.getAssetTransactionData(filters);
      return {
        success: true,
        data: result.data || []
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load transaction data'
      };
    }
  }

  async exportAssetTransaction(filters) {
    try {
      const blob = await this.api.exportAssetTransactionExcel(filters);
      const fileName = `Asset_Transaction_Report_${new Date().toISOString().split('T')[0]}.xlsx`;
      utilsHelper.downloadFile(blob, fileName);
      
      Swal.fire({
        title: 'Success',
        text: 'Report exported successfully',
        icon: 'success',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: true };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to export report',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }

  async loadAssetInventoryData(filters) {
    try {
      const result = await this.api.getAssetInventoryData(filters);
      return {
        success: true,
        data: result.data || []
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load inventory data'
      };
    }
  }

  async exportAssetInventory(filters) {
    try {
      const blob = await this.api.exportAssetInventoryExcel(filters);
      const fileName = `Asset_Inventory_Report_${new Date().toISOString().split('T')[0]}.xlsx`;
      utilsHelper.downloadFile(blob, fileName);
      
      Swal.fire({
        title: 'Success',
        text: 'Report exported successfully',
        icon: 'success',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: true };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to export report',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }

  async loadEmployeeAssetData(filters) {
    try {
      const result = await this.api.getEmployeeAssetData(filters);
      return {
        success: true,
        data: result.data || []
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load employee asset data'
      };
    }
  }

  async exportEmployeeAsset(filters) {
    try {
      const blob = await this.api.exportEmployeeAssetExcel(filters);
      const fileName = `Employee_Asset_Report_${new Date().toISOString().split('T')[0]}.xlsx`;
      utilsHelper.downloadFile(blob, fileName);
      
      Swal.fire({
        title: 'Success',
        text: 'Report exported successfully',
        icon: 'success',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: true };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to export report',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }

  async loadMaintenanceData(filters) {
    try {
      const result = await this.api.getMaintenanceData(filters);
      return {
        success: true,
        data: result.data || []
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load maintenance data'
      };
    }
  }

  async exportMaintenance(filters) {
    try {
      const blob = await this.api.exportMaintenanceExcel(filters);
      const fileName = `Maintenance_Report_${new Date().toISOString().split('T')[0]}.xlsx`;
      utilsHelper.downloadFile(blob, fileName);
      
      Swal.fire({
        title: 'Success',
        text: 'Report exported successfully',
        icon: 'success',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: true };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to export report',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }

  async loadFinancialData(filters) {
    try {
      const result = await this.api.getFinancialData(filters);
      return {
        success: true,
        data: result.data || []
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load financial data'
      };
    }
  }

  async exportFinancial(filters) {
    try {
      const blob = await this.api.exportFinancialExcel(filters);
      const fileName = `Financial_Report_${new Date().toISOString().split('T')[0]}.xlsx`;
      utilsHelper.downloadFile(blob, fileName);
      
      Swal.fire({
        title: 'Success',
        text: 'Report exported successfully',
        icon: 'success',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: true };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to export report',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }

  async loadFilterOptions() {
    try {
      const [categories, suppliers, employees] = await Promise.all([
        this.api.getCategories(),
        this.api.getSuppliers(),
        this.api.getEmployees()
      ]);
      
      const departments = [...new Set(employees.data?.map(e => e.department).filter(Boolean))];
      
      return {
        success: true,
        data: {
          categories: categories.data || [],
          suppliers: suppliers.data || [],
          employees: employees.data || [],
          departments
        }
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load filter options'
      };
    }
  }
}

export default ReportsData;