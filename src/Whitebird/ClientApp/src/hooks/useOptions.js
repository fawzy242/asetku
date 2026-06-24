import { useMemo } from 'react';

/**
 * Centralized hook for building select options from reference data
 * Eliminates duplicate option building across all pages
 * 
 * @param {Array} data - Reference data array with value and label fields
 * @param {string} placeholder - Placeholder label (default: 'Select...')
 * @param {string} valueField - Field name for value (default: 'value')
 * @param {string} labelField - Field name for label (default: 'label')
 * @param {Function} transformLabel - Optional function to transform label
 * @returns {Array} Options array with { value, label }
 */
export const useOptions = (data, placeholder = 'Select...', valueField = 'value', labelField = 'label', transformLabel = null) => {
  return useMemo(() => {
    const options = [{ value: '', label: placeholder }];
    
    if (!data || !Array.isArray(data)) return options;
    
    data.forEach(item => {
      const value = item[valueField] !== undefined ? item[valueField] : item.value;
      let label = item[labelField] !== undefined ? item[labelField] : item.label;
      
      if (transformLabel && typeof transformLabel === 'function') {
        label = transformLabel(item);
      }
      
      options.push({ value, label });
    });
    
    return options;
  }, [data, placeholder, valueField, labelField, transformLabel]);
};

export const useSelectOptions = (data, placeholder = 'Select...') => {
  return useOptions(data, placeholder);
};

export default useOptions;