import React, { memo } from 'react';
import './IconButton.scss';

const IconButton = memo(({
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
});

IconButton.displayName = 'IconButton';
export default IconButton;