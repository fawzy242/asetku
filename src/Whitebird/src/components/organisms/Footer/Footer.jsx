import React from "react";
import "./Footer.scss";

const Footer = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="footer">
      <div className="footer__content">
        <div className="footer__copyright">
          &copy; {currentYear} Whitebird Asset Management
        </div>
        <div className="footer__links">
          <a href="#" className="footer__link">Privacy</a>
          <a href="#" className="footer__link">Terms</a>
          <a href="#" className="footer__link">Help</a>
        </div>
      </div>
    </footer>
  );
};

export default Footer;