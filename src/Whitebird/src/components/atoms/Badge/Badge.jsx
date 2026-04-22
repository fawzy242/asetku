import React from "react";
import "./Badge.scss";

const Badge = ({ children, variant = "secondary", size = "md", className = "" }) => {
  const classNames = ["badge", `badge--${variant}`, `badge--${size}`, className]
    .filter(Boolean)
    .join(" ");

  return <span className={classNames}>{children}</span>;
};

export default Badge;