import CategoriesApi from './Categories.api';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class CategoriesData {
  constructor() { this.api = CategoriesApi; }

  async fetchGridData({ page, pageSize, search = '' }) {
    try {
      const result = await this.api.getGridData({ page, pageSize, search });
      return { success: true, data: result.data };
    } catch { return { success: false, error: 'Failed to load categories' }; }
  }

  async fetchById(id) {
    try {
      const result = await this.api.getById(id);
      return result.isSuccess ? { success: true, data: result.data } : { success: false, error: result.message };
    } catch { return { success: false, error: 'Failed to load category' }; }
  }

  async fetchParentCategories() {
    try {
      const result = await this.api.getActiveOnly();
      return { success: true, data: result.data || [] };
    } catch { return { success: false, error: 'Failed to load parent categories' }; }
  }

  async create(data) {
    try {
      const result = await this.api.create(data);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Success', 'Category created'); return { success: true, data: result.data }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed to create');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to create'); return { success: false }; }
  }

  async update(id, data) {
    try {
      const result = await this.api.update(id, data);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Success', 'Category updated'); return { success: true }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed to update');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to update'); return { success: false }; }
  }

  async delete(id) {
    const confirmed = await ConfirmDialog.showDelete('Delete Category', 'Are you sure?');
    if (!confirmed) return { success: false, cancelled: true };
    try {
      const result = await this.api.delete(id);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Deleted', 'Category deleted'); return { success: true }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed to delete');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to delete'); return { success: false }; }
  }
}

export default CategoriesData;