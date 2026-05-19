import React from 'react';
import './Skeleton.scss';

/**
 * Lightweight skeleton loader for inline content
 * @param {Object} props
 * @param {string} [props.variant] - 'text' | 'circle' | 'rect'
 * @param {string|number} [props.width] - Width
 * @param {string|number} [props.height] - Height
 * @param {string} [props.className] - Additional CSS class
 */
const Skeleton = ({ variant = 'text', width, height, className = '' }) => {
  const style = {
    width: width || (variant === 'circle' ? '40px' : '100%'),
    height: height || (variant === 'text' ? '16px' : variant === 'circle' ? '40px' : '100px'),
  };

  return (
    <div
      className={`skeleton skeleton--${variant} ${className}`}
      style={style}
      aria-hidden="true"
      role="presentation"
    />
  );
};

export default Skeleton;