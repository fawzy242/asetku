import AssetTransactionsApi from './AssetTransactions.api';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class AssetTransactionsData extends BaseData {
  constructor() {
    super(AssetTransactionsApi);
  }

  async approve(id, isApproved, approvalNotes = '') {
    try {
      const result = await this.api.approve(id, { assetTransactionId: id, isApproved, approvalNotes });
      if (result.isSuccess) {
        ConfirmDialog.toast.success(isApproved ? 'Transaction approved' : 'Transaction rejected');
        return { success: true };
      }
      ConfirmDialog.toast.error(result.message || 'Failed to process');
      return { success: false };
    } catch {
      ConfirmDialog.toast.error('Failed to process');
      return { success: false };
    }
  }

  async returnAsset(transactionId, actualReturnDate, conditionAfter, notes) {
    try {
      const result = await this.api.returnAsset({
        assetTransactionId: transactionId,
        actualReturnDate,
        conditionAfter,
        notes
      });
      if (result.isSuccess) {
        ConfirmDialog.toast.success('Asset returned successfully');
        return { success: true };
      }
      ConfirmDialog.toast.error(result.message || 'Failed to return asset');
      return { success: false };
    } catch {
      ConfirmDialog.toast.error('Failed to return asset');
      return { success: false };
    }
  }

  async cancel(id) {
    const confirmed = await ConfirmDialog.show({
      title: 'Cancel Transaction',
      text: 'Are you sure you want to cancel this transaction?',
      icon: 'warning',
      confirmButtonText: 'Yes, cancel'
    });
    if (!confirmed) return { success: false, cancelled: true };

    try {
      const result = await this.api.cancel(id);
      if (result.isSuccess) {
        ConfirmDialog.toast.success('Transaction cancelled');
        return { success: true };
      }
      ConfirmDialog.toast.error(result.message || 'Failed to cancel');
      return { success: false };
    } catch {
      ConfirmDialog.toast.error('Failed to cancel');
      return { success: false };
    }
  }

  async importTransactions(file) {
    try {
      const result = await this.api.import(file);
      if (result.isSuccess && result.data) {
        const importResult = result.data;
        if (importResult.errorCount > 0) {
          ConfirmDialog.toast.warning(`Import completed: ${importResult.successCount} success, ${importResult.errorCount} errors`);
        } else {
          ConfirmDialog.toast.success(`Import completed: ${importResult.successCount} transactions imported successfully`);
        }
        return { success: true, data: importResult };
      }
      ConfirmDialog.toast.error(result.message || 'Import failed');
      return { success: false };
    } catch {
      ConfirmDialog.toast.error('Failed to import');
      return { success: false };
    }
  }

  async downloadTemplate() {
    try {
      const blob = await this.api.downloadImportTemplate();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', 'Transaction_Import_Template.xlsx');
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      ConfirmDialog.toast.success('Template downloaded');
      return { success: true };
    } catch {
      ConfirmDialog.toast.error('Failed to download template');
      return { success: false };
    }
  }

  getCreateMessage() { return 'Transaction created successfully (Pending approval)'; }
  getUpdateMessage() { return 'Transaction updated successfully'; }
  getDeleteMessage() { return 'Transaction cancelled successfully'; }
  getDeleteTitle() { return 'Cancel Transaction'; }
  getDeleteText() { return 'Are you sure you want to cancel this transaction?'; }
}

export default AssetTransactionsData;