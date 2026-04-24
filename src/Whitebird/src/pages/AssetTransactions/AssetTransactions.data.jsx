import AssetTransactionsApi from './AssetTransactions.api';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class AssetTransactionsData {
  constructor() { this.api = AssetTransactionsApi; }

  async fetchGridData({ page, pageSize, search = '', status = '', type = '' }) {
    try {
      const result = await this.api.getGridData({ page, pageSize, search, status, type });
      return { success: true, data: result.data };
    } catch { return { success: false, error: 'Failed to load transactions' }; }
  }

  async fetchById(id) {
    try {
      const result = await this.api.getById(id);
      return result.isSuccess ? { success: true, data: result.data } : { success: false, error: result.message };
    } catch { return { success: false, error: 'Failed to load transaction' }; }
  }

  async fetchDropdownData() {
    try {
      const [assets, employees, locations] = await Promise.all([this.api.getAssets(), this.api.getEmployees(), this.api.getLocations()]);
      return { success: true, data: { assets: assets.data || [], employees: employees.data || [], locations: locations.data || [] } };
    } catch { return { success: false, error: 'Failed to load dropdown data' }; }
  }

  async create(data) {
    try {
      const result = await this.api.create(data);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Success', 'Transaction created'); return { success: true, data: result.data }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed to create');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to create'); return { success: false }; }
  }

  async update(id, data) {
    try {
      const result = await this.api.update(id, data);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Success', 'Transaction updated'); return { success: true }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed to update');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to update'); return { success: false }; }
  }

  async approve(id, isApproved) {
    try {
      const result = await this.api.approve(id, { assetTransactionId: id, isApproved, approvalNotes: '' });
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Success', isApproved ? 'Approved' : 'Rejected'); return { success: true }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to process'); return { success: false }; }
  }

  async returnAsset(transactionId, actualReturnDate, conditionAfter, notes) {
    try {
      const result = await this.api.returnAsset({ assetTransactionId: transactionId, actualReturnDate, conditionAfter, notes });
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Success', 'Asset returned'); return { success: true }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to return'); return { success: false }; }
  }

  async cancel(id) {
    const confirmed = await ConfirmDialog.show({ title: 'Cancel Transaction', text: 'Are you sure?', icon: 'warning', confirmButtonText: 'Yes, cancel' });
    if (!confirmed) return { success: false, cancelled: true };
    try {
      const result = await this.api.cancel(id);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Cancelled', 'Transaction cancelled'); return { success: true }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to cancel'); return { success: false }; }
  }
}

export default AssetTransactionsData;