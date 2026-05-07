import React, { useMemo } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import Sidebar from '../../components/organisms/Sidebar/Sidebar';
import Topbar from '../../components/organisms/Topbar/Topbar';
import Footer from '../../components/organisms/Footer/Footer';
import { useUIStore } from '../../stores/uiStore';
import { useAuth } from '../../context/AuthContext';
import './MainLayout.scss';

const MainLayout = () => {
  const location = useLocation();
  const { sidebarCollapsed, toggleSidebar, theme } = useUIStore();
  const { user } = useAuth();

  return (
    <div className="main-layout" data-theme={theme}>
      <Sidebar collapsed={sidebarCollapsed} onToggle={toggleSidebar} />
      <div className={`main-layout__content ${sidebarCollapsed ? 'main-layout__content--expanded' : ''}`}>
        <Topbar user={user} />
        <main className="main-layout__main">
          {/* FIX: Hapus page-title dari sini, setiap page punya header sendiri */}
          <Outlet />
        </main>
        <Footer />
      </div>
    </div>
  );
};

export default MainLayout;