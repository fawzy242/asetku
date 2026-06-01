import React, { useState, useEffect, useCallback } from "react";
import { FiUser, FiMail, FiShield, FiLock, FiSave, FiX, FiBriefcase, FiCalendar, FiEdit2, FiCheck, FiCamera, FiTrash2, FiUpload } from "react-icons/fi";
import { Grid, Box, Avatar, Typography, IconButton, CircularProgress } from "@mui/material";
import { useAuth } from "../../context/AuthContext";
import ProfileData from "./Profile.data";
import Card from "../../components/atoms/Card/Card";
import Button from "../../components/atoms/Button/Button";
import Input from "../../components/atoms/Input/Input";
import Spinner from "../../components/atoms/Spinner/Spinner";
import ConfirmDialog from "../../components/molecules/ConfirmDialog/ConfirmDialog";
import apiService from "../../core/services/api.service";
import utilsHelper from "../../core/utils/utils.helper";
import "./Profile.scss";

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
  const { refreshUser } = useAuth();

  useEffect(() => { 
    loadProfile(); 
  }, []);

  const loadProfile = async () => {
    setLoading(true);
    const r = await profileData.fetchProfile();
    if (r.success) {
      setProfile(r.data);
      setFormData({
        fullName: r.data.fullName || '',
        email: r.data.email || '',
        phoneNumber: r.data.phoneNumber || '',
        username: r.data.username || '',
      });
      loadProfilePhoto(r.data.userId);
    }
    setLoading(false);
  };

  const loadProfilePhoto = async (userId) => {
    if (!userId) return;
    try {
      const response = await apiService.get(`/Auth/profile-photo/${userId}`, { responseType: 'blob' });
      if (response.data && response.data.size > 0) {
        const url = URL.createObjectURL(response.data);
        setProfilePhotoUrl(url);
      }
    } catch (error) {
      setProfilePhotoUrl(null);
    }
  };

  // FIXED: Refresh user after photo upload
  const handlePhotoUpload = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      ConfirmDialog.toast.error('Please select an image file (JPEG, PNG, GIF, WEBP)');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      ConfirmDialog.toast.error('Image size must be less than 5MB');
      return;
    }

    setUploadingPhoto(true);
    const formData = new FormData();
    formData.append('file', file);

    try {
      const response = await apiService.post('/Auth/profile-photo', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      if (response.data?.isSuccess) {
        ConfirmDialog.toast.success('Profile photo updated');
        // Refresh user data to update topbar
        await refreshUser();
        // Reload photo
        loadProfilePhoto(profile?.userId);
      } else {
        ConfirmDialog.toast.error(response.data?.message || 'Failed to upload photo');
      }
    } catch (error) {
      ConfirmDialog.toast.error('Failed to upload photo');
    } finally {
      setUploadingPhoto(false);
    }
  };

  // FIXED: Refresh user after photo delete
  const handleDeletePhoto = async () => {
    const confirmed = await ConfirmDialog.show({
      title: 'Delete Profile Photo',
      text: 'Are you sure you want to delete your profile photo?',
      icon: 'warning',
      confirmButtonText: 'Delete',
    });
    if (!confirmed) return;

    try {
      const response = await apiService.delete('/Auth/profile-photo');
      if (response.data?.isSuccess) {
        ConfirmDialog.toast.success('Profile photo deleted');
        setProfilePhotoUrl(null);
        // Refresh user data to update topbar
        await refreshUser();
      } else {
        ConfirmDialog.toast.error(response.data?.message || 'Failed to delete photo');
      }
    } catch (error) {
      ConfirmDialog.toast.error('Failed to delete photo');
    }
  };

  const validatePassword = () => {
    const err = {};
    if (!passwordData.oldPassword) err.oldPassword = 'Required';
    if (!passwordData.newPassword) err.newPassword = 'Required';
    else if (passwordData.newPassword.length < 4) err.newPassword = 'Min 4 chars';
    if (passwordData.newPassword !== passwordData.confirmPassword) err.confirmPassword = 'Not match';
    setErrors(err);
    return !Object.keys(err).length;
  };

  const validateProfile = () => {
    const err = {};
    if (!formData.fullName?.trim()) err.fullName = 'Required';
    if (!formData.email?.trim()) err.email = 'Required';
    else if (!utilsHelper.validateEmail(formData.email)) err.email = 'Invalid';
    setErrors(err);
    return !Object.keys(err).length;
  };

  const handlePasswordChange = async (e) => {
    e.preventDefault();
    if (!validatePassword()) return;
    const confirmed = await ConfirmDialog.show({ title: 'Change Password', text: 'Are you sure?', confirmButtonText: 'Yes' });
    if (!confirmed) return;
    setSaving(true);
    const r = await profileData.changePassword(passwordData.oldPassword, passwordData.newPassword, passwordData.confirmPassword);
    setSaving(false);
    if (r.success) {
      setShowPasswordForm(false);
      setPasswordData({ oldPassword: '', newPassword: '', confirmPassword: '' });
      setErrors({});
    }
  };

  const handleProfileUpdate = async (e) => {
    e.preventDefault();
    if (!validateProfile()) return;
    setSaving(true);
    await new Promise(resolve => setTimeout(resolve, 500));
    setSaving(false);
    setEditMode(false);
    setProfile(prev => ({ ...prev, ...formData }));
    // Update local storage user data
    const storedUser = utilsHelper.getLocalStorage("user");
    if (storedUser) {
      const updatedUser = { ...storedUser, ...formData };
      utilsHelper.setLocalStorage("user", updatedUser);
    }
    ConfirmDialog.toast.success('Profile updated');
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
      <Box sx={{ mb: 2 }}>
        <Typography variant="body2" color="text.secondary">Manage your account settings and preferences</Typography>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={7}>
          <Card>
            <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 3, mb: 3, pb: 3, borderBottom: '1px solid var(--border)' }}>
              <Box sx={{ position: 'relative' }}>
                <Avatar 
                  src={profilePhotoUrl || undefined} 
                  sx={{ width: 100, height: 100, bgcolor: 'var(--primary)', fontSize: 40, fontWeight: 600 }}
                >
                  {!profilePhotoUrl && userInitial}
                </Avatar>
                {uploadingPhoto && (
                  <Box sx={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', bgcolor: 'rgba(0,0,0,0.5)', borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <CircularProgress size={30} sx={{ color: 'white' }} />
                  </Box>
                )}
                <Box sx={{ position: 'absolute', bottom: -8, right: -8, display: 'flex', gap: 1 }}>
                  <IconButton
                    size="small"
                    component="label"
                    sx={{ bgcolor: 'var(--primary)', color: 'white', '&:hover': { bgcolor: 'var(--primary-hover)' } }}
                  >
                    <FiCamera size={16} />
                    <input type="file" accept="image/jpeg,image/png,image/gif,image/webp" hidden onChange={handlePhotoUpload} />
                  </IconButton>
                  {profilePhotoUrl && (
                    <IconButton
                      size="small"
                      onClick={handleDeletePhoto}
                      sx={{ bgcolor: 'var(--error)', color: 'white', '&:hover': { bgcolor: 'var(--error-dark)', opacity: 0.9 } }}
                    >
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
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Box sx={{ width: 40, height: 40, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'var(--surface)', borderRadius: 2, color: 'var(--primary)' }}>
                    <FiUser size={20} />
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5 }}>Full Name</Typography>
                    <Typography variant="body1" fontWeight={500}>{profile?.fullName}</Typography>
                  </Box>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Box sx={{ width: 40, height: 40, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'var(--surface)', borderRadius: 2, color: 'var(--primary)' }}>
                    <FiMail size={20} />
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5 }}>Email</Typography>
                    <Typography variant="body1" fontWeight={500}>{profile?.email}</Typography>
                  </Box>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Box sx={{ width: 40, height: 40, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'var(--surface)', borderRadius: 2, color: 'var(--primary)' }}>
                    <FiBriefcase size={20} />
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5 }}>Username</Typography>
                    <Typography variant="body1" fontWeight={500}>@{profile?.username}</Typography>
                  </Box>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Box sx={{ width: 40, height: 40, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'var(--surface)', borderRadius: 2, color: 'var(--primary)' }}>
                    <FiShield size={20} />
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5 }}>Role</Typography>
                    <Typography variant="body1" fontWeight={500}>{profile?.roleId || 'User'}</Typography>
                  </Box>
                </Box>
                {profile?.phoneNumber && (
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Box sx={{ width: 40, height: 40, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'var(--surface)', borderRadius: 2, color: 'var(--primary)' }}>
                      <FiBriefcase size={20} />
                    </Box>
                    <Box>
                      <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5 }}>Phone</Typography>
                      <Typography variant="body1" fontWeight={500}>{profile?.phoneNumber}</Typography>
                    </Box>
                  </Box>
                )}
              </Box>
            ) : (
              <form onSubmit={handleProfileUpdate}>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Input label="Full Name" value={formData.fullName} onChange={e => setFormData({ ...formData, fullName: e.target.value })} error={errors.fullName} required />
                  </Grid>
                  <Grid item xs={12}>
                    <Input label="Email" type="email" value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })} error={errors.email} required />
                  </Grid>
                  <Grid item xs={12}>
                    <Input label="Phone (Optional)" value={formData.phoneNumber} onChange={e => setFormData({ ...formData, phoneNumber: e.target.value })} />
                  </Grid>
                  <Grid item xs={12}>
                    <Input label="Username" value={formData.username} disabled />
                  </Grid>
                  <Grid item xs={12}>
                    <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
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
                    <Input label="Current Password" type="password" value={passwordData.oldPassword} onChange={e => setPasswordData({ ...passwordData, oldPassword: e.target.value })} error={errors.oldPassword} />
                  </Grid>
                  <Grid item xs={12}>
                    <Input label="New Password" type="password" value={passwordData.newPassword} onChange={e => setPasswordData({ ...passwordData, newPassword: e.target.value })} error={errors.newPassword} />
                  </Grid>
                  <Grid item xs={12}>
                    <Input label="Confirm New Password" type="password" value={passwordData.confirmPassword} onChange={e => setPasswordData({ ...passwordData, confirmPassword: e.target.value })} error={errors.confirmPassword} />
                  </Grid>
                  <Grid item xs={12}>
                    <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
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