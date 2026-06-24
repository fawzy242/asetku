import React from 'react';
import { Box } from '@mui/material';
import DatePickerInput from '../../atoms/Input/DatePickerInput';
import Select from '../../atoms/Select/Select';
import Input from '../../atoms/Input/Input';
import Button from '../../atoms/Button/Button';
import './PostMaintenanceModalContent.scss';

/**
 * Centralized Post Maintenance Modal Content component
 * 
 * @param {Object} props
 * @param {Object} props.transaction - Selected transaction
 * @param {Object} props.postData - Post maintenance form data
 * @param {Function} props.onPostDataChange - Post data change handler
 * @param {Function} props.onSubmit - Submit handler
 * @param {Function} props.onCancel - Cancel handler
 * @param {boolean} props.isSubmitting - Loading state
 * @param {Array} props.conditionOptions - Condition options
 */
const PostMaintenanceModalContent = ({
  transaction,
  postData,
  onPostDataChange,
  onSubmit,
  onCancel,
  isSubmitting = false,
  conditionOptions = [],
}) => {
  const handleChange = (field) => (value) => {
    onPostDataChange({ ...postData, [field]: value });
  };

  return (
    <Box className="post-maintenance-modal-content" sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Box className="post-maintenance-modal-content__info" sx={{ p: 2, bgcolor: 'var(--surface)', borderRadius: 2 }}>
        <strong style={{ color: 'var(--text-primary)' }}>
          {transaction?.assetCode} - {transaction?.assetName}
        </strong>
        <p className="post-maintenance-modal-content__info-detail">
          Transaction ID: {transaction?.assetTransactionId}
        </p>
        <p className="post-maintenance-modal-content__info-detail">
          Type: {transaction?.transactionTypeName} | Maintenance Transaction
        </p>
      </Box>
      
      <DatePickerInput
        label="Completion Date"
        value={postData.completionDate}
        onChange={(e) => handleChange('completionDate')(e.target.value)}
        required
      />
      
      <Select
        label="Condition After"
        value={postData.conditionAfter}
        onChange={(e) => handleChange('conditionAfter')(e.target.value)}
        options={conditionOptions}
        required
      />
      
      <Input
        label="Notes"
        value={postData.notes || ''}
        onChange={(e) => handleChange('notes')(e.target.value)}
        multiline
        rows={2}
      />
      
      <Box className="post-maintenance-modal-content__actions" sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 2 }}>
        <Button variant="outline" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button variant="primary" onClick={onSubmit} loading={isSubmitting}>
          Confirm Post-Maintenance
        </Button>
      </Box>
    </Box>
  );
};

export default PostMaintenanceModalContent;