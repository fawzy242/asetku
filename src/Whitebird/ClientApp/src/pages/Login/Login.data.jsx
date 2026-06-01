import LoginApi from './Login.api';
import BaseData from '../../core/services/BaseData';
import utilsHelper from '../../core/utils/utils.helper';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class LoginData extends BaseData {
  constructor() {
    super(LoginApi);
  }

  async handleLogin(username, password) {
    if (!username || username.trim().length < 3) {
      return { success: false, error: 'Username must be at least 3 characters' };
    }
    if (!password || password.length < 4) {
      return { success: false, error: 'Password must be at least 4 characters' };
    }
    try {
      const r = await this.api.login(username, password);
      if (r.isSuccess) return { success: true, data: r.data };
      return { success: false, error: r.message || 'Login failed' };
    } catch {
      return { success: false, error: 'Invalid username or password' };
    }
  }

  async handleForgotPassword(email) {
    if (!utilsHelper.validateEmail(email)) {
      ConfirmDialog.toast.error('Invalid email');
      return { success: false };
    }
    try {
      const r = await this.api.forgotPassword(email);
      if (r.isSuccess) ConfirmDialog.toast.success('Reset code sent');
      return { success: r.isSuccess, message: r.message };
    } catch {
      ConfirmDialog.toast.error('Failed');
      return { success: false };
    }
  }

  async handleResetPassword(email, token, newPass, confirmPass) {
    if (!email || !token) {
      ConfirmDialog.toast.error('Email and token required');
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
      const r = await this.api.resetPassword(email, token, newPass, confirmPass);
      if (r.isSuccess) ConfirmDialog.toast.success('Password reset');
      return { success: r.isSuccess, message: r.message };
    } catch {
      ConfirmDialog.toast.error('Failed');
      return { success: false };
    }
  }
}

export default LoginData;