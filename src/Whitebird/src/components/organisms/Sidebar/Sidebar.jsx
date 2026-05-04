import React, { useState, useEffect } from 'react';
import { FiHome, FiBox, FiRefreshCw, FiUsers, FiFolder, FiBarChart2, FiChevronDown, FiChevronLeft, FiGrid, FiTruck, FiMapPin, FiMenu, FiCrosshair, FiUserCheck } from 'react-icons/fi';
import { useNavigate, useLocation } from 'react-router-dom';
import './Sidebar.scss';

const menuItems = [
  { id: 'dashboard', label: 'Dashboard', icon: FiHome, path: '/dashboard' },
  { id: 'assets', label: 'Assets', icon: FiBox, path: '/assets' },
  { id: 'transactions', label: 'Transactions', icon: FiRefreshCw, path: '/transactions' },
  { id: 'tracking', label: 'Asset Tracking', icon: FiCrosshair, path: '/tracking' },
  { id: 'employees', label: 'Employees', icon: FiUsers, path: '/employees' },
  { id: 'employee-summary', label: 'Employee Summary', icon: FiUserCheck, path: '/employee-summary' },
  { id: 'master-data', label: 'Master Data', icon: FiFolder, path: null, children: [
    { id: 'categories', label: 'Categories', icon: FiGrid, path: '/categories' },
    { id: 'suppliers', label: 'Suppliers', icon: FiTruck, path: '/suppliers' },
    { id: 'locations', label: 'Locations', icon: FiMapPin, path: '/locations' },
  ]},
  { id: 'reports', label: 'Reports', icon: FiBarChart2, path: '/reports' },
];

const Sidebar = ({ collapsed = false, onToggle }) => {
  const [expandedItems, setExpandedItems] = useState(new Set());
  const [logoError, setLogoError] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const activePath = location.pathname;

  useEffect(() => {
    menuItems.forEach(item => {
      if (item.children?.some(child => child.path === activePath)) {
        setExpandedItems(prev => new Set([...prev, item.id]));
      }
    });
  }, [activePath]);

  const toggleExpand = (id) => setExpandedItems(prev => { 
    const next = new Set(prev); 
    next.has(id) ? next.delete(id) : next.add(id); 
    return next; 
  });

  const renderItem = (item, level = 0) => {
    const hasChildren = item.children?.length > 0;
    const isExpanded = expandedItems.has(item.id);
    const isActive = item.path === activePath || item.children?.some(c => c.path === activePath);
    const Icon = item.icon;

    return (
      <li key={item.id} className="sidebar__item">
        <a
          href={item.path || '#'}
          className={`sidebar__link ${isActive ? 'sidebar__link--active' : ''} ${level > 0 ? 'sidebar__link--child' : ''}`}
          onClick={e => {
            e.preventDefault();
            hasChildren ? toggleExpand(item.id) : item.path && navigate(item.path);
          }}
          aria-current={isActive ? 'page' : undefined}
        >
          <Icon className="sidebar__icon" size={level > 0 ? 18 : 20} aria-hidden="true" />
          {!collapsed && (
            <>
              <span className="sidebar__label">{item.label}</span>
              {hasChildren && (
                <FiChevronDown
                  className={`sidebar__arrow ${isExpanded ? 'sidebar__arrow--expanded' : ''}`}
                  size={14}
                  aria-hidden="true"
                />
              )}
            </>
          )}
        </a>
        {hasChildren && isExpanded && !collapsed && (
          <ul className="sidebar__submenu">
            {item.children.map(c => renderItem(c, level + 1))}
          </ul>
        )}
      </li>
    );
  };

  const handleLogoClick = () => navigate('/dashboard');
  const appVersion = typeof __APP_VERSION__ !== 'undefined' ? __APP_VERSION__ : '1.0.0';

  return (
    <aside className={`sidebar ${collapsed ? 'sidebar--collapsed' : ''}`} role="navigation" aria-label="Main navigation">
      <div className="sidebar__header">
        {!collapsed ? (
          <>
            <div className="sidebar__logo" onClick={handleLogoClick} style={{ cursor: 'pointer' }} role="button" tabIndex={0} onKeyDown={(e) => e.key === 'Enter' && handleLogoClick()} aria-label="Go to dashboard">
              {logoError ? (
                <div className="sidebar__logo-fallback" aria-hidden="true">W</div>
              ) : (
                <img
                  src="/logo.png"
                  alt="Whitebird Asset Management System Logo"
                  className="sidebar__logo-img"
                  onError={() => setLogoError(true)}
                />
              )}
              <span className="sidebar__logo-text">Whitebird</span>
            </div>
            <button className="sidebar__toggle-btn" onClick={onToggle} aria-label="Collapse sidebar">
              <FiChevronLeft size={20} />
            </button>
          </>
        ) : (
          <button className="sidebar__toggle-btn sidebar__toggle-btn--expand" onClick={onToggle} aria-label="Expand sidebar">
            <FiMenu size={22} />
          </button>
        )}
      </div>
      <nav className="sidebar__nav">
        <ul className="sidebar__menu">
          {menuItems.map(item => renderItem(item))}
        </ul>
      </nav>
      <div className="sidebar__footer">
        {!collapsed && (
          <div className="sidebar__version">
            <span>v{appVersion}</span>
          </div>
        )}
      </div>
    </aside>
  );
};

export default Sidebar;