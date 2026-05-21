import apiService from '../../core/services/api.service';

class LoginApi {
  // UPDATED: Login now uses username instead of email
  async login(username, password) { 
    return (await apiService.post('/Auth/login', { username, password })).data; 
  }
  
  async forgotPassword(email) { 
    return (await apiService.post('/Auth/forgot-password', { email })).data; 
  }
  
  async resetPassword(email, resetToken, newPassword, confirmPassword) { 
    return (await apiService.post('/Auth/reset-password-with-token', { email, resetToken, newPassword, confirmPassword })).data; 
  }
}

export default new LoginApi();