import apiService from '../../core/services/api.service';

class LoginApi {
  async login(email, password) { return (await apiService.post('/Auth/login', { email, password })).data; }
  async forgotPassword(email) { return (await apiService.post('/Auth/forgot-password', { email })).data; }
  async resetPassword(email, resetToken, newPassword, confirmPassword) { return (await apiService.post('/Auth/reset-password-with-token', { email, resetToken, newPassword, confirmPassword })).data; }
}

export default new LoginApi();