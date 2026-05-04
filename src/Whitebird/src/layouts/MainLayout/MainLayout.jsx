import React, { useMemo } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import Sidebar from '../../components/organisms/Sidebar/Sidebar';
import Topbar from '../../components/organisms/Topbar/Topbar';
import Footer from '../../components/organisms/Footer/Footer';
import { useUIStore } from '../../stores/uiStore';
import { useAuth } from '../../context/AuthContext';
import './MainLayout.scss';

const pageTitles = {
  '/dashboard': 'Dashboard',
  '/assets': 'Asset Management',
  '/transactions': 'Asset Transactions',
  '/tracking': 'Asset Tracking',
  '/employees': 'Employee Management',
  '/employee-summary': 'Employee Summary',
  '/categories': 'Category Management',
  '/suppliers': 'Supplier Management',
  '/locations': 'Location Management',
  '/reports': 'Reports & Analytics',
  '/profile': 'My Profile',
};

const MainLayout = () => {
  const location = useLocation();
  const { sidebarCollapsed, toggleSidebar, theme } = useUIStore();
  const { user } = useAuth();

  const normalizedPath = useMemo(() => {
    const path = location.pathname;
    return path.length > 1 && path.endsWith('/') ? path.slice(0, -1) : path;
  }, [location.pathname]);

  const title = pageTitles[normalizedPath] || 'Dashboard';

  return (
    <div className="main-layout" data-theme={theme}>
      <Sidebar collapsed={sidebarCollapsed} onToggle={toggleSidebar} />
      <div className={`main-layout__content ${sidebarCollapsed ? 'main-layout__content--expanded' : ''}`}>
        <Topbar user={user} />
        <main className="main-layout__main">
          <h1 className="page-title">{title}</h1>
          <Outlet />
        </main>
        <Footer />
      </div>
    </div>
  );
};

export default MainLayout;