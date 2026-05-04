import React, { useState, useRef, useEffect } from "react";
import { FiSearch, FiSun, FiMoon, FiChevronDown, FiX, FiUser, FiLogOut } from "react-icons/fi";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../../../context/AuthContext";
import { useUIStore } from "../../../stores/uiStore";
import ConfirmDialog from "../../molecules/ConfirmDialog/ConfirmDialog";
import apiService from "../../../core/services/api.service";
import { useDebounce } from "../../../hooks/useDebounce";
import "./Topbar.scss";

const fetchSearchResults = async (keyword) => {
  if (!keyword || keyword.trim().length < 2) return [];
  const [assetRes, employeeRes] = await Promise.all([
    apiService.get(`/Asset/search?keyword=${encodeURIComponent(keyword)}`),
    apiService.get(`/Employee/grid?search=${encodeURIComponent(keyword)}&pageSize=2`)
      .catch(() => apiService.get(`/Employee?search=${encodeURIComponent(keyword)}`))
      .catch(() => ({ data: { data: [] } })),
  ]);
  const assets = (assetRes.data?.data || []).slice(0, 3).map(item => ({
    type: "asset", id: item.assetId, title: item.assetName, code: item.assetCode,
  }));
  const employees = (employeeRes.data?.data || []).slice(0, 2).map(item => ({
    type: "employee", id: item.employeeId, title: item.fullName, code: item.employeeCode,
  }));
  return [...assets, ...employees];
};

const Topbar = ({ user = null }) => {
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [searchValue, setSearchValue] = useState("");
  const [showSearchResults, setShowSearchResults] = useState(false);
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const searchRef = useRef(null);
  const userMenuRef = useRef(null);
  const navigate = useNavigate();
  const { logout } = useAuth();
  const { theme, toggleTheme } = useUIStore();

  const { data: searchResults = [], isLoading: isSearching } = useQuery({
    queryKey: ['search', debouncedSearch],
    queryFn: () => fetchSearchResults(debouncedSearch),
    enabled: debouncedSearch.length >= 2,
    staleTime: 30000,
    placeholderData: (prev) => prev,
  });

  const debouncedSetSearch = useDebounce((value) => setDebouncedSearch(value), 300);

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (searchRef.current && !searchRef.current.contains(e.target)) setShowSearchResults(false);
      if (userMenuRef.current && !userMenuRef.current.contains(e.target)) setShowUserMenu(false);
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleSearchInput = (e) => {
    const v = e.target.value;
    setSearchValue(v);
    setShowSearchResults(true);
    debouncedSetSearch(v);
  };

  const handleClearSearch = () => {
    setSearchValue("");
    setDebouncedSearch("");
    setShowSearchResults(false);
  };

  const handleSearchSelect = (r) => {
    setSearchValue("");
    setDebouncedSearch("");
    setShowSearchResults(false);
    if (r.type === "asset") navigate(`/assets?search=${r.code}`);
    else if (r.type === "employee") navigate(`/employees?search=${r.code}`);
  };

  const handleLogout = async () => {
    const confirmed = await ConfirmDialog.show({
      title: "Logout",
      text: "Are you sure you want to logout?",
      icon: "question",
      confirmButtonText: "Logout",
      confirmButtonColor: "#dc2626",
    });
    if (confirmed) {
      setShowUserMenu(false);
      await logout();
      navigate("/login");
    }
  };

  const userInitial = user?.fullName?.charAt(0)?.toUpperCase() || "U";

  return (
    <header className="topbar">
      {/* ISSUE #2: Hapus topbar__title — page-title di MainLayout sudah menampilkan judul */}
      <div className="topbar__left">
        {/* <div className="topbar__brand">
          <span className="topbar__brand-icon">W</span>
          <span className="topbar__brand-text">Whitebird</span>
        </div> */}
      </div>

      <div className="topbar__center">
        <div className="topbar__search" ref={searchRef}>
          <FiSearch className="topbar__search-icon" aria-hidden="true" />
          <input
            type="text"
            className="topbar__search-input"
            placeholder="Search assets, employees..."
            value={searchValue}
            onChange={handleSearchInput}
            onFocus={() => setShowSearchResults(true)}
            aria-label="Search assets and employees"
          />
          {searchValue && (
            <button className="topbar__search-clear" onClick={handleClearSearch} aria-label="Clear search">
              <FiX size={16} />
            </button>
          )}
          {showSearchResults && (searchResults.length > 0 || isSearching || searchValue.length >= 2) && (
            <div className="topbar__search-results">
              {isSearching ? (
                <div className="topbar__search-loading"><div className="global-spinner" /></div>
              ) : searchResults.length > 0 ? (
                searchResults.map((r, i) => (
                  <div key={`${r.type}-${r.id}-${i}`} className="topbar__search-result" onClick={() => handleSearchSelect(r)}>
                    <span className="topbar__search-result-type">{r.type}</span>
                    <span className="topbar__search-result-title">{r.title}</span>
                    <span className="topbar__search-result-code">{r.code}</span>
                  </div>
                ))
              ) : searchValue.length >= 2 && (
                <div className="topbar__search-empty">No results found</div>
              )}
            </div>
          )}
        </div>
      </div>

      <div className="topbar__right">
        <button
          className="topbar__theme-toggle"
          onClick={toggleTheme}
          aria-label={`Switch to ${theme === "light" ? "dark" : "light"} mode`}
        >
          {theme === "light" ? <FiMoon size={20} /> : <FiSun size={20} />}
        </button>

        <div className="topbar__user" ref={userMenuRef}>
          <button
            className="topbar__user-btn"
            onClick={() => setShowUserMenu(!showUserMenu)}
            aria-label="User menu"
            aria-expanded={showUserMenu}
          >
            <span className="topbar__user-avatar">{userInitial}</span>
            <span className="topbar__user-name">{user?.fullName || "User"}</span>
            <FiChevronDown size={16} />
          </button>
          {showUserMenu && (
            <div className="topbar__user-dropdown">
              <div className="topbar__user-header">
                <span className="topbar__user-avatar--large">{userInitial}</span>
                <div>
                  <p className="topbar__user-fullname">{user?.fullName}</p>
                  <p className="topbar__user-email">{user?.email}</p>
                </div>
              </div>
              <ul className="topbar__user-menu">
                <li>
                  <button onClick={() => { setShowUserMenu(false); navigate("/profile"); }}>
                    <FiUser size={18} /> Profile
                  </button>
                </li>
                <li className="topbar__user-menu-divider" />
                <li>
                  <button onClick={handleLogout}>
                    <FiLogOut size={18} /> Logout
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