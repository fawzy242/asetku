// src/pages/Profile/Profile.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import { handleSuccess, handleApiError } from '../../core/services';

class ProfileData extends BaseData {
  constructor() {
    super(apiClient.user);
    this.setEntityName('profile');
  }

  async fetchProfile() {
    try {
      const result = await this.api.getMe();
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { success: false, error: result.data.message || 'Failed to load profile' };
        }
        return { success: true, data: result.data.data };
      }
      
      return { success: true, data: result.data };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load profile',
      };
    }
  }

  async updateProfile(formData) {
    try {
      const result = await this.api.updateProfile(formData);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to update profile';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false, errors: result.data.errors };
        }
        
        handleSuccess({
          operation: 'update-profile',
          entityName: 'profile',
          message: 'Profile updated successfully',
          result: result.data,
        });
        
        return { success: true, data: result.data.data };
      }
      
      ConfirmDialog.toast.success('Profile updated successfully');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to update profile',
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
      const result = await this.api.changePassword({
        oldPassword,
        newPassword,
        confirmPassword,
      });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to change password';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        handleSuccess({
          operation: 'change-password',
          entityName: 'profile',
          message: 'Password changed successfully',
          result: result.data,
        });
        
        return { success: true };
      }
      
      ConfirmDialog.toast.success('Password changed successfully');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to change password',
      };
    }
  }

  async uploadProfilePhoto(file) {
    try {
      const formData = new FormData();
      formData.append('file', file);
      
      const result = await this.api.uploadPhoto(formData);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to upload photo';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        handleSuccess({
          operation: 'upload',
          entityName: 'photo',
          message: 'Profile photo uploaded successfully',
          result: result.data,
        });
        
        return { success: true, data: result.data.data };
      }
      
      ConfirmDialog.toast.success('Profile photo uploaded successfully');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to upload photo',
      };
    }
  }

  async deleteProfilePhoto() {
    try {
      const result = await this.api.deletePhoto();
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to delete photo';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        handleSuccess({
          operation: 'delete',
          entityName: 'photo',
          message: 'Profile photo deleted successfully',
          result: result.data,
        });
        
        return { success: true };
      }
      
      ConfirmDialog.toast.success('Profile photo deleted successfully');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to delete photo',
      };
    }
  }

  async getProfilePhoto() {
    try {
      const result = await this.api.getPhoto();
      // Response is blob
      if (result.data && result.data.size > 0) {
        return { success: true, data: result.data };
      }
      return { success: false, error: 'No photo found' };
    } catch (error) {
      // 404 is expected when no photo exists
      if (error.response?.status === 404) {
        return { success: false, error: 'No photo found' };
      }
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load photo',
      };
    }
  }
}

export default ProfileData;