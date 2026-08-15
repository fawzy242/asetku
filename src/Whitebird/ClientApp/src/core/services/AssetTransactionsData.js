import { transactionApi } from '../api/transaction.api';
import BaseData from './BaseData';
import { fileToBase64 } from '../utils/file.utils';

class AssetTransactionsData extends BaseData {
  constructor() {
    super(transactionApi);
    this.setEntityName('transaction');
  }

  // ============================================================
  // UPDATE WITH ATTACHMENTS (base64)
  // ============================================================
  async updateWithAttachments(id, data) {
    try {
      const submitData = this.buildSubmitData(data);

      // Convert attachments to base64
      if (data.attachmentFiles && data.attachmentFiles.length > 0) {
        submitData.attachmentFilesBase64 = [];
        submitData.attachmentFileNames = [];
        for (const file of data.attachmentFiles) {
          if (file instanceof File && file.size > 0) {
            try {
              const base64 = await fileToBase64(file);
              submitData.attachmentFilesBase64.push(base64);
              submitData.attachmentFileNames.push(file.name);
            } catch (err) {
              console.error('Failed to convert attachment to base64:', err);
            }
          }
        }
      }

      if (data.attachmentIdsToKeep && data.attachmentIdsToKeep.length > 0) {
        submitData.attachmentIdsToKeep = data.attachmentIdsToKeep;
      }

      if (data.replaceAttachments !== undefined) {
        submitData.replaceAttachments = data.replaceAttachments;
      }

      // Approve attachments
      if (data.approveAttachments && data.approveAttachments.length > 0) {
        submitData.approveAttachments = [];
        for (const att of data.approveAttachments) {
          if (att.file instanceof File && att.file.size > 0) {
            try {
              const base64 = await fileToBase64(att.file);
              submitData.approveAttachments.push({
                documentType: att.documentType ? parseInt(att.documentType, 10) : 0,
                fileBase64: base64,
                fileName: att.file.name,
                order: att.order || 0,
              });
            } catch (err) {
              console.error('Failed to convert approve attachment to base64:', err);
            }
          }
        }
      }

      if (data.approveAttachmentIdsToKeep && data.approveAttachmentIdsToKeep.length > 0) {
        submitData.approveAttachmentIdsToKeep = data.approveAttachmentIdsToKeep;
      }

      if (data.replaceApproveAttachments !== undefined) {
        submitData.replaceApproveAttachments = data.replaceApproveAttachments;
      }

      const result = await this.api.update(id, submitData);
      return this.handleApiResponse(result);
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      const errorMsg = error.userMessage || error.message || 'Failed to update';
      return { success: false, error: errorMsg };
    }
  }

  // ============================================================
  // BULK APPROVAL
  // ============================================================
  async bulkApprove(ids) {
    try {
      const result = await this.api.bulkApproved({ ids });
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Failed to approve',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data || ids.length };
      }
      return { success: true, data: ids.length };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return { success: false, error: error.userMessage || error.message || 'Failed to approve' };
    }
  }

  async bulkReject(ids) {
    try {
      const result = await this.api.bulkRejected({ ids });
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Failed to reject',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data || ids.length };
      }
      return { success: true, data: ids.length };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return { success: false, error: error.userMessage || error.message || 'Failed to reject' };
    }
  }

  async bulkPending(ids) {
    try {
      const result = await this.api.bulkPending({ ids });
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Failed to revert to pending',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data || ids.length };
      }
      return { success: true, data: ids.length };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return { success: false, error: error.userMessage || error.message || 'Failed to revert to pending' };
    }
  }

  // ============================================================
  // COMPLETE / UNCOMPLETE
  // ============================================================
  async complete(id) {
    try {
      const result = await this.api.complete(id);
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Failed to complete',
            errorType: result.data.errorType,
          };
        }
        return { success: true };
      }
      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return { success: false, error: error.userMessage || error.message || 'Failed to complete' };
    }
  }

  async uncomplete(id) {
    try {
      const result = await this.api.uncomplete(id);
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Failed to uncomplete',
            errorType: result.data.errorType,
          };
        }
        return { success: true };
      }
      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return { success: false, error: error.userMessage || error.message || 'Failed to uncomplete' };
    }
  }

  // ============================================================
  // SHORTCUT
  // ============================================================
  async createReturn(id, data) {
    try {
      const result = await this.api.createReturn(id, data);
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Failed to create return',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data };
      }
      return { success: true, data: result.data };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return { success: false, error: error.userMessage || error.message || 'Failed to create return' };
    }
  }

  async createPostMaintenance(id, data) {
    try {
      const result = await this.api.createPostMaintenance(id, data);
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Failed to create post-maintenance',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data };
      }
      return { success: true, data: result.data };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return { success: false, error: error.userMessage || error.message || 'Failed to create post-maintenance' };
    }
  }

  // ============================================================
  // GET PAIRED OPTIONS
  // ============================================================
  async getPairedOptions(assetId, sourceType) {
    try {
      const result = await this.api.getGridAdd({ assetId, page: 1, pageSize: 100 });
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { success: false, data: [], error: result.data.message };
        }
        const allTxns = result.data.data?.data || [];
        const validPairs = allTxns.filter(
          t => t.transactionType === sourceType && t.approved === true && t.fromAssetTransactionId === null
        );
        return { success: true, data: validPairs };
      }
      return { success: true, data: result.data?.data || [] };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, data: [], error: 'canceled' };
      }
      return { success: false, data: [], error: error.message || 'Failed to load paired options' };
    }
  }

  // ============================================================
  // IMPORT
  // ============================================================
  async importTransactions(file) {
    try {
      const result = await this.api.import(file);
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Import failed',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data };
      }
      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return { 
        success: false, 
        error: error.userMessage || error.message || 'Failed to import' 
      };
    }
  }

  // ============================================================
  // DOWNLOAD TEMPLATE
  // ============================================================
  async downloadTemplate() {
    return this.downloadTemplate('Transaction_Import_Template.xlsx');
  }

  // ============================================================
  // PRIVATE HELPERS
  // ============================================================

  buildSubmitData(data) {
    return {
      assetId: data.assetId ? parseInt(data.assetId, 10) : 0,
      transactionType: data.transactionType ? parseInt(data.transactionType, 10) : 0,
      employeeId: data.employeeId ? parseInt(data.employeeId, 10) : null,
      officeId: data.officeId ? parseInt(data.officeId, 10) : null,
      transactionDate: data.transactionDate || null,
      expectedLoanReturnDate: data.expectedLoanReturnDate || null,
      notes: data.notes || null,
      condition: data.condition ? parseInt(data.condition, 10) : null,
      approved: data.approved,
      approvedBy: data.approvedBy || null,
      completed: data.completed,
      completedBy: data.completedBy || null,
      maintenanceType: data.maintenanceType ? parseInt(data.maintenanceType, 10) : null,
      maintenanceCost: data.maintenanceCost ? parseFloat(data.maintenanceCost) : null,
      isActive: data.isActive === true || data.isActive === 'true' || data.isActive === 1,
    };
  }

  handleApiResponse(result) {
    if (result.data?.isSuccess !== undefined) {
      if (!result.data.isSuccess) {
        return { 
          success: false, 
          errors: result.data.errors || [result.data.message || 'Operation failed'],
          errorType: result.data.errorType,
        };
      }
      return { success: true, data: result.data.data };
    }
    return { success: true, data: result.data };
  }

  handleError(error, defaultMessage) {
    if (this.isCanceledError(error)) {
      return { success: false, error: 'canceled', isCanceled: true };
    }
    return { 
      success: false, 
      error: error.userMessage || error.message || defaultMessage 
    };
  }

  getCreateMessage() { return 'Transaction created successfully (Pending approval)'; }
  getUpdateMessage() { return 'Transaction updated successfully'; }
  getDeleteMessage() { return 'Transaction cancelled successfully'; }
  getDeleteTitle() { return 'Cancel Transaction'; }
  getDeleteText() { return 'Are you sure you want to cancel this transaction?'; }
}

export default AssetTransactionsData;