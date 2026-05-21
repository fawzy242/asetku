/**
 * Mapping helpers for converting between frontend string values and backend integer codes
 * Based on MasterData tables
 * 
 * NOTE: Gunakan constants dari file constants/*.js untuk nilai yang sudah fixed.
 * Helper ini digunakan untuk transformasi data form ke payload API.
 */

import { 
  TRANSACTION_TYPES, 
  getTransactionTypeCode as getCodeFromConstants,
  getTransactionTypeName as getNameFromConstants
} from '../constants/transactionTypes';

import {
  ASSET_CONDITION_OPTIONS,
  ASSET_CONDITION_PURCHASE_OPTIONS
} from '../constants/assetStatuses';

// ============================================================
// ASSET CONDITION MAPPING
// ============================================================

export const ASSET_CONDITION_MAP = {
  'Good': 1,
  'Normal': 2,
  'Damaged': 3,
};

export const ASSET_CONDITION_REVERSE_MAP = {
  1: 'Good',
  2: 'Normal',
  3: 'Damaged',
};

export const getAssetConditionCode = (value) => {
  if (!value) return null;
  const numValue = typeof value === 'string' ? parseInt(value, 10) : value;
  if (!isNaN(numValue) && numValue >= 1 && numValue <= 3) return numValue;
  return ASSET_CONDITION_MAP[value] || null;
};

export const getAssetConditionName = (code) => {
  if (!code) return null;
  return ASSET_CONDITION_REVERSE_MAP[code] || 'Unknown';
};

// ============================================================
// ASSET CONDITION PURCHASE MAPPING
// ============================================================

export const ASSET_CONDITION_PURCHASE_MAP = {
  'New': 1,
  'Second Hand': 2,
};

export const ASSET_CONDITION_PURCHASE_REVERSE_MAP = {
  1: 'New',
  2: 'Second Hand',
};

export const getAssetConditionPurchaseCode = (value) => {
  if (!value) return null;
  const numValue = typeof value === 'string' ? parseInt(value, 10) : value;
  if (!isNaN(numValue) && (numValue === 1 || numValue === 2)) return numValue;
  return ASSET_CONDITION_PURCHASE_MAP[value] || null;
};

export const getAssetConditionPurchaseName = (code) => {
  if (!code) return null;
  return ASSET_CONDITION_PURCHASE_REVERSE_MAP[code] || 'Unknown';
};

// ============================================================
// TRANSACTION TYPE MAPPING (delegates to constants)
// ============================================================

export const TRANSACTION_TYPE_MAP = TRANSACTION_TYPES;

export const TRANSACTION_TYPE_REVERSE_MAP = {
  1: 'HANDOVER',
  2: 'TRANSFER',
  3: 'LOAN',
  4: 'RETURN',
  5: 'LOAN_RETURN',
  6: 'MAINTENANCE',
  7: 'POST_MAINTENANCE',
  8: 'DISPOSAL',
};

export const getTransactionTypeCode = (value) => {
  if (!value) return null;
  if (typeof value === 'number') return value;
  return getCodeFromConstants(value);
};

export const getTransactionTypeName = (code) => {
  if (!code) return null;
  if (typeof code === 'string') return code;
  return getNameFromConstants(code);
};

// ============================================================
// EMPLOYEE MAPPING (with caching support)
// ============================================================

let positionCache = new Map();
let statusCache = new Map();

export const setPositionCache = (positions) => {
  positionCache.clear();
  positions.forEach(p => {
    positionCache.set(p.name, p.code);
    positionCache.set(p.code, p.name);
  });
};

export const getPositionCode = (name) => {
  if (!name) return null;
  return positionCache.get(name) || null;
};

export const getPositionName = (code) => {
  if (!code) return null;
  return positionCache.get(code) || null;
};

export const setStatusCache = (statuses) => {
  statusCache.clear();
  statuses.forEach(s => {
    statusCache.set(s.name, s.code);
    statusCache.set(s.code, s.name);
  });
};

export const getStatusCode = (name) => {
  if (!name) return null;
  return statusCache.get(name) || null;
};

export const getStatusName = (code) => {
  if (!code) return null;
  return statusCache.get(code) || null;
};

// ============================================================
// ASSET STATUS (derived from active transaction)
// ============================================================

export const ASSET_STATUS = {
  AVAILABLE: 'Available',
  ASSIGNED: 'Assigned',
  ON_LOAN: 'On Loan',
  IN_MAINTENANCE: 'In Maintenance',
  DISPOSED: 'Disposed',
};

export const getAssetStatusFromTransactionType = (transactionType) => {
  switch (transactionType) {
    case 1: // HANDOVER
    case 2: // TRANSFER
      return ASSET_STATUS.ASSIGNED;
    case 3: // LOAN
      return ASSET_STATUS.ON_LOAN;
    case 6: // MAINTENANCE
      return ASSET_STATUS.IN_MAINTENANCE;
    case 8: // DISPOSAL
      return ASSET_STATUS.DISPOSED;
    default:
      return ASSET_STATUS.AVAILABLE;
  }
};

// ============================================================
// TRANSFORM HELPERS
// ============================================================

/**
 * Transform null/empty string values to null for API
 */
export const transformNullValues = (data, nullFields = []) => {
  const result = { ...data };
  nullFields.forEach(field => {
    if (result[field] === '' || result[field] === undefined || result[field] === null) {
      result[field] = null;
    } else if (typeof result[field] === 'string' && result[field].trim() === '') {
      result[field] = null;
    }
  });
  return result;
};

/**
 * Transform ID fields from string to int
 */
export const transformIdFields = (data, idFields = []) => {
  const result = { ...data };
  idFields.forEach(field => {
    if (result[field] === '' || result[field] === undefined || result[field] === null) {
      result[field] = null;
    } else if (typeof result[field] === 'string') {
      const parsed = parseInt(result[field], 10);
      result[field] = isNaN(parsed) ? null : parsed;
    }
  });
  return result;
};

/**
 * Transform float fields from string to number
 */
export const transformFloatFields = (data, floatFields = []) => {
  const result = { ...data };
  floatFields.forEach(field => {
    if (result[field] === '' || result[field] === undefined || result[field] === null) {
      result[field] = null;
    } else if (typeof result[field] === 'string') {
      const parsed = parseFloat(result[field]);
      result[field] = isNaN(parsed) ? null : parsed;
    }
  });
  return result;
};

/**
 * Transform date fields (ensure null for empty)
 */
export const transformDateFields = (data, dateFields = []) => {
  const result = { ...data };
  dateFields.forEach(field => {
    if (result[field] === '' || result[field] === undefined) {
      result[field] = null;
    }
  });
  return result;
};