import React from 'react';
import './StatCard.scss';

/**
 * Centralized Stat Card component for Dashboard
 * 
 * @param {Object} props
 * @param {React.ComponentType} props.icon - Icon component
 * @param {string} props.label - Stat label
 * @param {string|number} props.value - Stat value
 * @param {string} props.color - Icon color
 * @param {string} props.bgColor - Icon background color
 * @param {Function} props.onClick - Click handler
 * @param {boolean} props.clickable - Is card clickable
 */
const StatCard = ({ 
  icon: Icon, 
  label, 
  value, 
  color, 
  bgColor, 
  onClick, 
  clickable = false,
  className = ''
}) => {
  const classNames = [
    'stat-card',
    clickable ? 'stat-card--clickable' : '',
    className
  ].filter(Boolean).join(' ');

  return (
    <div className={classNames} onClick={onClick}>
      <div className="stat-card__icon" style={{ backgroundColor: bgColor, color }}>
        <Icon size={24} />
      </div>
      <div className="stat-card__info">
        <h3 className="stat-card__label">{label}</h3>
        <p className="stat-card__value">{value}</p>
      </div>
    </div>
  );
};

export default StatCard;