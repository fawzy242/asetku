import React, { useState, useRef, useEffect, useCallback } from 'react';
import { FiChevronDown, FiUser, FiLogOut } from 'react-icons/fi';
import { Avatar } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../context/AuthContext';
import { UserData } from '../../../core/services';
import ConfirmDialog from '../../molecules/ConfirmDialog/ConfirmDialog';

const userData = new UserData();

const UserMenu = ({ user }) => {
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [profilePhotoUrl, setProfilePhotoUrl] = useState(null);
  const userMenuRef = useRef(null);
  const navigate = useNavigate();
  const { logout } = useAuth();

  const loadProfilePhoto = useCallback(async () => {
    try {
      const result = await userData.getPhoto();
      if (result.success && result.data) {
        const url = URL.createObjectURL(result.data);
        setProfilePhotoUrl(url);
      } else {
        setProfilePhotoUrl(null);
      }
    } catch (error) {
      console.warn('Failed to load profile photo:', error);
      setProfilePhotoUrl(null);
    }
  }, []);

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (userMenuRef.current && !userMenuRef.current.contains(e.target)) {
        setShowUserMenu(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  useEffect(() => {
    if (user?.userId) {
      loadProfilePhoto();
    }
  }, [user?.userId, loadProfilePhoto]);

  // Refresh when Profile page dispatches 'profile-photo-updated'
  useEffect(() => {
    const handler = () => {
      loadProfilePhoto();
    };
    window.addEventListener('profile-photo-updated', handler);
    return () => {
      window.removeEventListener('profile-photo-updated', handler);
    };
  }, [loadProfilePhoto]);

  const handleLogout = async () => {
    const confirmed = await ConfirmDialog.show({
      title: 'Logout',
      text: 'Are you sure you want to logout?',
      icon: 'question',
      confirmButtonText: 'Logout',
      confirmButtonColor: '#dc2626',
    });
    if (confirmed) {
      setShowUserMenu(false);
      await logout();
      navigate('/login');
    }
  };

  const userInitial = user?.fullName?.charAt(0)?.toUpperCase() || 'U';

  return (
    <div className="topbar__user" ref={userMenuRef}>
      <button
        className="topbar__user-btn"
        onClick={() => setShowUserMenu(!showUserMenu)}
        aria-label="User menu"
        aria-expanded={showUserMenu}
      >
        {profilePhotoUrl ? (
          <Avatar
            src={profilePhotoUrl}
            sx={{ width: 36, height: 36 }}
            onError={() => setProfilePhotoUrl(null)}
          />
        ) : (
          <span className="topbar__user-avatar">{userInitial}</span>
        )}
        <span className="topbar__user-name">{user?.fullName || 'User'}</span>
        <FiChevronDown size={16} />
      </button>
      {showUserMenu && (
        <div className="topbar__user-dropdown">
          <div className="topbar__user-header">
            {profilePhotoUrl ? (
              <Avatar
                src={profilePhotoUrl}
                sx={{ width: 48, height: 48 }}
                onError={() => setProfilePhotoUrl(null)}
              />
            ) : (
              <span className="topbar__user-avatar--large">{userInitial}</span>
            )}
            <div>
              <p className="topbar__user-fullname">{user?.fullName}</p>
              <p className="topbar__user-email">{user?.email}</p>
            </div>
          </div>
          <ul className="topbar__user-menu">
            <li>
              <button
                onClick={() => {
                  setShowUserMenu(false);
                  navigate('/profile');
                }}
              >
                <FiUser size={18} /> Profile
              </button>
            </li>
            <li className="topbar__user-menu-divider" />
            <li>
              <button onClick={handleLogout}>
                <FiLogOut size={18} /> Logout
              </button>
            </li>
          </ul>
        </div>
      )}
    </div>
  );
};

export default UserMenu;