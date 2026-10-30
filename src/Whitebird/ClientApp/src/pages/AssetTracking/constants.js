// src/pages/AssetTracking/constants.js

export const TRANSACTION_TYPE_COLORS = {
  HANDOVER: '#3b82f6',
  TRANSFER: '#8b5cf6',
  LOAN: '#f59e0b',
  RETURN: '#10b981',
  LOAN_RETURN: '#34d399',
  MAINTENANCE: '#f97316',
  POST_MAINTENANCE: '#14b8a6',
  DISPOSAL: '#ef4444',
  OPERATIONAL_OFFICE: '#6366f1',
};

export const historyFilterTabs = [
  { id: 'all', label: 'All History' },
  { id: 'handover', label: 'Handover' },
  { id: 'transfer', label: 'Transfer' },
  { id: 'loan', label: 'Loan' },
  { id: 'movement', label: 'Movement' },
  { id: 'maintenance', label: 'Maintenance' },
  { id: 'disposal', label: 'Disposal' },
];