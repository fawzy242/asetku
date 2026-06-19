import React from 'react';
import { Box } from '@mui/material';
import Button from '../../atoms/Button/Button';

/**
 * Reusable modal footer with Cancel and Submit buttons
 * @param {Object} props
 * @param {Function} props.onCancel - Cancel handler
 * @param {Function} props.onSubmit - Submit handler
 * @param {boolean} props.isSubmitting - Loading state for submit button
 * @param {string} props.submitText - Submit button text (default: 'Submit')
 * @param {string} props.cancelText - Cancel button text (default: 'Cancel')
 * @param {boolean} props.showCancel - Show cancel button (default: true)
 * @param {string} props.submitVariant - Submit button variant (default: 'primary')
 * @param {boolean} props.disabledSubmit - Disable submit button
 */
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
      <Button 
        type="submit" 
        variant={submitVariant} 
        loading={isSubmitting}
        onClick={onSubmit}
        disabled={disabledSubmit}
      >
        {submitText}
      </Button>
    </Box>
  );
};

export default ModalActions;