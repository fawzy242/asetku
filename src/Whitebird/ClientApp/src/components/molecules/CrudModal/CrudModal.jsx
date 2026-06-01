import React from 'react';
import Modal from '../Modal/Modal';
import ModalActions from '../ModalActions/ModalActions';
import { Box } from '@mui/material';

/**
 * Standardized CRUD modal component
 * @param {Object} props
 * @param {boolean} props.isOpen - Modal visibility
 * @param {Function} props.onClose - Close handler
 * @param {string} props.title - Modal title
 * @param {React.ReactNode} props.children - Form content
 * @param {Function} props.onSubmit - Submit handler
 * @param {boolean} props.isSubmitting - Loading state
 * @param {string} props.submitText - Submit button text (default: 'Submit')
 * @param {string} props.cancelText - Cancel button text (default: 'Cancel')
 * @param {boolean} props.showCancel - Show cancel button (default: true)
 * @param {string} props.size - Modal size: 'sm' | 'md' | 'lg' | 'xl' (default: 'md')
 * @param {boolean} props.disableSubmit - Disable submit button
 * @param {React.ReactNode} props.extraActions - Extra actions below form
 */
const CrudModal = ({
  isOpen,
  onClose,
  title,
  children,
  onSubmit,
  isSubmitting = false,
  submitText = 'Submit',
  cancelText = 'Cancel',
  showCancel = true,
  size = 'md',
  disableSubmit = false,
  extraActions = null,
}) => {
  const handleSubmit = (e) => {
    e.preventDefault();
    if (onSubmit) onSubmit(e);
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title} size={size}>
      <form onSubmit={handleSubmit}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {children}
        </Box>
        <ModalActions
          onCancel={onClose}
          isSubmitting={isSubmitting}
          submitText={submitText}
          cancelText={cancelText}
          showCancel={showCancel}
          disabledSubmit={disableSubmit}
        />
        {extraActions && (
          <Box sx={{ mt: 2 }}>
            {extraActions}
          </Box>
        )}
      </form>
    </Modal>
  );
};

export default CrudModal;