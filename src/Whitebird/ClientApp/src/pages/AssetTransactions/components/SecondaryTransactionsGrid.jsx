// src/pages/AssetTransactions/components/SecondaryTransactionsGrid.jsx
import React from 'react';
import Card from '../../../components/atoms/Card/Card';
import DataTable from '../../../components/molecules/DataTable/DataTable';

const SecondaryTransactionsGrid = ({
  data,
  loading,
  totalCount,
  page,
  pageSize,
  onPageChange,
  onPageSizeChange,
  columns,
  checkboxSelection,
  onSelectionChange,
  getRowId,
  bulkActions = null,
  title = "Secondary Transactions",
  subtitle = "Return, Loan Return, Post Maintenance"
}) => {
  return (
    <Card
      className="transactions-menu__grid-card"
      title={title}
      subtitle={subtitle}
      actions={bulkActions}
    >
      <DataTable
        rows={data}
        columns={columns}
        loading={loading}
        pageSize={pageSize}
        page={page}
        totalRowCount={totalCount}
        onPageChange={onPageChange}
        onPageSizeChange={onPageSizeChange}
        getRowId={getRowId}
        checkboxSelection={checkboxSelection}
        onRowSelectionModelChange={onSelectionChange}
        ariaLabel="Secondary transactions"
        paginationMode="server"
      />
    </Card>
  );
};

export default SecondaryTransactionsGrid;