import AssetTransactionsApi from './AssetTransactions.api';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class AssetTransactionsData extends BaseData {
  constructor() {
    super(AssetTransactionsApi);
  }

  async approve(id, isApproved) {
    try {
      const result = await this.api.approve(id, { 
        assetTransactionId: id, 
        isApproved, 
        approvalNotes: '' 
      });
      if (result.isSuccess) { 
        ConfirmDialog.toast.success(isApproved ? 'Approved' : 'Rejected'); 
        return { success: true }; 
      }
      ConfirmDialog.toast.error(result.message || 'Failed');
      return { success: false };
    } catch { 
      ConfirmDialog.toast.error('Failed to process'); 
      return { success: false }; 
    }
  }

  async returnAsset(transactionId, actualReturnDate, conditionAfter, notes, damageReason) {
    try {
      const result = await this.api.returnAsset({ 
        assetTransactionId: transactionId, 
        actualReturnDate, 
        conditionAfter, 
        notes,
        damageReason: damageReason || null,
      });
      if (result.isSuccess) { 
        ConfirmDialog.toast.success('Asset returned'); 
        return { success: true }; 
      }
      ConfirmDialog.toast.error(result.message || 'Failed');
      return { success: false };
    } catch { 
      ConfirmDialog.toast.error('Failed to return'); 
      return { success: false }; 
    }
  }

  async cancel(id) {
    const confirmed = await ConfirmDialog.show({ 
      title: 'Cancel Transaction', 
      text: 'Are you sure?', 
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
      ConfirmDialog.toast.error(result.message || 'Failed');
      return { success: false };
    } catch {
      ConfirmDialog.toast.error('Failed to cancel');
      return { success: false };
    }
  }

  getCreateMessage() { return 'Transaction created successfully'; }
  getUpdateMessage() { return 'Transaction updated successfully'; }
  getDeleteMessage() { return 'Transaction cancelled successfully'; }
  getDeleteTitle() { return 'Cancel Transaction'; }
  getDeleteText() { return 'Are you sure you want to cancel this transaction?'; }
}

export default AssetTransactionsData;