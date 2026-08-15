// src/core/constants/employmentStatuses.js
/**
 * CENTRALIZED EMPLOYMENT STATUS CONSTANTS
 * SYNCED WITH BACKEND MasterData (EmployeeStatus)
 * 
 * Backend ReferenceName: "EmployeeStatus"
 * ReferenceCode mapping:
 *   1 = Internship
 *   2 = Training
 *   3 = Contract
 *   4 = Permanent Employee
 *   5 = Resigned
 * 
 * Usage:
 *   import { EMPLOYMENT_STATUS_COLORS, EMPLOYMENT_STATUS_OPTIONS } from '../../core/constants/employmentStatuses';
 */

export const EMPLOYMENT_STATUSES = {
  INTERNSHIP: 'Internship',
  TRAINING: 'Training',
  CONTRACT: 'Contract',
  PERMANENT: 'Permanent Employee',
  RESIGNED: 'Resigned',
};

// Status codes (matching backend MasterData ReferenceCode)
export const EMPLOYMENT_STATUS_CODES = {
  INTERNSHIP: 1,
  TRAINING: 2,
  CONTRACT: 3,
  PERMANENT: 4,
  RESIGNED: 5,
};

export const EMPLOYMENT_STATUS_OPTIONS = [
  { value: EMPLOYMENT_STATUS_CODES.INTERNSHIP, label: EMPLOYMENT_STATUSES.INTERNSHIP },
  { value: EMPLOYMENT_STATUS_CODES.TRAINING, label: EMPLOYMENT_STATUSES.TRAINING },
  { value: EMPLOYMENT_STATUS_CODES.CONTRACT, label: EMPLOYMENT_STATUSES.CONTRACT },
  { value: EMPLOYMENT_STATUS_CODES.PERMANENT, label: EMPLOYMENT_STATUSES.PERMANENT },
  { value: EMPLOYMENT_STATUS_CODES.RESIGNED, label: EMPLOYMENT_STATUSES.RESIGNED },
];

export const EMPLOYMENT_STATUS_COLORS = {
  [EMPLOYMENT_STATUSES.INTERNSHIP]: { bg: 'rgba(16, 185, 129, 0.15)', color: '#10b981' },
  [EMPLOYMENT_STATUSES.TRAINING]: { bg: 'rgba(59, 130, 246, 0.15)', color: '#3b82f6' },
  [EMPLOYMENT_STATUSES.CONTRACT]: { bg: 'rgba(245, 158, 11, 0.15)', color: '#f59e0b' },
  [EMPLOYMENT_STATUSES.PERMANENT]: { bg: 'rgba(139, 92, 246, 0.15)', color: '#8b5cf6' },
  [EMPLOYMENT_STATUSES.RESIGNED]: { bg: 'rgba(239, 68, 68, 0.15)', color: '#ef4444' },
};

export const getEmploymentStatusStyles = (statusName) => {
  if (!statusName) return { bg: 'rgba(107, 114, 128, 0.15)', color: '#6b7280' };
  return EMPLOYMENT_STATUS_COLORS[statusName] || { bg: 'rgba(107, 114, 128, 0.15)', color: '#6b7280' };
};

export const getEmploymentStatusName = (code) => {
  const entry = Object.entries(EMPLOYMENT_STATUS_CODES).find(([_, value]) => value === code);
  return entry ? EMPLOYMENT_STATUSES[entry[0]] : null;
};

export default {
  EMPLOYMENT_STATUSES,
  EMPLOYMENT_STATUS_CODES,
  EMPLOYMENT_STATUS_OPTIONS,
  EMPLOYMENT_STATUS_COLORS,
  getEmploymentStatusStyles,
  getEmploymentStatusName,
};