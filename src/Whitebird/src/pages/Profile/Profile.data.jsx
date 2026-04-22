import ProfileApi from './Profile.api';
import Swal from 'sweetalert2';

class ProfileData {
  constructor() {
    this.api = ProfileApi;
  }

  async loadProfile() {
    try {
      const result = await this.api.getCurrentUser();
      
      if (result.isSuccess) {
        return {
          success: true,
          data: result.data
        };
      }
      
      return {
        success: false,
        error: result.message
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load profile'
      };
    }
  }

  async updateProfile(data) {
    try {
      const result = await this.api.updateProfile(data);
      
      if (result.isSuccess) {
        return {
          success: true,
          data: result.data
        };
      }
      
      return {
        success: false,
        error: result.message
      };
    } catch (error) {
      return {
        success: false,
        error: error.response?.data?.message || 'Failed to update profile'
      };
    }
  }

  async changePassword(oldPassword, newPassword, confirmPassword) {
    if (newPassword !== confirmPassword) {
      Swal.fire({
        title: 'Error',
        text: 'Passwords do not match',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
    
    if (newPassword.length < 4) {
      Swal.fire({
        title: 'Error',
        text: 'Password must be at least 4 characters',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
    
    try {
      const result = await this.api.changePassword({
        oldPassword,
        newPassword,
        confirmPassword
      });
      
      if (result.isSuccess) {
        return { success: true };
      }
      
      return { 
        success: false, 
        error: result.message || 'Failed to change password' 
      };
    } catch (error) {
      return { 
        success: false, 
        error: error.response?.data?.message || 'Failed to change password' 
      };
    }
  }
}

export default ProfileData;