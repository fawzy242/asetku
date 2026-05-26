import React from 'react';
import { Chip } from '@mui/material';
import IconButton from '../../components/atoms/IconButton/IconButton';
import { FiEdit2, FiTrash2 } from 'react-icons/fi';
import { getStatusChipStyles } from '../../core/constants/statusColors';
import utilsHelper from '../../core/utils/utils.helper';

export const getAssetColumns = (handleEdit, handleDelete) => [
  { field: "assetCode", headerName: "Code", width: 120 },
  { field: "assetName", headerName: "Name", flex: 1, minWidth: 180 },
  { field: "categoryName", headerName: "Category", width: 150 },
  { field: "brand", headerName: "Brand", width: 100 },
  { field: "model", headerName: "Model", width: 100 },
  { 
    field: "displayStatus", 
    headerName: "Status", 
    width: 140, 
    renderCell: (p) => <Chip label={p?.value || '-'} size="small" sx={getStatusChipStyles(p?.value)} /> 
  },
  { field: "displayCondition", headerName: "Condition", width: 100 },
  // REMOVED: holder column
  { field: "officeName", headerName: "Office", width: 150 },
  { 
    field: "purchasePrice", 
    headerName: "Price", 
    width: 130, 
    valueFormatter: (params) => {
      if (!params || params.value === null || params.value === undefined) return '-';
      return utilsHelper.formatCurrency(params.value);
    }
  },
  { 
    field: "actions", 
    headerName: "Actions", 
    width: 100, 
    sortable: false, 
    renderCell: (p) => (
      <div className="table-actions">
        <IconButton onClick={() => handleEdit(p?.row)} title="Edit asset" size="lg"><FiEdit2 size={18} /></IconButton>
        <IconButton onClick={() => handleDelete(p?.row)} title="Delete asset" variant="danger" size="lg"><FiTrash2 size={18} /></IconButton>
      </div>
    )
  },
];