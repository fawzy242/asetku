// src/hooks/useIsAdmin.js
import { useAuth } from '../context/AuthContext';

// Per the backend changelog, role-gated endpoints accept either
// "ADMIN" or "Administrator". Comparison is case-insensitive so the hook
// keeps working if the seed data or future role names use different casing.
const ADMIN_ROLE_NAMES = ['admin', 'administrator'];

export const useIsAdmin = () => {
  const { user } = useAuth();
  const roleName = user?.roleName;
  if (!roleName) return false;
  return ADMIN_ROLE_NAMES.includes(String(roleName).toLowerCase());
};

export default useIsAdmin;