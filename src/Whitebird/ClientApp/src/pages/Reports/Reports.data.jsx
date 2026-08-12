// src/pages/Reports/Reports.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';
import utilsHelper from '../../core/utils/utils.helper';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import { handleSuccess, handleApiError } from '../../core/services';

class ReportsData extends BaseData {
  constructor() {
    super(apiClient.reports);
    this.setEntityName('report');
  }

  /**
   * Generic export method with blob download
   */
  async _exportReport(exportFn, params, fileNamePrefix) {
    try {
      const blob = await exportFn(params);
      
      if (!(blob instanceof Blob) || blob.size === 0) {
        throw new Error('Empty response from server');
      }
      
      const fileName = `${fileNamePrefix}_${new Date().toISOString().split('T')[0]}.xlsx`;
      utilsHelper.downloadFile(blob, fileName);
      
      handleSuccess({
        operation: 'export',
        entityName: 'report',
        message: `${fileNamePrefix} exported successfully`,
      });
      
      return { success: true };
    } catch (error) {
      // Check if it's a blob error with message
      if (error.response?.data instanceof Blob) {
        try {
          const text = await error.response.data.text();
          const parsed = JSON.parse(text);
          if (parsed.message) {
            ConfirmDialog.toast.error(parsed.message);
            return { success: false, error: parsed.message };
          }
        } catch {
          // Not JSON, continue
        }
      }
      
      const errorMsg = error.userMessage || error.message || 'Failed to export report';
      ConfirmDialog.toast.error(errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  async exportTransaction(params) {
    return this._exportReport(
      this.api.exportTransaction,
      params,
      'Transaction_Report'
    );
  }

  async exportInventory(params) {
    return this._exportReport(
      this.api.exportInventory,
      params,
      'Inventory_Report'
    );
  }

  async exportEmployee(params) {
    return this._exportReport(
      this.api.exportEmployeeAsset,
      params,
      'Employee_Asset_Report'
    );
  }

  async exportMaintenance(params) {
    const reportType = params.isUpcoming ? 'Upcoming_Maintenance' : 'Maintenance_History';
    return this._exportReport(
      this.api.exportMaintenance,
      params,
      reportType
    );
  }

  async exportFinancial(params) {
    return this._exportReport(
      this.api.exportFinancial,
      params,
      'Financial_Report'
    );
  }
}

export default ReportsData;