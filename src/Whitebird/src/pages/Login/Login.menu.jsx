import React, { useState } from 'react';
import Swal from 'sweetalert2';
import LoginData from './Login.data';
import Input from '../../components/atoms/Input/Input';
import Button from '../../components/atoms/Button/Button';
import Modal from '../../components/molecules/Modal/Modal';

const LoginMenu = ({ onLoginSuccess }) => {
  const [formData, setFormData] = useState({ email: '', password: '' });
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});
  const [showForgotModal, setShowForgotModal] = useState(false);
  const [showResetModal, setShowResetModal] = useState(false);
  const [forgotEmail, setForgotEmail] = useState('');
  const [resetData, setResetData] = useState({ email: '', token: '', newPassword: '', confirmPassword: '' });
  
  const loginData = new LoginData();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    setErrors(prev => ({ ...prev, [name]: '' }));
  };

  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.email) {
      newErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Invalid email format';
    }
    
    if (!formData.password) {
      newErrors.password = 'Password is required';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) return;
    
    setLoading(true);
    
    const result = await loginData.handleLogin(formData.email, formData.password);
    
    if (result.success) {
      onLoginSuccess?.(result.data);
    } else {
      Swal.fire({
        title: 'Login Failed',
        text: result.error,
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
    }
    
    setLoading(false);
  };

  const handleForgotPassword = async () => {
    if (!forgotEmail) {
      Swal.fire({
        title: 'Error',
        text: 'Please enter your email address',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return;
    }
    
    setLoading(true);
    const result = await loginData.handleForgotPassword(forgotEmail);
    setLoading(false);
    
    if (result.success) {
      setShowForgotModal(false);
      Swal.fire({
        title: 'Reset Email Sent',
        text: result.message,
        icon: 'success',
        confirmButtonColor: '#dc2626'
      });
    } else {
      Swal.fire({
        title: 'Error',
        text: result.error,
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
    }
  };

  const handleResetPassword = async () => {
    const result = await loginData.handleResetPassword(
      resetData.email,
      resetData.token,
      resetData.newPassword,
      resetData.confirmPassword
    );
    
    if (result.success) {
      setShowResetModal(false);
      setResetData({ email: '', token: '', newPassword: '', confirmPassword: '' });
      Swal.fire({
        title: 'Password Reset',
        text: result.message,
        icon: 'success',
        confirmButtonColor: '#dc2626'
      });
    } else {
      Swal.fire({
        title: 'Error',
        text: result.error,
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
    }
  };

  return (
    <>
      <form className="login-form" onSubmit={handleSubmit}>
        <Input
          label="Email"
          type="email"
          name="email"
          placeholder="Enter your email"
          value={formData.email}
          onChange={handleChange}
          error={errors.email}
          required
        />
        
        <Input
          label="Password"
          type="password"
          name="password"
          placeholder="Enter your password"
          value={formData.password}
          onChange={handleChange}
          error={errors.password}
          required
        />
        
        <div className="login-form__forgot">
          <button
            type="button"
            className="login-form__link"
            onClick={() => setShowForgotModal(true)}
          >
            Forgot password?
          </button>
        </div>
        
        <Button
          type="submit"
          variant="primary"
          size="lg"
          fullWidth
          loading={loading}
        >
          Sign In
        </Button>
        
        <div className="login-form__reset">
          <button
            type="button"
            className="login-form__link"
            onClick={() => setShowResetModal(true)}
          >
            Have a reset token?
          </button>
        </div>
      </form>

      {/* Forgot Password Modal */}
      <Modal
        isOpen={showForgotModal}
        onClose={() => setShowForgotModal(false)}
        title="Forgot Password"
        size="sm"
      >
        <div className="forgot-password-form">
          <p className="forgot-password-form__text">
            Enter your email address and we'll send you a password reset code.
          </p>
          
          <Input
            label="Email"
            type="email"
            placeholder="Enter your email"
            value={forgotEmail}
            onChange={(e) => setForgotEmail(e.target.value)}
            required
          />
          
          <div className="modal-actions">
            <Button
              variant="outline"
              onClick={() => setShowForgotModal(false)}
            >
              Cancel
            </Button>
            <Button
              variant="primary"
              onClick={handleForgotPassword}
              loading={loading}
            >
              Send Reset Code
            </Button>
          </div>
        </div>
      </Modal>

      {/* Reset Password Modal */}
      <Modal
        isOpen={showResetModal}
        onClose={() => setShowResetModal(false)}
        title="Reset Password"
        size="sm"
      >
        <div className="reset-password-form">
          <Input
            label="Email"
            type="email"
            placeholder="Enter your email"
            value={resetData.email}
            onChange={(e) => setResetData(prev => ({ ...prev, email: e.target.value }))}
            required
          />
          
          <Input
            label="Reset Token"
            type="text"
            placeholder="Enter 6-digit code"
            value={resetData.token}
            onChange={(e) => setResetData(prev => ({ ...prev, token: e.target.value }))}
            required
          />
          
          <Input
            label="New Password"
            type="password"
            placeholder="Enter new password"
            value={resetData.newPassword}
            onChange={(e) => setResetData(prev => ({ ...prev, newPassword: e.target.value }))}
            required
          />
          
          <Input
            label="Confirm Password"
            type="password"
            placeholder="Confirm new password"
            value={resetData.confirmPassword}
            onChange={(e) => setResetData(prev => ({ ...prev, confirmPassword: e.target.value }))}
            required
          />
          
          <div className="modal-actions">
            <Button
              variant="outline"
              onClick={() => setShowResetModal(false)}
            >
              Cancel
            </Button>
            <Button
              variant="primary"
              onClick={handleResetPassword}
            >
              Reset Password
            </Button>
          </div>
        </div>
      </Modal>
    </>
  );
};

export default LoginMenu;