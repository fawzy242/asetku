import apiService from '../services/api.service';

export const userApi = {
  getMe: () => apiService.get('/User/me'),
  updateProfile: (data) => apiService.put('/User/profile', data),
  changePassword: (data) => apiService.post('/User/change-password', data),
  uploadPhoto: (data) =>
    apiService.post('/User/profile-photo', data, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),
  getPhoto: () => apiService.get('/User/profile-photo', { responseType: 'blob' }),
  deletePhoto: () => apiService.delete('/User/profile-photo'),
};