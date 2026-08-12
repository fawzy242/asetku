// src/pages/Login/Login.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';
import utilsHelper from '../../core/utils/utils.helper';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import { handleSuccess, handleApiError } from '../../core/services';

class LoginData extends BaseData {
  constructor() {
    super(apiClient.auth);
    this.setEntityName('user');
  }

  async handleLogin(username, password) {
    // Validation
    if (!username || username.trim().length < 3) {
      ConfirmDialog.toast.error('Username must be at least 3 characters');
      return { success: false, error: 'Username must be at least 3 characters' };
    }
    if (!password || password.length < 4) {
      ConfirmDialog.toast.error('Password must be at least 4 characters');
      return { success: false, error: 'Password must be at least 4 characters' };
    }

    try {
      const result = await this.api.login({ username, password });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Login failed';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false, error: errorMsg };
        }
        
        handleSuccess({
          operation: 'login',
          entityName: 'user',
          message: `Welcome back, ${username}!`,
          result: result.data,
        });
        
        return { success: true, data: result.data.data };
      }
      
      // Fallback
      ConfirmDialog.toast.success(`Welcome back, ${username}!`);
      return { success: true, data: result.data };
    } catch (error) {
      // Error already handled by api.service.js interceptor
      return {
        success: false,
        error: error.userMessage || error.message || 'Login failed',
      };
    }
  }

  async handleForgotPassword(email) {
    if (!utilsHelper.validateEmail(email)) {
      ConfirmDialog.toast.error('Invalid email format');
      return { success: false, error: 'Invalid email format' };
    }

    try {
      const result = await this.api.forgotPassword({ email });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to send reset code';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false, error: errorMsg };
        }
        
        ConfirmDialog.toast.success('Password reset code sent to your email');
        return { success: true, message: result.data.message };
      }
      
      ConfirmDialog.toast.success('Password reset code sent to your email');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to send reset code',
      };
    }
  }

  async handleResetPassword(email, token, newPass, confirmPass) {
    if (!email || !token) {
      ConfirmDialog.toast.error('Email and reset token are required');
      return { success: false };
    }
    if (newPass !== confirmPass) {
      ConfirmDialog.toast.error('Passwords do not match');
      return { success: false };
    }
    if (newPass.length < 4) {
      ConfirmDialog.toast.error('Password must be at least 4 characters');
      return { success: false };
    }

    try {
      const result = await this.api.resetPasswordWithToken({
        email,
        resetToken: token,
        newPassword: newPass,
        confirmPassword: confirmPass,
      });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to reset password';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false, error: errorMsg };
        }
        
        handleSuccess({
          operation: 'reset-password',
          entityName: 'user',
          message: 'Password reset successfully. Please login with your new password.',
          result: result.data,
        });
        
        return { success: true };
      }
      
      ConfirmDialog.toast.success('Password reset successfully');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to reset password',
      };
    }
  }
}

export default LoginData;