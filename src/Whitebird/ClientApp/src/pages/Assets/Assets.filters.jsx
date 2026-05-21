import React, { useMemo } from 'react';
import Select from '../../components/atoms/Select/Select';
import { ASSET_STATUS_OPTIONS } from '../../core/constants/assetStatuses';

export const AssetFilters = ({ 
  statusFilter, setStatusFilter, 
  categoryFilter, setCategoryFilter, 
  categories, setPage 
}) => {
  const categoryOptions = useMemo(() => [
    { value: "", label: "All Categories" },
    ...categories.map(c => ({ value: c.value, label: c.label }))
  ], [categories]);

  return (
    <>
      <Select 
        label="Status" 
        value={statusFilter} 
        onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} 
        options={[{ value: "", label: "All Statuses" }, ...ASSET_STATUS_OPTIONS]} 
      />
      <Select 
        label="Category" 
        value={categoryFilter} 
        onChange={(e) => { setCategoryFilter(e.target.value); setPage(1); }} 
        options={categoryOptions} 
      />
    </>
  );
};