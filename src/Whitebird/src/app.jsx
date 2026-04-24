import React from 'react';
import { BrowserRouter, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import { NotificationProvider } from './context/NotificationContext';
import './app.scss';

import MainLayout from './layouts/MainLayout/MainLayout';
import AuthLayout from './layouts/AuthLayout/AuthLayout';

import Login from './pages/Login/Login';
import Dashboard from './pages/Dashboard/Dashboard';
import Assets from './pages/Assets/Assets';
import AssetTransactions from './pages/AssetTransactions/AssetTransactions';
import AssetTracking from './pages/AssetTracking/AssetTracking';
import Employees from './pages/Employees/Employees';
import EmployeeSummary from './pages/EmployeeSummary/EmployeeSummary';
import Categories from './pages/Categories/Categories';
import Suppliers from './pages/Suppliers/Suppliers';
import Locations from './pages/Locations/Locations';
import Reports from './pages/Reports/Reports';
import Profile from './pages/Profile/Profile';
import NotFound from './pages/NotFound/NotFound';

const ProtectedRoute = ({ children }) => {
  const { isAuthenticated, loading } = useAuth();
  const location = useLocation();
  if (loading) return <div className="app__loading"><div className="spinner" /></div>;
  if (!isAuthenticated) return <Navigate to="/login" state={{ from: location }} replace />;
  return children;
};

const PublicRoute = ({ children }) => {
  const { isAuthenticated, loading } = useAuth();
  if (loading) return <div className="app__loading"><div className="spinner" /></div>;
  if (isAuthenticated) return <Navigate to="/dashboard" replace />;
  return children;
};

const AppRoutes = () => (
  <Routes>
    <Route path="/login" element={<PublicRoute><AuthLayout title="Sign in to your account"><Login /></AuthLayout></PublicRoute>} />
    <Route path="/" element={<ProtectedRoute><MainLayout /></ProtectedRoute>}>
      <Route index element={<Navigate to="/dashboard" replace />} />
      <Route path="dashboard" element={<Dashboard />} />
      <Route path="assets" element={<Assets />} />
      <Route path="transactions" element={<AssetTransactions />} />
      <Route path="tracking" element={<AssetTracking />} />
      <Route path="employees" element={<Employees />} />
      <Route path="employee-summary" element={<EmployeeSummary />} />
      <Route path="categories" element={<Categories />} />
      <Route path="suppliers" element={<Suppliers />} />
      <Route path="locations" element={<Locations />} />
      <Route path="reports" element={<Reports />} />
      <Route path="profile" element={<Profile />} />
    </Route>
    <Route path="/404" element={<NotFound />} />
    <Route path="*" element={<Navigate to="/404" replace />} />
  </Routes>
);

const App = () => (
  <ThemeProvider>
    <NotificationProvider>
      <AuthProvider>
        <BrowserRouter>
          <div className="app">
            <React.Suspense fallback={<div className="app__loading"><div className="spinner" /></div>}>
              <AppRoutes />
            </React.Suspense>
          </div>
        </BrowserRouter>
      </AuthProvider>
    </NotificationProvider>
  </ThemeProvider>
);

export default App;