import React from 'react';
import { FiSun, FiMoon } from 'react-icons/fi';
import { useUIStore } from '../../../stores/uiStore';
import SearchBar from './SearchBar';
import UserMenu from './UserMenu';
import './Topbar.scss';

const Topbar = ({ user = null }) => {
  const { theme, toggleTheme } = useUIStore();

  return (
    <header className="topbar">
      <div className="topbar__left"></div>

      <div className="topbar__center">
        <SearchBar />
      </div>

      <div className="topbar__right">
        <button
          className="topbar__theme-toggle"
          onClick={toggleTheme}
          aria-label={`Switch to ${theme === "light" ? "dark" : "light"} mode`}
        >
          {theme === "light" ? <FiMoon size={20} /> : <FiSun size={20} />}
        </button>

        <UserMenu user={user} />
      </div>
    </header>
  );
};

export default Topbar;