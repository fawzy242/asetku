import React, { useState, useEffect } from "react";
import { FiUser, FiMail, FiShield, FiLock, FiSave, FiX, FiBriefcase, FiCalendar, FiEdit2, FiCheck } from "react-icons/fi";
import { Grid, Box, Avatar, Typography } from "@mui/material";
import ProfileData from "./Profile.data";
import Card from "../../components/atoms/Card/Card";
import Button from "../../components/atoms/Button/Button";
import Input from "../../components/atoms/Input/Input";
import Spinner from "../../components/atoms/Spinner/Spinner";
import ConfirmDialog from "../../components/molecules/ConfirmDialog/ConfirmDialog";
import utilsHelper from "../../core/utils/utils.helper";
import "./Profile.scss";

const profileData = new ProfileData();

const ProfileMenu = () => {
  const [loading, setLoading] = useState(true);
  const [profile, setProfile] = useState(null);
  const [showPasswordForm, setShowPasswordForm] = useState(false);
  const [editMode, setEditMode] = useState(false);
  const [saving, setSaving] = useState(false);
  const [passwordData, setPasswordData] = useState({ oldPassword: '', newPassword: '', confirmPassword: '' });
  const [formData, setFormData] = useState({ fullName: '', email: '', phoneNumber: '', username: '' });
  const [errors, setErrors] = useState({});

  useEffect(() => { loadProfile(); }, []);

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
    }
    setLoading(false);
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

  // NOTE: Profile update endpoint does not exist in backend
  // This is a local-only update for demo purposes
  const handleProfileUpdate = async (e) => {
    e.preventDefault();
    if (!validateProfile()) return;
    setSaving(true);
    // Simulate API call - backend does not have /Auth/profile endpoint
    await new Promise(resolve => setTimeout(resolve, 500));
    setSaving(false);
    setEditMode(false);
    setProfile(prev => ({ ...prev, ...formData }));
    ConfirmDialog.toast.success('Profile updated locally');
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
              <Avatar sx={{ width: 80, height: 80, bgcolor: 'var(--primary)', fontSize: 32, fontWeight: 600 }}>{userInitial}</Avatar>
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