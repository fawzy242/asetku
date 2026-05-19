import React, { createContext, useContext, useState, useEffect, useCallback, useRef } from "react";
import apiService from "../core/services/api.service";
import utilsHelper from "../core/utils/utils.helper";
import ConfirmDialog from "../components/molecules/ConfirmDialog/ConfirmDialog";

const AuthContext = createContext(null);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within AuthProvider");
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const checkingRef = useRef(false);
  const mountedRef = useRef(true);

  const performLogout = useCallback(async () => {
    const token = localStorage.getItem("whitebird_session_token");
    if (token) { 
      try { await apiService.post("/auth/logout", { sessionToken: token }); } catch {} 
    }
    localStorage.removeItem("whitebird_session_token");
    utilsHelper.removeLocalStorage("user");
    if (mountedRef.current) {
      setUser(null);
      setIsAuthenticated(false);
    }
  }, []);

  const checkAuth = useCallback(async () => {
    if (checkingRef.current) return;
    checkingRef.current = true;

    const token = localStorage.getItem("whitebird_session_token");
    const savedUser = utilsHelper.getLocalStorage("user");
    
    if (token && savedUser) {
      try {
        const response = await apiService.get("/auth/validate-session");
        const isValid = response.data?.data?.isValid || response.data?.isValid;
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

  const login = useCallback(async (email, password) => {
    try {
      const response = await apiService.post("/auth/login", { email, password });
      if (response.data?.isSuccess) {
        const { sessionToken, user: userData } = response.data.data;
        localStorage.setItem("whitebird_session_token", sessionToken);
        utilsHelper.setLocalStorage("user", userData);
        if (mountedRef.current) {
          setUser(userData);
          setIsAuthenticated(true);
        }
        ConfirmDialog.toast.success(`Welcome back, ${userData.fullName}!`);
        return { success: true };
      }
      ConfirmDialog.toast.error(response.data?.message || "Login failed");
      return { success: false, error: response.data?.message || "Login failed" };
    } catch (error) {
      const message = error.response?.data?.message || "Invalid email or password";
      ConfirmDialog.toast.error(message);
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
    checkAuth 
  };
  
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};