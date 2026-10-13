import apiService from '../services/api.service';

export const authApi = {
  login: (data) => apiService.post('/Auth/login', data),
  logout: (data) => apiService.post('/Auth/logout', data),
  validateSession: () => apiService.get('/Auth/validate-session'),
};