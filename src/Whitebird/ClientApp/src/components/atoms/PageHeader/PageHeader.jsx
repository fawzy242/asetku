import React from 'react';
import './PageHeader.scss';

/**
 * Page-level header. Single-purpose presentational component.
 * Slots:
 *  - title (string)
 *  - description (string, optional)
 *  - actions (ReactNode, right side — Add / Import / custom buttons)
 *  - filters (ReactNode, optional — a second row of controls)
 */
const PageHeader = ({
  title,
  description,
  actions = null,
  filters = null,
  className = '',
}) => {
  return (
    <div className={`page-header-wrapper ${className}`}>
      <div className="page-header">
        <div className="page-header__text">
          {title && <h1 className="page-title">{title}</h1>}
          {description && <p className="page-description">{description}</p>}
        </div>
        {actions && <div className="page-header__actions">{actions}</div>}
      </div>
      {filters && <div className="page-header__filters">{filters}</div>}
    </div>
  );
};

export default PageHeader;