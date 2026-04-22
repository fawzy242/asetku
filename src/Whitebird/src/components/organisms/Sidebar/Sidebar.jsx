import React, { useState, useEffect } from "react";
import {
  FiHome,
  FiBox,
  FiRefreshCw,
  FiUsers,
  FiFolder,
  FiBarChart2,
  FiChevronDown,
  FiChevronLeft,
  FiGrid,
  FiTruck,
  FiMapPin,
  FiMenu,
} from "react-icons/fi";
import { useNavigate, useLocation } from "react-router-dom";
import "./Sidebar.scss";

const menuItems = [
  { id: "dashboard", label: "Dashboard", icon: FiHome, path: "/dashboard" },
  { id: "assets", label: "Assets", icon: FiBox, path: "/assets" },
  {
    id: "transactions",
    label: "Transactions",
    icon: FiRefreshCw,
    path: "/transactions",
  },
  { id: "employees", label: "Employees", icon: FiUsers, path: "/employees" },
  {
    id: "master-data",
    label: "Master Data",
    icon: FiFolder,
    path: null,
    children: [
      {
        id: "categories",
        label: "Categories",
        icon: FiGrid,
        path: "/categories",
      },
      {
        id: "suppliers",
        label: "Suppliers",
        icon: FiTruck,
        path: "/suppliers",
      },
      {
        id: "locations",
        label: "Locations",
        icon: FiMapPin,
        path: "/locations",
      },
    ],
  },
  { id: "reports", label: "Reports", icon: FiBarChart2, path: "/reports" },
];

const Sidebar = ({
  menuItems: customMenuItems,
  collapsed = false,
  onToggle,
}) => {
  const items = customMenuItems || menuItems;
  const [expandedItems, setExpandedItems] = useState(new Set());
  const navigate = useNavigate();
  const location = useLocation();
  const activePath = location.pathname;

  useEffect(() => {
    items.forEach((item) => {
      if (item.children) {
        const hasActiveChild = item.children.some(
          (child) => child.path === activePath
        );
        if (hasActiveChild) {
          setExpandedItems((prev) => new Set([...prev, item.id]));
        }
      }
    });
  }, [activePath, items]);

  const toggleExpand = (itemId) => {
    setExpandedItems((prev) => {
      const next = new Set(prev);
      if (next.has(itemId)) {
        next.delete(itemId);
      } else {
        next.add(itemId);
      }
      return next;
    });
  };

  const handleNavigate = (path) => {
    if (path) navigate(path);
  };

  const renderMenuItem = (item, level = 0) => {
    const hasChildren = item.children && item.children.length > 0;
    const isExpanded = expandedItems.has(item.id);
    const isActive =
      item.path === activePath ||
      (item.children &&
        item.children.some((child) => child.path === activePath));

    const IconComponent = item.icon;

    return (
      <li key={item.id} className="sidebar__item">
        <a
          href={item.path || "#"}
          className={`sidebar__link ${
            isActive ? "sidebar__link--active" : ""
          } ${level > 0 ? "sidebar__link--child" : ""}`}
          onClick={(e) => {
            e.preventDefault();
            if (hasChildren) {
              toggleExpand(item.id);
            } else if (item.path) {
              handleNavigate(item.path);
            }
          }}
        >
          <IconComponent className="sidebar__icon" size={level > 0 ? 18 : 20} />
          {!collapsed && (
            <>
              <span className="sidebar__label">{item.label}</span>
              {hasChildren && (
                <FiChevronDown
                  className={`sidebar__arrow ${
                    isExpanded ? "sidebar__arrow--expanded" : ""
                  }`}
                  size={14}
                />
              )}
            </>
          )}
        </a>
        {hasChildren && isExpanded && !collapsed && (
          <ul className="sidebar__submenu">
            {item.children.map((child) => renderMenuItem(child, level + 1))}
          </ul>
        )}
      </li>
    );
  };

  return (
    <aside className={`sidebar ${collapsed ? "sidebar--collapsed" : ""}`}>
      <div className="sidebar__header">
        {!collapsed ? (
          <>
            <div
              className="sidebar__logo"
              onClick={() => navigate("/dashboard")}
              style={{ cursor: "pointer" }}
            >
              <div className="sidebar__logo-icon">W</div>
              <span className="sidebar__logo-text">Whitebird</span>
            </div>
            <button
              className="sidebar__toggle-btn"
              onClick={onToggle}
              aria-label="Collapse sidebar"
            >
              <FiChevronLeft size={20} />
            </button>
          </>
        ) : (
          <button
            className="sidebar__toggle-btn sidebar__toggle-btn--expand"
            onClick={onToggle}
            aria-label="Expand sidebar"
          >
            <FiMenu size={22} />
          </button>
        )}
      </div>

      <nav className="sidebar__nav">
        <ul className="sidebar__menu">
          {items.map((item) => renderMenuItem(item))}
        </ul>
      </nav>

      <div className="sidebar__footer">
        {!collapsed && (
          <div className="sidebar__version">
            <span>v1.0.0</span>
          </div>
        )}
      </div>
    </aside>
  );
};

export default Sidebar;
