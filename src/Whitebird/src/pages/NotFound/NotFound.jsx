import React from 'react';
import Button from '../../components/atoms/Button/Button';
import './NotFound.scss';

const NotFound = () => {
  const handleGoHome = () => {
    window.location.href = '/dashboard';
  };

  return (
    <div className="not-found">
      <div className="not-found__content">
        <h1 className="not-found__title">404</h1>
        <p className="not-found__message">Page not found</p>
        <p className="not-found__description">
          The page you are looking for doesn't exist or has been moved.
        </p>
        <Button variant="primary" onClick={handleGoHome}>
          Go to Dashboard
        </Button>
      </div>
    </div>
  );
};

export default NotFound;