import React from 'react';
import MainLayout from '../../layouts/MainLayout/MainLayout';
import AssetsMenu from './Assets.menu';
import { useTheme } from '../../context/ThemeContext';
import { useAuth } from '../../context/AuthContext';
import './Assets.scss';

const Assets = () => {
  const { theme, toggleTheme } = useTheme();
  const { user, logout } = useAuth();

  return (
    <MainLayout
      title="Asset Management"
      user={user}
      theme={theme}
      onThemeToggle={toggleTheme}
      onLogout={logout}
      onProfileClick={() => window.location.href = '/profile'}
      onSearch={(keyword) => console.log('Search:', keyword)}
    >
      <AssetsMenu />
    </MainLayout>
  );
};

export default Assets;