import React from 'react';
import { Grid, Box } from '@mui/material';
import FilterPanel from '../FilterPanel/FilterPanel';
import Button from '../../atoms/Button/Button';
import { FiFilter, FiX } from 'react-icons/fi';

/**
 * Standardized filter panel component for all grid views
 * @param {Object} props
 * @param {React.ReactNode} props.children - Filter inputs
 * @param {boolean} props.visible - Whether filter panel is visible
 * @param {Function} props.onToggle - Toggle filter panel visibility
 * @param {Function} props.onReset - Reset all filters
 * @param {number} props.columns - Number of columns in grid (default: 3)
 * @param {boolean} props.showResetButton - Show reset button (default: true)
 */
const GridFilterPanel = ({
  children,
  visible = false,
  onToggle,
  onReset,
  columns = 3,
  showResetButton = true,
}) => {
  if (!visible && !children) return null;

  return (
    <>
      {/* Filter toggle button row */}
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
        <Button
          variant="outline"
          size="sm"
          onClick={onToggle}
          startIcon={<FiFilter />}
        >
          {visible ? 'Hide Filters' : 'Show Filters'}
        </Button>
      </Box>

      {/* Filter panel */}
      {visible && (
        <FilterPanel visible={visible} columns={columns}>
          {children}
          {showResetButton && onReset && (
            <Box sx={{ display: 'flex', alignItems: 'flex-end', pb: 1 }}>
              <Button
                variant="outline"
                size="sm"
                onClick={onReset}
                startIcon={<FiX />}
              >
                Reset Filters
              </Button>
            </Box>
          )}
        </FilterPanel>
      )}
    </>
  );
};

export default GridFilterPanel;