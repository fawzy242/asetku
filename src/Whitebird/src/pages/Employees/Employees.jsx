import React from 'react';
import MainLayout from '../../layouts/MainLayout/MainLayout';
import EmployeesMenu from './Employees.menu';
import { useTheme } from '../../context/ThemeContext';
import { useAuth } from '../../context/AuthContext';
import './Employees.scss';

const Employees = () => {
  const { theme, toggleTheme } = useTheme();
  const { user, logout } = useAuth();

  return (
    <MainLayout
      title="Employee Management"
      user={user}
      theme={theme}
      onThemeToggle={toggleTheme}
      onLogout={logout}
      onProfileClick={() => window.location.href = '/profile'}
      onSearch={(keyword) => console.log('Search:', keyword)}
    >
      <EmployeesMenu />
    </MainLayout>
  );
};

export default Employees;