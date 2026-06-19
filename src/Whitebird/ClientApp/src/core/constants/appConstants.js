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
  MAX_PAGE_SIZE: 100,
  MIN_PAGE_SIZE: 5,
};

// File Upload Configuration
export const UPLOAD_CONFIG = {
  MAX_FILE_SIZE_MB: 10,
  MAX_FILE_SIZE_BYTES: 10 * 1024 * 1024,
  MAX_IMAGE_SIZE_MB: 5,
  MAX_IMAGE_SIZE_BYTES: 5 * 1024 * 1024,
  ALLOWED_IMAGE_EXTENSIONS: ['.jpg', '.jpeg', '.png', '.gif', '.webp'],
  ALLOWED_IMAGE_MIME_TYPES: ['image/jpeg', 'image/png', 'image/gif', 'image/webp'],
  ALLOWED_DOCUMENT_EXTENSIONS: ['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.txt'],
  ALLOWED_DOCUMENT_MIME_TYPES: [
    'application/pdf',
    'application/msword',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    'application/vnd.ms-excel',
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    'text/plain'
  ],
  MAX_FILES_PER_ENTITY: 10,
};

// Debounce Configuration
export const DEBOUNCE_CONFIG = {
  SEARCH_DELAY: 300,
  FORM_DELAY: 500,
  TYPEAHEAD_DELAY: 250,
  RESIZE_DELAY: 150,
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
  TRANSITION_DURATION: '200ms',
};

// Date Format Configuration
export const DATE_FORMAT_CONFIG = {
  DISPLAY_DATE: 'DD/MM/YYYY',
  DISPLAY_DATETIME: 'DD/MM/YYYY HH:mm',
  API_DATE: 'YYYY-MM-DD',
  API_DATETIME: 'YYYY-MM-DDTHH:mm:ss',
  SHORT_DATE: 'DD/MM/YY',
  MONTH_YEAR: 'MMM YYYY',
  YEAR_MONTH: 'YYYY-MM',
};

// Chart Configuration
export const CHART_CONFIG = {
  DEFAULT_HEIGHT: 320,
  LARGE_HEIGHT: 400,
  DOUGHNUT_CUTOUT: '65%',
  ANIMATION_DURATION: 1000,
  LEGEND_POSITION: 'bottom',
};

// Toast Configuration
export const TOAST_CONFIG = {
  DURATION_SUCCESS: 3000,
  DURATION_ERROR: 4000,
  DURATION_WARNING: 3500,
  DURATION_INFO: 3000,
  POSITION: 'bottom-right',
};

// SweetAlert2 Configuration
export const SWEETALERT_CONFIG = {
  CONFIRM_BUTTON_COLOR: '#dc2626',
  CANCEL_BUTTON_COLOR: '#6b7280',
  DELETE_BUTTON_COLOR: '#ef4444',
  SUCCESS_BUTTON_COLOR: '#10b981',
  TIMER: 3000,
};

// Grid Configuration
export const GRID_CONFIG = {
  DEFAULT_PAGE: 1,
  DEFAULT_ROWS_PER_PAGE: 10,
  ROWS_PER_PAGE_OPTIONS: [5, 10, 25, 50, 100],
  DEFAULT_ROW_HEIGHT: 52,
  DEFAULT_HEADER_HEIGHT: 56,
  ACTION_COLUMN_WIDTH: 80,
  CHECKBOX_COLUMN_WIDTH: 50,
};

// Asset Status Mapping (for display)
export const ASSET_STATUS_DISPLAY = {
  'Available': 'Available',
  'Assigned': 'Assigned',
  'On Loan': 'On Loan',
  'In Maintenance': 'In Maintenance',
  'Under Repair': 'Under Repair',
  'Damaged': 'Damaged',
  'Retired': 'Retired',
  'Disposed': 'Disposed',
};

// Transaction Type Values (from backend MasterData)
export const TRANSACTION_TYPE_VALUES = {
  HANDOVER: 1,
  TRANSFER: 2,
  LOAN: 3,
  RETURN: 4,
  LOAN_RETURN: 5,
  MAINTENANCE: 6,
  POST_MAINTENANCE: 7,
  DISPOSAL: 8,
};

// Employee Status Values
export const EMPLOYEE_STATUS_VALUES = {
  ACTIVE: 1,
  INACTIVE: 2,
  RESIGNED: 3,
  ON_LEAVE: 4,
};

// Office Type Values
export const OFFICE_TYPE_VALUES = {
  HEAD_OFFICE: 1,
  BRANCH_OFFICE: 2,
  WAREHOUSE: 3,
};

// Asset Condition Values
export const ASSET_CONDITION_VALUES = {
  GOOD: 1,
  NORMAL: 2,
  DAMAGED: 3,
};

// Asset Condition Purchase Values
export const ASSET_CONDITION_PURCHASE_VALUES = {
  NEW: 1,
  SECOND_HAND: 2,
};