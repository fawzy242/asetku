import React, { useState, useEffect } from 'react';
import './Sidebar.scss';

const Sidebar = ({
  menuItems = [],
  collapsed = false,
  onToggle,
  activePath = '/',
  onNavigate
}) => {
  const [expandedItems, setExpandedItems] = useState(new Set());

  useEffect(() => {
    // Auto-expand parent of active item
    menuItems.forEach(item => {
      if (item.children) {
        const hasActiveChild = item.children.some(child => child.path === activePath);
        if (hasActiveChild) {
          setExpandedItems(prev => new Set([...prev, item.id]));
        }
      }
    });
  }, [activePath, menuItems]);

  const toggleExpand = (itemId) => {
    setExpandedItems(prev => {
      const next = new Set(prev);
      if (next.has(itemId)) {
        next.delete(itemId);
      } else {
        next.add(itemId);
      }
      return next;
    });
  };

  const renderMenuItem = (item, level = 0) => {
    const hasChildren = item.children && item.children.length > 0;
    const isExpanded = expandedItems.has(item.id);
    const isActive = item.path === activePath || 
      (item.children && item.children.some(child => child.path === activePath));

    return (
      <li key={item.id} className="sidebar__item">
        <a
          href={item.path}
          className={`sidebar__link ${isActive ? 'sidebar__link--active' : ''} ${level > 0 ? 'sidebar__link--child' : ''}`}
          onClick={(e) => {
            e.preventDefault();
            if (hasChildren) {
              toggleExpand(item.id);
            } else {
              onNavigate?.(item.path);
            }
          }}
        >
          {item.icon && <span className="sidebar__icon">{item.icon}</span>}
          {!collapsed && (
            <>
              <span className="sidebar__label">{item.label}</span>
              {hasChildren && (
                <span className={`sidebar__arrow ${isExpanded ? 'sidebar__arrow--expanded' : ''}`}>
                  ▼
                </span>
              )}
              {item.badge && (
                <span className={`sidebar__badge sidebar__badge--${item.badge.variant}`}>
                  {item.badge.text}
                </span>
              )}
            </>
          )}
        </a>
        
        {hasChildren && isExpanded && !collapsed && (
          <ul className="sidebar__submenu">
            {item.children.map(child => renderMenuItem(child, level + 1))}
          </ul>
        )}
      </li>
    );
  };

  return (
    <aside className={`sidebar ${collapsed ? 'sidebar--collapsed' : ''}`}>
      <div className="sidebar__header">
        {!collapsed && (
          <div className="sidebar__logo">
            <span className="sidebar__logo-icon">🪽</span>
            <span className="sidebar__logo-text">Whitebird</span>
          </div>
        )}
        {collapsed && (
          <div className="sidebar__logo sidebar__logo--collapsed">
            <span className="sidebar__logo-icon">🪽</span>
          </div>
        )}
      </div>
      
      <nav className="sidebar__nav">
        <ul className="sidebar__menu">
          {menuItems.map(item => renderMenuItem(item))}
        </ul>
      </nav>
      
      <div className="sidebar__footer">
        <button 
          className="sidebar__toggle"
          onClick={onToggle}
          aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          {collapsed ? '→' : '←'}
        </button>
      </div>
    </aside>
  );
};

export default Sidebar;