import dayjs from 'dayjs';

export const formatDate = (date, format = 'YYYY-MM-DD') => {
  if (!date) return '-';
  const d = dayjs(date);
  if (!d.isValid()) return '-';
  return d.format(format);
};

export const formatDateTime = (dateTime, format = 'YYYY-MM-DD HH:mm') => {
  if (!dateTime) return '-';
  const d = dayjs(dateTime);
  if (!d.isValid()) return '-';
  return d.format(format);
};