import React, { useState } from 'react';
import Select from '../../atoms/Select/Select';
import Input from '../../atoms/Input/Input';
import Button from '../../atoms/Button/Button';
import './FilterBar.scss';

const FilterBar = ({
  filters = [],
  onFilter,
  onReset,
  className = ''
}) => {
  const [filterValues, setFilterValues] = useState({});

  const handleChange = (field, value) => {
    setFilterValues(prev => ({ ...prev, [field]: value }));
  };

  const handleApply = () => {
    onFilter?.(filterValues);
  };

  const handleReset = () => {
    setFilterValues({});
    onReset?.();
    onFilter?.({});
  };

  const renderFilter = (filter) => {
    switch (filter.type) {
      case 'select':
        return (
          <Select
            key={filter.field}
            label={filter.label}
            value={filterValues[filter.field] || ''}
            onChange={(e) => handleChange(filter.field, e.target.value)}
            options={[
              { value: '', label: `All ${filter.label}` },
              ...(filter.options || [])
            ]}
          />
        );
      case 'date':
        return (
          <Input
            key={filter.field}
            label={filter.label}
            type="date"
            value={filterValues[filter.field] || ''}
            onChange={(e) => handleChange(filter.field, e.target.value)}
          />
        );
      default:
        return null;
    }
  };

  return (
    <div className={`filter-bar ${className}`}>
      <div className="filter-bar__filters">
        {filters.map(renderFilter)}
      </div>
      <div className="filter-bar__actions">
        <Button variant="primary" size="sm" onClick={handleApply}>
          Apply
        </Button>
        <Button variant="text" size="sm" onClick={handleReset}>
          Reset
        </Button>
      </div>
    </div>
  );
};

export default FilterBar;