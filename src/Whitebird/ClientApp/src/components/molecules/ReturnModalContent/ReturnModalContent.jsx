import React from 'react';
import { Box } from '@mui/material';
import DatePickerInput from '../../atoms/Input/DatePickerInput';
import Select from '../../atoms/Select/Select';
import Input from '../../atoms/Input/Input';
import Button from '../../atoms/Button/Button';
import './ReturnModalContent.scss';

/**
 * Centralized Return Modal Content component
 * 
 * @param {Object} props
 * @param {Object} props.transaction - Selected transaction
 * @param {Object} props.returnData - Return form data
 * @param {Function} props.onReturnDataChange - Return data change handler
 * @param {Function} props.onSubmit - Submit handler
 * @param {Function} props.onCancel - Cancel handler
 * @param {boolean} props.isSubmitting - Loading state
 * @param {Array} props.conditionOptions - Condition options
 */
const ReturnModalContent = ({
  transaction,
  returnData,
  onReturnDataChange,
  onSubmit,
  onCancel,
  isSubmitting = false,
  conditionOptions = [],
}) => {
  const handleChange = (field) => (value) => {
    onReturnDataChange({ ...returnData, [field]: value });
  };

  return (
    <Box className="return-modal-content" sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <Box className="return-modal-content__info" sx={{ p: 2, bgcolor: 'var(--surface)', borderRadius: 2 }}>
        <strong style={{ color: 'var(--text-primary)' }}>
          {transaction?.assetCode} - {transaction?.assetName}
        </strong>
        <p className="return-modal-content__info-detail">
          Transaction ID: {transaction?.assetTransactionId}
        </p>
        <p className="return-modal-content__info-detail">
          Type: {transaction?.transactionTypeName} | Holder: {transaction?.toEmployeeName || 'None'}
        </p>
      </Box>
      
      <DatePickerInput
        label="Actual Return Date"
        value={returnData.actualReturnDate}
        onChange={(e) => handleChange('actualReturnDate')(e.target.value)}
        required
      />
      
      <Select
        label="Condition After"
        value={returnData.conditionAfter}
        onChange={(e) => handleChange('conditionAfter')(e.target.value)}
        options={conditionOptions}
        required
      />
      
      <Input
        label="Notes"
        value={returnData.notes || ''}
        onChange={(e) => handleChange('notes')(e.target.value)}
        multiline
        rows={2}
      />
      
      <Box className="return-modal-content__actions" sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 2 }}>
        <Button variant="outline" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button variant="primary" onClick={onSubmit} loading={isSubmitting}>
          Confirm Return
        </Button>
      </Box>
    </Box>
  );
};

export default ReturnModalContent;