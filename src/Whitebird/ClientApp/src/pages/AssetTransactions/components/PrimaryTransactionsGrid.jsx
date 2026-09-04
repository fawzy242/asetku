// src/pages/AssetTransactions/components/PrimaryTransactionsGrid.jsx
import React from 'react';
import Card from '../../../components/atoms/Card/Card';
import DataTable from '../../../components/molecules/DataTable/DataTable';

const PrimaryTransactionsGrid = ({
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
  title = "Primary Transactions",
  subtitle = "Handover, Transfer, Loan, Maintenance, Disposal, Operational Office"
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
        ariaLabel="Primary transactions"
        paginationMode="server"
      />
    </Card>
  );
};

export default PrimaryTransactionsGrid;