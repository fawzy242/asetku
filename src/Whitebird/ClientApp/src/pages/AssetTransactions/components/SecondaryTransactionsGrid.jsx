import React from 'react';
import Card from '../../../components/atoms/Card/Card';
import DataTable from '../../../components/molecules/DataTable/DataTable';
import { getManagementReturnColumns } from '../../../core/constants/columns';

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
}) => {
  return (
    <Card
      className="transactions-menu__grid-card"
      title="Secondary Transactions"
      subtitle="Return, Loan Return, Post Maintenance"
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