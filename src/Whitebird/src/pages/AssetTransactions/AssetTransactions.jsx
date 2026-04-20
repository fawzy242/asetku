import React from 'react';
import MainLayout from '../../layouts/MainLayout/MainLayout';
import AssetTransactionsMenu from './AssetTransactions.menu';
import { useTheme } from '../../context/ThemeContext';
import { useAuth } from '../../context/AuthContext';
import './AssetTransactions.scss';

const AssetTransactions = () => {
  const { theme, toggleTheme } = useTheme();
  const { user, logout } = useAuth();

  return (
    <MainLayout
      title="Asset Transactions"
      user={user}
      theme={theme}
      onThemeToggle={toggleTheme}
      onLogout={logout}
      onProfileClick={() => window.location.href = '/profile'}
      onSearch={(keyword) => console.log('Search:', keyword)}
    >
      <AssetTransactionsMenu />
    </MainLayout>
  );
};

export default AssetTransactions;