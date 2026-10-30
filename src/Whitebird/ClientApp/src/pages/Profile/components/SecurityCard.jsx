// src/pages/Profile/components/SecurityCard.jsx
import React from 'react';
import { Box, Typography, Grid } from '@mui/material';
import { FiLock, FiSave, FiX } from 'react-icons/fi';
import Card from '../../../components/atoms/Card/Card';
import Button from '../../../components/atoms/Button/Button';
import Input from '../../../components/atoms/Input/Input';

const SecurityCard = ({
  showPasswordForm,
  onShowPasswordForm,
  onCancelPassword,
  passwordData,
  setPasswordData,
  errors,
  saving,
  onPasswordSubmit,
}) => {
  return (
    <Card>
      <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
        <Box
          sx={{
            width: 48,
            height: 48,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            bgcolor: 'rgba(220,38,38,0.1)',
            borderRadius: 2,
            color: 'var(--primary)',
          }}
        >
          <FiLock size={24} />
        </Box>
        <Box>
          <Typography variant="h6" fontWeight={600}>
            Security
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Update your password
          </Typography>
        </Box>
      </Box>

      {!showPasswordForm ? (
        <Button
          variant="outline"
          onClick={onShowPasswordForm}
          startIcon={<FiLock />}
          fullWidth
        >
          Change Password
        </Button>
      ) : (
        <form onSubmit={onPasswordSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input
                label="Current Password"
                type="password"
                value={passwordData.oldPassword}
                onChange={(e) =>
                  setPasswordData({
                    ...passwordData,
                    oldPassword: e.target.value,
                  })
                }
                error={errors.oldPassword}
              />
            </Grid>
            <Grid item xs={12}>
              <Input
                label="New Password"
                type="password"
                value={passwordData.newPassword}
                onChange={(e) =>
                  setPasswordData({
                    ...passwordData,
                    newPassword: e.target.value,
                  })
                }
                error={errors.newPassword}
              />
            </Grid>
            <Grid item xs={12}>
              <Input
                label="Confirm New Password"
                type="password"
                value={passwordData.confirmPassword}
                onChange={(e) =>
                  setPasswordData({
                    ...passwordData,
                    confirmPassword: e.target.value,
                  })
                }
                error={errors.confirmPassword}
              />
            </Grid>
            <Grid item xs={12}>
              <Box
                sx={{
                  display: 'flex',
                  gap: 2,
                  justifyContent: 'flex-end',
                  mt: 2,
                }}
              >
                <Button
                  type="button"
                  variant="outline"
                  onClick={onCancelPassword}
                  startIcon={<FiX />}
                >
                  Cancel
                </Button>
                <Button
                  type="submit"
                  variant="primary"
                  loading={saving}
                  startIcon={<FiSave />}
                >
                  Update Password
                </Button>
              </Box>
            </Grid>
          </Grid>
        </form>
      )}
    </Card>
  );
};

export default SecurityCard;