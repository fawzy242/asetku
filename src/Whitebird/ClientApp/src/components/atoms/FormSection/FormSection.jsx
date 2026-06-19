import React from 'react';
import { Box, Typography, Divider } from '@mui/material';
import './FormSection.scss';

const FormSection = ({ title, description, children, spacing = 2 }) => {
  return (
    <Box className="form-section" sx={{ mb: spacing }}>
      {(title || description) && (
        <>
          <Box className="form-section__header">
            {title && (
              <Typography variant="subtitle1" className="form-section__title" fontWeight={600}>
                {title}
              </Typography>
            )}
            {description && (
              <Typography variant="caption" className="form-section__description" color="text.secondary">
                {description}
              </Typography>
            )}
          </Box>
          <Divider className="form-section__divider" />
        </>
      )}
      <Box className="form-section__content" sx={{ mt: 2 }}>
        {children}
      </Box>
    </Box>
  );
};

export default FormSection;