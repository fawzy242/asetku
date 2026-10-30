// src/pages/Forbidden/Forbidden.jsx
import React from 'react';
import { useNavigate } from 'react-router-dom';
import Button from '../../components/atoms/Button/Button';
import './Forbidden.scss';

const Forbidden = () => {
  const navigate = useNavigate();

  const handleGoHome = () => {
    navigate('/dashboard');
  };

  return (
    <div className="forbidden">
      <div className="forbidden__content">
        <h1 className="forbidden__title">403</h1>
        <p className="forbidden__message">Access denied</p>
        <p className="forbidden__description">
          You do not have permission to access this page. If you believe this
          is a mistake, please contact your system administrator.
        </p>
        <Button variant="primary" onClick={handleGoHome}>
          Go to Dashboard
        </Button>
      </div>
    </div>
  );
};

export default Forbidden;