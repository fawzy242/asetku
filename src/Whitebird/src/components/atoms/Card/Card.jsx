import React from "react";
import "./Card.scss";

const Card = ({
  children,
  title = null,
  subtitle = null,
  actions = null,
  className = "",
  hoverable = false,
  padding = true,
}) => {
  const classNames = [
    "card",
    hoverable ? "card--hoverable" : "",
    padding ? "" : "card--no-padding",
    className,
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={classNames}>
      {(title || subtitle || actions) && (
        <div className="card__header">
          <div className="card__header-content">
            {title && <h3 className="card__title">{title}</h3>}
            {subtitle && <p className="card__subtitle">{subtitle}</p>}
          </div>
          {actions && <div className="card__actions">{actions}</div>}
        </div>
      )}
      <div className="card__body">{children}</div>
    </div>
  );
};

export default Card;