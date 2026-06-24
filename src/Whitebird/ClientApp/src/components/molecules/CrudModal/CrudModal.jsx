import React from 'react';
import { Box } from '@mui/material';
import Modal from '../Modal/Modal';
import ModalActions from '../ModalActions/ModalActions';

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
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
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