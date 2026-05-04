import LoginApi from './Login.api';
import utilsHelper from '../../core/utils/utils.helper';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class LoginData {
  constructor() { this.api = LoginApi; }

  async handleLogin(email, password) {
    if (!utilsHelper.validateEmail(email)) return { success: false, error: 'Invalid email' };
    if (!password || password.length < 4) return { success: false, error: 'Min 4 characters' };
    try { const r = await this.api.login(email, password); if (r.isSuccess) return { success: true, data: r.data }; return { success: false, error: r.message || 'Login failed' }; }
    catch { return { success: false, error: 'Invalid credentials' }; }
  }

  async handleForgotPassword(email) {
    if (!utilsHelper.validateEmail(email)) { ConfirmDialog.toast.error('Invalid email'); return { success: false }; }
    try { const r = await this.api.forgotPassword(email); if (r.isSuccess) ConfirmDialog.toast.success('Reset code sent'); return { success: r.isSuccess, message: r.message }; }
    catch { ConfirmDialog.toast.error('Failed'); return { success: false }; }
  }

  async handleResetPassword(email, token, newPass, confirmPass) {
    if (!email || !token) { ConfirmDialog.toast.error('Email and token required'); return { success: false }; }
    if (newPass !== confirmPass) { ConfirmDialog.toast.error('Passwords do not match'); return { success: false }; }
    if (newPass.length < 4) { ConfirmDialog.toast.error('Min 4 characters'); return { success: false }; }
    try { const r = await this.api.resetPassword(email, token, newPass, confirmPass); if (r.isSuccess) ConfirmDialog.toast.success('Password reset'); return { success: r.isSuccess, message: r.message }; }
    catch { ConfirmDialog.toast.error('Failed'); return { success: false }; }
  }
}

export default LoginData;