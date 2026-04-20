import React, { useState, useEffect, useCallback } from 'react';
import AssetTransactionsData from './AssetTransactions.data';
import DataTable from '../../components/molecules/DataTable/DataTable';
import Pagination from '../../components/molecules/Pagination/Pagination';
import SearchBar from '../../components/molecules/SearchBar/SearchBar';
import FilterBar from '../../components/molecules/FilterBar/FilterBar';
import Button from '../../components/atoms/Button/Button';
import Badge from '../../components/atoms/Badge/Badge';
import Modal from '../../components/molecules/Modal/Modal';
import Input from '../../components/atoms/Input/Input';
import Select from '../../components/atoms/Select/Select';
import Spinner from '../../components/atoms/Spinner/Spinner';
import utilsHelper from '../../core/utils/utils.helper';

const AssetTransactionsMenu = () => {
  const [loading, setLoading] = useState(true);
  const [transactions, setTransactions] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  
  const [showModal, setShowModal] = useState(false);
  const [showReturnModal, setShowReturnModal] = useState(false);
  const [editingTransaction, setEditingTransaction] = useState(null);
  const [selectedTransaction, setSelectedTransaction] = useState(null);
  const [formData, setFormData] = useState({
    assetId: '',
    transactionType: 'Assignment',
    toEmployeeId: '',
    fromEmployeeId: '',
    toLocationId: '',
    fromLocationId: '',
    expectedReturnDate: '',
    notes: '',
    conditionBefore: ''
  });
  const [returnData, setReturnData] = useState({
    actualReturnDate: '',
    conditionAfter: '',
    notes: ''
  });
  
  const [dropdownData, setDropdownData] = useState({
    assets: [],
    employees: [],
    locations: []
  });

  const transactionsData = new AssetTransactionsData();

  const loadData = useCallback(async () => {
    setLoading(true);
    
    const result = await transactionsData.loadGridData(page, pageSize, search, statusFilter);
    
    if (result.success) {
      setTransactions(result.data.data || []);
      setTotalCount(result.data.totalCount || 0);
    }
    
    setLoading(false);
  }, [page, pageSize, search, statusFilter]);

  const loadDropdowns = async () => {
    const result = await transactionsData.loadDropdownData();
    if (result.success) {
      setDropdownData(result.data);
    }
  };

  useEffect(() => {
    loadData();
    loadDropdowns();
  }, [loadData]);

  const handleSearch = (value) => {
    setSearch(value);
    setPage(1);
  };

  const handleFilter = (filters) => {
    setStatusFilter(filters.status || '');
    setPage(1);
  };

  const handleCreate = () => {
    setEditingTransaction(null);
    setFormData({
      assetId: '',
      transactionType: 'Assignment',
      toEmployeeId: '',
      fromEmployeeId: '',
      toLocationId: '',
      fromLocationId: '',
      expectedReturnDate: '',
      notes: '',
      conditionBefore: ''
    });
    setShowModal(true);
  };

  const handleEdit = async (transaction) => {
    const result = await transactionsData.loadTransaction(transaction.assetTransactionId);
    if (result.success) {
      setEditingTransaction(result.data);
      setFormData({
        assetId: result.data.assetId || '',
        transactionType: result.data.transactionType || 'Assignment',
        toEmployeeId: result.data.toEmployeeId || '',
        fromEmployeeId: result.data.fromEmployeeId || '',
        toLocationId: result.data.toLocationId || '',
        fromLocationId: result.data.fromLocationId || '',
        expectedReturnDate: result.data.expectedReturnDate ? result.data.expectedReturnDate.split('T')[0] : '',
        notes: result.data.notes || '',
        conditionBefore: result.data.conditionBefore || ''
      });
      setShowModal(true);
    }
  };

  const handleApprove = async (transaction) => {
    const result = await Swal.fire({
      title: 'Approve Transaction?',
      text: `Approve ${transaction.transactionType} for ${transaction.assetName}?`,
      icon: 'question',
      showCancelButton: true,
      showDenyButton: true,
      confirmButtonText: 'Approve',
      denyButtonText: 'Reject',
      confirmButtonColor: '#10b981',
      denyButtonColor: '#ef4444'
    });
    
    if (result.isConfirmed) {
      await transactionsData.approveTransaction(transaction.assetTransactionId, true, '');
      loadData();
    } else if (result.isDenied) {
      await transactionsData.approveTransaction(transaction.assetTransactionId, false, '');
      loadData();
    }
  };

  const handleReturn = (transaction) => {
    setSelectedTransaction(transaction);
    setReturnData({
      actualReturnDate: new Date().toISOString().split('T')[0],
      conditionAfter: transaction.conditionBefore || 'Good',
      notes: ''
    });
    setShowReturnModal(true);
  };

  const handleReturnSubmit = async () => {
    if (!selectedTransaction) return;
    
    const result = await transactionsData.returnAsset(
      selectedTransaction.assetTransactionId,
      returnData.actualReturnDate,
      returnData.conditionAfter,
      returnData.notes
    );
    
    if (result.success) {
      setShowReturnModal(false);
      loadData();
    }
  };

  const handleCancel = async (transaction) => {
    const result = await transactionsData.cancelTransaction(transaction.assetTransactionId);
    if (result.success) {
      loadData();
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    let result;
    if (editingTransaction) {
      result = await transactionsData.updateTransaction(editingTransaction.assetTransactionId, formData);
    } else {
      result = await transactionsData.createTransaction(formData);
    }
    
    if (result.success) {
      setShowModal(false);
      loadData();
    }
  };

  const columns = [
    { field: 'assetCode', headerName: 'Asset Code', width: 120 },
    { field: 'assetName', headerName: 'Asset Name', width: 180 },
    { field: 'transactionType', headerName: 'Type', width: 120 },
    { field: 'fromEmployeeName', headerName: 'From', width: 150 },
    { field: 'toEmployeeName', headerName: 'To', width: 150 },
    {
      field: 'transactionDate',
      headerName: 'Date',
      width: 160,
      valueFormatter: (params) => utilsHelper.formatDateTime(params.value)
    },
    {
      field: 'transactionStatus',
      headerName: 'Status',
      width: 120,
      renderCell: (params) => (
        <Badge variant={utilsHelper.getStatusColor(params.value)}>
          {params.value}
        </Badge>
      )
    },
    {
      field: 'expectedReturnDate',
      headerName: 'Expected Return',
      width: 130,
      valueFormatter: (params) => params.value ? utilsHelper.formatDate(params.value) : '-'
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 200,
      sortable: false,
      renderCell: (params) => (
        <div className="table-actions">
          {params.row.transactionStatus === 'Pending' && (
            <>
              <Button
                variant="success"
                size="sm"
                onClick={() => handleApprove(params.row)}
              >
                ✓
              </Button>
              <Button
                variant="text"
                size="sm"
                onClick={() => handleEdit(params.row)}
              >
                ✏️
              </Button>
              <Button
                variant="text"
                size="sm"
                onClick={() => handleCancel(params.row)}
              >
                ✕
              </Button>
            </>
          )}
          {params.row.transactionStatus === 'Approved' && 
           params.row.transactionType === 'Assignment' && 
           !params.row.actualReturnDate && (
            <Button
              variant="primary"
              size="sm"
              onClick={() => handleReturn(params.row)}
            >
              ↩️ Return
            </Button>
          )}
        </div>
      )
    }
  ];

  if (loading && transactions.length === 0) {
    return (
      <div className="transactions-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="transactions-menu">
      <div className="transactions-menu__header">
        <h2 className="transactions-menu__title">Asset Transactions</h2>
        <Button variant="primary" onClick={handleCreate}>
          + New Transaction
        </Button>
      </div>
      
      <div className="transactions-menu__toolbar">
        <SearchBar
          placeholder="Search transactions..."
          onSearch={handleSearch}
          debounceTime={300}
        />
        <FilterBar
          filters={[
            { 
              field: 'status', 
              label: 'Status', 
              type: 'select', 
              options: [
                { value: '', label: 'All' },
                { value: 'Pending', label: 'Pending' },
                { value: 'Approved', label: 'Approved' },
                { value: 'Rejected', label: 'Rejected' },
                { value: 'Completed', label: 'Completed' },
                { value: 'Cancelled', label: 'Cancelled' }
              ]
            }
          ]}
          onFilter={handleFilter}
        />
      </div>
      
      <div className="transactions-menu__tabs">
        <button 
          className={`transactions-menu__tab ${!statusFilter ? 'active' : ''}`}
          onClick={() => setStatusFilter('')}
        >
          All
        </button>
        <button 
          className={`transactions-menu__tab ${statusFilter === 'Pending' ? 'active' : ''}`}
          onClick={() => setStatusFilter('Pending')}
        >
          Pending
        </button>
        <button 
          className={`transactions-menu__tab ${statusFilter === 'Approved' ? 'active' : ''}`}
          onClick={() => setStatusFilter('Approved')}
        >
          Approved
        </button>
      </div>
      
      <div className="transactions-menu__table">
        <DataTable
          rows={transactions}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.assetTransactionId}
        />
      </div>
      
      <Pagination
        currentPage={page}
        totalPages={Math.ceil(totalCount / pageSize)}
        pageSize={pageSize}
        totalItems={totalCount}
        onPageChange={setPage}
        onPageSizeChange={(size) => {
          setPageSize(size);
          setPage(1);
        }}
      />

      {/* Transaction Form Modal */}
      <Modal
        isOpen={showModal}
        onClose={() => setShowModal(false)}
        title={editingTransaction ? 'Edit Transaction' : 'New Transaction'}
        size="lg"
      >
        <form className="transaction-form" onSubmit={handleSubmit}>
          <div className="transaction-form__grid">
            <Select
              label="Asset"
              value={formData.assetId}
              onChange={(e) => setFormData({ ...formData, assetId: e.target.value })}
              options={dropdownData.assets.map(a => ({ 
                value: a.assetId, 
                label: `${a.assetCode} - ${a.assetName}` 
              }))}
              required
            />
            
            <Select
              label="Transaction Type"
              value={formData.transactionType}
              onChange={(e) => setFormData({ ...formData, transactionType: e.target.value })}
              options={[
                { value: 'Assignment', label: 'Assignment' },
                { value: 'Return', label: 'Return' },
                { value: 'Maintenance', label: 'Maintenance' },
                { value: 'Transfer', label: 'Transfer' },
                { value: 'Disposal', label: 'Disposal' }
              ]}
              required
            />
            
            <Select
              label="From Employee"
              value={formData.fromEmployeeId}
              onChange={(e) => setFormData({ ...formData, fromEmployeeId: e.target.value })}
              options={[
                { value: '', label: 'None' },
                ...dropdownData.employees.map(e => ({ value: e.employeeId, label: e.fullName }))
              ]}
            />
            
            <Select
              label="To Employee"
              value={formData.toEmployeeId}
              onChange={(e) => setFormData({ ...formData, toEmployeeId: e.target.value })}
              options={[
                { value: '', label: 'None' },
                ...dropdownData.employees.map(e => ({ value: e.employeeId, label: e.fullName }))
              ]}
            />
            
            <Select
              label="From Location"
              value={formData.fromLocationId}
              onChange={(e) => setFormData({ ...formData, fromLocationId: e.target.value })}
              options={[
                { value: '', label: 'None' },
                ...dropdownData.locations.map(l => ({ value: l.locationId, label: l.locationName }))
              ]}
            />
            
            <Select
              label="To Location"
              value={formData.toLocationId}
              onChange={(e) => setFormData({ ...formData, toLocationId: e.target.value })}
              options={[
                { value: '', label: 'None' },
                ...dropdownData.locations.map(l => ({ value: l.locationId, label: l.locationName }))
              ]}
            />
            
            <Input
              label="Expected Return Date"
              type="date"
              value={formData.expectedReturnDate}
              onChange={(e) => setFormData({ ...formData, expectedReturnDate: e.target.value })}
            />
            
            <Input
              label="Condition Before"
              value={formData.conditionBefore}
              onChange={(e) => setFormData({ ...formData, conditionBefore: e.target.value })}
              placeholder="e.g., Good, Fair, Poor"
            />
            
            <div className="transaction-form__full-width">
              <Input
                label="Notes"
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                placeholder="Additional notes..."
              />
            </div>
          </div>
          
          <div className="modal-actions">
            <Button variant="outline" onClick={() => setShowModal(false)}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              {editingTransaction ? 'Update' : 'Create'} Transaction
            </Button>
          </div>
        </form>
      </Modal>

      {/* Return Asset Modal */}
      <Modal
        isOpen={showReturnModal}
        onClose={() => setShowReturnModal(false)}
        title="Return Asset"
        size="md"
      >
        <div className="return-form">
          <p className="return-form__asset">
            Returning: <strong>{selectedTransaction?.assetCode} - {selectedTransaction?.assetName}</strong>
          </p>
          
          <Input
            label="Actual Return Date"
            type="date"
            value={returnData.actualReturnDate}
            onChange={(e) => setReturnData({ ...returnData, actualReturnDate: e.target.value })}
            required
          />
          
          <Select
            label="Condition After"
            value={returnData.conditionAfter}
            onChange={(e) => setReturnData({ ...returnData, conditionAfter: e.target.value })}
            options={[
              { value: 'Good', label: 'Good' },
              { value: 'Fair', label: 'Fair' },
              { value: 'Poor', label: 'Poor' },
              { value: 'Damaged', label: 'Damaged' }
            ]}
            required
          />
          
          <Input
            label="Notes"
            value={returnData.notes}
            onChange={(e) => setReturnData({ ...returnData, notes: e.target.value })}
            placeholder="Return notes..."
          />
          
          <div className="modal-actions">
            <Button variant="outline" onClick={() => setShowReturnModal(false)}>
              Cancel
            </Button>
            <Button variant="primary" onClick={handleReturnSubmit}>
              Confirm Return
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
};

export default AssetTransactionsMenu;