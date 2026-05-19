import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import LoginMenu from './Login.menu';
import './Login.scss';

const Login = () => {
  const navigate = useNavigate();
  const auth = useAuth();

  const handleLoginSuccess = () => {
    navigate('/dashboard');
  };

  return <LoginMenu onLoginSuccess={handleLoginSuccess} />;
};

export default Login;