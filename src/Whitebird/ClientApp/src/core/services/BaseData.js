import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

/**
 * Base class untuk semua Data layer
 * Menyediakan method CRUD standard dengan error handling
 * 
 * NOTE: Semua success/error menggunakan MODAL (blocking), bukan toast
 * NOTE: Delete confirmation hanya di sini, JANGAN di menu
 */
class BaseData {
  constructor(api) {
    this.api = api;
  }

  async fetchGridData(params) {
    try {
      const result = await this.api.getGridData(params);
      return { success: true, data: result.data };
    } catch (error) {
      console.error('fetchGridData error:', error);
      return { success: false, error: 'Failed to load data' };
    }
  }

  async fetchById(id) {
    try {
      const result = await this.api.getById(id);
      return result.isSuccess ? { success: true, data: result.data } : { success: false, error: result.message };
    } catch (error) {
      console.error('fetchById error:', error);
      return { success: false, error: 'Failed to load record' };
    }
  }

  async create(data) {
    try {
      const result = await this.api.create(data);
      if (result.isSuccess) {
        await ConfirmDialog.showSuccess('Success', this.getCreateMessage());
        return { success: true, data: result.data };
      }
      await ConfirmDialog.showError('Failed', result.message || 'Failed to create');
      return { success: false, error: result.message };
    } catch (error) {
      console.error('create error:', error);
      await ConfirmDialog.showError('Failed', 'Failed to create');
      return { success: false, error: 'Failed to create' };
    }
  }

  async update(id, data) {
    try {
      const result = await this.api.update(id, data);
      if (result.isSuccess) {
        await ConfirmDialog.showSuccess('Success', this.getUpdateMessage());
        return { success: true };
      }
      await ConfirmDialog.showError('Failed', result.message || 'Failed to update');
      return { success: false, error: result.message };
    } catch (error) {
      console.error('update error:', error);
      await ConfirmDialog.showError('Failed', 'Failed to update');
      return { success: false, error: 'Failed to update' };
    }
  }

  async delete(id) {
    const confirmed = await ConfirmDialog.showDelete(this.getDeleteTitle(), this.getDeleteText());
    if (!confirmed) return { success: false, cancelled: true };
    
    try {
      const result = await this.api.delete(id);
      if (result.isSuccess) {
        await ConfirmDialog.showSuccess('Success', this.getDeleteMessage());
        return { success: true };
      }
      await ConfirmDialog.showError('Failed', result.message || 'Failed to delete');
      return { success: false, error: result.message };
    } catch (error) {
      console.error('delete error:', error);
      await ConfirmDialog.showError('Failed', 'Failed to delete');
      return { success: false, error: 'Failed to delete' };
    }
  }

  getCreateMessage() { return 'Record created successfully'; }
  getUpdateMessage() { return 'Record updated successfully'; }
  getDeleteMessage() { return 'Record deleted successfully'; }
  getDeleteTitle() { return 'Delete Record'; }
  getDeleteText() { return 'Are you sure?'; }
}

export default BaseData;