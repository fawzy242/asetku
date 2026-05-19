import React from 'react';
import './IconButton.scss';

/**
 * Reusable icon button untuk table actions dan toolbar
 * @param {Object} props
 * @param {React.ReactNode} props.children - Icon element
 * @param {Function} props.onClick - Click handler
 * @param {string} [props.variant] - 'default' | 'danger' | 'success' | 'warning'
 * @param {string} [props.size] - 'sm' | 'md' | 'lg'
 * @param {string} [props.title] - Tooltip text (also used as aria-label)
 * @param {string} [props.className] - Additional CSS class
 * @param {boolean} [props.disabled] - Disabled state
 * @param {string} [props.type] - Button type
 */
const IconButton = ({
  children,
  onClick,
  variant = 'default',
  size = 'md',
  title = '',
  className = '',
  disabled = false,
  type = 'button'
}) => {
  const classes = ['icon-btn', `icon-btn--${variant}`, `icon-btn--${size}`, className]
    .filter(Boolean)
    .join(' ');

  return (
    <button
      className={classes}
      onClick={onClick}
      title={title}
      aria-label={title || 'Action button'}
      type={type}
      disabled={disabled}
    >
      {children}
    </button>
  );
};

export default IconButton;