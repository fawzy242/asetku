// src/components/organisms/Topbar/UserMenu.jsx
import React, { useState, useRef, useEffect, useCallback } from 'react';
import { FiChevronDown, FiUser, FiLogOut } from 'react-icons/fi';
import { Avatar } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../context/AuthContext';
import { useFileBlob } from '../../../hooks/useFileBlob';
import ConfirmDialog from '../../molecules/ConfirmDialog/ConfirmDialog';

const UserMenu = ({ user }) => {
  const [showUserMenu, setShowUserMenu] = useState(false);
  const userMenuRef = useRef(null);
  const navigate = useNavigate();
  const { logout } = useAuth();

  // Authenticated blob fetch.
  const { blobUrl } = useFileBlob(user?.profilePhotoUrl);

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (userMenuRef.current && !userMenuRef.current.contains(e.target)) {
        setShowUserMenu(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleLogout = useCallback(async () => {
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
  }, [logout, navigate]);

  const userInitial = user?.fullName?.charAt(0)?.toUpperCase() || 'U';
  const showPhoto = Boolean(blobUrl);

  return (
    <div className="topbar__user" ref={userMenuRef}>
      <button
        className="topbar__user-btn"
        onClick={() => setShowUserMenu(!showUserMenu)}
        aria-label="User menu"
        aria-expanded={showUserMenu}
      >
        {showPhoto ? (
          <Avatar src={blobUrl} sx={{ width: 36, height: 36 }} />
        ) : (
          <span className="topbar__user-avatar">{userInitial}</span>
        )}
        <span className="topbar__user-name">{user?.fullName || 'User'}</span>
        <FiChevronDown size={16} />
      </button>
      {showUserMenu && (
        <div className="topbar__user-dropdown">
          <div className="topbar__user-header">
            {showPhoto ? (
              <Avatar src={blobUrl} sx={{ width: 48, height: 48 }} />
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
              <button onClick={() => { setShowUserMenu(false); navigate('/profile'); }}>
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