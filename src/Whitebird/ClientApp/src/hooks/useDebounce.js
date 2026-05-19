import { useCallback, useRef, useEffect } from 'react';

/**
 * Debounce hook dengan proper cleanup untuk mencegah memory leak
 * @param {Function} callback - Function yang akan di-debounce
 * @param {number} delay - Delay dalam milliseconds
 * @returns {Function} Debounced function
 */
export const useDebounce = (callback, delay = 300) => {
  const timeoutRef = useRef(null);
  const callbackRef = useRef(callback);
  const isMountedRef = useRef(true);

  useEffect(() => {
    callbackRef.current = callback;
  }, [callback]);

  useEffect(() => {
    isMountedRef.current = true;
    return () => {
      isMountedRef.current = false;
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
        timeoutRef.current = null;
      }
    };
  }, []);

  const debouncedCallback = useCallback((...args) => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
    timeoutRef.current = setTimeout(() => {
      if (isMountedRef.current) {
        callbackRef.current(...args);
      }
      timeoutRef.current = null;
    }, delay);
  }, [delay]);

  return debouncedCallback;
};