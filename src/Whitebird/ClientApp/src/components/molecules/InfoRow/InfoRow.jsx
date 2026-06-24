import React from 'react';
import './InfoRow.scss';

/**
 * Centralized Info Row component for profile displays
 * 
 * @param {Object} props
 * @param {React.ComponentType} props.icon - Icon component
 * @param {string} props.label - Label text
 * @param {string|number} props.value - Value text
 * @param {string} props.className - Additional class
 */
const InfoRow = ({ icon: Icon, label, value, className = '' }) => {
  return (
    <div className={`info-row ${className}`}>
      {Icon && <Icon size={16} className="info-row__icon" />}
      <span className="info-row__label">{label}:</span>
      <span className="info-row__value">{value || '-'}</span>
    </div>
  );
};

export default InfoRow;