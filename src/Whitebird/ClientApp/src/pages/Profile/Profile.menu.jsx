// src/pages/Profile/Profile.menu.jsx
import React, { useState, useEffect, useCallback, useRef } from 'react';
import { FiUser, FiMail, FiShield, FiLock, FiSave, FiX, FiBriefcase, FiCalendar, FiEdit2, FiCheck, FiCamera, FiTrash2 } from 'react-icons/fi';
import { Grid, Box, Typography, Avatar, IconButton, CircularProgress } from '@mui/material';
import { useAuth } from '../../context/AuthContext';
import ProfileData from './Profile.data';
import apiClient from '../../core/api/apiClient';
import Card from '../../components/atoms/Card/Card';
import Button from '../../components/atoms/Button/Button';
import Input from '../../components/atoms/Input/Input';
import Spinner from '../../components/atoms/Spinner/Spinner';
import FormSection from '../../components/atoms/FormSection/FormSection';
import InfoRow from '../../components/molecules/InfoRow/InfoRow';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { alertUtils } from '../../lib/sweetAlert';
import utilsHelper from '../../core/utils/utils.helper';
import './Profile.scss';

const profileData = new ProfileData();

const ProfileMenu = () => {
  const [loading, setLoading] = useState(true);
  const [profile, setProfile] = useState(null);
  const [showPasswordForm, setShowPasswordForm] = useState(false);
  const [editMode, setEditMode] = useState(false);
  const [saving, setSaving] = useState(false);
  const [uploadingPhoto, setUploadingPhoto] = useState(false);
  const [profilePhotoUrl, setProfilePhotoUrl] = useState(null);
  const [passwordData, setPasswordData] = useState({ oldPassword: '', newPassword: '', confirmPassword: '' });
  const [formData, setFormData] = useState({ fullName: '', email: '', phoneNumber: '', username: '' });
  const [errors, setErrors] = useState({});
  const { refreshUser: refreshAuth, user } = useAuth();
  const { toast, confirm } = useSweetAlert();
  
  // FIX: Ref untuk tracking request ID agar request yang dibatalkan tidak diproses
  const loadProfileIdRef = useRef(0);

  useEffect(() => { loadProfile(); }, []);

  const loadProfile = async () => {
    // Increment ID untuk tracking request
    const currentRequestId = ++loadProfileIdRef.current;
    
    setLoading(true);
    const r = await profileData.fetchProfile();
    
    // FIX: Jika ada request baru yang lebih tinggi, abaikan response ini
    if (currentRequestId !== loadProfileIdRef.current) {
      return;
    }
    
    // FIX: Jika request dibatalkan, jangan tampilkan error
    if (r.isCanceled) {
      setLoading(false);
      return;
    }
    
    if (r.success && r.data) {
      const profileData = r.data;
      setProfile(profileData);
      setFormData({
        fullName: profileData.fullName || '',
        email: profileData.email || '',
        phoneNumber: profileData.phoneNumber || '',
        username: profileData.username || '',
      });
      loadProfilePhoto();
    } else {
      // FIX: Hanya tampilkan error jika bukan canceled
      if (r.error && r.error !== 'canceled' && r.error !== 'not found' && !r.isNotFound) {
        await alertUtils.error('Error', r.error || 'Failed to load profile');
      }
    }
    setLoading(false);
  };

  const loadProfilePhoto = async () => {
    try {
      const response = await apiClient.user.getPhoto();
      if (response.data && response.data.size > 0) {
        const url = URL.createObjectURL(response.data);
        setProfilePhotoUrl(url);
      } else {
        setProfilePhotoUrl(null);
      }
    } catch {
      setProfilePhotoUrl(null);
    }
  };

  const handlePhotoUpload = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Reset input agar bisa upload file yang sama lagi
    event.target.value = '';

    if (!file.type.startsWith('image/')) {
      await alertUtils.error('Error', 'Please select an image file');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      await alertUtils.error('Error', 'Image size must be less than 5MB');
      return;
    }

    // Konfirmasi sebelum upload
    const confirmed = await confirm({
      title: 'Update Profile Photo',
      text: `Are you sure you want to update your profile photo with "${file.name}"?`,
      icon: 'question',
      confirmButtonText: 'Yes, Upload',
      confirmButtonColor: '#dc2626',
    });

    if (!confirmed) return;

    setUploadingPhoto(true);
    const formDataUpload = new FormData();
    formDataUpload.append('file', file);

    try {
      const response = await apiClient.user.uploadPhoto(formDataUpload);
      if (response.data?.isSuccess) {
        await alertUtils.success('Success!', 'Profile photo updated successfully');
        
        await loadProfilePhoto();
        await refreshAuth();
        
        setTimeout(() => {
          window.dispatchEvent(new CustomEvent('profile-photo-updated'));
        }, 200);
      } else {
        const errorMsg = response.data?.errors?.join('\n') || response.data?.message || 'Failed to upload photo';
        await alertUtils.error('Error', errorMsg);
      }
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to upload photo';
      await alertUtils.error('Error', errorMsg);
    } finally {
      setUploadingPhoto(false);
    }
  };

  const handleDeletePhoto = async () => {
    const confirmed = await confirm({
      title: 'Delete Profile Photo',
      text: 'Are you sure you want to delete your profile photo?',
      icon: 'warning',
      confirmButtonText: 'Yes, Delete',
      confirmButtonColor: '#ef4444',
    });
    
    if (!confirmed) return;

    try {
      const response = await apiClient.user.deletePhoto();
      if (response.data?.isSuccess) {
        await alertUtils.success('Success!', 'Profile photo deleted successfully');
        setProfilePhotoUrl(null);
        await refreshAuth();
        
        setTimeout(() => {
          window.dispatchEvent(new CustomEvent('profile-photo-updated'));
        }, 200);
      } else {
        const errorMsg = response.data?.errors?.join('\n') || response.data?.message || 'Failed to delete photo';
        await alertUtils.error('Error', errorMsg);
      }
    } catch (error) {
      const errorMsg = error.userMessage || error.message || 'Failed to delete photo';
      await alertUtils.error('Error', errorMsg);
    }
  };

  const validatePassword = () => {
    const err = {};
    if (!passwordData.oldPassword) err.oldPassword = 'Current password is required';
    if (!passwordData.newPassword) err.newPassword = 'New password is required';
    else if (passwordData.newPassword.length < 4) err.newPassword = 'Password must be at least 4 characters';
    if (passwordData.newPassword !== passwordData.confirmPassword) err.confirmPassword = 'Passwords do not match';
    setErrors(err);
    return Object.keys(err).length === 0;
  };

  const validateProfile = () => {
    const err = {};
    if (!formData.fullName?.trim()) err.fullName = 'Full name is required';
    if (!formData.email?.trim()) err.email = 'Email is required';
    else if (!utilsHelper.validateEmail(formData.email)) err.email = 'Please enter a valid email address';
    setErrors(err);
    return Object.keys(err).length === 0;
  };

  const handlePasswordChange = async (e) => {
    e.preventDefault();
    if (!validatePassword()) return;

    const confirmed = await confirm({
      title: 'Change Password',
      text: 'Are you sure you want to change your password?',
      confirmButtonText: 'Yes, Change',
    });
    if (!confirmed) return;

    setSaving(true);
    const r = await profileData.changePassword(passwordData.oldPassword, passwordData.newPassword, passwordData.confirmPassword);
    setSaving(false);

    if (r.success) {
      await alertUtils.success('Success!', 'Password changed successfully');
      setShowPasswordForm(false);
      setPasswordData({ oldPassword: '', newPassword: '', confirmPassword: '' });
      setErrors({});
    }
  };

  const handleProfileUpdate = async (e) => {
    e.preventDefault();
    if (!validateProfile()) return;

    setSaving(true);
    try {
      const response = await apiClient.user.updateProfile({
        fullName: formData.fullName,
        email: formData.email,
        phoneNumber: formData.phoneNumber || null,
      });
      if (response.data?.isSuccess) {
        setSaving(false);
        setEditMode(false);
        setProfile(prev => ({ ...prev, ...formData }));
        const storedUser = utilsHelper.getLocalStorage('user');
        if (storedUser) {
          const updatedUser = { ...storedUser, ...formData };
          utilsHelper.setLocalStorage('user', updatedUser);
        }
        await alertUtils.success('Success!', 'Profile updated successfully');
        await refreshAuth();
      } else {
        setSaving(false);
        const errorMsg = response.data?.errors?.join('\n') || response.data?.message || 'Failed to update profile';
        await alertUtils.error('Error', errorMsg);
      }
    } catch (error) {
      setSaving(false);
      const errorMsg = error.userMessage || error.message || 'Failed to update profile';
      await alertUtils.error('Error', errorMsg);
    }
  };

  const handleCancelEdit = () => {
    setEditMode(false);
    setFormData({
      fullName: profile?.fullName || '',
      email: profile?.email || '',
      phoneNumber: profile?.phoneNumber || '',
      username: profile?.username || '',
    });
    setErrors({});
  };

  const handleCancelPassword = () => {
    setShowPasswordForm(false);
    setPasswordData({ oldPassword: '', newPassword: '', confirmPassword: '' });
    setErrors({});
  };

  if (loading) return <div className="profile-loading"><Spinner size="lg" /></div>;

  const userInitial = profile?.fullName?.charAt(0) || profile?.username?.charAt(0) || 'U';

  return (
    <div className="profile-menu fade-transition">
      <div className="page-header">
        <h1 className="page-title">My Profile</h1>
      </div>
      <Box sx={{ mb: 3 }}>
        <Typography variant="body2" color="text.secondary">Manage your account settings and preferences</Typography>
      </Box>

      <Grid container spacing={4}>
        <Grid item xs={12} md={7}>
          <Card>
            <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 3, mb: 4, pb: 3, borderBottom: '1px solid var(--border)' }}>
              <Box sx={{ position: 'relative' }}>
                <Avatar src={profilePhotoUrl || undefined} sx={{ width: 100, height: 100, bgcolor: 'var(--primary)', fontSize: 40, fontWeight: 600 }}>
                  {!profilePhotoUrl && userInitial}
                </Avatar>
                {uploadingPhoto && (
                  <Box sx={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', bgcolor: 'rgba(0,0,0,0.5)', borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <CircularProgress size={30} sx={{ color: 'white' }} />
                  </Box>
                )}
                <Box sx={{ position: 'absolute', bottom: -8, right: -8, display: 'flex', gap: 1 }}>
                  <IconButton size="small" component="label" sx={{ bgcolor: 'var(--primary)', color: 'white', '&:hover': { bgcolor: 'var(--primary-hover)' } }}>
                    <FiCamera size={16} />
                    <input type="file" accept="image/jpeg,image/png,image/gif,image/webp" hidden onChange={handlePhotoUpload} />
                  </IconButton>
                  {profilePhotoUrl && (
                    <IconButton size="small" onClick={handleDeletePhoto} sx={{ bgcolor: 'var(--error)', color: 'white', '&:hover': { opacity: 0.9 } }}>
                      <FiTrash2 size={14} />
                    </IconButton>
                  )}
                </Box>
              </Box>
              <Box sx={{ flex: 1 }}>
                <Typography variant="h6" fontWeight={600}>{profile?.fullName}</Typography>
                <Typography variant="body2" color="text.secondary">{profile?.roleId || 'User'}</Typography>
                <Typography variant="caption" color="text.secondary" sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                  <FiCalendar size={12} /> Member since {utilsHelper.formatDate(profile?.createdDate, 'MMMM D, YYYY')}
                </Typography>
              </Box>
              {!editMode && (
                <Button variant="outline" size="sm" onClick={() => setEditMode(true)} startIcon={<FiEdit2 />}>Edit</Button>
              )}
            </Box>

            {!editMode ? (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                <InfoRow icon={FiUser} label="Full Name" value={profile?.fullName} />
                <InfoRow icon={FiMail} label="Email" value={profile?.email} />
                <InfoRow icon={FiBriefcase} label="Username" value={profile?.username || '-'} />
                <InfoRow icon={FiShield} label="Role" value={profile?.roleId || 'User'} />
                {profile?.phoneNumber && <InfoRow icon={FiBriefcase} label="Phone" value={profile?.phoneNumber} />}
              </Box>
            ) : (
              <form onSubmit={handleProfileUpdate}>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Input
                      label="Full Name"
                      value={formData.fullName}
                      onChange={e => setFormData({ ...formData, fullName: e.target.value })}
                      error={errors.fullName}
                      required
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <Input
                      label="Email"
                      type="email"
                      value={formData.email}
                      onChange={e => setFormData({ ...formData, email: e.target.value })}
                      error={errors.email}
                      required
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <Input
                      label="Phone (Optional)"
                      value={formData.phoneNumber}
                      onChange={e => setFormData({ ...formData, phoneNumber: e.target.value })}
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <Input label="Username" value={formData.username} disabled />
                  </Grid>
                  <Grid item xs={12}>
                    <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 2 }}>
                      <Button type="button" variant="outline" onClick={handleCancelEdit} startIcon={<FiX />}>Cancel</Button>
                      <Button type="submit" variant="primary" loading={saving} startIcon={<FiCheck />}>Save Changes</Button>
                    </Box>
                  </Grid>
                </Grid>
              </form>
            )}
          </Card>
        </Grid>

        <Grid item xs={12} md={5}>
          <Card>
            <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
              <Box sx={{ width: 48, height: 48, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'rgba(220,38,38,0.1)', borderRadius: 2, color: 'var(--primary)' }}>
                <FiLock size={24} />
              </Box>
              <Box>
                <Typography variant="h6" fontWeight={600}>Security</Typography>
                <Typography variant="body2" color="text.secondary">Update your password</Typography>
              </Box>
            </Box>

            {!showPasswordForm ? (
              <Button variant="outline" onClick={() => setShowPasswordForm(true)} startIcon={<FiLock />} fullWidth>Change Password</Button>
            ) : (
              <form onSubmit={handlePasswordChange}>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Input
                      label="Current Password"
                      type="password"
                      value={passwordData.oldPassword}
                      onChange={e => setPasswordData({ ...passwordData, oldPassword: e.target.value })}
                      error={errors.oldPassword}
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <Input
                      label="New Password"
                      type="password"
                      value={passwordData.newPassword}
                      onChange={e => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                      error={errors.newPassword}
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <Input
                      label="Confirm New Password"
                      type="password"
                      value={passwordData.confirmPassword}
                      onChange={e => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
                      error={errors.confirmPassword}
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 2 }}>
                      <Button type="button" variant="outline" onClick={handleCancelPassword} startIcon={<FiX />}>Cancel</Button>
                      <Button type="submit" variant="primary" loading={saving} startIcon={<FiSave />}>Update Password</Button>
                    </Box>
                  </Grid>
                </Grid>
              </form>
            )}
          </Card>
        </Grid>
      </Grid>
    </div>
  );
};

export default ProfileMenu;