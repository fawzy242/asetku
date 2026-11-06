import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  useRef,
} from 'react';
import { LoginData, UserData } from '../core/services';
import { STORAGE_KEYS } from '../core/constants/storageKeys';
import {
  getStorage,
  setStorage,
  removeStorage,
} from '../core/utils/storage.utils';

const AuthContext = createContext(null);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
};

const loginData = new LoginData();
const userData = new UserData();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const checkingRef = useRef(false);
  const mountedRef = useRef(true);

  const performLogout = useCallback(async () => {
    const token = localStorage.getItem(STORAGE_KEYS.SESSION_TOKEN);
    if (token) {
      try {
        await loginData.logout(token);
      } catch {}
    }
    localStorage.removeItem(STORAGE_KEYS.SESSION_TOKEN);
    removeStorage(STORAGE_KEYS.USER);
    if (mountedRef.current) {
      setUser(null);
      setIsAuthenticated(false);
    }
  }, []);

  const refreshUser = useCallback(async () => {
    const token = localStorage.getItem(STORAGE_KEYS.SESSION_TOKEN);
    if (!token) return false;

    try {
      const result = await userData.getMe();
      if (result.success && result.data) {
        // Backend is the source of truth for profilePhotoUrl (relative,
        // or null when no photo exists). Pass through unchanged.
        const updatedUser = { ...result.data };

        setStorage(STORAGE_KEYS.USER, updatedUser);
        if (mountedRef.current) {
          setUser(updatedUser);
        }
        return true;
      }
      return false;
    } catch (error) {
      console.error('Failed to refresh user:', error);
      return false;
    }
  }, []);

  const checkAuth = useCallback(async () => {
    if (checkingRef.current) return;
    checkingRef.current = true;

    const token = localStorage.getItem(STORAGE_KEYS.SESSION_TOKEN);
    const savedUser = getStorage(STORAGE_KEYS.USER);

    if (token && savedUser) {
      try {
        const result = await loginData.validateSession();
        const isValid = result.success && result.data?.isValid === true;
        if (!mountedRef.current) return;
        if (isValid) {
          setUser(savedUser);
          setIsAuthenticated(true);
        } else {
          await performLogout();
        }
      } catch {
        if (mountedRef.current) {
          await performLogout();
        }
      }
    }

    if (mountedRef.current) {
      setLoading(false);
    }
    checkingRef.current = false;
  }, [performLogout]);

  useEffect(() => {
    mountedRef.current = true;
    checkAuth();
    return () => {
      mountedRef.current = false;
    };
  }, [checkAuth]);

  const login = useCallback(async (username, password) => {
    try {
      const result = await loginData.handleLogin(username, password);
      if (result.success) {
        const { sessionToken, user: userDataResult } = result.data;

        // Backend login response carries a relative profilePhotoUrl
        // (or null when no photo). Do NOT construct a URL client-side.
        const userWithPhoto = { ...userDataResult };

        localStorage.setItem(STORAGE_KEYS.SESSION_TOKEN, sessionToken);
        setStorage(STORAGE_KEYS.USER, userWithPhoto);
        if (mountedRef.current) {
          setUser(userWithPhoto);
          setIsAuthenticated(true);
        }
        return { success: true };
      }
      return { success: false, error: result.error || 'Login failed' };
    } catch (error) {
      const message =
        error.response?.data?.message || 'Invalid username or password';
      return { success: false, error: message };
    }
  }, []);

  const logout = useCallback(async () => {
    await performLogout();
  }, [performLogout]);

  const value = {
    user,
    loading,
    isAuthenticated,
    login,
    logout,
    checkAuth,
    refreshUser,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};