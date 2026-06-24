/**
 * CENTRALIZED EMPLOYMENT STATUS CONSTANTS
 * Single source of truth untuk semua employment status options.
 * 
 * Usage:
 *   import { EMPLOYMENT_STATUS_COLORS, EMPLOYMENT_STATUS_OPTIONS } from '../../core/constants/employmentStatuses';
 */

export const EMPLOYMENT_STATUSES = {
  PERMANENT: 'Permanent',
  CONTRACT: 'Contract',
  PROBATION: 'Probation',
  INTERN: 'Intern',
  FREELANCE: 'Freelance',
  RESIGNED: 'Resigned',
  TERMINATED: 'Terminated',
  ON_LEAVE: 'On Leave',
};

export const EMPLOYMENT_STATUS_OPTIONS = [
  { value: EMPLOYMENT_STATUSES.PERMANENT, label: 'Permanent' },
  { value: EMPLOYMENT_STATUSES.CONTRACT, label: 'Contract' },
  { value: EMPLOYMENT_STATUSES.PROBATION, label: 'Probation' },
  { value: EMPLOYMENT_STATUSES.INTERN, label: 'Intern' },
  { value: EMPLOYMENT_STATUSES.FREELANCE, label: 'Freelance' },
  { value: EMPLOYMENT_STATUSES.RESIGNED, label: 'Resigned' },
  { value: EMPLOYMENT_STATUSES.TERMINATED, label: 'Terminated' },
  { value: EMPLOYMENT_STATUSES.ON_LEAVE, label: 'On Leave' },
];

export const EMPLOYMENT_STATUS_COLORS = {
  [EMPLOYMENT_STATUSES.PERMANENT]: { bg: 'rgba(59, 130, 246, 0.15)', color: '#3b82f6' },
  [EMPLOYMENT_STATUSES.CONTRACT]: { bg: 'rgba(245, 158, 11, 0.15)', color: '#f59e0b' },
  [EMPLOYMENT_STATUSES.PROBATION]: { bg: 'rgba(139, 92, 246, 0.15)', color: '#8b5cf6' },
  [EMPLOYMENT_STATUSES.INTERN]: { bg: 'rgba(16, 185, 129, 0.15)', color: '#10b981' },
  [EMPLOYMENT_STATUSES.FREELANCE]: { bg: 'rgba(236, 72, 153, 0.15)', color: '#ec4899' },
  [EMPLOYMENT_STATUSES.RESIGNED]: { bg: 'rgba(239, 68, 68, 0.15)', color: '#ef4444' },
  [EMPLOYMENT_STATUSES.TERMINATED]: { bg: 'rgba(239, 68, 68, 0.15)', color: '#dc2626' },
  [EMPLOYMENT_STATUSES.ON_LEAVE]: { bg: 'rgba(245, 158, 11, 0.15)', color: '#d97706' },
};

export const getEmploymentStatusStyles = (statusName) => {
  if (!statusName) return { bg: 'rgba(107, 114, 128, 0.15)', color: '#6b7280' };
  return EMPLOYMENT_STATUS_COLORS[statusName] || { bg: 'rgba(107, 114, 128, 0.15)', color: '#6b7280' };
};

export default {
  EMPLOYMENT_STATUSES,
  EMPLOYMENT_STATUS_OPTIONS,
  EMPLOYMENT_STATUS_COLORS,
  getEmploymentStatusStyles,
};