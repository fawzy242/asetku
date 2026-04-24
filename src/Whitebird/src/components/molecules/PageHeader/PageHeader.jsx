import React from "react";
import Button from "../../atoms/Button/Button";
import "./PageHeader.scss";

const PageHeader = ({ title, buttonText, onButtonClick, buttonIcon, className = "" }) => (
  <div className={`page-header ${className}`}>
    <h1 className="page-title">{title}</h1>
    {buttonText && <Button variant="primary" onClick={onButtonClick} startIcon={buttonIcon}>{buttonText}</Button>}
  </div>
);

export default PageHeader;