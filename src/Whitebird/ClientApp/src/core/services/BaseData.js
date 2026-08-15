import { downloadBlob } from '../utils/file.utils';
import { isCanceledError } from '../utils/error.utils';

/**
 * Base class for all Data layer services
 * Pure data operations — NO UI logic (toast, alert, modal)
 * UI feedback should be handled by components/hooks
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
    return isCanceledError(error);
  }

  // ============================================================
  // GRID
  // ============================================================

  async fetchGridData(params) {
    try {
      const result = await this.api.getGrid(params);
      
      if (result.data?.isSuccess !== undefined) {
        if (result.data.isSuccess) {
          return { 
            success: true, 
            data: result.data.data || { data: [], totalCount: 0 } 
          };
        }
        return { 
          success: false, 
          error: result.data.errors?.join('\n') || result.data.message || 'Failed to load data',
          errorType: result.data.errorType,
        };
      }
      
      return { success: true, data: result.data || { data: [], totalCount: 0 } };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return { 
        success: false, 
        error: error.userMessage || error.message || 'Failed to load data' 
      };
    }
  }

  // ============================================================
  // CRUD
  // ============================================================

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
        return { 
          success: false, 
          error: errorMsg, 
          isNotFound: result.data.errorType === 'NotFound',
          errorType: result.data.errorType,
        };
      }
      
      return { success: false, error: 'Record not found', isNotFound: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      const errorMsg = error.userMessage || error.message || 'Failed to load record';
      return { 
        success: false, 
        error: errorMsg, 
        isNotFound: error.response?.status === 404 
      };
    }
  }

  async create(data) {
    try {
      const result = await this.api.create(data);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            errors: result.data.errors || [result.data.message || 'Failed to create'],
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
      return { 
        success: false, 
        error: error.userMessage || error.message || 'Failed to create' 
      };
    }
  }

  async update(id, data) {
    try {
      const result = await this.api.update(id, data);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            errors: result.data.errors || [result.data.message || 'Failed to update'],
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
      return { 
        success: false, 
        error: error.userMessage || error.message || 'Failed to update' 
      };
    }
  }

  async delete(id) {
    try {
      const result = await this.api.delete(id);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Failed to delete',
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
      return { 
        success: false, 
        error: error.userMessage || error.message || 'Failed to delete' 
      };
    }
  }

  async softDelete(id) {
    try {
      const result = await this.api.softDelete(id);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Failed to soft delete',
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
      return { 
        success: false, 
        error: error.userMessage || error.message || 'Failed to soft delete' 
      };
    }
  }

  // ============================================================
  // GENERIC BULK ACTIVATE
  // ============================================================

  async bulkActivate(ids, activate) {
    const actionText = activate ? 'activate' : 'deactivate';
    
    try {
      const endpoint = activate ? this.api.activate : this.api.deactivate;
      const result = await endpoint({ ids });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || `Failed to ${actionText}`,
            errorType: result.data.errorType,
          };
        }
        
        const count = result.data.data || ids.length;
        return { 
          success: true, 
          data: count,
          message: `${count} ${this._entityName}(s) ${actionText}d successfully`,
        };
      }
      
      return { 
        success: true, 
        data: ids.length,
        message: `${ids.length} ${this._entityName}(s) ${actionText}d successfully`,
      };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return { 
        success: false, 
        error: error.userMessage || error.message || `Failed to ${actionText}` 
      };
    }
  }

  // ============================================================
  // DOWNLOAD TEMPLATE
  // ============================================================

  async downloadTemplate(templateName) {
    try {
      const response = await this.api.getImportTemplate();
      const blob = response?.data;
      
      if (!blob || !(blob instanceof Blob) || blob.size === 0) {
        return { success: false, error: 'Template file is empty' };
      }
      
      const fileName = templateName || `${this._entityName}_import_template.xlsx`;
      downloadBlob(blob, fileName);
      
      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      
      if (error.response?.data instanceof Blob) {
        try {
          const text = await error.response.data.text();
          const parsed = JSON.parse(text);
          if (parsed.message) {
            return { success: false, error: parsed.message };
          }
        } catch {
          // Not JSON, continue
        }
      }
      
      return { 
        success: false, 
        error: error.userMessage || error.message || 'Failed to download template' 
      };
    }
  }
}

export default BaseData;