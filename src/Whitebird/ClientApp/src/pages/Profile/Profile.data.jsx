import ProfileApi from './Profile.api';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class ProfileData {
  constructor() {
    this.api = ProfileApi;
  }

  async fetchProfile() {
    try {
      const r = await this.api.getCurrentUser();
      return r.isSuccess ? { success: true, data: r.data } : { success: false, error: r.message };
    } catch {
      return { success: false, error: 'Failed to load profile' };
    }
  }

  async changePassword(oldPassword, newPassword, confirmPassword) {
    if (newPassword !== confirmPassword) {
      ConfirmDialog.toast.error('Passwords do not match');
      return { success: false };
    }
    if (newPassword.length < 4) {
      ConfirmDialog.toast.error('Password must be at least 4 characters');
      return { success: false };
    }
    try {
      const r = await this.api.changePassword({ oldPassword, newPassword, confirmPassword });
      if (r.isSuccess) {
        ConfirmDialog.toast.success('Password changed');
        return { success: true };
      }
      ConfirmDialog.toast.error(r.message || 'Failed');
      return { success: false };
    } catch {
      ConfirmDialog.toast.error('Failed to change password');
      return { success: false };
    }
  }
}

export default ProfileData;