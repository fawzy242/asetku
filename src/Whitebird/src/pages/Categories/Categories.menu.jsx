import React, { useState, useEffect, useCallback } from 'react';
import CategoriesData from './Categories.data';
import DataTable from '../../components/molecules/DataTable/DataTable';
import Pagination from '../../components/molecules/Pagination/Pagination';
import SearchBar from '../../components/molecules/SearchBar/SearchBar';
import Button from '../../components/atoms/Button/Button';
import Badge from '../../components/atoms/Badge/Badge';
import Modal from '../../components/molecules/Modal/Modal';
import Input from '../../components/atoms/Input/Input';
import Select from '../../components/atoms/Select/Select';
import Spinner from '../../components/atoms/Spinner/Spinner';

const CategoriesMenu = () => {
  const [loading, setLoading] = useState(true);
  const [categories, setCategories] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');
  
  const [showModal, setShowModal] = useState(false);
  const [editingCategory, setEditingCategory] = useState(null);
  const [formData, setFormData] = useState({
    categoryName: '',
    description: '',
    parentCategoryId: ''
  });
  const [parentCategories, setParentCategories] = useState([]);

  const categoriesData = new CategoriesData();

  const loadData = useCallback(async () => {
    setLoading(true);
    
    const result = await categoriesData.loadGridData(page, pageSize, search);
    
    if (result.success) {
      setCategories(result.data.data || []);
      setTotalCount(result.data.totalCount || 0);
    }
    
    setLoading(false);
  }, [page, pageSize, search]);

  const loadParentCategories = async () => {
    const result = await categoriesData.loadParentCategories();
    if (result.success) {
      setParentCategories(result.data);
    }
  };

  useEffect(() => {
    loadData();
    loadParentCategories();
  }, [loadData]);

  const handleSearch = (value) => {
    setSearch(value);
    setPage(1);
  };

  const handleCreate = () => {
    setEditingCategory(null);
    setFormData({
      categoryName: '',
      description: '',
      parentCategoryId: ''
    });
    setShowModal(true);
  };

  const handleEdit = async (category) => {
    const result = await categoriesData.loadCategory(category.categoryId);
    if (result.success) {
      setEditingCategory(result.data);
      setFormData({
        categoryName: result.data.categoryName || '',
        description: result.data.description || '',
        parentCategoryId: result.data.parentCategoryId || ''
      });
      setShowModal(true);
    }
  };

  const handleDelete = async (category) => {
    const result = await categoriesData.deleteCategory(category.categoryId);
    if (result.success) {
      loadData();
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!formData.categoryName.trim()) {
      Swal.fire({
        title: 'Error',
        text: 'Category name is required',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return;
    }
    
    let result;
    if (editingCategory) {
      result = await categoriesData.updateCategory(editingCategory.categoryId, formData);
    } else {
      result = await categoriesData.createCategory(formData);
    }
    
    if (result.success) {
      setShowModal(false);
      loadData();
    }
  };

  const columns = [
    { field: 'categoryName', headerName: 'Name', width: 250 },
    { field: 'description', headerName: 'Description', width: 300 },
    { field: 'parentCategoryName', headerName: 'Parent Category', width: 200 },
    { field: 'childCount', headerName: 'Subcategories', width: 120 },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 100,
      renderCell: (params) => (
        <Badge variant={params.value ? 'success' : 'secondary'}>
          {params.value ? 'Active' : 'Inactive'}
        </Badge>
      )
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

  if (loading && categories.length === 0) {
    return (
      <div className="categories-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="categories-menu">
      <div className="categories-menu__header">
        <h2 className="categories-menu__title">Category Management</h2>
        <Button variant="primary" onClick={handleCreate}>
          + Add Category
        </Button>
      </div>
      
      <div className="categories-menu__toolbar">
        <SearchBar
          placeholder="Search categories..."
          onSearch={handleSearch}
          debounceTime={300}
        />
      </div>
      
      <div className="categories-menu__table">
        <DataTable
          rows={categories}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.categoryId}
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

      {/* Category Form Modal */}
      <Modal
        isOpen={showModal}
        onClose={() => setShowModal(false)}
        title={editingCategory ? 'Edit Category' : 'Add New Category'}
        size="md"
      >
        <form className="category-form" onSubmit={handleSubmit}>
          <Input
            label="Category Name"
            value={formData.categoryName}
            onChange={(e) => setFormData({ ...formData, categoryName: e.target.value })}
            required
          />
          
          <Input
            label="Description"
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            placeholder="Optional description"
          />
          
          <Select
            label="Parent Category"
            value={formData.parentCategoryId}
            onChange={(e) => setFormData({ ...formData, parentCategoryId: e.target.value })}
            options={[
              { value: '', label: 'None (Top Level)' },
              ...parentCategories
                .filter(c => !editingCategory || c.categoryId !== editingCategory.categoryId)
                .map(c => ({ value: c.categoryId, label: c.categoryName }))
            ]}
          />
          
          <div className="modal-actions">
            <Button variant="outline" onClick={() => setShowModal(false)}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              {editingCategory ? 'Update' : 'Create'} Category
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default CategoriesMenu;