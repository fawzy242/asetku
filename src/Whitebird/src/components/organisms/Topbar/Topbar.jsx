import React, { useState, useRef, useEffect } from "react";
import { FiSearch, FiSun, FiMoon, FiChevronDown, FiX, FiUser, FiLogOut } from "react-icons/fi";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../../context/AuthContext";
import ConfirmDialog from "../../molecules/ConfirmDialog/ConfirmDialog";
import apiService from "../../../core/services/api.service";
import { useDebounce } from "../../../hooks/useDebounce";
import "./Topbar.scss";

const Topbar = ({ title = "Dashboard", user = null, onThemeToggle, theme = "light" }) => {
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [searchValue, setSearchValue] = useState("");
  const [isSearching, setIsSearching] = useState(false);
  const [searchResults, setSearchResults] = useState([]);
  const [showSearchResults, setShowSearchResults] = useState(false);
  const searchRef = useRef(null);
  const userMenuRef = useRef(null);
  const navigate = useNavigate();
  const { logout } = useAuth();

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (searchRef.current && !searchRef.current.contains(e.target)) setShowSearchResults(false);
      if (userMenuRef.current && !userMenuRef.current.contains(e.target)) setShowUserMenu(false);
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const performSearch = async (value) => {
    if (value.trim().length < 2) { setSearchResults([]); setIsSearching(false); return; }
    setIsSearching(true);
    try {
      const [assetRes, employeeRes] = await Promise.all([
        apiService.get(`/Asset/search?keyword=${encodeURIComponent(value)}`),
        apiService.get(`/Employee?search=${encodeURIComponent(value)}`).catch(() => ({ data: { data: [] } }))
      ]);
      const assets = (assetRes.data?.data || []).slice(0, 3).map(item => ({ type: "asset", id: item.assetId, title: item.assetName, code: item.assetCode }));
      const employees = (employeeRes.data?.data || []).slice(0, 2).map(item => ({ type: "employee", id: item.employeeId, title: item.fullName, code: item.employeeCode }));
      setSearchResults([...assets, ...employees]);
    } catch { setSearchResults([]); }
    finally { setIsSearching(false); }
  };

  const debouncedSearch = useDebounce(performSearch, 300);

  const handleSearchInput = (e) => { const v = e.target.value; setSearchValue(v); setShowSearchResults(true); debouncedSearch(v); };
  const handleClearSearch = () => { setSearchValue(""); setSearchResults([]); setShowSearchResults(false); };
  const handleSearchSelect = (r) => { setSearchValue(""); setSearchResults([]); setShowSearchResults(false); if (r.type === "asset") navigate(`/assets?search=${r.code}`); else if (r.type === "employee") navigate(`/employees?search=${r.code}`); };

  const handleLogout = async () => {
    const confirmed = await ConfirmDialog.show({ title: "Logout", text: "Are you sure?", icon: "question", confirmButtonText: "Logout", confirmButtonColor: "#dc2626" });
    if (confirmed) { setShowUserMenu(false); await logout(); navigate("/login"); }
  };

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
              {isSearching ? <div className="topbar__search-loading"><div className="spinner" /></div> : searchResults.length > 0 ? searchResults.map((r, i) => (
                <div key={`${r.type}-${r.id}-${i}`} className="topbar__search-result" onClick={() => handleSearchSelect(r)}><span className="topbar__search-result-type">{r.type}</span><span>{r.title}</span><span>{r.code}</span></div>
              )) : searchValue.length >= 2 && <div className="topbar__search-empty">No results</div>}
            </div>
          )}
        </div>
      </div>
      <div className="topbar__right">
        <button className="topbar__theme-toggle" onClick={onThemeToggle}>{theme === "light" ? <FiMoon size={20} /> : <FiSun size={20} />}</button>
        <div className="topbar__user" ref={userMenuRef}>
          <button className="topbar__user-btn" onClick={() => setShowUserMenu(!showUserMenu)}><span className="topbar__user-avatar">{userInitial}</span><span className="topbar__user-name">{user?.fullName || "User"}</span><FiChevronDown size={16} /></button>
          {showUserMenu && (
            <div className="topbar__user-dropdown">
              <div className="topbar__user-header"><span className="topbar__user-avatar--large">{userInitial}</span><div><p className="topbar__user-fullname">{user?.fullName}</p><p className="topbar__user-email">{user?.email}</p></div></div>
              <ul className="topbar__user-menu">
                <li><button onClick={() => { setShowUserMenu(false); navigate("/profile"); }}><FiUser /> Profile</button></li>
                <li className="topbar__user-menu-divider" />
                <li><button onClick={handleLogout}><FiLogOut /> Logout</button></li>
              </ul>
            </div>
          )}
        </div>
      </div>
    </header>
  );
};

export default Topbar;