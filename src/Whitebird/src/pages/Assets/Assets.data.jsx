import AssetsApi from './Assets.api';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class AssetsData {
  constructor() { this.api = AssetsApi; }

  async fetchGridData({ page, pageSize, search = '', sortBy = 'assetCode', sortDescending = false, ...filters }) {
    try {
      const result = await this.api.getGridData({ page, pageSize, search, sortBy, sortDescending, ...filters });
      return { success: true, data: result.data };
    } catch { return { success: false, error: 'Failed to load assets' }; }
  }

  async fetchById(id) {
    try {
      const result = await this.api.getById(id);
      return result.isSuccess ? { success: true, data: result.data } : { success: false, error: result.message };
    } catch { return { success: false, error: 'Failed to load asset' }; }
  }

  async fetchDropdownData() {
    try {
      const [categories, suppliers, employees] = await Promise.all([
        this.api.getCategories(), this.api.getSuppliers(), this.api.getEmployees()
      ]);
      return { success: true, data: { categories: categories.data || [], suppliers: suppliers.data || [], employees: employees.data || [] } };
    } catch { return { success: false, error: 'Failed to load dropdown data' }; }
  }

  async create(data) {
    try {
      const result = await this.api.create(data);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Success', 'Asset created'); return { success: true, data: result.data }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed to create');
      return { success: false, error: result.message };
    } catch { await ConfirmDialog.showError('Error', 'Failed to create'); return { success: false }; }
  }

  async update(id, data) {
    try {
      const result = await this.api.update(id, data);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Success', 'Asset updated'); return { success: true, data: result.data }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed to update');
      return { success: false, error: result.message };
    } catch { await ConfirmDialog.showError('Error', 'Failed to update'); return { success: false }; }
  }

  async delete(id) {
    const confirmed = await ConfirmDialog.showDelete('Delete Asset', 'Are you sure?');
    if (!confirmed) return { success: false, cancelled: true };
    try {
      const result = await this.api.delete(id);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Deleted', 'Asset deleted'); return { success: true }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed to delete');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to delete'); return { success: false }; }
  }
}

export default AssetsData;