// src/app.jsx
import React, { Suspense, lazy } from 'react';
import { BrowserRouter, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import { NotificationProvider } from './context/NotificationContext';
import ErrorBoundary from './components/atoms/ErrorBoundary/ErrorBoundary';
import { ConfirmDialogProvider } from './components/molecules/ConfirmDialog/ConfirmDialog';
import './app.scss';

// Layouts
import MainLayout from './layouts/MainLayout/MainLayout';
import AuthLayout from './layouts/AuthLayout/AuthLayout';

// Critical pages
import Login from './pages/Login/Login';
import Dashboard from './pages/Dashboard/Dashboard';

// Lazy loaded pages
const Assets = lazy(() => import('./pages/Assets/Assets'));
const AssetTransactionsMonitoring = lazy(() =>
  import('./pages/AssetTransactions/AssetTransactionsMonitoring')
);
const AssetTransactionsManagement = lazy(() =>
  import('./pages/AssetTransactions/AssetTransactionsManagement')
);
const AssetTracking = lazy(() => import('./pages/AssetTracking/AssetTracking'));
const Employees = lazy(() => import('./pages/Employees/Employees'));
const EmployeeSummary = lazy(() => import('./pages/EmployeeSummary/EmployeeSummary'));
const Categories = lazy(() => import('./pages/Categories/Categories'));
const Suppliers = lazy(() => import('./pages/Suppliers/Suppliers'));
const Departments = lazy(() => import('./pages/Departments/Departments'));
const Offices = lazy(() => import('./pages/Offices/Offices'));
const Reports = lazy(() => import('./pages/Reports/Reports'));
const Profile = lazy(() => import('./pages/Profile/Profile'));
const NotFound = lazy(() => import('./pages/NotFound/NotFound'));

// Schedule menus
const ScheduleHandover = lazy(() => import('./pages/ScheduleHandover/ScheduleHandover'));
const ScheduleReturn = lazy(() => import('./pages/ScheduleReturn/ScheduleReturn'));
const ScheduleMaintenance = lazy(() =>
  import('./pages/ScheduleMaintenance/ScheduleMaintenance')
);

const PageLoader = () => (
  <div className="app__loading" role="status" aria-label="Loading page">
    <div className="global-spinner" />
  </div>
);

const LazyPage = ({ children }) => (
  <ErrorBoundary>
    <Suspense fallback={<PageLoader />}>{children}</Suspense>
  </ErrorBoundary>
);

const ProtectedRoute = ({ children }) => {
  const { isAuthenticated, loading } = useAuth();
  const location = useLocation();
  if (loading) return <PageLoader />;
  if (!isAuthenticated)
    return <Navigate to="/login" state={{ from: location }} replace />;
  return children;
};

const PublicRoute = ({ children }) => {
  const { isAuthenticated, loading } = useAuth();
  if (loading) return <PageLoader />;
  if (isAuthenticated) return <Navigate to="/dashboard" replace />;
  return children;
};

const AppRoutes = () => (
  <Routes>
    <Route
      path="/login"
      element={
        <PublicRoute>
          <AuthLayout title="Sign in to your account">
            <Login />
          </AuthLayout>
        </PublicRoute>
      }
    />
    <Route
      path="/"
      element={
        <ProtectedRoute>
          <MainLayout />
        </ProtectedRoute>
      }
    >
      <Route index element={<Navigate to="/dashboard" replace />} />
      <Route path="dashboard" element={<LazyPage><Dashboard /></LazyPage>} />
      <Route path="assets" element={<LazyPage><Assets /></LazyPage>} />
      <Route
        path="transactions/monitoring"
        element={<LazyPage><AssetTransactionsMonitoring /></LazyPage>}
      />
      <Route
        path="transactions/management"
        element={<LazyPage><AssetTransactionsManagement /></LazyPage>}
      />
      <Route
        path="transactions"
        element={<Navigate to="/transactions/monitoring" replace />}
      />
      <Route path="tracking" element={<LazyPage><AssetTracking /></LazyPage>} />
      <Route path="employees" element={<LazyPage><Employees /></LazyPage>} />
      <Route
        path="employee-summary"
        element={<LazyPage><EmployeeSummary /></LazyPage>}
      />
      <Route path="categories" element={<LazyPage><Categories /></LazyPage>} />
      <Route path="suppliers" element={<LazyPage><Suppliers /></LazyPage>} />
      <Route path="departments" element={<LazyPage><Departments /></LazyPage>} />
      <Route path="offices" element={<LazyPage><Offices /></LazyPage>} />
      <Route path="reports" element={<LazyPage><Reports /></LazyPage>} />
      <Route path="profile" element={<LazyPage><Profile /></LazyPage>} />
      <Route
        path="schedule-handover"
        element={<LazyPage><ScheduleHandover /></LazyPage>}
      />
      <Route
        path="schedule-return"
        element={<LazyPage><ScheduleReturn /></LazyPage>}
      />
      <Route
        path="schedule-maintenance"
        element={<LazyPage><ScheduleMaintenance /></LazyPage>}
      />
    </Route>
    <Route path="/404" element={<LazyPage><NotFound /></LazyPage>} />
    <Route path="*" element={<Navigate to="/404" replace />} />
  </Routes>
);

const App = () => (
  <ErrorBoundary>
    <ThemeProvider>
      <NotificationProvider>
        <AuthProvider>
          <ConfirmDialogProvider>
            <BrowserRouter>
              <div className="app">
                <AppRoutes />
                <Toaster
                  position="bottom-right"
                  toastOptions={{
                    duration: 3000,
                    style: {
                      background: 'var(--card-bg)',
                      color: 'var(--text-primary)',
                      borderRadius: '12px',
                      border: '1px solid var(--border)',
                      padding: '12px 20px',
                      fontSize: '14px',
                      fontWeight: 500,
                    },
                    success: {
                      style: {
                        background: 'var(--success)',
                        color: '#ffffff',
                      },
                    },
                    error: {
                      style: {
                        background: 'var(--error)',
                        color: '#ffffff',
                      },
                    },
                  }}
                />
              </div>
            </BrowserRouter>
          </ConfirmDialogProvider>
        </AuthProvider>
      </NotificationProvider>
    </ThemeProvider>
  </ErrorBoundary>
);

export default App;