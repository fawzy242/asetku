import React from 'react';
import MainLayout from '../../layouts/MainLayout/MainLayout';
import CategoriesMenu from './Categories.menu';
import { useTheme } from '../../context/ThemeContext';
import { useAuth } from '../../context/AuthContext';
import './Categories.scss';

const Categories = () => {
  const { theme, toggleTheme } = useTheme();
  const { user, logout } = useAuth();

  return (
    <MainLayout
      title="Category Management"
      user={user}
      theme={theme}
      onThemeToggle={toggleTheme}
      onLogout={logout}
      onProfileClick={() => window.location.href = '/profile'}
      onSearch={(keyword) => console.log('Search:', keyword)}
    >
      <CategoriesMenu />
    </MainLayout>
  );
};

export default Categories;