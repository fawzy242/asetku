import { userApi } from '../api/user.api';
import BaseData from './BaseData';

class UserData extends BaseData {
  constructor() {
    super(userApi);
    this.setEntityName('user');
  }

  async getMe() {
    try {
      const result = await this.api.getMe();
      
      if (result?.data?.isSuccess === true && result?.data?.data) {
        return { success: true, data: result.data.data };
      }
      
      if (result?.data && !result?.data?.isSuccess) {
        return { success: true, data: result.data };
      }
      
      if (result?.data?.isSuccess === false) {
        return { 
          success: false, 
          error: result.data.message || 'Failed to load profile',
          errorType: result.data.errorType,
        };
      }
      
      return { success: false, error: 'Failed to load profile' };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      if (error.response?.status === 404) {
        return { success: false, error: 'not found', isNotFound: true };
      }
      return { 
        success: false, 
        error: error.userMessage || error.message || 'Failed to load profile' 
      };
    }
  }

  async updateProfile(data) {
    try {
      const result = await this.api.updateProfile(data);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            errors: result.data.errors || [result.data.message || 'Failed to update profile'],
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
        error: error.userMessage || error.message || 'Failed to update profile' 
      };
    }
  }

  async changePassword(oldPassword, newPassword, confirmPassword) {
    try {
      const result = await this.api.changePassword({ oldPassword, newPassword, confirmPassword });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            errors: result.data.errors || [result.data.message || 'Failed to change password'],
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
        error: error.userMessage || error.message || 'Failed to change password' 
      };
    }
  }

  async getPhoto() {
    try {
      const response = await this.api.getPhoto();
      if (response.data && response.data.size > 0) {
        return { success: true, data: response.data };
      }
      return { success: false, error: 'No photo found', isNotFound: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      if (error.response?.status === 404) {
        return { success: false, error: 'not found', isNotFound: true };
      }
      return { 
        success: false, 
        error: error.message || 'Failed to load photo' 
      };
    }
  }

  async uploadPhoto(file) {
    try {
      const formData = new FormData();
      formData.append('file', file);
      
      const result = await this.api.uploadPhoto(formData);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            errors: result.data.errors || [result.data.message || 'Failed to upload photo'],
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
        error: error.userMessage || error.message || 'Failed to upload photo' 
      };
    }
  }

  async deletePhoto() {
    try {
      const result = await this.api.deletePhoto();
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.message || 'Failed to delete photo',
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
        error: error.userMessage || error.message || 'Failed to delete photo' 
      };
    }
  }
}

export default UserData;