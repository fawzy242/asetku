import React from 'react';
import './AuthLayout.scss';

const AuthLayout = ({ children, title = 'Welcome Back' }) => {
  return (
    <div className="auth-layout">
      <div className="auth-layout__container">
        <div className="auth-layout__card">
          <div className="auth-layout__logo">
            <span className="auth-layout__logo-icon">🪽</span>
            <h1 className="auth-layout__logo-text">Whitebird</h1>
          </div>
          <h2 className="auth-layout__title">{title}</h2>
          {children}
        </div>
      </div>
      <div className="auth-layout__footer">
        <p>&copy; 2024 Whitebird Asset Management System. All rights reserved.</p>
      </div>
    </div>
  );
};

export default AuthLayout;