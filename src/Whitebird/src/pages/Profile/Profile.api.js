import apiService from '../../core/services/api.service';

class ProfileApi {
  async getCurrentUser() { return (await apiService.get('/Auth/me')).data; }
  async updateProfile(data) { return (await apiService.put('/Users/profile', data)).data; }
  async changePassword(data) { return (await apiService.post('/Auth/change-password', data)).data; }
}

export default new ProfileApi();