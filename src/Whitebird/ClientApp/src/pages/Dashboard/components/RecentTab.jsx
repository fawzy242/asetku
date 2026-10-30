// src/pages/Dashboard/components/RecentTab.jsx
import React from 'react';
import Card from '../../../components/atoms/Card/Card';
import DataTable from '../../../components/molecules/DataTable/DataTable';
import { transactionColumns, expiredWarrantyColumns } from '../columns';

const RecentTab = ({
  recentTransactions,
  recentPage,
  onRecentPageChange,
  expiredWarranty,
  expiredWarrantyPage,
  onExpiredWarrantyPageChange,
}) => {
  const paginatedRecent = (recentTransactions || []).slice(
    (recentPage - 1) * 10,
    recentPage * 10
  );
  const paginatedExpiredWarranty = (expiredWarranty || []).slice(
    (expiredWarrantyPage - 1) * 10,
    expiredWarrantyPage * 10
  );

  return (
    <div className="dashboard__tab-content">
      <Card title="Recent Transactions">
        <div className="dashboard__table-wrapper">
          <DataTable
            rows={paginatedRecent}
            columns={transactionColumns}
            pageSize={10}
            page={recentPage}
            onPageChange={onRecentPageChange}
            getRowId={(row) => row?.assetTransactionId || `txn-${Math.random()}`}
            hideFooter={false}
            ariaLabel="Recent transactions table"
            paginationMode="client"
            autoHeight={false}
          />
        </div>
      </Card>

      {expiredWarranty.length > 0 && (
        <Card
          title="Expired Warranty Assets"
          className="dashboard__summary-card"
        >
          <DataTable
            rows={paginatedExpiredWarranty}
            columns={expiredWarrantyColumns}
            pageSize={10}
            page={expiredWarrantyPage}
            onPageChange={onExpiredWarrantyPageChange}
            getRowId={(row) => row?.assetId || `ew-${Math.random()}`}
            hideFooter={false}
            ariaLabel="Expired warranty table"
            paginationMode="client"
            autoHeight={false}
          />
        </Card>
      )}
    </div>
  );
};

export default RecentTab;