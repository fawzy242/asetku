/**
 * CENTRALIZED TRANSACTION TYPE CONSTANTS
 * Single source of truth untuk semua transaction type options.
 * 
 * NOTE: Values are integers (FK to MasterData table in backend)
 * 
 * Usage:
 *   import { TRANSACTION_TYPE_OPTIONS, TRANSACTION_TYPES } from '../../core/constants/transactionTypes';
 */

export const TRANSACTION_TYPES = {
  HANDOVER: 1,
  TRANSFER: 2,
  LOAN: 3,
  RETURN: 4,
  LOAN_RETURN: 5,
  MAINTENANCE: 6,
  POST_MAINTENANCE: 7,
  DISPOSAL: 8,
};

/**
 * UPDATED: Transaction type options untuk dropdown/select.
 * REMOVED: RETURN and LOAN_RETURN (these are created via shortcuts only)
 */
export const TRANSACTION_TYPE_OPTIONS = [
  { value: "", label: "Select Type" },
  { value: TRANSACTION_TYPES.HANDOVER, label: "HANDOVER (Company → Employee)" },
  { value: TRANSACTION_TYPES.TRANSFER, label: "TRANSFER (Employee → Employee)" },
  { value: TRANSACTION_TYPES.LOAN, label: "LOAN (Company → Employee)" },
  // RETURN and LOAN_RETURN are REMOVED - use shortcut buttons instead
  { value: TRANSACTION_TYPES.MAINTENANCE, label: "MAINTENANCE" },
  // POST_MAINTENANCE is REMOVED - use shortcut button instead
  { value: TRANSACTION_TYPES.DISPOSAL, label: "DISPOSAL" },
];

/**
 * Transaction type options untuk filter dropdown (include all for filtering)
 */
export const TRANSACTION_TYPE_FILTER_OPTIONS = [
  { value: "", label: "All Types" },
  { value: TRANSACTION_TYPES.HANDOVER, label: "Handover" },
  { value: TRANSACTION_TYPES.TRANSFER, label: "Transfer" },
  { value: TRANSACTION_TYPES.LOAN, label: "Loan" },
  { value: TRANSACTION_TYPES.RETURN, label: "Return" },
  { value: TRANSACTION_TYPES.LOAN_RETURN, label: "Loan Return" },
  { value: TRANSACTION_TYPES.MAINTENANCE, label: "Maintenance" },
  { value: TRANSACTION_TYPES.POST_MAINTENANCE, label: "Post-Maintenance" },
  { value: TRANSACTION_TYPES.DISPOSAL, label: "Disposal" },
];

/**
 * Transaction types yang memerlukan PairedTransactionId (fromAssetTransactionId).
 */
export const TRANSACTION_TYPES_REQUIRING_PAIR = [
  TRANSACTION_TYPES.LOAN_RETURN,
  TRANSACTION_TYPES.POST_MAINTENANCE,
];

/**
 * Transaction types yang membuat pairing (bisa dipilih sebagai pair source).
 */
export const TRANSACTION_TYPES_CREATING_PAIR = [
  TRANSACTION_TYPES.LOAN,
  TRANSACTION_TYPES.MAINTENANCE,
];

/**
 * Transaction types yang memerlukan ToEmployeeId (penerima).
 */
export const TRANSACTION_TYPES_REQUIRING_TO_EMPLOYEE = [
  TRANSACTION_TYPES.HANDOVER,
  TRANSACTION_TYPES.TRANSFER,
  TRANSACTION_TYPES.LOAN,
];

/**
 * Transaction types yang memerlukan FromEmployeeId (pemberi).
 */
export const TRANSACTION_TYPES_REQUIRING_FROM_EMPLOYEE = [
  TRANSACTION_TYPES.TRANSFER,
  TRANSACTION_TYPES.RETURN,
  TRANSACTION_TYPES.LOAN_RETURN,
];

/**
 * Transaction types that can be returned via shortcut
 */
export const TRANSACTION_TYPES_RETURNABLE = [
  TRANSACTION_TYPES.HANDOVER,
  TRANSACTION_TYPES.LOAN,
  TRANSACTION_TYPES.MAINTENANCE,
];

/**
 * Helper: Get transaction type name from code
 */
export const getTransactionTypeName = (code) => {
  const entry = Object.entries(TRANSACTION_TYPES).find(([_, value]) => value === code);
  return entry ? entry[0] : 'UNKNOWN';
};

/**
 * Helper: Get transaction type code from name
 */
export const getTransactionTypeCode = (name) => {
  return TRANSACTION_TYPES[name] || null;
};

export default TRANSACTION_TYPES;