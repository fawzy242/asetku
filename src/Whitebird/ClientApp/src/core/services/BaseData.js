// src/core/services/BaseData.js
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import { handleSuccess } from './successHandler';
import { handleApiError } from './apiErrorHandler';

/**
 * Base class untuk semua Data layer
 * Menggunakan apiClient yang terpusat.
 */
class BaseData {
  constructor(api) {
    this.api = api;
    this._entityName = this.constructor.name.replace('Data', '').toLowerCase() || 'record';
  }

  setEntityName(name) {
    this._entityName = name;
    return this;
  }

  getEntityName() {
    return this._entityName;
  }

  async fetchGridData(params) {
    try {
      const result = await this.api.getGrid(params);
      if (result.data?.isSuccess !== undefined) {
        if (result.data.isSuccess) {
          return { success: true, data: result.data.data || { data: [], totalCount: 0 } };
        }
        // FIX: Tampilkan error dari backend
        const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to load data';
        ConfirmDialog.toast.error(errorMsg);
        return { success: false, error: errorMsg };
      }
      return { success: true, data: result.data || { data: [], totalCount: 0 } };
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to load data';
      ConfirmDialog.toast.error(errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  async fetchById(id) {
    try {
      const result = await this.api.getById(id);
      if (result.data?.isSuccess !== undefined) {
        if (result.data.isSuccess) {
          return { success: true, data: result.data.data };
        }
        const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Record not found';
        ConfirmDialog.toast.error(errorMsg);
        return { success: false, error: errorMsg };
      }
      return { success: true, data: result.data };
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to load record';
      ConfirmDialog.toast.error(errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  async create(data) {
    try {
      const result = await this.api.create(data);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to create';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false, errors: result.data.errors };
        }
        
        handleSuccess({
          operation: 'create',
          entityName: this._entityName,
          result: result.data,
        });
        
        return { success: true, data: result.data.data };
      }
      
      ConfirmDialog.toast.success(`${this._entityName} created successfully`);
      return { success: true, data: result.data };
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to create';
      ConfirmDialog.toast.error(errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  async update(id, data) {
    try {
      const result = await this.api.update(id, data);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to update';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false, errors: result.data.errors };
        }
        
        handleSuccess({
          operation: 'update',
          entityName: this._entityName,
          result: result.data,
        });
        
        return { success: true, data: result.data.data };
      }
      
      ConfirmDialog.toast.success(`${this._entityName} updated successfully`);
      return { success: true, data: result.data };
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to update';
      ConfirmDialog.toast.error(errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  async delete(id) {
    try {
      const result = await this.api.delete(id);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to delete';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        handleSuccess({
          operation: 'delete',
          entityName: this._entityName,
          result: result.data,
        });
        
        return { success: true };
      }
      
      ConfirmDialog.toast.success(`${this._entityName} deleted successfully`);
      return { success: true };
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to delete';
      ConfirmDialog.toast.error(errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  async softDelete(id) {
    try {
      const result = await this.api.softDelete(id);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to soft delete';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        handleSuccess({
          operation: 'delete',
          entityName: this._entityName,
          message: `${this._entityName} soft deleted successfully`,
          result: result.data,
        });
        
        return { success: true };
      }
      
      ConfirmDialog.toast.success(`${this._entityName} soft deleted successfully`);
      return { success: true };
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to soft delete';
      ConfirmDialog.toast.error(errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  getCreateMessage() {
    return `${this._entityName} created successfully`;
  }
  getUpdateMessage() {
    return `${this._entityName} updated successfully`;
  }
  getDeleteMessage() {
    return `${this._entityName} deleted successfully`;
  }
  getDeleteTitle() {
    return `Delete ${this._entityName.charAt(0).toUpperCase() + this._entityName.slice(1)}`;
  }
  getDeleteText() {
    return `Are you sure you want to delete this ${this._entityName}?`;
  }
}

export default BaseData;