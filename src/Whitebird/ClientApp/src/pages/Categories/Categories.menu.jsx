import React, { useState, useMemo, useCallback, useEffect } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import CategoriesData from "./Categories.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Modal from "../../components/molecules/Modal/Modal";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import SearchToolbar from "../../components/molecules/SearchToolbar/SearchToolbar";
import Tabs from "../../components/molecules/Tabs/Tabs";
import IconButton from "../../components/atoms/IconButton/IconButton";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudForm } from "../../hooks/useCrudForm";
import "./Categories.scss";

const categoriesData = new CategoriesData();

const INITIAL_FORM_DATA = {
  categoryName: "", description: "", parentCategoryId: "",
};

const CRUD_OPTIONS = {
  idField: 'categoryId',
};

const TABS = [
  { id: "all", label: "All" },
  { id: "active", label: "Active" },
  { id: "inactive", label: "Inactive" },
];

const CategoriesMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");
  const queryClient = useQueryClient();

  const { categories: parentCategories } = useReferenceData();

  const {
    showModal,
    editingRecord: editingCategory,
    isSubmitting,
    formData,
    setFormData,
    handleCreate,
    handleEdit,
    handleClose,
  } = useCrudForm(INITIAL_FORM_DATA, categoriesData, CRUD_OPTIONS);

  const fetchGridData = useCallback(async (params) => {
    const filters = { 
      page: params.page || 1,
      pageSize: params.pageSize || 10,
      search: params.search || searchTerm,
      ...params 
    };
    const result = await categoriesData.fetchGridData(filters);
    if (result.success) {
      const rawData = result.data;
      let dataArray = [];

      if (rawData?.data?.data && Array.isArray(rawData.data.data)) {
        dataArray = rawData.data.data;
      } else if (rawData?.data && Array.isArray(rawData.data)) {
        dataArray = rawData.data;
      } else if (Array.isArray(rawData)) {
        dataArray = rawData;
      }

      if (activeTab === "active") {
        dataArray = dataArray.filter(c => c.isActive === true);
      } else if (activeTab === "inactive") {
        dataArray = dataArray.filter(c => c.isActive === false);
      }

      return { success: true, data: { data: dataArray, totalCount: dataArray.length } };
    }
    return result;
  }, [activeTab, searchTerm]);

  const {
    data: categories,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    updateFilters,
    reload
  } = useGridData(['categories', activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    reload();
  }, [activeTab, reload]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    updateFilters({ search });
    setPage(1);
  }, [updateFilters, setPage]);

  const handleDelete = useCallback(async (cat) => {
    const r = await categoriesData.delete(cat.categoryId);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'categories'] });
      reload();
    }
  }, [reload, queryClient]);

  const onSubmit = useCallback(async (e) => {
    e.preventDefault();
    if (!formData.categoryName.trim()) return;
    if (isSubmitting) return;
    
    const data = { ...formData, isActive: editingCategory ? editingCategory.isActive : true };
    if (data.parentCategoryId === "" || data.parentCategoryId === undefined || data.parentCategoryId === null) {
      data.parentCategoryId = null;
    }
    
    const r = editingCategory
      ? await categoriesData.update(editingCategory.categoryId, data)
      : await categoriesData.create(data);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'categories'] });
      handleClose();
      reload();
    }
  }, [formData, isSubmitting, editingCategory, handleClose, reload, queryClient]);

  const columns = useMemo(() => [
    { field: "categoryName", headerName: "Name", flex: 1, minWidth: 200 },
    { field: "description", headerName: "Description", flex: 1, minWidth: 200 },
    { field: "parentCategoryName", headerName: "Parent", width: 180 },
    { field: "childCount", headerName: "Subcategories", width: 130 },
    {
      field: "isActive",
      headerName: "Status",
      width: 110,
      renderCell: (p) => (
        <Chip
          label={p.value ? 'Active' : 'Inactive'}
          size="small"
          sx={{
            bgcolor: p.value ? 'rgba(16, 185, 129, 0.1)' : 'rgba(107, 114, 128, 0.1)',
            color: p.value ? '#10b981' : '#6b7280',
            fontWeight: 500,
            fontSize: '0.75rem',
            height: 24,
            borderRadius: '4px',
          }}
        />
      ),
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 100,
      sortable: false,
      renderCell: (p) => (
        <div className="table-actions">
          <IconButton onClick={() => handleEdit(p.row)} title="Edit category" size="lg">
            <FiEdit2 size={18} />
          </IconButton>
          <IconButton onClick={() => handleDelete(p.row)} title="Delete category" variant="danger" size="lg">
            <FiTrash2 size={18} />
          </IconButton>
        </div>
      )
    },
  ], [handleEdit, handleDelete]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
  }, [setPage]);

  if (loading && !categories.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="categories-menu">
      <div className="page-header">
        <h1 className="page-title">Category</h1>
        <PageHeader title="Category Management" buttonText="Add Category" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      </div>

      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} placeholder="Search by name..." />
      
      <div className="categories-menu__table" style={{ width: '100%', minWidth: 0 }}>
        <DataTable
          rows={categories}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.categoryId}
          hideFooter={true}
          autoHeight={true}
          ariaLabel="Categories data table"
        />
      </div>
      
      <Pagination
        currentPage={page}
        totalPages={Math.ceil(totalCount / pageSize) || 1}
        pageSize={pageSize}
        totalItems={totalCount}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
      />

      <Modal isOpen={showModal} onClose={handleClose} title={editingCategory ? "Edit Category" : "Add Category"} size="md">
        <form onSubmit={onSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input label="Category Name" value={formData.categoryName} onChange={e => setFormData({ ...formData, categoryName: e.target.value })} required />
            </Grid>
            <Grid item xs={12}>
              <Input label="Description" value={formData.description} onChange={e => setFormData({ ...formData, description: e.target.value })} multiline rows={2} />
            </Grid>
            <Grid item xs={12}>
              <Select
                label="Parent Category"
                value={formData.parentCategoryId || ""}
                onChange={e => setFormData({ ...formData, parentCategoryId: e.target.value || "" })}
                options={[
                  { value: "", label: "None (Top Level)" },
                  ...parentCategories
                    .filter(c => !editingCategory || c.value !== editingCategory.categoryId)
                    .map(c => ({ value: c.value, label: c.label }))
                ]}
              />
            </Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={handleClose} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>
              {editingCategory ? "Update" : "Create"}
            </Button>
          </Box>
        </form>
      </Modal>
    </div>
  );
};

export default CategoriesMenu;