import apiService from '../../core/services/api.service';

class LoginApi {
  async login(email, password) {
    const response = await apiService.post('/auth/login', { email, password });
    return response.data;
  }

  async forgotPassword(email) {
    const response = await apiService.post('/auth/forgot-password', { email });
    return response.data;
  }

  async resetPassword(email, resetToken, newPassword, confirmPassword) {
    const response = await apiService.post('/auth/reset-password-with-token', {
      email,
      resetToken,
      newPassword,
      confirmPassword
    });
    return response.data;
  }
}

export default new LoginApi();