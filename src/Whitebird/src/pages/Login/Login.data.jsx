import LoginApi from './Login.api';
import utilsHelper from '../../core/utils/utils.helper';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class LoginData {
  constructor() { this.api = LoginApi; }

  async handleLogin(email, password) {
    if (!utilsHelper.validateEmail(email)) return { success: false, error: 'Invalid email' };
    if (!password || password.length < 4) return { success: false, error: 'Min 4 characters' };
    try {
      const result = await this.api.login(email, password);
      if (result.isSuccess) return { success: true, data: result.data };
      return { success: false, error: result.message || 'Login failed' };
    } catch { return { success: false, error: 'Invalid credentials' }; }
  }

  async handleForgotPassword(email) {
    if (!utilsHelper.validateEmail(email)) return { success: false, error: 'Invalid email' };
    try { const r = await this.api.forgotPassword(email); return { success: r.isSuccess, message: r.message }; }
    catch { return { success: false, error: 'Failed' }; }
  }

  async handleResetPassword(email, token, newPass, confirmPass) {
    if (!email || !token) return { success: false, error: 'Email and token required' };
    if (newPass !== confirmPass) { await ConfirmDialog.showError('Error', 'Passwords do not match'); return { success: false }; }
    if (newPass.length < 4) { await ConfirmDialog.showError('Error', 'Min 4 characters'); return { success: false }; }
    try { const r = await this.api.resetPassword(email, token, newPass, confirmPass); return { success: r.isSuccess, message: r.message }; }
    catch { return { success: false, error: 'Failed' }; }
  }
}

export default LoginData;