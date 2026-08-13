// src/pages/AssetTransactions/AssetTransactions.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import { handleSuccess } from '../../core/services/successHandler';

class AssetTransactionsData extends BaseData {
  constructor() {
    super(apiClient.transaction);
    this.setEntityName('transaction');
  }

  async bulkApprove(ids) {
    try {
      const result = await this.api.bulkApproved({ ids });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to approve';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        const count = result.data.data || ids.length;
        handleSuccess({
          operation: 'approve',
          entityName: 'transaction',
          message: `${count} transaction(s) approved successfully`,
          result: result.data,
        });
        
        return { success: true, data: count };
      }
      
      ConfirmDialog.toast.success(`${ids.length} transaction(s) approved`);
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to approve',
      };
    }
  }

  async bulkReject(ids) {
    try {
      const result = await this.api.bulkRejected({ ids });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to reject';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        const count = result.data.data || ids.length;
        handleSuccess({
          operation: 'reject',
          entityName: 'transaction',
          message: `${count} transaction(s) rejected successfully`,
          result: result.data,
        });
        
        return { success: true, data: count };
      }
      
      ConfirmDialog.toast.success(`${ids.length} transaction(s) rejected`);
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to reject',
      };
    }
  }

  async bulkPending(ids) {
    try {
      const result = await this.api.bulkPending({ ids });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to revert to pending';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        const count = result.data.data || ids.length;
        handleSuccess({
          operation: 'revert',
          entityName: 'transaction',
          message: `${count} transaction(s) moved to pending successfully`,
          result: result.data,
        });
        
        return { success: true, data: count };
      }
      
      ConfirmDialog.toast.success(`${ids.length} transaction(s) moved to pending`);
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to revert to pending',
      };
    }
  }

  async complete(id) {
    try {
      const result = await this.api.complete(id);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to complete';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        handleSuccess({
          operation: 'complete',
          entityName: 'transaction',
          result: result.data,
        });
        
        return { success: true };
      }
      
      ConfirmDialog.toast.success('Transaction completed');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to complete',
      };
    }
  }

  async uncomplete(id) {
    try {
      const result = await this.api.uncomplete(id);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to uncomplete';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        handleSuccess({
          operation: 'uncomplete',
          entityName: 'transaction',
          message: 'Transaction uncompleted successfully',
          result: result.data,
        });
        
        return { success: true };
      }
      
      ConfirmDialog.toast.success('Transaction uncompleted');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to uncomplete',
      };
    }
  }

  async createReturn(id, data) {
    try {
      const result = await this.api.createReturn(id, data);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to create return';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        handleSuccess({
          operation: 'return',
          entityName: 'transaction',
          message: 'Return transaction created successfully',
          result: result.data,
        });
        
        return { success: true, data: result.data.data };
      }
      
      ConfirmDialog.toast.success('Return transaction created');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to create return',
      };
    }
  }

  async createPostMaintenance(id, data) {
    try {
      const result = await this.api.createPostMaintenance(id, data);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to create post-maintenance';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        handleSuccess({
          operation: 'post-maintenance',
          entityName: 'transaction',
          message: 'Post-maintenance transaction created successfully',
          result: result.data,
        });
        
        return { success: true, data: result.data.data };
      }
      
      ConfirmDialog.toast.success('Post-maintenance transaction created');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to create post-maintenance',
      };
    }
  }

  async importTransactions(file) {
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
            entityName: 'transaction',
            message: `Import completed: ${importResult?.successCount || 0} transactions imported successfully`,
            result: result.data,
          });
        }
        return { success: true, data: importResult };
      }
      
      ConfirmDialog.toast.success('Transactions imported successfully');
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
   * OVERRIDE: Perbaiki blob handling
   */
  async downloadTemplate() {
    try {
      const response = await this.api.getImportTemplate();
      
      // FIX: Ambil data dari response (axios response wrapper)
      const blob = response?.data;
      
      if (!blob || !(blob instanceof Blob) || blob.size === 0) {
        console.warn('[AssetTransactionsData.downloadTemplate] Empty or invalid blob:', blob);
        ConfirmDialog.toast.error('Template file is empty. Please try again.');
        return { success: false };
      }
      
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', 'Transaction_Import_Template.xlsx');
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      
      ConfirmDialog.toast.success('Template downloaded successfully');
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
            ConfirmDialog.toast.error(parsed.message);
            return { success: false, error: parsed.message };
          }
        } catch {
          // Not JSON, continue
        }
      }
      
      const errorMsg = error.userMessage || error.message || 'Failed to download template';
      ConfirmDialog.toast.error(errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  getCreateMessage() { return 'Transaction created successfully (Pending approval)'; }
  getUpdateMessage() { return 'Transaction updated successfully'; }
  getDeleteMessage() { return 'Transaction cancelled successfully'; }
  getDeleteTitle() { return 'Cancel Transaction'; }
  getDeleteText() { return 'Are you sure you want to cancel this transaction?'; }
}

export default AssetTransactionsData;