// src/core/services/UserRolesData.js
import { userRolesApi } from '../api/api';
import BaseData from './BaseData';

class UserRolesData extends BaseData {
  constructor() {
    super(userRolesApi);
    this.setEntityName('userRole');
  }

  // `fetchGridData` from BaseData works because userRolesApi.getGrid exists
  // and the response envelope matches the shared shape.

  async fetchByUser(userId) {
    try {
      const result = await this.api.getByUser(userId);
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return {
            success: false,
            error:
              result.data.errors?.join('\n') ||
              result.data.message ||
              'Failed to load user roles',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data || [] };
      }
      return { success: true, data: result.data || [] };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, data: [], error: 'canceled', isCanceled: true };
      }
      return {
        success: false,
        data: [],
        error: error.userMessage || error.message || 'Failed to load user roles',
      };
    }
  }

  async fetchByRole(roleId) {
    try {
      const result = await this.api.getByRole(roleId);
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return {
            success: false,
            error:
              result.data.errors?.join('\n') ||
              result.data.message ||
              'Failed to load role assignments',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data || [] };
      }
      return { success: true, data: result.data || [] };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return {
          success: false,
          data: [],
          error: 'canceled',
          isCanceled: true,
        };
      }
      return {
        success: false,
        data: [],
        error:
          error.userMessage ||
          error.message ||
          'Failed to load role assignments',
      };
    }
  }

  async assignRole(userId, roleId) {
    try {
      const result = await this.api.assign({ userId, roleId });
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return {
            success: false,
            error:
              result.data.errors?.join('\n') ||
              result.data.message ||
              'Failed to assign role',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data };
      }
      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to assign role',
      };
    }
  }

  async unassignRole(userId, roleId) {
    try {
      const result = await this.api.unassign(userId, roleId);
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return {
            success: false,
            error:
              result.data.errors?.join('\n') ||
              result.data.message ||
              'Failed to remove role',
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
        error: error.userMessage || error.message || 'Failed to remove role',
      };
    }
  }

  async replaceUserRoles(userId, roleIds) {
    try {
      const result = await this.api.replaceUserRoles(userId, { roleIds });
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return {
            success: false,
            error:
              result.data.errors?.join('\n') ||
              result.data.message ||
              'Failed to update roles',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data };
      }
      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return {
        success: false,
        error:
          error.userMessage || error.message || 'Failed to update roles',
      };
    }
  }
}

export default UserRolesData;