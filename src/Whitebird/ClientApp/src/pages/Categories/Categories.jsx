import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { Grid } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { CategoriesData } from '../../core/services';
import GridView from '../../components/organisms/GridView/GridView';
import CrudModal from '../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../components/atoms/FormSection/FormSection';
import Input from '../../components/atoms/Input/Input';
import Select from '../../components/atoms/Select/Select';
import Spinner from '../../components/atoms/Spinner/Spinner';
import BulkActivateModal from '../../components/molecules/BulkActivateModal/BulkActivateModal';
import Button from '../../components/atoms/Button/Button';
import StatusChip from '../../components/atoms/StatusChip/StatusChip';
import { FiCheckSquare } from 'react-icons/fi';
import { ACTION_TYPES, useGridActions } from '../../hooks/useGridActions';
import { useBulkSelection } from '../../hooks/useBulkSelection';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useGridData } from '../../hooks/useGridData';
import { useReferenceData } from '../../hooks/useReferenceData';
import { useCrudFormBase } from '../../hooks/useCrudFormBase';
import { useOptions } from '../../hooks/useOptions';
import './Categories.scss';

const categoriesData = new CategoriesData();

const INITIAL_FORM_DATA = {
  categoryName: '',
  description: '',
  parentCategoryId: null,
};

const TABS = [
  { id: 'all', label: 'All' },
  { id: 'active', label: 'Active' },
  { id: 'inactive', label: 'Inactive' },
];

const Categories = () => {
  const [activeTab, setActiveTab] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirmDelete, confirm } = useSweetAlert();
  const { categories: parentCategories } = useReferenceData();

  const {
    showModal,
    editingRecord: editingCategory,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, categoriesData, {
    idField: 'categoryId',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reference', 'categories'] });
      toast.success(editingCategory ? 'Category updated successfully' : 'Category created successfully');
      reload();
      clearSelection();
    },
    onError: (error) => {
      toast.error(error || 'Failed to save category');
    },
  });

  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } =
    useBulkSelection({ idField: 'categoryId' });

  const showCheckbox = activeTab === 'active' || activeTab === 'inactive';

  const buildFilters = useCallback(() => {
    const filters = {};
    if (activeTab === 'active') filters.isActive = true;
    else if (activeTab === 'inactive') filters.isActive = false;
    if (searchTerm) filters.search = searchTerm;
    return filters;
  }, [activeTab, searchTerm]);

  const fetchGridData = useCallback(
    async (params) => {
      const filters = buildFilters();
      const requestParams = {
        page: params.page || 1,
        pageSize: params.pageSize || 10,
        ...filters,
        ...params,
      };
      const result = await categoriesData.fetchGridData(requestParams);
      return result;
    },
    [buildFilters]
  );

  const { data, totalCount, loading, page, setPage, pageSize, setPageSize, reload } = useGridData(
    ['categories', activeTab, searchTerm],
    fetchGridData
  );

  useEffect(() => {
    isMountedRef.current = true;
    reload();
    clearSelection();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload, clearSelection]);

  const handleSearch = useCallback(
    (search) => {
      setSearchTerm(search);
      setPage(1);
      clearSelection();
    },
    [setPage, clearSelection]
  );

  const handleDelete = useCallback(
    async (cat) => {
      const confirmed = await confirmDelete('Delete Category', `Are you sure you want to delete "${cat.categoryName}"?`);
      if (!confirmed) return;
      const r = await categoriesData.delete(cat.categoryId);
      if (r.success && isMountedRef.current) {
        toast.success('Category deleted successfully');
        queryClient.invalidateQueries({ queryKey: ['reference', 'categories'] });
        reload();
        clearSelection();
      }
    },
    [reload, queryClient, toast, confirmDelete, clearSelection]
  );

  const handleBulkActivate = useCallback(
    async (ids, activate) => {
      const actionText = activate ? 'activate' : 'deactivate';
      const confirmed = await confirm({
        title: activate ? 'Activate Categories' : 'Deactivate Categories',
        text: `Are you sure you want to ${actionText} ${ids.length} categor(y/ies)?`,
        confirmButtonText: activate ? 'Yes, Activate' : 'Yes, Deactivate',
      });
      if (!confirmed) return;
      const r = await categoriesData.bulkActivate(ids, activate);
      if (r.success && isMountedRef.current) {
        const count = r.data || ids.length;
        toast.success(`${count} categor(y/ies) ${actionText}d successfully`);
        clearSelection();
        queryClient.invalidateQueries({ queryKey: ['reference', 'categories'] });
        reload();
      }
    },
    [reload, queryClient, toast, confirm, clearSelection]
  );

  const handleTabChange = useCallback(
    (tab) => {
      setActiveTab(tab);
      setPage(1);
      setSearchTerm('');
      clearSelection();
    },
    [setPage, clearSelection]
  );

  const handleGridAction = useCallback(
    (actionType, row) => {
      if (actionType === ACTION_TYPES.EDIT) handleEdit(row);
      else if (actionType === ACTION_TYPES.DELETE) handleDelete(row);
    },
    [handleEdit, handleDelete]
  );

  const getConditionalActions = useCallback((row) => {
    const actions = [];
    if (row.canEdit !== false) actions.push(ACTION_TYPES.EDIT);
    if (row.canDelete !== false) actions.push(ACTION_TYPES.DELETE);
    return actions;
  }, []);

  const { actionColumn } = useGridActions({
    actions: [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleGridAction,
    getConditionalActions,
    rowIdField: 'categoryId',
  });

  const parentCategoryOptions = useOptions(
    parentCategories.filter((c) => !editingCategory || c.value !== editingCategory.categoryId),
    'None (Top Level)'
  );

  const columns = useMemo(
    () => [
      { field: 'categoryName', headerName: 'Name', flex: 1, minWidth: 200 },
      { field: 'description', headerName: 'Description', flex: 1, minWidth: 200 },
      { field: 'parentCategoryName', headerName: 'Parent', width: 180 },
      { field: 'childCount', headerName: 'Subcategories', width: 130 },
      {
        field: 'isActive',
        headerName: 'Status',
        width: 110,
        renderCell: (p) => {
          const status = p.value ? 'Active' : 'Inactive';
          return <StatusChip status={status} />;
        },
      },
      actionColumn,
    ],
    [actionColumn]
  );

  const extraActions = (
    <>
      {hasSelection && showCheckbox && (
        <Button
          variant="primary"
          size="sm"
          onClick={() => setShowBulkActivateModal(true)}
          className="u-inline-flex u-btn-gap"
        >
          <FiCheckSquare size={16} /> {activeTab === 'active' ? 'Deactivate' : 'Activate'} ({selectionCount})
        </Button>
      )}
    </>
  );

  if (loading && !data.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === 'active' ? false : true;
  const bulkTitle = bulkActivateValue ? 'Activate Categories' : 'Deactivate Categories';
  const bulkDescription = `This action will ${bulkActivateValue ? 'activate' : 'deactivate'} the selected categories.`;

  return (
    <div className="categories-menu">
      <GridView
        title="Category Management"
        tabs={TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={data}
        loading={loading}
        totalCount={totalCount}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onSearch={handleSearch}
        showCheckbox={showCheckbox}
        selectedRows={selectedRowIds}
        onSelectionChange={handleSelectionChange}
        createButtonText="Add Category"
        ariaLabel="Categories data table"
        extraActions={extraActions}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingCategory ? 'Edit Category' : 'Add Category'}
        onSubmit={crudHandleSubmit}
        isSubmitting={isSubmitting}
        submitText={editingCategory ? 'Update' : 'Create'}
        size="md"
      >
        <FormSection title="Basic Information" description="Category name and description">
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input
                label="Category Name"
                value={formData.categoryName || ''}
                onChange={(e) => setFormField('categoryName')(e.target.value)}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <Input
                label="Description"
                value={formData.description || ''}
                onChange={(e) => setFormField('description')(e.target.value)}
                multiline
                rows={2}
              />
            </Grid>
            <Grid item xs={12}>
              <Select
                label="Parent Category"
                value={formData.parentCategoryId || ''}
                onChange={(e) => setFormField('parentCategoryId')(e.target.value || null)}
                options={parentCategoryOptions}
              />
            </Grid>
          </Grid>
        </FormSection>
      </CrudModal>

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={getSelectedIds(data)}
        itemName="categories"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default Categories;