export const EMPLOYMENT_STATUSES = {
  INTERNSHIP: 'Internship',
  TRAINING: 'Training',
  CONTRACT: 'Contract',
  PERMANENT: 'Permanent Employee',
  RESIGNED: 'Resigned',
};

export const EMPLOYMENT_STATUS_CODES = {
  INTERNSHIP: 1,
  TRAINING: 2,
  CONTRACT: 3,
  PERMANENT: 4,
  RESIGNED: 5,
};

export const EMPLOYMENT_STATUS_COLORS = {
  [EMPLOYMENT_STATUSES.INTERNSHIP]: { bg: 'rgba(16, 185, 129, 0.15)', color: '#10b981' },
  [EMPLOYMENT_STATUSES.TRAINING]: { bg: 'rgba(59, 130, 246, 0.15)', color: '#3b82f6' },
  [EMPLOYMENT_STATUSES.CONTRACT]: { bg: 'rgba(245, 158, 11, 0.15)', color: '#f59e0b' },
  [EMPLOYMENT_STATUSES.PERMANENT]: { bg: 'rgba(139, 92, 246, 0.15)', color: '#8b5cf6' },
  [EMPLOYMENT_STATUSES.RESIGNED]: { bg: 'rgba(239, 68, 68, 0.15)', color: '#ef4444' },
};

export const getEmploymentStatusStyles = (statusName) => {
  if (!statusName) return { bg: 'rgba(107, 114, 128, 0.15)', color: '#6b7280' };
  return (
    EMPLOYMENT_STATUS_COLORS[statusName] || {
      bg: 'rgba(107, 114, 128, 0.15)',
      color: '#6b7280',
    }
  );
};

export default {
  EMPLOYMENT_STATUSES,
  EMPLOYMENT_STATUS_CODES,
  EMPLOYMENT_STATUS_COLORS,
  getEmploymentStatusStyles,
};