import React from "react";
import "./Footer.scss";

const Footer = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="footer" role="contentinfo">
      <div className="footer__content">
        <div className="footer__copyright">
          &copy; {currentYear} Whitebird Asset Management System. All rights reserved.
        </div>
        <nav className="footer__links" aria-label="Footer navigation">
          <button className="footer__link" onClick={() => {}} aria-label="Privacy policy (coming soon)">Privacy</button>
          <button className="footer__link" onClick={() => {}} aria-label="Terms of service (coming soon)">Terms</button>
          <button className="footer__link" onClick={() => {}} aria-label="Help center (coming soon)">Help</button>
        </nav>
      </div>
    </footer>
  );
};

export default Footer;