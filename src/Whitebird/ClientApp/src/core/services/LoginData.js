import { authApi } from '../api/api';
import BaseData from './BaseData';

class LoginData extends BaseData {
  constructor() {
    super(authApi);
    this.setEntityName('user');
  }

  async handleLogin(username, password) {
    try {
      const result = await this.api.login({ username, password });

      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return {
            success: false,
            error:
              result.data.errors?.join('\n') ||
              result.data.message ||
              'Login failed',
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
        error: error.userMessage || error.message || 'Login failed',
      };
    }
  }

  async validateSession() {
    try {
      const result = await this.api.validateSession();

      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return {
            success: false,
            error: result.data.message || 'Invalid session',
            data: { isValid: false },
          };
        }
        const isValid = result.data.data?.isValid === true;
        return { success: true, data: { isValid } };
      }

      return { success: true, data: { isValid: true } };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return {
          success: false,
          error: 'canceled',
          isCanceled: true,
          data: { isValid: false },
        };
      }
      return {
        success: false,
        error: error.message || 'Failed to validate session',
        data: { isValid: false },
      };
    }
  }

  async logout(sessionToken) {
    try {
      const result = await this.api.logout({ sessionToken });

      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return {
            success: false,
            error: result.data.message || 'Failed to logout',
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
        error: error.message || 'Failed to logout',
      };
    }
  }
}

export default LoginData;