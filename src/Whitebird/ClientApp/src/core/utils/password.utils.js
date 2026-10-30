// src/core/utils/password.utils.js
//
// Shared password complexity rules — mirrors the backend validator
// (backend changelog §1 / §8):
//   ^(?=.*[A-Za-z])(?=.*\d).{8,}$
//
// Single source of truth for the client-side mirror of this rule.
// Used by the change-password form; login does not enforce complexity
// (backend validates credentials, not complexity, at login).

export const PASSWORD_COMPLEXITY_REGEX = /^(?=.*[A-Za-z])(?=.*\d).{8,}$/;

export const PASSWORD_COMPLEXITY_MESSAGE =
  'Password must be at least 8 characters and include at least one letter and one number.';

export const isPasswordComplex = (password) => {
  if (!password || typeof password !== 'string') return false;
  return PASSWORD_COMPLEXITY_REGEX.test(password);
};

export const getPasswordComplexityError = (password) => {
  if (!password) return 'Password is required';
  if (!isPasswordComplex(password)) return PASSWORD_COMPLEXITY_MESSAGE;
  return null;
};