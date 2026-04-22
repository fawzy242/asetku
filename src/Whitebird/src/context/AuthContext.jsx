import React, { createContext, useContext, useState, useEffect } from "react";
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

  useEffect(() => { checkAuth(); }, []);

  const checkAuth = async () => {
    const token = localStorage.getItem("whitebird_session_token");
    const savedUser = utilsHelper.getLocalStorage("user");
    if (token && savedUser) {
      try {
        const response = await apiService.get("/auth/validate-session");
        const isValid = response.data?.data?.isValid || response.data?.isValid;
        if (isValid) {
          setUser(savedUser);
          setIsAuthenticated(true);
        } else {
          logout();
        }
      } catch {
        logout();
      }
    }
    setLoading(false);
  };

  const login = async (email, password) => {
    try {
      const response = await apiService.post("/auth/login", { email, password });
      if (response.data?.isSuccess) {
        const { sessionToken, user: userData } = response.data.data;
        localStorage.setItem("whitebird_session_token", sessionToken);
        utilsHelper.setLocalStorage("user", userData);
        setUser(userData);
        setIsAuthenticated(true);
        await ConfirmDialog.showSuccess("Login Successful", `Welcome back, ${userData.fullName}!`);
        return { success: true };
      }
      return { success: false, error: response.data?.message || "Login failed" };
    } catch (error) {
      return { success: false, error: error.response?.data?.message || "Invalid email or password" };
    }
  };

  const logout = async () => {
    const token = localStorage.getItem("whitebird_session_token");
    if (token) {
      try { await apiService.post("/auth/logout", { sessionToken: token }); } catch {}
    }
    localStorage.removeItem("whitebird_session_token");
    utilsHelper.removeLocalStorage("user");
    setUser(null);
    setIsAuthenticated(false);
  };

  const value = { user, loading, isAuthenticated, login, logout, checkAuth };
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};