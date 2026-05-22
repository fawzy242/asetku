import React from "react";
import { FiSun, FiMoon } from "react-icons/fi";
import { useTheme } from "../../context/ThemeContext";
import "./AuthLayout.scss";

const AuthLayout = ({ children, title = "Welcome Back" }) => {
  const { theme, toggleTheme } = useTheme();

  return (
    <div className="auth-layout" data-theme={theme}>
      <div className="auth-layout__theme-toggle">
        <button onClick={toggleTheme} aria-label="Toggle theme">
          {theme === "light" ? <FiMoon size={20} /> : <FiSun size={20} />}
        </button>
      </div>

      <div className="auth-layout__container">
        <div className="auth-layout__card">
          <div className="auth-layout__logo">
            <img 
              src="/logo.png" 
              alt="AsetKu Logo" 
              className="auth-layout__logo-img"
              onError={(e) => {
                e.target.onerror = null;
                e.target.style.display = 'none';
                e.target.nextSibling.style.display = 'flex';
              }}
            />
            <div className="auth-layout__logo-fallback" style={{ display: 'none' }}>
              <span>A</span>
            </div>
            <h1 className="auth-layout__logo-text">AsetKu</h1>
          </div>
          <h2 className="auth-layout__title">{title}</h2>
          {children}
        </div>
      </div>

      <div className="auth-layout__footer">
        <p>&copy; {new Date().getFullYear()} AsetKu Asset Management System. All rights reserved.</p>
      </div>
    </div>
  );
};

export default AuthLayout;