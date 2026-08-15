import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';

dayjs.extend(relativeTime);

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

export const formatRelativeTime = (date) => {
  if (!date) return '-';
  const d = dayjs(date);
  if (!d.isValid()) return '-';
  return d.fromNow();
};

export const parseDate = (date) => {
  if (!date) return null;
  const d = dayjs(date);
  return d.isValid() ? d : null;
};

export const isDateValid = (date) => {
  if (!date) return false;
  return dayjs(date).isValid();
};

export const getCurrentDate = () => dayjs().format('YYYY-MM-DD');

export const getCurrentDateTime = () => dayjs().toISOString();