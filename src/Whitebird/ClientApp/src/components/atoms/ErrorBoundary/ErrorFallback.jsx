import React from 'react';
import { FiAlertTriangle, FiRefreshCw, FiHome } from 'react-icons/fi';
import Button from '../Button/Button';
import './ErrorFallback.scss';

/**
 * Fallback UI untuk ErrorBoundary
 * Menampilkan pesan error dan opsi recovery
 */
const ErrorFallback = ({ error, errorInfo, onReset, onReload }) => {
  const isDev = import.meta.env.DEV;

  return (
    <div className="error-fallback">
      <div className="error-fallback__content">
        <div className="error-fallback__icon">
          <FiAlertTriangle size={48} />
        </div>
        <h1 className="error-fallback__title">Something went wrong</h1>
        <p className="error-fallback__message">
          An unexpected error occurred. Please try again or contact support if the problem persists.
        </p>

        {isDev && error && (
          <div className="error-fallback__details">
            <p className="error-fallback__error-name">{error.name}: {error.message}</p>
            {errorInfo && (
              <pre className="error-fallback__stack">
                {errorInfo.componentStack}
              </pre>
            )}
          </div>
        )}

        <div className="error-fallback__actions">
          <Button variant="primary" onClick={onReset} startIcon={<FiRefreshCw />}>
            Try Again
          </Button>
          <Button variant="outline" onClick={() => window.location.href = '/dashboard'} startIcon={<FiHome />}>
            Go to Dashboard
          </Button>
          {onReload && (
            <Button variant="text" onClick={onReload}>
              Reload Page
            </Button>
          )}
        </div>
      </div>
    </div>
  );
};

export default ErrorFallback;