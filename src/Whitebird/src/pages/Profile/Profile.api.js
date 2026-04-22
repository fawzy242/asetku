import apiService from '../../core/services/api.service';

class ProfileApi {
  async getCurrentUser() {
    const response = await apiService.get('/auth/me');
    return response.data;
  }

  async updateProfile(data) {
    const response = await apiService.put('/users/profile', data);
    return response.data;
  }

  async changePassword(data) {
    const response = await apiService.post('/auth/change-password', data);
    return response.data;
  }

  async uploadAvatar(file) {
    const formData = new FormData();
    formData.append('avatar', file);
    
    const response = await apiService.post('/users/avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    return response.data;
  }
}

export default new ProfileApi();