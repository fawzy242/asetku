// src/core/services/BaseData.js
import { alertUtils } from '../../lib/sweetAlert';
// HAPUS: import { handleSuccess } from './successHandler';

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

  isCanceledError(error) {
    return error?.name === 'CanceledError' || 
           error?.message === 'canceled' || 
           error?.isCanceled === true ||
           error?.code === 'ERR_CANCELED';
  }

  async fetchGridData(params) {
    try {
      const result = await this.api.getGrid(params);
      if (result.data?.isSuccess !== undefined) {
        if (result.data.isSuccess) {
          return { success: true, data: result.data.data || { data: [], totalCount: 0 } };
        }
        const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to load data';
        alertUtils.error('Error', errorMsg);
        return { success: false, error: errorMsg };
      }
      return { success: true, data: result.data || { data: [], totalCount: 0 } };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      const errorMsg = error.userMessage || error.message || 'Failed to load data';
      alertUtils.error('Error', errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  async fetchById(id) {
    try {
      const result = await this.api.getById(id);
      
      if (result?.data?.isSuccess === true && result?.data?.data) {
        return { success: true, data: result.data.data };
      }
      
      if (result?.data && !result?.data?.isSuccess) {
        return { success: true, data: result.data };
      }
      
      if (result?.data?.isSuccess === false) {
        const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Record not found';
        if (result.data.errorType === 'NotFound' || result.data.status === 404) {
          return { success: false, error: errorMsg, isNotFound: true };
        }
        alertUtils.error('Error', errorMsg);
        return { success: false, error: errorMsg };
      }
      
      return { success: false, error: 'Record not found', isNotFound: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      const errorMsg = error.userMessage || error.message || 'Failed to load record';
      if (error.response?.status === 404) {
        return { success: false, error: errorMsg, isNotFound: true };
      }
      alertUtils.error('Error', errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  async create(data) {
    try {
      const result = await this.api.create(data);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to create';
          alertUtils.error('Error', errorMsg);
          return { success: false, errors: result.data.errors };
        }
        
        return { success: true, data: result.data.data };
      }
      
      return { success: true, data: result.data };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      const errorMsg = error.userMessage || error.message || 'Failed to create';
      alertUtils.error('Error', errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  async update(id, data) {
    try {
      const result = await this.api.update(id, data);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to update';
          alertUtils.error('Error', errorMsg);
          return { success: false, errors: result.data.errors };
        }
        
        return { success: true, data: result.data.data };
      }
      
      return { success: true, data: result.data };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      const errorMsg = error.userMessage || error.message || 'Failed to update';
      alertUtils.error('Error', errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  async delete(id) {
    try {
      const result = await this.api.delete(id);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to delete';
          alertUtils.error('Error', errorMsg);
          return { success: false };
        }
        
        return { success: true };
      }
      
      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      const errorMsg = error.userMessage || error.message || 'Failed to delete';
      alertUtils.error('Error', errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  async softDelete(id) {
    try {
      const result = await this.api.softDelete(id);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to soft delete';
          alertUtils.error('Error', errorMsg);
          return { success: false };
        }
        
        return { success: true };
      }
      
      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      const errorMsg = error.userMessage || error.message || 'Failed to soft delete';
      alertUtils.error('Error', errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  // ============================================================
  // DEFAULT DOWNLOAD TEMPLATE - OVERRIDE DI CHILD CLASS
  // ============================================================
  async downloadTemplate() {
    try {
      // FIX: api.getImportTemplate() mengembalikan axios response
      // Kita perlu mengambil response.data (Blob)
      const response = await this.api.getImportTemplate();
      
      // FIX: Ambil data dari response (axios response wrapper)
      const blob = response?.data;
      
      if (!blob || !(blob instanceof Blob) || blob.size === 0) {
        console.warn('[BaseData.downloadTemplate] Empty or invalid blob:', blob);
        alertUtils.error('Error', 'Template file is empty. Please try again.');
        return { success: false };
      }
      
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `${this._entityName}_import_template.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      
      alertUtils.success('Success', 'Template downloaded successfully');
      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      const errorMsg = error.userMessage || error.message || 'Failed to download template';
      alertUtils.error('Error', errorMsg);
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