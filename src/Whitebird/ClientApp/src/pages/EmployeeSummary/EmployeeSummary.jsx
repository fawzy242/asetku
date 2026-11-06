// src/pages/EmployeeSummary/EmployeeSummary.jsx
import React, { useState, useEffect, useCallback } from 'react';
import { FiSearch, FiUser, FiBox, FiCalendar, FiRefreshCw, FiTool } from 'react-icons/fi';
import { Grid, Typography } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useLocation } from 'react-router-dom';
import { EmployeeSummaryData } from '../../core/services';
import DataTable from '../../components/molecules/DataTable/DataTable';
import Card from '../../components/atoms/Card/Card';
import Select from '../../components/atoms/Select/Select';
import Spinner from '../../components/atoms/Spinner/Spinner';
import InfoRow from '../../components/molecules/InfoRow/InfoRow';
import RefreshButton from '../../components/atoms/RefreshButton/RefreshButton';
import { useOptions } from '../../hooks/useOptions';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { formatDateTime } from '../../core/utils/date.utils';
import { assetColumns, historyColumns } from './columns';
import EmployeeSummaryProfileCard from './components/EmployeeSummaryProfileCard';
import EmployeeSummaryStatsGrid from './components/EmployeeSummaryStatsGrid';
import './EmployeeSummary.scss';

const summaryData = new EmployeeSummaryData();

const EmployeeSummary = () => {
  const [selectedEmployeeId, setSelectedEmployeeId] = useState(null);
  const location = useLocation();
  const { toast } = useSweetAlert();
  const queryClient = useQueryClient();

  const [currentHoldingPage, setCurrentHoldingPage] = useState(1);
  const [currentHoldingPageSize, setCurrentHoldingPageSize] = useState(10);
  const [historyPage, setHistoryPage] = useState(1);
  const [historyPageSize, setHistoryPageSize] = useState(10);

  const { data: employees = [], isLoading: loading } = useQuery({
    queryKey: ['employee-summary-list'],
    queryFn: async () => (await summaryData.fetchEmployees()).data,
    staleTime: 5 * 60 * 1000,
  });

  const { data: employeeDetail = null, isLoading: detailLoading } = useQuery({
    queryKey: ['employee-summary-detail', selectedEmployeeId],
    queryFn: async () => (await summaryData.fetchEmployeeDetail(selectedEmployeeId)).data,
    enabled: !!selectedEmployeeId,
    staleTime: 60 * 1000,
  });

  // Fetches the stats grid values, INCLUDING totalAssetValue
  const { data: assetSummary = null, isLoading: summaryLoading } = useQuery({
    queryKey: ['employee-summary-assets', selectedEmployeeId],
    queryFn: async () => (await summaryData.fetchAssetSummary(selectedEmployeeId)).data,
    enabled: !!selectedEmployeeId,
    staleTime: 60 * 1000,
  });

  const { data: currentHoldingData = { data: [], totalCount: 0 }, isLoading: holdingLoading } = useQuery({
    queryKey: ['employee-summary-current-holding', selectedEmployeeId, currentHoldingPage, currentHoldingPageSize],
    queryFn: async () => (await summaryData.fetchCurrentHolding(selectedEmployeeId, { page: currentHoldingPage, pageSize: currentHoldingPageSize })).data,
    enabled: !!selectedEmployeeId,
    staleTime: 60 * 1000,
  });

  const { data: historyData = { data: [], totalCount: 0 }, isLoading: historyLoading } = useQuery({
    queryKey: ['employee-summary-history', selectedEmployeeId, historyPage, historyPageSize],
    queryFn: async () => (await summaryData.fetchTransactionHistory(selectedEmployeeId, { page: historyPage, pageSize: historyPageSize })).data,
    enabled: !!selectedEmployeeId,
    staleTime: 60 * 1000,
  });

  const loadingData = detailLoading || summaryLoading || holdingLoading || historyLoading;

  useEffect(() => {
    if (!employees || employees.length === 0) return;
    const params = new URLSearchParams(location.search);
    const employeeIdParam = params.get('employeeId');
    if (!employeeIdParam) return;
    const employeeId = parseInt(employeeIdParam, 10);
    if (isNaN(employeeId)) return;
    const exists = employees.some((e) => e.employeeId === employeeId);
    if (exists && selectedEmployeeId !== employeeId) setSelectedEmployeeId(employeeId);
  }, [employees, location.search, selectedEmployeeId]);

  const handleEmployeeSelect = useCallback((employeeId) => {
    setSelectedEmployeeId(employeeId || null);
    setCurrentHoldingPage(1);
    setHistoryPage(1);
  }, []);

  const handleRefresh = useCallback(() => {
    if (!selectedEmployeeId) return;
    queryClient.invalidateQueries({ queryKey: ['employee-summary-detail', selectedEmployeeId] });
    queryClient.invalidateQueries({ queryKey: ['employee-summary-assets', selectedEmployeeId] });
    queryClient.invalidateQueries({ queryKey: ['employee-summary-current-holding', selectedEmployeeId] });
    queryClient.invalidateQueries({ queryKey: ['employee-summary-history', selectedEmployeeId] });
    toast.success('Data refreshed');
  }, [selectedEmployeeId, queryClient, toast]);

  const currentAssets = currentHoldingData?.data || [];
  const assetHistory = historyData?.data || [];

  const employeeOptions = useOptions(
    Array.isArray(employees) ? employees.map((e) => ({ value: e.employeeId, label: `${e.employeeCode} - ${e.fullName}` })) : [],
    'Choose an employee...'
  );

  if (loading) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="employee-summary">
      <div className="page-header">
        <h1 className="page-title">Employee Summary</h1>
        <p className="page-description">View asset assignments and transaction history by employee</p>
      </div>

      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Card className="employee-summary__selection-card">
            <div className="employee-summary__selection-content">
              <div className="employee-summary__selection-icon"><FiSearch size={24} /></div>
              <div className="employee-summary__selection-select">
                <Select label="Select Employee" value={selectedEmployeeId || ''} onChange={(e) => handleEmployeeSelect(e.target.value)} options={employeeOptions} />
              </div>
              {selectedEmployeeId && <RefreshButton onClick={handleRefresh} loading={loadingData} />}
            </div>
          </Card>
        </Grid>

        {selectedEmployeeId && !loadingData && employeeDetail && (
          <>
            <Grid item xs={12}><EmployeeSummaryProfileCard employeeDetail={employeeDetail} /></Grid>
            
            {/* Pass assetSummary totalAssetValue directly */}
            <EmployeeSummaryStatsGrid 
              assetSummary={assetSummary}
              totalAssetValue={assetSummary?.totalAssetValue || 0} 
            />

            <Grid item xs={12} md={6}>
              <Card title="Last Transaction" subtitle="Most recent activity">
                {assetHistory.length === 0 ? (
                  <div className="employee-summary__empty-state u-p-24"><FiRefreshCw size={32} /><Typography variant="body2">No transactions found</Typography></div>
                ) : (
                  <div className="employee-summary__last-transaction">
                    <InfoRow icon={FiCalendar} label="Date" value={formatDateTime(assetHistory[0]?.transactionDate)} />
                    <InfoRow icon={FiRefreshCw} label="Type" value={assetHistory[0]?.transactionTypeName || assetHistory[0]?.typeName} />
                    <InfoRow icon={FiBox} label="Asset" value={`${assetHistory[0]?.assetCode || '-'} - ${assetHistory[0]?.assetName || '-'}`} />
                    <InfoRow icon={FiTool} label="Condition" value={assetHistory[0]?.conditionAfterName || assetHistory[0]?.conditionAfter} />
                  </div>
                )}
              </Card>
            </Grid>

            <Grid item xs={12}>
              <Card title="Current Assets" subtitle={`${currentHoldingData?.totalCount || 0} asset(s) currently assigned`}>
                {currentAssets.length === 0 ? (
                  <div className="employee-summary__empty-state"><FiBox size={48} /><Typography variant="h6" gutterBottom>No Assets Assigned</Typography><Typography variant="body2">This employee has no assets currently assigned.</Typography></div>
                ) : (
                  <DataTable rows={currentAssets} columns={assetColumns} loading={holdingLoading} pageSize={currentHoldingPageSize} page={currentHoldingPage} totalRowCount={currentHoldingData?.totalCount || 0} onPageChange={setCurrentHoldingPage} onPageSizeChange={setCurrentHoldingPageSize} getRowId={(row) => row.assetId || `asset-${Math.random()}`} hideFooter={false} autoHeight={false} ariaLabel="Employee current assets table" paginationMode="server" />
                )}
              </Card>
            </Grid>

            <Grid item xs={12}>
              <Card title="Asset History" subtitle={`${historyData?.totalCount || 0} transaction(s) found`}>
                {assetHistory.length === 0 ? (
                  <div className="employee-summary__empty-state"><FiRefreshCw size={48} /><Typography variant="h6" gutterBottom>No Transaction History</Typography><Typography variant="body2">No asset transactions found for this employee.</Typography></div>
                ) : (
                  <DataTable rows={assetHistory} columns={historyColumns} loading={historyLoading} pageSize={historyPageSize} page={historyPage} totalRowCount={historyData?.totalCount || 0} onPageChange={setHistoryPage} onPageSizeChange={setHistoryPageSize} getRowId={(row) => row.assetTransactionId || `hist-${Math.random()}`} hideFooter={false} autoHeight={false} ariaLabel="Employee asset history table" paginationMode="server" />
                )}
              </Card>
            </Grid>
          </>
        )}

        {selectedEmployeeId && loadingData && (
          <Grid item xs={12}><Card><div className="employee-summary__loading-container"><Spinner size="lg" /></div></Card></Grid>
        )}

        {!selectedEmployeeId && !loading && (
          <Grid item xs={12}>
            <Card>
              <div className="employee-summary__empty-state"><FiUser size={64} /><Typography variant="h6" gutterBottom>Select an Employee to View Summary</Typography><Typography variant="body2">Choose an employee from the dropdown above to view their asset summary and transaction history.</Typography></div>
            </Card>
          </Grid>
        )}
      </Grid>
    </div>
  );
};

export default EmployeeSummary;