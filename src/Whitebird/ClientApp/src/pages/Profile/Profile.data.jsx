// src/pages/Profile/Profile.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class ProfileData extends BaseData {
  constructor() {
    super(apiClient.user);
    this.setEntityName('profile');
  }

  async fetchProfile() {
    try {
      const result = await this.api.getMe();
      
      // Response structure: { isSuccess: true, data: { userId, email, fullName, username, ... } }
      if (result?.data?.isSuccess === true && result?.data?.data) {
        return { success: true, data: result.data.data };
      }
      
      // Fallback: jika response langsung data (tanpa wrapper isSuccess)
      if (result?.data && !result?.data?.isSuccess) {
        return { success: true, data: result.data };
      }
      
      // Jika ada error dari backend
      if (result?.data?.isSuccess === false) {
        return { 
          success: false, 
          error: result.data.message || 'Failed to load profile' 
        };
      }
      
      return { success: false, error: 'Failed to load profile' };
    } catch (error) {
      // Cek apakah error adalah canceled
      if (error?.name === 'CanceledError' || 
          error?.message === 'canceled' || 
          error?.isCanceled === true ||
          error?.code === 'ERR_CANCELED') {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      
      // Jika 404, jangan tampilkan error
      if (error.response?.status === 404) {
        return { success: false, error: 'not found', isNotFound: true };
      }
      
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load profile',
      };
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
      const result = await this.api.changePassword({ oldPassword, newPassword, confirmPassword });
      if (result.data?.isSuccess !== undefined) {
        if (result.data.isSuccess) {
          ConfirmDialog.toast.success('Password changed');
          return { success: true };
        }
        const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to change password';
        ConfirmDialog.toast.error(errorMsg);
        return { success: false };
      }
      ConfirmDialog.toast.success('Password changed');
      return { success: true };
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to change password';
      ConfirmDialog.toast.error(errorMsg);
      return { success: false };
    }
  }
}

export default ProfileData;