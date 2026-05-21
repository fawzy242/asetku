import apiService from '../../core/services/api.service';

class ProfileApi {
  async getCurrentUser() { 
    return (await apiService.get('/Auth/me')).data; 
  }
  
  // NOTE: /Auth/profile endpoint does not exist in backend
  // Profile update is handled locally only
  
  async changePassword(data) { 
    return (await apiService.post('/Auth/change-password', data)).data; 
  }
}

export default new ProfileApi();