import React, { useState, useRef, useEffect, useCallback } from "react";
import {
  FiSearch, FiBell, FiSun, FiMoon, FiChevronDown, FiCheckCircle,
  FiAlertCircle, FiInfo, FiX, FiUser, FiLogOut,
} from "react-icons/fi";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../../context/AuthContext";
import ConfirmDialog from "../../molecules/ConfirmDialog/ConfirmDialog";
import apiService from "../../../core/services/api.service";
import utilsHelper from "../../../core/utils/utils.helper";
import "./Topbar.scss";

const Topbar = ({ title = "Dashboard", user = null, onThemeToggle, theme = "light", notifications = [] }) => {
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [showNotifications, setShowNotifications] = useState(false);
  const [searchValue, setSearchValue] = useState("");
  const [isSearching, setIsSearching] = useState(false);
  const [searchResults, setSearchResults] = useState([]);
  const [showSearchResults, setShowSearchResults] = useState(false);
  const searchRef = useRef(null);
  const navigate = useNavigate();
  const { logout } = useAuth();

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (searchRef.current && !searchRef.current.contains(e.target)) setShowSearchResults(false);
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const performSearch = async (value) => {
    if (value.trim().length < 2) {
      setSearchResults([]);
      setIsSearching(false);
      return;
    }
    setIsSearching(true);
    try {
      const [assetRes, employeeRes] = await Promise.all([
        apiService.get(`/Asset/search?keyword=${encodeURIComponent(value)}`),
        apiService.get(`/Employee?search=${encodeURIComponent(value)}`).catch(() => ({ data: { data: [] } })),
      ]);
      const assets = (assetRes.data?.data || []).slice(0, 3).map((item) => ({ type: "asset", id: item.assetId, title: item.assetName, code: item.assetCode }));
      const employees = (employeeRes.data?.data || []).slice(0, 2).map((item) => ({ type: "employee", id: item.employeeId, title: item.fullName, code: item.employeeCode }));
      setSearchResults([...assets, ...employees]);
    } catch {
      setSearchResults([]);
    } finally {
      setIsSearching(false);
    }
  };

  const debouncedSearch = useCallback(utilsHelper.debounce(performSearch, 300), []);

  const handleSearchInput = (e) => {
    const value = e.target.value;
    setSearchValue(value);
    setShowSearchResults(true);
    debouncedSearch(value);
  };

  const handleClearSearch = () => {
    setSearchValue("");
    setSearchResults([]);
    setShowSearchResults(false);
  };

  const handleSearchSelect = (result) => {
    setSearchValue("");
    setSearchResults([]);
    setShowSearchResults(false);
    if (result.type === "asset") navigate(`/assets?search=${result.code}`);
    else if (result.type === "employee") navigate(`/employees?search=${result.code}`);
  };

  const handleLogout = async () => {
    const confirmed = await ConfirmDialog.show({ title: "Logout", text: "Are you sure you want to logout?", icon: "question", confirmButtonText: "Yes, logout", confirmButtonColor: "#dc2626" });
    if (confirmed) {
      setShowUserMenu(false);
      await logout();
      navigate("/login");
    }
  };

  const unreadCount = notifications.filter((n) => !n.read).length;
  const userInitial = user?.fullName?.charAt(0)?.toUpperCase() || "U";

  return (
    <header className="topbar">
      <div className="topbar__left"><h1 className="topbar__title">{title}</h1></div>
      <div className="topbar__center">
        <div className="topbar__search" ref={searchRef}>
          <FiSearch className="topbar__search-icon" />
          <input type="text" className="topbar__search-input" placeholder="Search assets, employees..." value={searchValue} onChange={handleSearchInput} onFocus={() => setShowSearchResults(true)} />
          {searchValue && <button className="topbar__search-clear" onClick={handleClearSearch}><FiX size={16} /></button>}
          {showSearchResults && (searchResults.length > 0 || isSearching || searchValue.length >= 2) && (
            <div className="topbar__search-results">
              {isSearching ? <div className="topbar__search-loading"><div className="spinner" /></div> : searchResults.length > 0 ? searchResults.map((result, i) => (
                <div key={`${result.type}-${result.id}-${i}`} className="topbar__search-result" onClick={() => handleSearchSelect(result)}>
                  <span className="topbar__search-result-type">{result.type}</span>
                  <span className="topbar__search-result-title">{result.title}</span>
                  <span className="topbar__search-result-code">{result.code}</span>
                </div>
              )) : searchValue.length >= 2 && <div className="topbar__search-empty">No results found</div>}
            </div>
          )}
        </div>
      </div>
      <div className="topbar__right">
        <button className="topbar__theme-toggle" onClick={onThemeToggle}>{theme === "light" ? <FiMoon size={20} /> : <FiSun size={20} />}</button>
        <div className="topbar__notifications">
          <button className="topbar__notification-btn" onClick={() => setShowNotifications(!showNotifications)}><FiBell size={20} />{unreadCount > 0 && <span className="topbar__notification-badge">{unreadCount}</span>}</button>
          {showNotifications && (
            <div className="topbar__notification-dropdown">
              <div className="topbar__notification-header"><h4>Notifications</h4><button className="topbar__notification-mark-all">Mark all read</button></div>
              <div className="topbar__notification-list">
                {notifications.length === 0 ? <p className="topbar__notification-empty">No notifications</p> : notifications.map((n) => (
                  <div key={n.id} className={`topbar__notification-item ${!n.read ? "topbar__notification-item--unread" : ""}`}>
                    <span className="topbar__notification-icon">{n.type === "success" ? <FiCheckCircle /> : n.type === "warning" ? <FiAlertCircle /> : n.type === "error" ? <FiAlertCircle /> : <FiInfo />}</span>
                    <div className="topbar__notification-content"><p className="topbar__notification-title">{n.title}</p><p className="topbar__notification-time">{n.time}</p></div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
        <div className="topbar__user">
          <button className="topbar__user-btn" onClick={() => setShowUserMenu(!showUserMenu)}>
            <span className="topbar__user-avatar">{userInitial}</span><span className="topbar__user-name">{user?.fullName || "User"}</span><FiChevronDown size={16} className="topbar__user-arrow" />
          </button>
          {showUserMenu && (
            <div className="topbar__user-dropdown">
              <div className="topbar__user-header"><span className="topbar__user-avatar--large">{userInitial}</span><div><p className="topbar__user-fullname">{user?.fullName || "User"}</p><p className="topbar__user-email">{user?.email || ""}</p></div></div>
              <ul className="topbar__user-menu">
                <li><button onClick={() => { setShowUserMenu(false); navigate("/profile"); }}><FiUser size={18} /> Profile</button></li>
                <li className="topbar__user-menu-divider" />
                <li><button onClick={handleLogout}><FiLogOut size={18} /> Logout</button></li>
              </ul>
            </div>
          )}
        </div>
      </div>
    </header>
  );
};

export default Topbar;