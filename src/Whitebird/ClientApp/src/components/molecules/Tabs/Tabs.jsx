import React from 'react';
import './Tabs.scss';

const Tabs = ({ tabs = [], activeTab, onTabChange, className = '' }) => {
  return (
    <div className={`tabs ${className}`}>
      {tabs.map(tab => (
        <button
          key={tab.id}
          className={`tabs__tab ${activeTab === tab.id ? 'active' : ''}`}
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