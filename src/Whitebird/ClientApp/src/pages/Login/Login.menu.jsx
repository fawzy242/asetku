import React, { useState, useCallback } from 'react';
import { Box } from '@mui/material';
import { useAuth } from '../../context/AuthContext';
import Input from '../../components/atoms/Input/Input';
import Button from '../../components/atoms/Button/Button';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import './Login.scss';

const LoginMenu = ({ onLoginSuccess }) => {
  const { login } = useAuth();
  const { toast } = useSweetAlert();
  const [formData, setFormData] = useState({ username: '', password: '' });
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  const validateForm = useCallback(() => {
    const err = {};
    if (!formData.username.trim()) {
      err.username = 'Username is required';
    } else if (formData.username.length < 3) {
      err.username = 'Username must be at least 3 characters';
    }
    if (!formData.password) {
      err.password = 'Password is required';
    } else if (formData.password.length < 4) {
      err.password = 'Password must be at least 4 characters';
    }
    setErrors(err);
    return Object.keys(err).length === 0;
  }, [formData]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;
    
    setLoading(true);
    const r = await login(formData.username, formData.password);
    setLoading(false);
    
    if (r.success) {
      toast.success(`Welcome back, ${formData.username}!`);
      onLoginSuccess?.();
    } else {
      toast.error(r.error || 'Login failed. Please check your username and password.');
    }
  };

  const handleChange = useCallback((field) => (e) => {
    setFormData(prev => ({ ...prev, [field]: e.target.value }));
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  }, [errors]);

  return (
    <Box
      component="form"
      onSubmit={handleSubmit}
      sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}
    >
      <Input
        label="Username"
        type="text"
        value={formData.username}
        onChange={handleChange('username')}
        error={errors.username}
        helperText={errors.username}
        autoComplete="username"
        autoFocus
      />
      <Input
        label="Password"
        type="password"
        value={formData.password}
        onChange={handleChange('password')}
        error={errors.password}
        helperText={errors.password}
        autoComplete="current-password"
      />
      <Button
        type="submit"
        variant="primary"
        size="md"
        fullWidth
        loading={loading}
      >
        Sign In
      </Button>
    </Box>
  );
};

export default LoginMenu;