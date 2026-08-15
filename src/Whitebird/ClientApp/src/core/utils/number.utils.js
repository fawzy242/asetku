export const formatCurrency = (amount, currency = 'IDR') => {
  if (amount === null || amount === undefined) return '-';
  if (isNaN(amount)) return '-';
  
  return new Intl.NumberFormat('id-ID', {
    style: 'currency',
    currency: currency,
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(amount);
};

export const formatNumber = (number, decimals = 0) => {
  if (number === null || number === undefined) return '-';
  if (isNaN(number)) return '-';
  
  return new Intl.NumberFormat('id-ID', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(number);
};

export const parseNumber = (value) => {
  if (value === null || value === undefined) return null;
  const parsed = parseFloat(String(value).replace(/[^0-9.,]/g, '').replace(/,/g, ''));
  return isNaN(parsed) ? null : parsed;
};

export const clamp = (value, min, max) => {
  return Math.max(min, Math.min(max, value));
};

export const roundTo = (value, decimals = 0) => {
  const factor = Math.pow(10, decimals);
  return Math.round(value * factor) / factor;
};