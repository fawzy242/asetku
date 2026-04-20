import LoginApi from './Login.api';
import utilsHelper from '../../core/utils/utils.helper';

class LoginData {
  constructor() {
    this.api = LoginApi;
  }

  async handleLogin(email, password) {
    if (!utilsHelper.validateEmail(email)) {
      return {
        success: false,
        error: 'Please enter a valid email address'
      };
    }

    if (!password || password.length < 4) {
      return {
        success: false,
        error: 'Password must be at least 4 characters'
      };
    }

    try {
      const result = await this.api.login(email, password);
      
      if (result.isSuccess) {
        return {
          success: true,
          data: result.data
        };
      }
      
      return {
        success: false,
        error: result.message || 'Login failed'
      };
    } catch (error) {
      return {
        success: false,
        error: 'Invalid email or password'
      };
    }
  }

  async handleForgotPassword(email) {
    if (!utilsHelper.validateEmail(email)) {
      return {
        success: false,
        error: 'Please enter a valid email address'
      };
    }

    try {
      const result = await this.api.forgotPassword(email);
      
      return {
        success: result.isSuccess,
        message: result.message
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to process request'
      };
    }
  }

  async handleResetPassword(email, resetToken, newPassword, confirmPassword) {
    if (!email || !resetToken) {
      return {
        success: false,
        error: 'Email and reset token are required'
      };
    }

    if (newPassword !== confirmPassword) {
      return {
        success: false,
        error: 'Passwords do not match'
      };
    }

    if (newPassword.length < 4) {
      return {
        success: false,
        error: 'Password must be at least 4 characters'
      };
    }

    try {
      const result = await this.api.resetPassword(email, resetToken, newPassword, confirmPassword);
      
      return {
        success: result.isSuccess,
        message: result.message
      };
    } catch (error) {
      return {
        success: false,
        error: error.response?.data?.message || 'Failed to reset password'
      };
    }
  }
}

export default LoginData;