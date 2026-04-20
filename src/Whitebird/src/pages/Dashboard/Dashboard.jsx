import React from 'react';
import MainLayout from '../../layouts/MainLayout/MainLayout';
import DashboardMenu from './Dashboard.menu';
import { useTheme } from '../../context/ThemeContext';
import { useAuth } from '../../context/AuthContext';
import './Dashboard.scss';

const Dashboard = () => {
  const { theme, toggleTheme } = useTheme();
  const { user, logout } = useAuth();

  return (
    <MainLayout
      title="Dashboard"
      user={user}
      theme={theme}
      onThemeToggle={toggleTheme}
      onLogout={logout}
      onProfileClick={() => window.location.href = '/profile'}
      onSearch={(keyword) => console.log('Search:', keyword)}
    >
      <DashboardMenu />
    </MainLayout>
  );
};

export default Dashboard;