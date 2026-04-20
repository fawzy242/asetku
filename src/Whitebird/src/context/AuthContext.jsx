import React, { createContext, useContext, useState, useEffect } from 'react';
import apiService from '../core/services/api.service';
import utilsHelper from '../core/utils/utils.helper';
import Swal from 'sweetalert2';

const AuthContext = createContext(null);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    checkAuth();
  }, []);

  const checkAuth = async () => {
    const token = localStorage.getItem('whitebird_session_token');
    const savedUser = utilsHelper.getLocalStorage('user');
    
    if (token && savedUser) {
      try {
        const response = await apiService.get('/auth/validate-session');
        if (response.data?.isValid) {
          setUser(savedUser);
          setIsAuthenticated(true);
        } else {
          logout();
        }
      } catch (error) {
        logout();
      }
    }
    
    setLoading(false);
  };

  const login = async (email, password) => {
    try {
      const response = await apiService.post('/auth/login', { email, password });
      
      if (response.data?.isSuccess) {
        const { sessionToken, user: userData } = response.data.data;
        localStorage.setItem('whitebird_session_token', sessionToken);
        utilsHelper.setLocalStorage('user', userData);
        setUser(userData);
        setIsAuthenticated(true);
        
        Swal.fire({
          title: 'Login Successful',
          text: `Welcome back, ${userData.fullName}!`,
          icon: 'success',
          timer: 2000,
          showConfirmButton: false
        });
        
        return { success: true };
      }
      
      return { success: false, error: response.data?.message || 'Login failed' };
    } catch (error) {
      return { 
        success: false, 
        error: error.response?.data?.message || 'Invalid email or password'
      };
    }
  };

  const logout = async () => {
    const token = localStorage.getItem('whitebird_session_token');
    
    if (token) {
      try {
        await apiService.post('/auth/logout', { sessionToken: token });
      } catch (error) {
        console.error('Logout error:', error);
      }
    }
    
    localStorage.removeItem('whitebird_session_token');
    utilsHelper.removeLocalStorage('user');
    setUser(null);
    setIsAuthenticated(false);
  };

  const forgotPassword = async (email) => {
    try {
      const response = await apiService.post('/auth/forgot-password', { email });
      
      if (response.data?.isSuccess) {
        Swal.fire({
          title: 'Reset Email Sent',
          text: 'If your email is registered, you will receive a password reset link.',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true };
      }
      
      return { success: false, error: response.data?.message };
    } catch (error) {
      return { 
        success: false, 
        error: error.response?.data?.message || 'Failed to process request'
      };
    }
  };

  const resetPassword = async (email, resetToken, newPassword, confirmPassword) => {
    try {
      const response = await apiService.post('/auth/reset-password-with-token', {
        email,
        resetToken,
        newPassword,
        confirmPassword
      });
      
      if (response.data?.isSuccess) {
        Swal.fire({
          title: 'Password Reset',
          text: 'Your password has been reset successfully. Please login.',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true };
      }
      
      return { success: false, error: response.data?.message };
    } catch (error) {
      return { 
        success: false, 
        error: error.response?.data?.message || 'Failed to reset password'
      };
    }
  };

  const changePassword = async (oldPassword, newPassword, confirmPassword) => {
    try {
      const response = await apiService.post('/auth/change-password', {
        oldPassword,
        newPassword,
        confirmPassword
      });
      
      if (response.data?.isSuccess) {
        Swal.fire({
          title: 'Password Changed',
          text: 'Your password has been changed successfully.',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true };
      }
      
      return { success: false, error: response.data?.message };
    } catch (error) {
      return { 
        success: false, 
        error: error.response?.data?.message || 'Failed to change password'
      };
    }
  };

  const value = {
    user,
    loading,
    isAuthenticated,
    login,
    logout,
    forgotPassword,
    resetPassword,
    changePassword,
    checkAuth
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};