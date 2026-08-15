import React from 'react';
import Card from '../../../components/atoms/Card/Card';
import DataTable from '../../../components/molecules/DataTable/DataTable';
import { getManagementColumns } from '../../../core/constants/columns';

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
}) => {
  return (
    <Card
      className="transactions-menu__grid-card"
      title="Primary Transactions"
      subtitle="Handover, Transfer, Loan, Maintenance, Disposal, Operational Office"
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