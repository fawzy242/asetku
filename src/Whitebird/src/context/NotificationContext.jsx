import React, { createContext, useContext, useState, useCallback } from 'react';
import ConfirmDialog from '../components/molecules/ConfirmDialog/ConfirmDialog';

const NotificationContext = createContext(null);

export const useNotification = () => {
  const context = useContext(NotificationContext);
  if (!context) throw new Error('useNotification must be used within NotificationProvider');
  return context;
};

export const NotificationProvider = ({ children }) => {
  const [notifications, setNotifications] = useState([]);

  const addNotification = useCallback((notification) => {
    const id = Date.now();
    const newNotification = { id, ...notification, read: false, createdAt: new Date().toISOString() };
    setNotifications(prev => [newNotification, ...prev].slice(0, 50));
    if (notification.showToast !== false) {
      ConfirmDialog.showInfo(notification.title, notification.message);
    }
    return id;
  }, []);

  const markAsRead = useCallback((id) => setNotifications(prev => prev.map(n => n.id === id ? { ...n, read: true } : n)), []);
  const markAllAsRead = useCallback(() => setNotifications(prev => prev.map(n => ({ ...n, read: true }))), []);
  const removeNotification = useCallback((id) => setNotifications(prev => prev.filter(n => n.id !== id)), []);
  const clearAll = useCallback(() => setNotifications([]), []);
  const getUnreadCount = useCallback(() => notifications.filter(n => !n.read).length, [notifications]);

  const success = useCallback((title, message, options = {}) => addNotification({ type: 'success', title, message, ...options }), [addNotification]);
  const error = useCallback((title, message, options = {}) => addNotification({ type: 'error', title, message, ...options }), [addNotification]);
  const warning = useCallback((title, message, options = {}) => addNotification({ type: 'warning', title, message, ...options }), [addNotification]);
  const info = useCallback((title, message, options = {}) => addNotification({ type: 'info', title, message, ...options }), [addNotification]);

  const value = { notifications, addNotification, markAsRead, markAllAsRead, removeNotification, clearAll, getUnreadCount, success, error, warning, info };
  return <NotificationContext.Provider value={value}>{children}</NotificationContext.Provider>;
};