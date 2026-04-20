import React, { useState } from 'react';
import './Topbar.scss';

const Topbar = ({
  title = 'Dashboard',
  user = null,
  onSearch,
  onThemeToggle,
  onLogout,
  onProfileClick,
  theme = 'light',
  notifications = []
}) => {
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [showNotifications, setShowNotifications] = useState(false);
  const [searchValue, setSearchValue] = useState('');

  const handleSearch = (e) => {
    e.preventDefault();
    onSearch?.(searchValue);
  };

  const unreadCount = notifications.filter(n => !n.read).length;

  return (
    <header className="topbar">
      <div className="topbar__left">
        <h1 className="topbar__title">{title}</h1>
      </div>
      
      <div className="topbar__center">
        <form className="topbar__search" onSubmit={handleSearch}>
          <input
            type="text"
            className="topbar__search-input"
            placeholder="Search assets, employees, transactions..."
            value={searchValue}
            onChange={(e) => setSearchValue(e.target.value)}
          />
          <button type="submit" className="topbar__search-btn" aria-label="Search">
            🔍
          </button>
        </form>
      </div>
      
      <div className="topbar__right">
        <button
          className="topbar__theme-toggle"
          onClick={onThemeToggle}
          aria-label="Toggle theme"
        >
          {theme === 'light' ? '🌙' : '☀️'}
        </button>
        
        <div className="topbar__notifications">
          <button
            className="topbar__notification-btn"
            onClick={() => setShowNotifications(!showNotifications)}
            aria-label="Notifications"
          >
            🔔
            {unreadCount > 0 && (
              <span className="topbar__notification-badge">{unreadCount}</span>
            )}
          </button>
          
          {showNotifications && (
            <div className="topbar__notification-dropdown">
              <div className="topbar__notification-header">
                <h4>Notifications</h4>
                <button className="topbar__notification-mark-all">Mark all read</button>
              </div>
              <div className="topbar__notification-list">
                {notifications.length === 0 ? (
                  <p className="topbar__notification-empty">No notifications</p>
                ) : (
                  notifications.map(notification => (
                    <div 
                      key={notification.id} 
                      className={`topbar__notification-item ${!notification.read ? 'topbar__notification-item--unread' : ''}`}
                    >
                      <span className="topbar__notification-icon">{notification.icon}</span>
                      <div className="topbar__notification-content">
                        <p className="topbar__notification-title">{notification.title}</p>
                        <p className="topbar__notification-time">{notification.time}</p>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </div>
          )}
        </div>
        
        <div className="topbar__user">
          <button
            className="topbar__user-btn"
            onClick={() => setShowUserMenu(!showUserMenu)}
          >
            <span className="topbar__user-avatar">
              {user?.avatar || user?.fullName?.charAt(0) || 'U'}
            </span>
            <span className="topbar__user-name">{user?.fullName || 'User'}</span>
            <span className="topbar__user-arrow">▼</span>
          </button>
          
          {showUserMenu && (
            <div className="topbar__user-dropdown">
              <div className="topbar__user-header">
                <span className="topbar__user-avatar--large">
                  {user?.avatar || user?.fullName?.charAt(0) || 'U'}
                </span>
                <div>
                  <p className="topbar__user-fullname">{user?.fullName}</p>
                  <p className="topbar__user-email">{user?.email}</p>
                </div>
              </div>
              <ul className="topbar__user-menu">
                <li>
                  <button onClick={() => { setShowUserMenu(false); onProfileClick?.(); }}>
                    👤 Profile
                  </button>
                </li>
                <li>
                  <button onClick={() => { setShowUserMenu(false); onLogout?.(); }}>
                    🚪 Logout
                  </button>
                </li>
              </ul>
            </div>
          )}
        </div>
      </div>
    </header>
  );
};

export default Topbar;