// src/pages/Assets/Assets.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import { handleSuccess, handleApiError } from '../../core/services';

class AssetsData extends BaseData {
  constructor() {
    super(apiClient.asset);
    this.setEntityName('asset');
  }

  /**
   * Bulk activate or deactivate assets
   */
  async bulkActivate(ids, activate) {
    const actionText = activate ? 'activate' : 'deactivate';
    
    try {
      const endpoint = activate ? this.api.activate : this.api.deactivate;
      const result = await endpoint({ ids });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || `Failed to ${actionText}`;
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        const count = result.data.data || ids.length;
        handleSuccess({
          operation: activate ? 'activate' : 'deactivate',
          entityName: 'asset',
          message: `${count} asset(s) ${actionText}d successfully`,
          result: result.data,
        });
        
        return { success: true, data: count };
      }
      
      ConfirmDialog.toast.success(`${ids.length} asset(s) ${actionText}d successfully`);
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || `Failed to ${actionText}`,
      };
    }
  }

  /**
   * Import assets from Excel file
   */
  async importAssets(file) {
    try {
      const result = await this.api.import(file);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Import failed';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        const importResult = result.data.data;
        if (importResult?.errorCount > 0) {
          ConfirmDialog.toast.warning(
            `Import completed: ${importResult.successCount} success, ${importResult.errorCount} errors`
          );
        } else {
          handleSuccess({
            operation: 'import',
            entityName: 'asset',
            message: `Import completed: ${importResult?.successCount || 0} assets imported successfully`,
            result: result.data,
          });
        }
        return { success: true, data: importResult };
      }
      
      ConfirmDialog.toast.success('Assets imported successfully');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to import',
      };
    }
  }

  /**
   * Download import template
   */
  async downloadTemplate() {
    try {
      const blob = await this.api.getImportTemplate();
      if (!(blob instanceof Blob) || blob.size === 0) {
        throw new Error('Empty template');
      }
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', 'Asset_Import_Template.xlsx');
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      ConfirmDialog.toast.success('Template downloaded');
      return { success: true };
    } catch (error) {
      ConfirmDialog.toast.error(error.message || 'Failed to download template');
      return { success: false };
    }
  }

  // Override default messages
  getCreateMessage() { return 'Asset created successfully'; }
  getUpdateMessage() { return 'Asset updated successfully'; }
  getDeleteMessage() { return 'Asset deleted successfully'; }
  getDeleteTitle() { return 'Delete Asset'; }
  getDeleteText() { return 'Are you sure you want to delete this asset? This action cannot be undone.'; }
}

export default AssetsData;