import React, { useState, useMemo, useCallback, useEffect } from "react";
import { Grid, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import CategoriesData from "./Categories.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import IconButton from "../../components/atoms/IconButton/IconButton";
import Button from "../../components/atoms/Button/Button";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import { getStatusChipStyles } from "../../core/constants/statusColors";
import { FiEdit2, FiTrash2, FiCheckSquare } from "react-icons/fi";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudFormBase } from "../../hooks/useCrudFormBase";
import { cleanCategoryFormData } from "../../core/utils/formHelpers";
import "./Categories.scss";

const categoriesData = new CategoriesData();
categoriesData.transformFormData = cleanCategoryFormData;

const INITIAL_FORM_DATA = {
  categoryName: "",
  description: "",
  parentCategoryId: null,
};

const TABS = [
  { id: "all", label: "All" },
  { id: "active", label: "Active" },
  { id: "inactive", label: "Inactive" },
];

const CategoriesMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedRows, setSelectedRows] = useState([]);
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const queryClient = useQueryClient();
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
    },
  });

  // Show checkbox only for Active and Inactive tabs
  const showCheckbox = activeTab === "active" || activeTab === "inactive";

  const buildFilters = useCallback(() => {
    const filters = {};
    if (activeTab === "active") {
      filters.isActive = true;
    } else if (activeTab === "inactive") {
      filters.isActive = false;
    }
    if (searchTerm) {
      filters.search = searchTerm;
    }
    return filters;
  }, [activeTab, searchTerm]);

  const fetchGridData = useCallback(async (params) => {
    const filters = buildFilters();
    const requestParams = {
      page: params.page || 1,
      pageSize: params.pageSize || 10,
      ...filters,
      ...params,
    };
    
    const result = await categoriesData.fetchGridData(requestParams);
    return result;
  }, [buildFilters]);

  const {
    data: categories,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    reload
  } = useGridData(['categories', activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    reload();
    setSelectedRows([]);
  }, [activeTab, reload]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
    setSelectedRows([]);
  }, [setPage]);

  const handleDelete = useCallback(async (cat) => {
    const r = await categoriesData.delete(cat.categoryId);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'categories'] });
      reload();
    }
  }, [reload, queryClient]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
    let successCount = 0;
    for (const id of ids) {
      // Fetch current category
      const result = await categoriesData.fetchById(id);
      if (result.success && result.data) {
        const updateData = { ...result.data, isActive: activate };
        const updateResult = await categoriesData.update(id, updateData);
        if (updateResult.success) successCount++;
      }
    }
    if (successCount > 0) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'categories'] });
      reload();
      setSelectedRows([]);
    }
    return successCount;
  }, [reload, queryClient]);

  const onSubmit = useCallback(async () => {
    const submitData = { 
      ...formData, 
      isActive: editingCategory ? editingCategory.isActive : true 
    };
    
    if (submitData.parentCategoryId === "" || submitData.parentCategoryId === null || submitData.parentCategoryId === undefined) {
      submitData.parentCategoryId = null;
    }
    
    Object.keys(submitData).forEach(key => {
      setFormField(key)(submitData[key]);
    });
    
    const success = await crudHandleSubmit();
    if (success) {
      reload();
    }
    return success;
  }, [formData, editingCategory, crudHandleSubmit, setFormField, reload]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
    setSearchTerm("");
    setSelectedRows([]);
  }, [setPage]);

  const parentCategoryOptions = useMemo(() => [
    { value: "", label: "None (Top Level)" },
    ...parentCategories
      .filter(c => !editingCategory || c.value !== editingCategory.categoryId)
      .map(c => ({ value: c.value, label: c.label }))
  ], [parentCategories, editingCategory]);

  const columns = useMemo(() => [
    { field: "categoryName", headerName: "Name", flex: 1, minWidth: 200 },
    { field: "description", headerName: "Description", flex: 1, minWidth: 200 },
    { field: "parentCategoryName", headerName: "Parent", width: 180 },
    { field: "childCount", headerName: "Subcategories", width: 130 },
    {
      field: "isActive",
      headerName: "Status",
      width: 110,
      renderCell: (p) => {
        const status = p.value ? 'Active' : 'Inactive';
        return <Chip label={status} size="small" sx={getStatusChipStyles(status)} />;
      },
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

  const extraActions = (
    <>
      {selectedRows.length > 0 && showCheckbox && (
        <Button variant="primary" onClick={() => setShowBulkActivateModal(true)} startIcon={<FiCheckSquare />}>
          {activeTab === "active" ? "Deactivate" : "Activate"} ({selectedRows.length})
        </Button>
      )}
    </>
  );

  if (loading && !categories.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === "active" ? false : true;
  const bulkButtonText = activeTab === "active" ? "Deactivate" : "Activate";

  return (
    <div className="categories-menu">
      <GridView
        title="Category Management"
        tabs={TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={categories}
        loading={loading}
        page={page}
        totalPages={Math.ceil(totalCount / pageSize) || 1}
        pageSize={pageSize}
        totalItems={totalCount}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onSearch={handleSearch}
        showCheckbox={showCheckbox}
        selectedRows={selectedRows}
        onSelectionChange={setSelectedRows}
        createButtonText="Add Category"
        ariaLabel="Categories data table"
        extraActions={extraActions}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingCategory ? "Edit Category" : "Add Category"}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingCategory ? "Update" : "Create"}
        size="md"
      >
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Input 
              label="Category Name" 
              value={formData.categoryName || ""} 
              onChange={(e) => setFormField('categoryName')(e.target.value)} 
              required 
            />
          </Grid>
          <Grid item xs={12}>
            <Input 
              label="Description" 
              value={formData.description || ""} 
              onChange={(e) => setFormField('description')(e.target.value)} 
              multiline 
              rows={2} 
            />
          </Grid>
          <Grid item xs={12}>
            <Select
              label="Parent Category"
              value={formData.parentCategoryId || ""}
              onChange={(e) => setFormField('parentCategoryId')(e.target.value || null)}
              options={parentCategoryOptions}
            />
          </Grid>
        </Grid>
      </CrudModal>

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={selectedRows}
        itemName="categories"
        title={bulkButtonText === "Activate" ? "Activate Categories" : "Deactivate Categories"}
        description={`This action will ${bulkButtonText.toLowerCase()} the selected categories.`}
      />
    </div>
  );
};

export default CategoriesMenu;