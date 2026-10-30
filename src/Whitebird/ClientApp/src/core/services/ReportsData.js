import { reportsApi } from '../api/api';
import BaseData from './BaseData';
import { downloadBlob } from '../utils/file.utils';

class ReportsData extends BaseData {
  constructor() {
    super(reportsApi);
    this.setEntityName('report');
  }

  async _exportReport(exportFn, params, fileNamePrefix) {
    try {
      const exportParams = { ...params, format: 'excel' };
      const response = await exportFn(exportParams);
      const blob = response?.data;

      if (!(blob instanceof Blob) || blob.size === 0) {
        return { success: false, error: 'Empty response from server' };
      }

      const fileName = `${fileNamePrefix}_${new Date().toISOString().split('T')[0]}.xlsx`;
      downloadBlob(blob, fileName);

      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }

      if (error.response?.data instanceof Blob) {
        try {
          const text = await error.response.data.text();
          const parsed = JSON.parse(text);
          if (parsed.message) {
            return { success: false, error: parsed.message };
          }
        } catch {
          // Not JSON, continue
        }
      }

      return {
        success: false,
        error:
          error.userMessage || error.message || 'Failed to export report',
      };
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
    const reportType = params.isUpcoming
      ? 'Upcoming_Maintenance'
      : 'Maintenance_History';
    return this._exportReport(this.api.exportMaintenance, params, reportType);
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