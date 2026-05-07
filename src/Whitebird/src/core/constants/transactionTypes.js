/**
 * CENTRALIZED TRANSACTION TYPE CONSTANTS
 * Single source of truth untuk semua transaction type options.
 * 
 * Usage:
 *   import { TRANSACTION_TYPE_OPTIONS, TRANSACTION_TYPES } from '../../core/constants/transactionTypes';
 */

export const TRANSACTION_TYPES = {
  HANDOVER: 'HANDOVER',
  TRANSFER: 'TRANSFER',
  LOAN: 'LOAN',
  RETURN: 'RETURN',
  LOAN_RETURN: 'LOAN_RETURN',
  MAINTENANCE: 'MAINTENANCE',
  POST_MAINTENANCE: 'POST_MAINTENANCE',
  DISPOSAL: 'DISPOSAL',
};

/**
 * Full transaction type options untuk dropdown/select.
 * Label menggunakan format human-readable.
 */
export const TRANSACTION_TYPE_OPTIONS = [
  { value: '', label: 'Select Type' },
  { value: TRANSACTION_TYPES.HANDOVER, label: 'Handover (Company → Employee)' },
  { value: TRANSACTION_TYPES.TRANSFER, label: 'Transfer (Employee → Employee)' },
  { value: TRANSACTION_TYPES.LOAN, label: 'Loan (Company → Employee)' },
  { value: TRANSACTION_TYPES.RETURN, label: 'Return (Employee → Company)' },
  { value: TRANSACTION_TYPES.LOAN_RETURN, label: 'Loan Return (Paired)' },
  { value: TRANSACTION_TYPES.MAINTENANCE, label: 'Maintenance' },
  { value: TRANSACTION_TYPES.POST_MAINTENANCE, label: 'Post-Maintenance (Paired)' },
  { value: TRANSACTION_TYPES.DISPOSAL, label: 'Disposal' },
];

/**
 * Compact transaction type options untuk filter dropdown.
 */
export const TRANSACTION_TYPE_FILTER_OPTIONS = [
  { value: '', label: 'All Types' },
  { value: TRANSACTION_TYPES.HANDOVER, label: 'Handover' },
  { value: TRANSACTION_TYPES.TRANSFER, label: 'Transfer' },
  { value: TRANSACTION_TYPES.LOAN, label: 'Loan' },
  { value: TRANSACTION_TYPES.RETURN, label: 'Return' },
  { value: TRANSACTION_TYPES.LOAN_RETURN, label: 'Loan Return' },
  { value: TRANSACTION_TYPES.MAINTENANCE, label: 'Maintenance' },
  { value: TRANSACTION_TYPES.POST_MAINTENANCE, label: 'Post-Maintenance' },
  { value: TRANSACTION_TYPES.DISPOSAL, label: 'Disposal' },
];

/**
 * Transaction types yang memerlukan PairedTransactionId.
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

export default TRANSACTION_TYPES;