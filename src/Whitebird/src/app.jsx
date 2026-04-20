import React, { useEffect } from 'react';
import { AuthProvider } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import router from './core/router/router';
import MainLayout from './layouts/MainLayout/MainLayout';
import './app.scss';

// Lazy load pages
const Login = React.lazy(() => import('./pages/Login/Login'));
const Dashboard = React.lazy(() => import('./pages/Dashboard/Dashboard'));
const Assets = React.lazy(() => import('./pages/Assets/Assets'));
const NotFound = React.lazy(() => import('./pages/NotFound/NotFound'));

const App = () => {
  useEffect(() => {
    // Setup router
    router.setLayout(() => MainLayout);
    
    router
      .register('/', () => Dashboard, { title: 'Dashboard', requiresAuth: true })
      .register('/dashboard', () => Dashboard, { title: 'Dashboard', requiresAuth: true })
      .register('/assets', () => Assets, { title: 'Assets', requiresAuth: true })
      .register('/transactions', () => NotFound, { title: 'Transactions', requiresAuth: true })
      .register('/employees', () => NotFound, { title: 'Employees', requiresAuth: true })
      .register('/categories', () => NotFound, { title: 'Categories', requiresAuth: true })
      .register('/suppliers', () => NotFound, { title: 'Suppliers', requiresAuth: true })
      .register('/locations', () => NotFound, { title: 'Locations', requiresAuth: true })
      .register('/reports', () => NotFound, { title: 'Reports', requiresAuth: true })
      .register('/profile', () => NotFound, { title: 'Profile', requiresAuth: true })
      .register('/login', () => Login, { title: 'Login', layout: 'none' })
      .register('/404', () => NotFound, { title: 'Not Found' });

    // Auth guard
    router.guard((route) => {
      if (route.requiresAuth) {
        const token = localStorage.getItem('whitebird_session_token');
        if (!token) {
          return '/login';
        }
      }
      
      if (route.path === '/login') {
        const token = localStorage.getItem('whitebird_session_token');
        if (token) {
          return '/dashboard';
        }
      }
      
      return true;
    });

    // Start router
    router.handleRoute();
  }, []);

  return (
    <ThemeProvider>
      <AuthProvider>
        <div className="app">
          <React.Suspense fallback={
            <div className="app__loading">
              <div className="spinner" />
            </div>
          }>
            <div id="app" />
          </React.Suspense>
        </div>
      </AuthProvider>
    </ThemeProvider>
  );
};

export default App;