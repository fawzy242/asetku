// src/core/constants/transactionTypes.js

export const TRANSACTION_TYPES = {
  HANDOVER: 1,
  TRANSFER: 2,
  LOAN: 3,
  RETURN: 4,
  LOAN_RETURN: 5,
  MAINTENANCE: 6,
  POST_MAINTENANCE: 7,
  DISPOSAL: 8,
  OPERATIONAL_OFFICE: 9,
};

export const TRANSACTION_TYPE_ADD = [
  TRANSACTION_TYPES.HANDOVER,
  TRANSACTION_TYPES.TRANSFER,
  TRANSACTION_TYPES.LOAN,
  TRANSACTION_TYPES.MAINTENANCE,
  TRANSACTION_TYPES.DISPOSAL,
  TRANSACTION_TYPES.OPERATIONAL_OFFICE,
];

export const TRANSACTION_TYPE_RETURN = [
  TRANSACTION_TYPES.RETURN,
  TRANSACTION_TYPES.LOAN_RETURN,
  TRANSACTION_TYPES.POST_MAINTENANCE,
];

export const TRANSACTION_TYPE_OPTIONS = [
  { value: '', label: 'Select Type' },
  { value: TRANSACTION_TYPES.HANDOVER, label: 'HANDOVER' },
  { value: TRANSACTION_TYPES.TRANSFER, label: 'TRANSFER' },
  { value: TRANSACTION_TYPES.LOAN, label: 'LOAN' },
  { value: TRANSACTION_TYPES.MAINTENANCE, label: 'MAINTENANCE' },
  { value: TRANSACTION_TYPES.DISPOSAL, label: 'DISPOSAL' },
  { value: TRANSACTION_TYPES.OPERATIONAL_OFFICE, label: 'OPERATIONAL OFFICE' },
];

export const TRANSACTION_TYPES_REQUIRING_EMPLOYEE = [
  TRANSACTION_TYPES.HANDOVER,
  TRANSACTION_TYPES.TRANSFER,
  TRANSACTION_TYPES.LOAN,
];

export const TRANSACTION_TYPES_REQUIRING_OFFICE = [
  TRANSACTION_TYPES.OPERATIONAL_OFFICE,
];

export const TRANSACTION_TYPES_REQUIRING_PAIR = [
  TRANSACTION_TYPES.LOAN_RETURN,
  TRANSACTION_TYPES.POST_MAINTENANCE,
];

export const TRANSACTION_TYPES_RETURNABLE = [
  TRANSACTION_TYPES.HANDOVER,
  TRANSACTION_TYPES.LOAN,
];

export const getTransactionTypeName = (code) => {
  const entry = Object.entries(TRANSACTION_TYPES).find(([_, value]) => value === code);
  return entry ? entry[0] : 'UNKNOWN';
};

export const isPrimaryTransactionType = (code) => TRANSACTION_TYPE_ADD.includes(code);

export const TRANSACTION_TYPE_COLORS = {
  HANDOVER: { bg: 'rgba(59, 130, 246, 0.15)', color: '#3b82f6', label: 'Handover' },
  TRANSFER: { bg: 'rgba(139, 92, 246, 0.15)', color: '#8b5cf6', label: 'Transfer' },
  LOAN: { bg: 'rgba(245, 158, 11, 0.15)', color: '#f59e0b', label: 'Loan' },
  RETURN: { bg: 'rgba(16, 185, 129, 0.15)', color: '#10b981', label: 'Return' },
  LOAN_RETURN: { bg: 'rgba(16, 185, 129, 0.15)', color: '#10b981', label: 'Loan Return' },
  MAINTENANCE: { bg: 'rgba(245, 158, 11, 0.15)', color: '#d97706', label: 'Maintenance' },
  POST_MAINTENANCE: { bg: 'rgba(16, 185, 129, 0.15)', color: '#10b981', label: 'Post Maintenance' },
  DISPOSAL: { bg: 'rgba(239, 68, 68, 0.15)', color: '#ef4444', label: 'Disposal' },
  OPERATIONAL_OFFICE: { bg: 'rgba(139, 92, 246, 0.15)', color: '#8b5cf6', label: 'Operational Office' },
};

export const getTransactionTypeColor = (typeName) => {
  if (!typeName)
    return { bg: 'rgba(107,114,128,0.1)', color: '#6b7280', label: 'Unknown' };
  return (
    TRANSACTION_TYPE_COLORS[typeName] || {
      bg: 'rgba(107,114,128,0.1)',
      color: '#6b7280',
      label: typeName,
    }
  );
};

export default TRANSACTION_TYPES;