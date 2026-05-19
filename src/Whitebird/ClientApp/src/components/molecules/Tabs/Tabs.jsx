import React, { useState, useEffect } from "react";
import "./Tabs.scss";

const Tabs = ({ tabs = [], activeTab, onTabChange, className = "" }) => {
  const [content, setContent] = useState(null);

  useEffect(() => {
    // Smooth transition effect
    setContent(null);
    const timer = setTimeout(() => setContent(activeTab), 50);
    return () => clearTimeout(timer);
  }, [activeTab]);

  return (
    <div className={`tabs ${className}`}>
      {tabs.map(tab => (
        <button
          key={tab.id}
          className={`tabs__tab ${activeTab === tab.id ? "active" : ""}`}
          onClick={() => onTabChange(tab.id)}
        >
          {tab.icon && <span className="tabs__tab-icon">{tab.icon}</span>}
          <span>{tab.label}</span>
        </button>
      ))}
    </div>
  );
};

export default Tabs;