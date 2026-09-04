import React from 'react';
import { Box } from '@mui/material';
import Button from '../../atoms/Button/Button';

const ModalActions = ({
  onCancel,
  onSubmit,
  isSubmitting = false,
  submitText = 'Submit',
  cancelText = 'Cancel',
  showCancel = true,
  submitVariant = 'primary',
  disabledSubmit = false,
}) => {
  return (
    <Box sx={{ 
      display: 'flex', 
      gap: 2, 
      justifyContent: 'flex-end', 
      mt: 4,
      pt: 2,
      borderTop: '1px solid var(--border)'
    }}>
      {showCancel && (
        <Button variant="outline" onClick={onCancel} type="button">
          {cancelText}
        </Button>
      )}
      {!disabledSubmit && (
        <Button 
          type="submit" 
          variant={submitVariant} 
          loading={isSubmitting}
          onClick={onSubmit}
          disabled={disabledSubmit}
        >
          {submitText}
        </Button>
      )}
    </Box>
  );
};

export default ModalActions;