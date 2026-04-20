import React, { useState, useEffect } from 'react';
import Sidebar from '../../components/organisms/Sidebar/Sidebar';
import Topbar from '../../components/organisms/Topbar/Topbar';
import './MainLayout.scss';

const menuItems = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    icon: '📊',
    path: '/dashboard'
  },
  {
    id: 'assets',
    label: 'Assets',
    icon: '📦',
    path: '/assets'
  },
  {
    id: 'transactions',
    label: 'Transactions',
    icon: '🔄',
    path: '/transactions'
  },
  {
    id: 'employees',
    label: 'Employees',
    icon: '👥',
    path: '/employees'
  },
  {
    id: 'master-data',
    label: 'Master Data',
    icon: '📋',
    path: null,
    children: [
      {
        id: 'categories',
        label: 'Categories',
        path: '/categories'
      },
      {
        id: 'suppliers',
        label: 'Suppliers',
        path: '/suppliers'
      },
      {
        id: 'locations',
        label: 'Locations',
        path: '/locations'
      }
    ]
  },
  {
    id: 'reports',
    label: 'Reports',
    icon: '📈',
    path: '/reports'
  }
];

const MainLayout = ({
  children,
  title = 'Dashboard',
  onSearch,
  onLogout,
  onProfileClick,
  theme = 'light',
  onThemeToggle,
  user = null
}) => {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [currentPath, setCurrentPath] = useState(window.location.pathname);

  useEffect(() => {
    const handleRouteChange = () => {
      setCurrentPath(window.location.pathname);
    };
    
    window.addEventListener('popstate', handleRouteChange);
    return () => window.removeEventListener('popstate', handleRouteChange);
  }, []);

  const handleNavigate = (path) => {
    window.history.pushState({}, '', path);
    setCurrentPath(path);
    window.dispatchEvent(new PopStateEvent('popstate'));
  };

  return (
    <div className="main-layout" data-theme={theme}>
      <Sidebar
        menuItems={menuItems}
        collapsed={sidebarCollapsed}
        onToggle={() => setSidebarCollapsed(!sidebarCollapsed)}
        activePath={currentPath}
        onNavigate={handleNavigate}
      />
      
      <div className={`main-layout__content ${sidebarCollapsed ? 'main-layout__content--expanded' : ''}`}>
        <Topbar
          title={title}
          user={user}
          onSearch={onSearch}
          onThemeToggle={onThemeToggle}
          onLogout={onLogout}
          onProfileClick={onProfileClick}
          theme={theme}
        />
        
        <main className="main-layout__main">
          {children}
        </main>
      </div>
    </div>
  );
};

export default MainLayout;