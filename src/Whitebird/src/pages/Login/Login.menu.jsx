import React, { useState } from 'react';
import { Box } from '@mui/material';
import { useAuth } from '../../context/AuthContext';
import Input from '../../components/atoms/Input/Input';
import Button from '../../components/atoms/Button/Button';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import utilsHelper from '../../core/utils/utils.helper';

const LoginMenu = ({ onLoginSuccess }) => {
  const { login } = useAuth();
  const [formData, setFormData] = useState({ email: '', password: '' });
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  const validateForm = () => {
    const err = {};
    if (!formData.email) err.email = 'Email is required';
    else if (!utilsHelper.validateEmail(formData.email)) err.email = 'Invalid email';
    if (!formData.password) err.password = 'Password is required';
    setErrors(err);
    return !Object.keys(err).length;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;
    setLoading(true);
    const r = await login(formData.email, formData.password);
    setLoading(false);
    if (r.success) onLoginSuccess?.();
    else await ConfirmDialog.showError('Login Failed', r.error);
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Input 
        label="Email Address" 
        type="email" 
        value={formData.email} 
        onChange={e => setFormData({...formData, email: e.target.value})} 
        error={errors.email} 
        size="small"
        autoComplete="email"
      />
      <Input 
        label="Password" 
        type="password" 
        value={formData.password} 
        onChange={e => setFormData({...formData, password: e.target.value})} 
        error={errors.password} 
        size="small"
        autoComplete="current-password"
      />
      <Button type="button" onClick={handleSubmit} variant="primary" size="md" fullWidth loading={loading}>
        Sign In
      </Button>
    </Box>
  );
};

export default LoginMenu;