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