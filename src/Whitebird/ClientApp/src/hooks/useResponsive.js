import { useState, useEffect } from 'react';

/**
 * Custom hook untuk responsive breakpoint detection
 * Mobile-first approach with debounced resize
 */
export const useResponsive = () => {
  const [windowWidth, setWindowWidth] = useState(
    typeof window !== 'undefined' ? window.innerWidth : 1024
  );

  useEffect(() => {
    let timeoutId;
    const handleResize = () => {
      clearTimeout(timeoutId);
      timeoutId = setTimeout(() => setWindowWidth(window.innerWidth), 150);
    };
    window.addEventListener('resize', handleResize);
    return () => {
      window.removeEventListener('resize', handleResize);
      clearTimeout(timeoutId);
    };
  }, []);

  return {
    width: windowWidth,
    isMobile: windowWidth < 640,
    isTablet: windowWidth >= 640 && windowWidth < 1024,
    isDesktop: windowWidth >= 1024,
    isSm: windowWidth < 640,
    isMd: windowWidth >= 640 && windowWidth < 768,
    isLg: windowWidth >= 768 && windowWidth < 1024,
    isXl: windowWidth >= 1024,
  };
};

export default useResponsive;