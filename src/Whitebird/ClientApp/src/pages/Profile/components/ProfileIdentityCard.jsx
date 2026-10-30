// src/pages/Profile/components/ProfileIdentityCard.jsx
import React from 'react';
import {
  Grid,
  Box,
  Typography,
  Avatar,
  IconButton,
  CircularProgress,
} from '@mui/material';
import {
  FiUser,
  FiMail,
  FiBriefcase,
  FiShield,
  FiCalendar,
  FiEdit2,
  FiCheck,
  FiX,
  FiCamera,
  FiTrash2,
} from 'react-icons/fi';
import Card from '../../../components/atoms/Card/Card';
import Button from '../../../components/atoms/Button/Button';
import Input from '../../../components/atoms/Input/Input';
import InfoRow from '../../../components/molecules/InfoRow/InfoRow';
import { formatDate } from '../../../core/utils/date.utils';

const ProfileIdentityCard = ({
  profile,
  editMode,
  onStartEdit,
  formData,
  setFormData,
  errors,
  profilePhotoUrl,
  uploadingPhoto,
  userInitial,
  onPhotoUpload,
  onDeletePhoto,
  onProfileUpdate,
  onCancelEdit,
}) => {
  return (
    <Card>
      <Box
        sx={{
          display: 'flex',
          alignItems: 'flex-start',
          gap: 3,
          mb: 4,
          pb: 3,
          borderBottom: '1px solid var(--border)',
        }}
      >
        <Box sx={{ position: 'relative' }}>
          <Avatar
            src={profilePhotoUrl || undefined}
            sx={{
              width: 100,
              height: 100,
              bgcolor: 'var(--primary)',
              fontSize: 40,
              fontWeight: 600,
            }}
          >
            {!profilePhotoUrl && userInitial}
          </Avatar>
          {uploadingPhoto && (
            <Box
              sx={{
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                height: '100%',
                bgcolor: 'rgba(0,0,0,0.5)',
                borderRadius: '50%',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <CircularProgress size={30} sx={{ color: 'white' }} />
            </Box>
          )}
          <Box
            sx={{
              position: 'absolute',
              bottom: -8,
              right: -8,
              display: 'flex',
              gap: 1,
            }}
          >
            <IconButton
              size="small"
              component="label"
              sx={{
                bgcolor: 'var(--primary)',
                color: 'white',
                '&:hover': { bgcolor: 'var(--primary-hover)' },
              }}
            >
              <FiCamera size={16} />
              <input
                type="file"
                accept="image/jpeg,image/png,image/gif,image/webp"
                hidden
                onChange={onPhotoUpload}
              />
            </IconButton>
            {profilePhotoUrl && (
              <IconButton
                size="small"
                onClick={onDeletePhoto}
                sx={{
                  bgcolor: 'var(--error)',
                  color: 'white',
                  '&:hover': { opacity: 0.9 },
                }}
              >
                <FiTrash2 size={14} />
              </IconButton>
            )}
          </Box>
        </Box>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h6" fontWeight={600}>
            {profile?.fullName}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {profile?.roleId || 'User'}
          </Typography>
          <Typography
            variant="caption"
            color="text.secondary"
            sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}
          >
            <FiCalendar size={12} /> Member since{' '}
            {formatDate(profile?.createdDate, 'MMMM D, YYYY')}
          </Typography>
        </Box>
        {!editMode && (
          <Button
            variant="outline"
            size="sm"
            onClick={onStartEdit}
            startIcon={<FiEdit2 />}
          >
            Edit
          </Button>
        )}
      </Box>

      {!editMode ? (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
          <InfoRow icon={FiUser} label="Full Name" value={profile?.fullName} />
          <InfoRow icon={FiMail} label="Email" value={profile?.email} />
          <InfoRow
            icon={FiBriefcase}
            label="Username"
            value={profile?.username || '-'}
          />
          <InfoRow
            icon={FiShield}
            label="Role"
            value={profile?.roleId || 'User'}
          />
          {profile?.phoneNumber && (
            <InfoRow
              icon={FiBriefcase}
              label="Phone"
              value={profile?.phoneNumber}
            />
          )}
        </Box>
      ) : (
        <form onSubmit={onProfileUpdate}>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input
                label="Full Name"
                value={formData.fullName}
                onChange={(e) =>
                  setFormData({ ...formData, fullName: e.target.value })
                }
                error={errors.fullName}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <Input
                label="Email"
                type="email"
                value={formData.email}
                onChange={(e) =>
                  setFormData({ ...formData, email: e.target.value })
                }
                error={errors.email}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <Input
                label="Phone (Optional)"
                value={formData.phoneNumber}
                onChange={(e) =>
                  setFormData({ ...formData, phoneNumber: e.target.value })
                }
              />
            </Grid>
            <Grid item xs={12}>
              <Input label="Username" value={formData.username} disabled />
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
                  onClick={onCancelEdit}
                  startIcon={<FiX />}
                >
                  Cancel
                </Button>
                <Button
                  type="submit"
                  variant="primary"
                  startIcon={<FiCheck />}
                >
                  Save Changes
                </Button>
              </Box>
            </Grid>
          </Grid>
        </form>
      )}
    </Card>
  );
};

export default ProfileIdentityCard;