import React, { useState, useEffect } from "react";
import { Outlet, useLocation, useNavigate } from "react-router-dom";
import Sidebar from "../../components/organisms/Sidebar/Sidebar";
import Topbar from "../../components/organisms/Topbar/Topbar";
import Footer from "../../components/organisms/Footer/Footer";
import { useTheme } from "../../context/ThemeContext";
import { useAuth } from "../../context/AuthContext";
import "./MainLayout.scss";

const pageTitles = {
  "/dashboard": "Dashboard",
  "/assets": "Asset Management",
  "/transactions": "Asset Transactions",
  "/employees": "Employee Management",
  "/categories": "Category Management",
  "/suppliers": "Supplier Management",
  "/locations": "Location Management",
  "/reports": "Reports & Analytics",
  "/profile": "My Profile",
  "/settings": "Settings",
};

const MainLayout = () => {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(() => {
    const saved = localStorage.getItem("sidebar-collapsed");
    return saved === "true";
  });
  const location = useLocation();
  const navigate = useNavigate();
  const { theme, toggleTheme } = useTheme();
  const { user } = useAuth();

  const currentPath = location.pathname;
  const title = pageTitles[currentPath] || "Dashboard";

  useEffect(() => {
    const sidebarWidth = sidebarCollapsed ? "80" : "260";
    document.documentElement.style.setProperty("--sidebar-width", `${sidebarWidth}px`);
    localStorage.setItem("sidebar-collapsed", sidebarCollapsed);
  }, [sidebarCollapsed]);

  const handleNavigate = (path) => {
    navigate(path);
  };

  return (
    <div className="main-layout" data-theme={theme}>
      <Sidebar
        collapsed={sidebarCollapsed}
        onToggle={() => setSidebarCollapsed(!sidebarCollapsed)}
        onNavigate={handleNavigate}
      />

      <div
        className={`main-layout__content ${
          sidebarCollapsed ? "main-layout__content--expanded" : ""
        }`}
      >
        <Topbar
          title={title}
          user={user}
          onThemeToggle={toggleTheme}
          theme={theme}
        />

        <main className="main-layout__main">
          <Outlet />
        </main>

        <Footer />
      </div>
    </div>
  );
};

export default MainLayout;