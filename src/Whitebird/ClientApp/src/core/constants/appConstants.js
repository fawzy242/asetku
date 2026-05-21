/**
 * Application-wide constants
 * Centralized location for magic numbers and configuration values
 */

// API Configuration
export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001',
  TIMEOUT: 30000,
  SESSION_TOKEN_KEY: import.meta.env.VITE_SESSION_TOKEN_KEY || 'whitebird_session_token',
};

// Cache Configuration
export const CACHE_CONFIG = {
  REFERENCE_DATA_STALE_TIME: 30 * 60 * 1000,      // 30 minutes
  MASTER_DATA_STALE_TIME: 24 * 60 * 60 * 1000,    // 24 hours
  GRID_DATA_STALE_TIME: 2 * 60 * 1000,            // 2 minutes
};

// Pagination Configuration
export const PAGINATION_CONFIG = {
  DEFAULT_PAGE_SIZE: 10,
  PAGE_SIZE_OPTIONS: [10, 25, 50, 100],
};

// File Upload Configuration
export const UPLOAD_CONFIG = {
  MAX_FILE_SIZE_MB: 10,
  MAX_FILE_SIZE_BYTES: 10 * 1024 * 1024,
  ALLOWED_IMAGE_EXTENSIONS: ['.jpg', '.jpeg', '.png', '.gif', '.webp'],
  ALLOWED_DOCUMENT_EXTENSIONS: ['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.txt'],
};

// Debounce Configuration
export const DEBOUNCE_CONFIG = {
  SEARCH_DELAY: 300,
  FORM_DELAY: 500,
};

// Port Configuration
export const PORT_CONFIG = {
  DEV_PORT: 3000,
  PREVIEW_PORT: 4173,
  BACKEND_PORT: 5001,
};

// Theme Configuration
export const THEME_CONFIG = {
  SIDEBAR_WIDTH_EXPANDED: '260px',
  SIDEBAR_WIDTH_COLLAPSED: '80px',
  TOPBAR_HEIGHT: '64px',
};