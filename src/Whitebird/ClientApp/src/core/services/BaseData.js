// src/core/services/BaseData.js
import { downloadBlob } from '../utils/file.utils';
import { isCanceledError } from '../utils/error.utils';

/**
 * Base class for all Data layer services.
 * Pure data operations — NO UI logic (toast, alert, modal).
 * UI feedback should be handled by components/hooks.
 */
class BaseData {
  constructor(api) {
    this.api = api;
    this._entityName =
      this.constructor.name.replace('Data', '').toLowerCase() || 'record';
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

  getTemplateFileName() {
    return `${this._entityName}_import_template.xlsx`;
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
            data: result.data.data || { data: [], totalCount: 0 },
          };
        }
        return {
          success: false,
          error:
            result.data.errors?.join('\n') ||
            result.data.message ||
            'Failed to load data',
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
        error: error.userMessage || error.message || 'Failed to load data',
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
        const errorMsg =
          result.data.errors?.join('\n') ||
          result.data.message ||
          'Record not found';
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
      const errorMsg =
        error.userMessage || error.message || 'Failed to load record';
      return {
        success: false,
        error: errorMsg,
        isNotFound: error.response?.status === 404,
      };
    }
  }

  async create(data) {
    try {
      const result = await this.api.create(data);

      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg =
            result.data.errors?.join('\n') ||
            result.data.message ||
            'Failed to create';
          return {
            success: false,
            error: errorMsg,
            errors: result.data.errors || [result.data.message || 'Failed to create'],
            errorType: result.data.errorType,
            fieldErrors:
              result.data.fieldErrors && typeof result.data.fieldErrors === 'object'
                ? result.data.fieldErrors
                : null,
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
        error: error.userMessage || error.message || 'Failed to create',
        fieldErrors:
          error.fieldErrors && typeof error.fieldErrors === 'object'
            ? error.fieldErrors
            : null,
      };
    }
  }

  async update(id, data) {
    try {
      const result = await this.api.update(id, data);

      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg =
            result.data.errors?.join('\n') ||
            result.data.message ||
            'Failed to update';
          return {
            success: false,
            error: errorMsg,
            errors: result.data.errors || [result.data.message || 'Failed to update'],
            errorType: result.data.errorType,
            fieldErrors:
              result.data.fieldErrors && typeof result.data.fieldErrors === 'object'
                ? result.data.fieldErrors
                : null,
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
        error: error.userMessage || error.message || 'Failed to update',
        fieldErrors:
          error.fieldErrors && typeof error.fieldErrors === 'object'
            ? error.fieldErrors
            : null,
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
            error:
              result.data.errors?.join('\n') ||
              result.data.message ||
              'Failed to delete',
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
        error: error.userMessage || error.message || 'Failed to delete',
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
            error:
              result.data.errors?.join('\n') ||
              result.data.message ||
              'Failed to soft delete',
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
        error: error.userMessage || error.message || 'Failed to soft delete',
      };
    }
  }

  // ============================================================
  // BULK ACTIVATE — single request, existing /activate|/deactivate endpoints
  // Payload matches Swagger BulkActivateRequest { ids, activate }.
  // ============================================================
  async bulkActivate(ids, activate) {
    const actionText = activate ? 'activate' : 'deactivate';
    const normalizedActivate = !!activate;

    try {
      const endpoint = normalizedActivate
        ? this.api.activate
        : this.api.deactivate;
      const result = await endpoint({ ids, activate: normalizedActivate });

      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return {
            success: false,
            error:
              result.data.errors?.join('\n') ||
              result.data.message ||
              `Failed to ${actionText}`,
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
        error: error.userMessage || error.message || `Failed to ${actionText}`,
      };
    }
  }

  // ============================================================
  // BULK DELETE — single POST to the documented /{Domain}/bulk-delete
  // endpoint (Swagger: BulkDeleteRequest { ids: int[] }, minItems: 1).
  //
  // On success the operation is all-or-nothing from the backend's
  // perspective; per-record status is not returned by the API. The
  // return shape below preserves the exact fields that callers rely on
  // (succeeded / failed / successCount / failCount / total) so no page-
  // level change is required.
  //
  // `partial` is retained for future-proofing (false unless the backend
  // ever starts returning per-record results).
  // ============================================================
  async bulkDelete(ids) {
    if (!Array.isArray(ids) || ids.length === 0) {
      return { success: false, partial: false, error: 'No records selected' };
    }

    // Guard: bulk-delete endpoint is not defined for every domain (e.g.,
    // UserRoles). Fail fast rather than throw on `undefined is not a
    // function`.
    if (typeof this.api.bulkDelete !== 'function') {
      return {
        success: false,
        partial: false,
        error: `Bulk delete is not supported for ${this._entityName}`,
      };
    }

    try {
      const result = await this.api.bulkDelete({ ids });

      // ServiceResult envelope
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg =
            result.data.errors?.join('\n') ||
            result.data.message ||
            'Failed to delete records';
          return {
            success: false,
            partial: false,
            data: {
              succeeded: [],
              failed: ids.map((id) => ({ id, error: errorMsg })),
              successCount: 0,
              failCount: ids.length,
              total: ids.length,
            },
            error: errorMsg,
            errorType: result.data.errorType,
          };
        }

        // Successful delete: the number of affected records may or may not
        // be in `data.data`. Fall back to `ids.length` (backend accepted
        // the request → assume all deleted).
        const count =
          typeof result.data.data === 'number' ? result.data.data : ids.length;
        const clampedCount = Math.min(count, ids.length);
        return {
          success: true,
          partial: false,
          data: {
            succeeded: ids.slice(0, clampedCount),
            failed: [],
            successCount: clampedCount,
            failCount: ids.length - clampedCount,
            total: ids.length,
          },
        };
      }

      // Legacy envelope (no isSuccess field) — treat 200 as success.
      return {
        success: true,
        partial: false,
        data: {
          succeeded: ids.slice(),
          failed: [],
          successCount: ids.length,
          failCount: 0,
          total: ids.length,
        },
      };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return {
          success: false,
          partial: false,
          error: 'canceled',
          isCanceled: true,
          data: {
            succeeded: [],
            failed: ids.map((id) => ({ id, error: 'canceled' })),
            successCount: 0,
            failCount: ids.length,
            total: ids.length,
          },
        };
      }
      const errorMsg =
        error.userMessage || error.message || 'Failed to delete records';
      return {
        success: false,
        partial: false,
        data: {
          succeeded: [],
          failed: ids.map((id) => ({ id, error: errorMsg })),
          successCount: 0,
          failCount: ids.length,
          total: ids.length,
        },
        error: errorMsg,
      };
    }
  }

  // ============================================================
  // DOWNLOAD TEMPLATE
  // ============================================================
  async downloadTemplate() {
    const fileName = this.getTemplateFileName();

    try {
      const response = await this.api.getImportTemplate();
      const blob = response?.data;

      if (!blob || !(blob instanceof Blob) || blob.size === 0) {
        return { success: false, error: 'Template file is empty' };
      }

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
          // Not JSON — fall through
        }
      }

      return {
        success: false,
        error:
          error.userMessage ||
          error.message ||
          'Failed to download template',
      };
    }
  }
}

export default BaseData;