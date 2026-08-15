import apiService from '../services/api.service';

export const authApi = {
  login: (data) => apiService.post('/Auth/login', data),
  logout: (data) => apiService.post('/Auth/logout', data),
  forgotPassword: (data) => apiService.post('/Auth/forgot-password', data),
  resetPassword: (data) => apiService.post('/Auth/reset-password', data),
  resetPasswordWithToken: (data) => apiService.post('/Auth/reset-password-with-token', data),
  validateSession: () => apiService.get('/Auth/validate-session'),
};