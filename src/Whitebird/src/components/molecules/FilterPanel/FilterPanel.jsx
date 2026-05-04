import React from 'react';
import { Box } from '@mui/material';
import './FilterPanel.scss';

/**
 * Reusable filter panel dengan styling konsisten
 * @param {Object} props
 * @param {React.ReactNode} props.children - Filter content
 * @param {boolean} [props.visible] - Show/hide panel
 * @param {number} [props.columns] - Number of columns in grid
 */
const FilterPanel = ({ children, visible = false, columns = 3 }) => {
  if (!visible) return null;

  return (
    <Box
      className="filter-panel"
      sx={{
        display: 'grid',
        gridTemplateColumns: {
          xs: '1fr',
          sm: `repeat(${Math.min(columns, 2)}, 1fr)`,
          md: `repeat(${columns}, 1fr)`
        },
        gap: 2,
        mb: 3,
        p: 2,
        bgcolor: 'var(--surface)',
        borderRadius: 2,
      }}
    >
      {children}
    </Box>
  );
};

export default FilterPanel;