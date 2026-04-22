import React, { useState, useEffect } from 'react';
import { 
  FiUser, 
  FiMail, 
  FiShield, 
  FiLock, 
  FiSave, 
  FiX,
  FiBriefcase,
  FiCalendar,
  FiEdit2,
  FiCheck,
  FiAlertCircle
} from 'react-icons/fi';
import Swal from 'sweetalert2';
import ProfileData from './Profile.data';
import Card from '../../components/atoms/Card/Card';
import Button from '../../components/atoms/Button/Button';
import Input from '../../components/atoms/Input/Input';
import Spinner from '../../components/atoms/Spinner/Spinner';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import utilsHelper from '../../core/utils/utils.helper';
import './Profile.scss';

const ProfileMenu = () => {
  const [loading, setLoading] = useState(true);
  const [profile, setProfile] = useState(null);
  const [showPasswordForm, setShowPasswordForm] = useState(false);
  const [editMode, setEditMode] = useState(false);
  const [passwordData, setPasswordData] = useState({
    oldPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [saving, setSaving] = useState(false);
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    phoneNumber: '',
    username: ''
  });
  const [errors, setErrors] = useState({});

  const profileData = new ProfileData();

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    setLoading(true);
    const result = await profileData.loadProfile();
    if (result.success) {
      setProfile(result.data);
      setFormData({
        fullName: result.data.fullName || '',
        email: result.data.email || '',
        phoneNumber: result.data.phoneNumber || '',
        username: result.data.username || ''
      });
    } else {
      Swal.fire({
        title: 'Error',
        text: result.error || 'Failed to load profile',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
    }
    setLoading(false);
  };

  const validatePasswordForm = () => {
    const newErrors = {};
    
    if (!passwordData.oldPassword) {
      newErrors.oldPassword = 'Current password is required';
    }
    
    if (!passwordData.newPassword) {
      newErrors.newPassword = 'New password is required';
    } else if (passwordData.newPassword.length < 4) {
      newErrors.newPassword = 'Password must be at least 4 characters';
    }
    
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const validateProfileForm = () => {
    const newErrors = {};
    
    if (!formData.fullName?.trim()) {
      newErrors.fullName = 'Full name is required';
    }
    
    if (!formData.email?.trim()) {
      newErrors.email = 'Email is required';
    } else if (!utilsHelper.validateEmail(formData.email)) {
      newErrors.email = 'Invalid email format';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handlePasswordChange = async (e) => {
    e.preventDefault();
    
    if (!validatePasswordForm()) return;
    
    const confirmed = await ConfirmDialog.show({
      title: 'Change Password',
      text: 'Are you sure you want to change your password?',
      icon: 'question',
      confirmButtonText: 'Yes, change it',
      confirmButtonColor: '#dc2626'
    });
    
    if (!confirmed) return;
    
    setSaving(true);
    const result = await profileData.changePassword(
      passwordData.oldPassword,
      passwordData.newPassword,
      passwordData.confirmPassword
    );
    setSaving(false);
    
    if (result.success) {
      setShowPasswordForm(false);
      setPasswordData({ oldPassword: '', newPassword: '', confirmPassword: '' });
      setErrors({});
      
      Swal.fire({
        title: 'Success',
        text: 'Password changed successfully',
        icon: 'success',
        confirmButtonColor: '#dc2626'
      });
    } else {
      setErrors({ oldPassword: result.error || 'Failed to change password' });
    }
  };

  const handleProfileUpdate = async (e) => {
    e.preventDefault();
    
    if (!validateProfileForm()) return;
    
    setSaving(true);
    const result = await profileData.updateProfile(formData);
    setSaving(false);
    
    if (result.success) {
      setEditMode(false);
      await loadProfile();
      
      Swal.fire({
        title: 'Success',
        text: 'Profile updated successfully',
        icon: 'success',
        confirmButtonColor: '#dc2626',
        timer: 2000,
        showConfirmButton: false
      });
    } else {
      Swal.fire({
        title: 'Error',
        text: result.error || 'Failed to update profile',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
    }
  };

  const handleCancelEdit = () => {
    setEditMode(false);
    setFormData({
      fullName: profile?.fullName || '',
      email: profile?.email || '',
      phoneNumber: profile?.phoneNumber || '',
      username: profile?.username || ''
    });
    setErrors({});
  };

  const handleCancelPassword = () => {
    setShowPasswordForm(false);
    setPasswordData({ oldPassword: '', newPassword: '', confirmPassword: '' });
    setErrors({});
  };

  if (loading) {
    return (
      <div className="profile-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  const userInitial = profile?.fullName?.charAt(0) || 'U';
  const memberSince = profile?.createdDate 
    ? utilsHelper.formatDate(profile.createdDate, 'MMMM D, YYYY')
    : '-';

  return (
    <div className="profile-menu">
      <div className="profile-menu__header">
        <h2 className="profile-menu__title">My Profile</h2>
        <p className="profile-menu__subtitle">Manage your account settings and preferences</p>
      </div>
      
      <div className="profile-menu__grid">
        {/* Profile Information Card */}
        <Card className="profile-menu__info-card">
          <div className="profile-menu__avatar-section">
            <div className="profile-menu__avatar">
              <span className="profile-menu__avatar-text">{userInitial}</span>
            </div>
            <div className="profile-menu__avatar-info">
              <h3>{profile?.fullName || 'User'}</h3>
              <p className="profile-menu__role">{profile?.roleId || 'Administrator'}</p>
              <p className="profile-menu__member-since">
                <FiCalendar /> Member since {memberSince}
              </p>
            </div>
            {!editMode && (
              <Button 
                variant="outline" 
                size="sm"
                onClick={() => setEditMode(true)}
                startIcon={<FiEdit2 />}
              >
                Edit Profile
              </Button>
            )}
          </div>
          
          {!editMode ? (
            <div className="profile-menu__details">
              <div className="profile-menu__detail-item">
                <div className="profile-menu__detail-icon">
                  <FiUser />
                </div>
                <div className="profile-menu__detail-content">
                  <label>Full Name</label>
                  <p>{profile?.fullName || '-'}</p>
                </div>
              </div>
              
              <div className="profile-menu__detail-item">
                <div className="profile-menu__detail-icon">
                  <FiMail />
                </div>
                <div className="profile-menu__detail-content">
                  <label>Email Address</label>
                  <p>{profile?.email || '-'}</p>
                  {profile?.emailVerified ? (
                    <span className="profile-menu__verified">
                      <FiCheck /> Verified
                    </span>
                  ) : (
                    <span className="profile-menu__unverified">
                      <FiAlertCircle /> Not verified
                    </span>
                  )}
                </div>
              </div>
              
              <div className="profile-menu__detail-item">
                <div className="profile-menu__detail-icon">
                  <FiBriefcase />
                </div>
                <div className="profile-menu__detail-content">
                  <label>Username</label>
                  <p>@{profile?.username || '-'}</p>
                </div>
              </div>
              
              <div className="profile-menu__detail-item">
                <div className="profile-menu__detail-icon">
                  <FiShield />
                </div>
                <div className="profile-menu__detail-content">
                  <label>Role</label>
                  <p>{profile?.roleId || 'User'}</p>
                </div>
              </div>
              
              {profile?.phoneNumber && (
                <div className="profile-menu__detail-item">
                  <div className="profile-menu__detail-icon">
                    <FiBriefcase />
                  </div>
                  <div className="profile-menu__detail-content">
                    <label>Phone Number</label>
                    <p>{profile.phoneNumber}</p>
                  </div>
                </div>
              )}
            </div>
          ) : (
            <form className="profile-menu__edit-form" onSubmit={handleProfileUpdate}>
              <Input
                label="Full Name"
                value={formData.fullName}
                onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
                error={errors.fullName}
                startAdornment={<FiUser />}
                placeholder="Enter your full name"
                required
              />
              
              <Input
                label="Email Address"
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                error={errors.email}
                startAdornment={<FiMail />}
                placeholder="Enter your email"
                required
              />
              
              <Input
                label="Username"
                value={formData.username}
                onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                error={errors.username}
                startAdornment={<FiBriefcase />}
                placeholder="Enter your username"
                disabled
              />
              
              <Input
                label="Phone Number (Optional)"
                value={formData.phoneNumber}
                onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                error={errors.phoneNumber}
                startAdornment={<FiBriefcase />}
                placeholder="Enter your phone number"
              />
              
              <div className="profile-menu__form-actions">
                <Button 
                  type="button" 
                  variant="text" 
                  onClick={handleCancelEdit}
                  startIcon={<FiX />}
                >
                  Cancel
                </Button>
                <Button 
                  type="submit" 
                  variant="primary"
                  loading={saving}
                  startIcon={<FiCheck />}
                >
                  Save Changes
                </Button>
              </div>
            </form>
          )}
        </Card>
        
        {/* Security Card */}
        <Card className="profile-menu__security-card">
          <div className="profile-menu__security-header">
            <div className="profile-menu__security-icon-wrapper">
              <FiLock className="profile-menu__security-icon" />
            </div>
            <div>
              <h3>Security</h3>
              <p>Update your password to keep your account secure</p>
            </div>
          </div>
          
          <div className="profile-menu__security-info">
            <div className="profile-menu__security-item">
              <span className="profile-menu__security-label">Password last changed</span>
              <span className="profile-menu__security-value">
                {profile?.passwordChangedAt 
                  ? utilsHelper.formatDate(profile.passwordChangedAt, 'MMMM D, YYYY')
                  : 'Never'}
              </span>
            </div>
            <div className="profile-menu__security-item">
              <span className="profile-menu__security-label">Two-factor authentication</span>
              <span className="profile-menu__security-value status--disabled">Disabled</span>
            </div>
          </div>
          
          {!showPasswordForm ? (
            <Button 
              variant="outline" 
              onClick={() => setShowPasswordForm(true)}
              className="profile-menu__change-password-btn"
              startIcon={<FiLock />}
            >
              Change Password
            </Button>
          ) : (
            <form className="profile-menu__password-form" onSubmit={handlePasswordChange}>
              <Input
                label="Current Password"
                type="password"
                placeholder="Enter current password"
                value={passwordData.oldPassword}
                onChange={(e) => setPasswordData({ ...passwordData, oldPassword: e.target.value })}
                error={errors.oldPassword}
                required
              />
              
              <Input
                label="New Password"
                type="password"
                placeholder="Enter new password"
                value={passwordData.newPassword}
                onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                error={errors.newPassword}
                required
              />
              
              <Input
                label="Confirm New Password"
                type="password"
                placeholder="Confirm new password"
                value={passwordData.confirmPassword}
                onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
                error={errors.confirmPassword}
                required
              />
              
              <div className="profile-menu__password-hint">
                <FiAlertCircle />
                <span>Password must be at least 4 characters long</span>
              </div>
              
              <div className="profile-menu__form-actions">
                <Button 
                  type="button" 
                  variant="text" 
                  onClick={handleCancelPassword}
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
              </div>
            </form>
          )}
        </Card>
        
        {/* Activity Card */}
        <Card className="profile-menu__activity-card">
          <h3 className="profile-menu__activity-title">Recent Activity</h3>
          <div className="profile-menu__activity-list">
            <div className="profile-menu__activity-item">
              <div className="profile-menu__activity-icon activity--login">
                <FiCheck />
              </div>
              <div className="profile-menu__activity-content">
                <p className="profile-menu__activity-text">Last login</p>
                <p className="profile-menu__activity-time">
                  {profile?.lastLoginDate 
                    ? utilsHelper.formatDateTime(profile.lastLoginDate)
                    : 'First time login'}
                </p>
              </div>
            </div>
            <div className="profile-menu__activity-item">
              <div className="profile-menu__activity-icon activity--device">
                <FiBriefcase />
              </div>
              <div className="profile-menu__activity-content">
                <p className="profile-menu__activity-text">Active sessions</p>
                <p className="profile-menu__activity-time">1 active device</p>
              </div>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
};

export default ProfileMenu;