import { useState, useCallback, useMemo } from 'react';
import { useDebounce } from './useDebounce';

/**
 * Standardized hook for managing filter parameters
 * Menghilangkan duplikasi logic filter di berbagai pages
 * 
 * @param {Object} initialFilters - Initial filter values
 * @param {number} debounceDelay - Debounce delay in ms (default: 300)
 * @returns {Object} Filter state and handlers
 */
export const useFilterParams = (initialFilters = {}, debounceDelay = 300) => {
  const [filters, setFilters] = useState(initialFilters);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  
  // Debounce search term
  const debouncedSetSearch = useDebounce((value) => {
    setDebouncedSearchTerm(value);
  }, debounceDelay);

  const handleSearchChange = useCallback((value) => {
    setSearchTerm(value);
    debouncedSetSearch(value);
  }, [debouncedSetSearch]);

  const updateFilter = useCallback((key, value) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  }, []);

  const updateFilters = useCallback((newFilters) => {
    setFilters(prev => ({ ...prev, ...newFilters }));
  }, []);

  const removeFilter = useCallback((key) => {
    setFilters(prev => {
      const newFilters = { ...prev };
      delete newFilters[key];
      return newFilters;
    });
  }, []);

  const resetFilters = useCallback(() => {
    setFilters(initialFilters);
    setSearchTerm('');
    setDebouncedSearchTerm('');
  }, [initialFilters]);

  const clearAllFilters = useCallback(() => {
    setFilters({});
    setSearchTerm('');
    setDebouncedSearchTerm('');
  }, []);

  const hasActiveFilters = useMemo(() => {
    return Object.keys(filters).length > 0 || searchTerm.length > 0;
  }, [filters, searchTerm]);

  const activeFilterCount = useMemo(() => {
    let count = Object.keys(filters).length;
    if (searchTerm.length > 0) count++;
    return count;
  }, [filters, searchTerm]);

  const buildQueryParams = useCallback(() => {
    const params = { ...filters };
    if (debouncedSearchTerm) {
      params.search = debouncedSearchTerm;
    }
    return params;
  }, [filters, debouncedSearchTerm]);

  return {
    // State
    filters,
    searchTerm,
    debouncedSearchTerm,
    hasActiveFilters,
    activeFilterCount,
    
    // Handlers
    updateFilter,
    updateFilters,
    removeFilter,
    resetFilters,
    clearAllFilters,
    handleSearchChange,
    buildQueryParams,
    
    // Convenience
    setFilters,
    setSearchTerm,
  };
};

export default useFilterParams;