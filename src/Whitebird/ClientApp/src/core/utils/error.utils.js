export const isCanceledError = (error) => {
  return (
    error?.name === 'CanceledError' ||
    error?.message === 'canceled' ||
    error?.isCanceled === true ||
    error?.code === 'ERR_CANCELED'
  );
};