import React from 'react';
import { FiCalendar } from 'react-icons/fi';
import DatePickerInput from '../../atoms/Input/DatePickerInput';
import Button from '../../atoms/Button/Button';
import './TransactionFilter.scss';

/**
 * Centralized Transaction Date Filter component
 * 
 * @param {Object} props
 * @param {string} props.startDate - Start date value
 * @param {string} props.endDate - End date value
 * @param {Function} props.onStartDateChange - Start date change handler
 * @param {Function} props.onEndDateChange - End date change handler
 * @param {Function} props.onClear - Clear filter handler
 * @param {boolean} props.hasFilter - Whether filter is active
 */
const TransactionFilter = ({
  startDate,
  endDate,
  onStartDateChange,
  onEndDateChange,
  onClear,
  hasFilter = false,
}) => {
  return (
    <div className="transaction-filter">
      <div className="transaction-filter__label">
        <FiCalendar size={16} />
        <span>Date Range:</span>
      </div>
      <div className="transaction-filter__inputs">
        <DatePickerInput
          label="Start Date"
          value={startDate}
          onChange={(e) => onStartDateChange(e.target.value)}
          size="small"
          className="transaction-filter__date"
        />
        <span className="transaction-filter__separator">to</span>
        <DatePickerInput
          label="End Date"
          value={endDate}
          onChange={(e) => onEndDateChange(e.target.value)}
          size="small"
          className="transaction-filter__date"
        />
        {hasFilter && (
          <Button
            variant="text"
            size="sm"
            onClick={onClear}
            className="transaction-filter__clear"
          >
            Clear
          </Button>
        )}
      </div>
    </div>
  );
};

export default TransactionFilter;