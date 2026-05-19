import React, { useState, useCallback } from 'react';
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

  const validateForm = useCallback(() => {
    const err = {};
    if (!formData.email.trim()) {
      err.email = 'Email is required';
    } else if (!utilsHelper.validateEmail(formData.email)) {
      err.email = 'Invalid email format';
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
    const r = await login(formData.email, formData.password);
    setLoading(false);
    if (r.success) {
      onLoginSuccess?.();
    } else {
      ConfirmDialog.toast.error(r.error || 'Login failed');
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
      sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}
    >
      <Input
        label="Email Address"
        type="email"
        value={formData.email}
        onChange={handleChange('email')}
        error={errors.email}
        helperText={errors.email}
        autoComplete="email"
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