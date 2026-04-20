import React from 'react';
import { useAuth } from '../../context/AuthContext';
import AuthLayout from '../../layouts/AuthLayout/AuthLayout';
import LoginMenu from './Login.menu';
import './Login.scss';

const Login = () => {
  const { login } = useAuth();

  const handleLoginSuccess = async (data) => {
    // Redirect to dashboard
    window.location.href = '/dashboard';
  };

  return (
    <AuthLayout title="Sign in to your account">
      <LoginMenu onLoginSuccess={handleLoginSuccess} />
    </AuthLayout>
  );
};

export default Login;