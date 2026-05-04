import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

/**
 * Base class untuk semua Data layer
 * Menyediakan method CRUD standard dengan error handling dan toast notification
 */
class BaseData {
  constructor(api) {
    this.api = api;
  }

  /**
   * Fetch grid data dengan parameter standard
   */
  async fetchGridData(params) {
    try {
      const result = await this.api.getGridData(params);
      return { success: true, data: result.data };
    } catch {
      return { success: false, error: 'Failed to load data' };
    }
  }

  /**
   * Fetch single record by ID
   */
  async fetchById(id) {
    try {
      const result = await this.api.getById(id);
      return result.isSuccess ? { success: true, data: result.data } : { success: false, error: result.message };
    } catch {
      return { success: false, error: 'Failed to load record' };
    }
  }

  /**
   * Create new record
   */
  async create(data) {
    try {
      const result = await this.api.create(data);
      if (result.isSuccess) {
        ConfirmDialog.toast.success(this.getCreateMessage());
        return { success: true, data: result.data };
      }
      ConfirmDialog.toast.error(result.message || 'Failed to create');
      return { success: false };
    } catch {
      ConfirmDialog.toast.error('Failed to create');
      return { success: false };
    }
  }

  /**
   * Update existing record
   */
  async update(id, data) {
    try {
      const result = await this.api.update(id, data);
      if (result.isSuccess) {
        ConfirmDialog.toast.success(this.getUpdateMessage());
        return { success: true };
      }
      ConfirmDialog.toast.error(result.message || 'Failed to update');
      return { success: false };
    } catch {
      ConfirmDialog.toast.error('Failed to update');
      return { success: false };
    }
  }

  /**
   * Delete record with confirmation
   */
  async delete(id) {
    const confirmed = await ConfirmDialog.showDelete(this.getDeleteTitle(), this.getDeleteText());
    if (!confirmed) return { success: false, cancelled: true };
    try {
      const result = await this.api.delete(id);
      if (result.isSuccess) {
        ConfirmDialog.toast.success(this.getDeleteMessage());
        return { success: true };
      }
      ConfirmDialog.toast.error(result.message || 'Failed to delete');
      return { success: false };
    } catch {
      ConfirmDialog.toast.error('Failed to delete');
      return { success: false };
    }
  }

  // Override methods for custom messages
  getCreateMessage() { return 'Record created successfully'; }
  getUpdateMessage() { return 'Record updated successfully'; }
  getDeleteMessage() { return 'Record deleted successfully'; }
  getDeleteTitle() { return 'Delete Record'; }
  getDeleteText() { return 'Are you sure?'; }
}

export default BaseData;