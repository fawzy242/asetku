import ProfileApi from './Profile.api';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class ProfileData {
  constructor() { this.api = ProfileApi; }

  async fetchProfile() {
    try { const r = await this.api.getCurrentUser(); return r.isSuccess ? { success: true, data: r.data } : { success: false, error: r.message }; }
    catch { return { success: false, error: 'Failed to load profile' }; }
  }

  async updateProfile(data) {
    try { const r = await this.api.updateProfile(data); if (r.isSuccess) { await ConfirmDialog.showSuccess('Success', 'Profile updated'); return { success: true }; } await ConfirmDialog.showError('Error', r.message || 'Failed'); return { success: false }; }
    catch { await ConfirmDialog.showError('Error', 'Failed to update'); return { success: false }; }
  }

  async changePassword(oldPassword, newPassword, confirmPassword) {
    if (newPassword !== confirmPassword) { await ConfirmDialog.showError('Error', 'Passwords do not match'); return { success: false }; }
    if (newPassword.length < 4) { await ConfirmDialog.showError('Error', 'Min 4 characters'); return { success: false }; }
    try { const r = await this.api.changePassword({ oldPassword, newPassword, confirmPassword }); if (r.isSuccess) { await ConfirmDialog.showSuccess('Success', 'Password changed'); return { success: true }; } await ConfirmDialog.showError('Error', r.message || 'Failed'); return { success: false }; }
    catch { await ConfirmDialog.showError('Error', 'Failed to change'); return { success: false }; }
  }
}

export default ProfileData;