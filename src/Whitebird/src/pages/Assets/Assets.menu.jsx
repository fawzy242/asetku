import React, { useState, useEffect, useCallback } from 'react';
import AssetsData from './Assets.data';
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

const AssetsMenu = () => {
  const [loading, setLoading] = useState(true);
  const [assets, setAssets] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState('assetCode');
  const [sortDescending, setSortDescending] = useState(false);
  
  const [showModal, setShowModal] = useState(false);
  const [editingAsset, setEditingAsset] = useState(null);
  const [formData, setFormData] = useState({
    assetName: '',
    categoryId: '',
    subCategory: '',
    brand: '',
    model: '',
    serialNumber: '',
    purchaseDate: '',
    purchasePrice: '',
    condition: 'Good',
    status: 'Available',
    location: '',
    currentHolderId: '',
    supplierId: ''
  });
  
  const [dropdownData, setDropdownData] = useState({
    categories: [],
    suppliers: [],
    employees: []
  });

  const assetsData = new AssetsData();

  const loadData = useCallback(async () => {
    setLoading(true);
    
    const result = await assetsData.loadGridData(page, pageSize, search, sortBy, sortDescending);
    
    if (result.success) {
      setAssets(result.data.data || []);
      setTotalCount(result.data.totalCount || 0);
    }
    
    setLoading(false);
  }, [page, pageSize, search, sortBy, sortDescending]);

  const loadDropdowns = async () => {
    const result = await assetsData.loadDropdownData();
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
    console.log('Filters:', filters);
  };

  const handleCreate = () => {
    setEditingAsset(null);
    setFormData({
      assetName: '',
      categoryId: '',
      subCategory: '',
      brand: '',
      model: '',
      serialNumber: '',
      purchaseDate: '',
      purchasePrice: '',
      condition: 'Good',
      status: 'Available',
      location: '',
      currentHolderId: '',
      supplierId: ''
    });
    setShowModal(true);
  };

  const handleEdit = async (asset) => {
    const result = await assetsData.loadAsset(asset.assetId);
    if (result.success) {
      setEditingAsset(result.data);
      setFormData({
        assetName: result.data.assetName || '',
        categoryId: result.data.categoryId || '',
        subCategory: result.data.subCategory || '',
        brand: result.data.brand || '',
        model: result.data.model || '',
        serialNumber: result.data.serialNumber || '',
        purchaseDate: result.data.purchaseDate ? result.data.purchaseDate.split('T')[0] : '',
        purchasePrice: result.data.purchasePrice || '',
        condition: result.data.condition || 'Good',
        status: result.data.status || 'Available',
        location: result.data.location || '',
        currentHolderId: result.data.currentHolderId || '',
        supplierId: result.data.supplierId || ''
      });
      setShowModal(true);
    }
  };

  const handleDelete = async (asset) => {
    const result = await assetsData.deleteAsset(asset.assetId);
    if (result.success) {
      loadData();
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    let result;
    if (editingAsset) {
      result = await assetsData.updateAsset(editingAsset.assetId, formData);
    } else {
      result = await assetsData.createAsset(formData);
    }
    
    if (result.success) {
      setShowModal(false);
      loadData();
    }
  };

  const columns = [
    { field: 'assetCode', headerName: 'Code', width: 120 },
    { field: 'assetName', headerName: 'Name', width: 200 },
    { field: 'categoryName', headerName: 'Category', width: 150 },
    { field: 'brand', headerName: 'Brand', width: 120 },
    { field: 'model', headerName: 'Model', width: 120 },
    { field: 'serialNumber', headerName: 'Serial Number', width: 150 },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      renderCell: (params) => (
        <Badge variant={utilsHelper.getStatusColor(params.value)}>
          {params.value}
        </Badge>
      )
    },
    {
      field: 'condition',
      headerName: 'Condition',
      width: 100
    },
    {
      field: 'purchasePrice',
      headerName: 'Price',
      width: 130,
      valueFormatter: (params) => utilsHelper.formatCurrency(params.value)
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 150,
      sortable: false,
      renderCell: (params) => (
        <div className="table-actions">
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
            onClick={() => handleDelete(params.row)}
          >
            🗑️
          </Button>
        </div>
      )
    }
  ];

  if (loading && assets.length === 0) {
    return (
      <div className="assets-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="assets-menu">
      <div className="assets-menu__header">
        <h2 className="assets-menu__title">Asset Management</h2>
        <Button variant="primary" onClick={handleCreate}>
          + Add Asset
        </Button>
      </div>
      
      <div className="assets-menu__toolbar">
        <SearchBar
          placeholder="Search assets..."
          onSearch={handleSearch}
          debounceTime={300}
        />
        <FilterBar
          filters={[
            { field: 'status', label: 'Status', type: 'select', options: [
              { value: 'Available', label: 'Available' },
              { value: 'Assigned', label: 'Assigned' },
              { value: 'Under Repair', label: 'Under Repair' },
              { value: 'Retired', label: 'Retired' }
            ]},
            { field: 'categoryId', label: 'Category', type: 'select', options: 
              dropdownData.categories.map(c => ({ value: c.categoryId, label: c.categoryName }))
            }
          ]}
          onFilter={handleFilter}
        />
      </div>
      
      <div className="assets-menu__table">
        <DataTable
          rows={assets}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.assetId}
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

      {/* Asset Form Modal */}
      <Modal
        isOpen={showModal}
        onClose={() => setShowModal(false)}
        title={editingAsset ? 'Edit Asset' : 'Add New Asset'}
        size="lg"
      >
        <form className="asset-form" onSubmit={handleSubmit}>
          <div className="asset-form__grid">
            <Input
              label="Asset Name"
              value={formData.assetName}
              onChange={(e) => setFormData({ ...formData, assetName: e.target.value })}
              required
            />
            
            <Select
              label="Category"
              value={formData.categoryId}
              onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
              options={dropdownData.categories.map(c => ({ value: c.categoryId, label: c.categoryName }))}
              required
            />
            
            <Input
              label="Sub Category"
              value={formData.subCategory}
              onChange={(e) => setFormData({ ...formData, subCategory: e.target.value })}
            />
            
            <Input
              label="Brand"
              value={formData.brand}
              onChange={(e) => setFormData({ ...formData, brand: e.target.value })}
            />
            
            <Input
              label="Model"
              value={formData.model}
              onChange={(e) => setFormData({ ...formData, model: e.target.value })}
            />
            
            <Input
              label="Serial Number"
              value={formData.serialNumber}
              onChange={(e) => setFormData({ ...formData, serialNumber: e.target.value })}
            />
            
            <Input
              label="Purchase Date"
              type="date"
              value={formData.purchaseDate}
              onChange={(e) => setFormData({ ...formData, purchaseDate: e.target.value })}
            />
            
            <Input
              label="Purchase Price"
              type="number"
              value={formData.purchasePrice}
              onChange={(e) => setFormData({ ...formData, purchasePrice: e.target.value })}
            />
            
            <Select
              label="Condition"
              value={formData.condition}
              onChange={(e) => setFormData({ ...formData, condition: e.target.value })}
              options={[
                { value: 'Good', label: 'Good' },
                { value: 'Fair', label: 'Fair' },
                { value: 'Poor', label: 'Poor' }
              ]}
            />
            
            <Select
              label="Status"
              value={formData.status}
              onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              options={[
                { value: 'Available', label: 'Available' },
                { value: 'Assigned', label: 'Assigned' },
                { value: 'Under Repair', label: 'Under Repair' },
                { value: 'Retired', label: 'Retired' }
              ]}
            />
            
            <Input
              label="Location"
              value={formData.location}
              onChange={(e) => setFormData({ ...formData, location: e.target.value })}
            />
            
            <Select
              label="Current Holder"
              value={formData.currentHolderId}
              onChange={(e) => setFormData({ ...formData, currentHolderId: e.target.value })}
              options={[
                { value: '', label: 'None' },
                ...dropdownData.employees.map(e => ({ value: e.employeeId, label: e.fullName }))
              ]}
            />
            
            <Select
              label="Supplier"
              value={formData.supplierId}
              onChange={(e) => setFormData({ ...formData, supplierId: e.target.value })}
              options={[
                { value: '', label: 'None' },
                ...dropdownData.suppliers.map(s => ({ value: s.supplierId, label: s.supplierName }))
              ]}
            />
          </div>
          
          <div className="modal-actions">
            <Button variant="outline" onClick={() => setShowModal(false)}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              {editingAsset ? 'Update' : 'Create'} Asset
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default AssetsMenu;