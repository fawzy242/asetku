import React from "react";
import "./Spinner.scss";

const Spinner = ({ size = "md", variant = "primary", className = "" }) => {
  const classNames = ["spinner", `spinner--${size}`, `spinner--${variant}`, className]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={classNames}>
      <div className="spinner__circle" />
    </div>
  );
};

export default Spinner;