import React, { useState, useMemo, useCallback, useEffect } from "react";
import { Grid } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import CategoriesData from "./Categories.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import FormSection from "../../components/atoms/FormSection/FormSection";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import StatusChip from "../../components/atoms/StatusChip/StatusChip";
import Button from "../../components/atoms/Button/Button";
import { FiCheckSquare } from "react-icons/fi";
import { ACTION_TYPES, useGridActions } from "../../hooks/useGridActions";
import { useBulkSelection } from "../../hooks/useBulkSelection";
import { useSweetAlert } from "../../hooks/useSweetAlert";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudFormBase } from "../../hooks/useCrudFormBase";
import { useOptions } from "../../hooks/useOptions";
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
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const queryClient = useQueryClient();
  const { confirm, modal } = useSweetAlert(); // HAPUS confirmDelete
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
    idField: "categoryId",
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reference", "categories"] });
    },
  });

  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } = useBulkSelection({ idField: "categoryId" });

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
  } = useGridData(["categories", activeTab, searchTerm], fetchGridData);

  useEffect(() => {
    reload();
    clearSelection();
  }, [activeTab, reload, clearSelection]);

  const handleSearch = useCallback((search) => {
    setSearchTerm(search);
    setPage(1);
    clearSelection();
  }, [setPage, clearSelection]);

  // HAPUS confirmDelete - BaseData yang handle
  const handleDelete = useCallback(async (cat) => {
    const r = await categoriesData.delete(cat.categoryId);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ["reference", "categories"] });
      reload();
      clearSelection();
    }
  }, [reload, queryClient, clearSelection]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
    const actionText = activate ? "activate" : "deactivate";
    const confirmed = await confirm({
      title: activate ? "Activate Categories" : "Deactivate Categories",
      text: `Are you sure you want to ${actionText} ${ids.length} categor(ies)?`,
      confirmButtonText: activate ? "Yes, Activate" : "Yes, Deactivate",
    });
    if (!confirmed) return;

    let successCount = 0;
    for (const id of ids) {
      const result = await categoriesData.fetchById(id);
      if (result.success && result.data) {
        const updateData = { ...result.data, isActive: activate };
        const updateResult = await categoriesData.update(id, updateData);
        if (updateResult.success) successCount++;
      }
    }
    if (successCount > 0) {
      queryClient.invalidateQueries({ queryKey: ["reference", "categories"] });
      reload();
      clearSelection();
    }
  }, [reload, queryClient, confirm, clearSelection]);

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
      clearSelection();
    }
    return success;
  }, [formData, editingCategory, crudHandleSubmit, setFormField, reload, clearSelection]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
    setSearchTerm("");
    clearSelection();
  }, [setPage, clearSelection]);

  const handleGridAction = useCallback((actionType, row) => {
    switch (actionType) {
      case ACTION_TYPES.EDIT:
        handleEdit(row);
        break;
      case ACTION_TYPES.DELETE:
        handleDelete(row);
        break;
      default:
        break;
    }
  }, [handleEdit, handleDelete]);

  const getConditionalActions = useCallback(() => {
    return [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE];
  }, []);

  const { actionColumn } = useGridActions({
    actions: [ACTION_TYPES.EDIT, ACTION_TYPES.DELETE],
    onAction: handleGridAction,
    getConditionalActions,
    rowIdField: "categoryId",
  });

  const parentCategoryOptions = useOptions(
    parentCategories.filter(c => !editingCategory || c.value !== editingCategory.categoryId),
    "None (Top Level)"
  );

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
        const status = p.value ? "Active" : "Inactive";
        return <StatusChip status={status} />;
      },
    },
    actionColumn,
  ], [actionColumn]);

  const extraActions = (
    <>
      {hasSelection && showCheckbox && (
        <Button variant="primary" size="sm" onClick={() => setShowBulkActivateModal(true)} className="u-inline-flex u-btn-gap">
          <FiCheckSquare size={16} /> {activeTab === "active" ? "Deactivate" : "Activate"} ({selectionCount})
        </Button>
      )}
    </>
  );

  if (loading && !categories.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === "active" ? false : true;
  const bulkButtonText = activeTab === "active" ? "Deactivate" : "Activate";
  const bulkTitle = bulkButtonText === "Activate" ? "Activate Categories" : "Deactivate Categories";
  const bulkDescription = `This action will ${bulkButtonText.toLowerCase()} the selected categories.`;

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
        title={editingCategory ? "Edit Category" : "Add Category"}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingCategory ? "Update" : "Create"}
        size="md"
      >
        <FormSection title="Basic Information" description="Category name and description">
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Input
                label="Category Name"
                value={formData.categoryName || ""}
                onChange={(e) => setFormField("categoryName")(e.target.value)}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <Input
                label="Description"
                value={formData.description || ""}
                onChange={(e) => setFormField("description")(e.target.value)}
                multiline
                rows={2}
              />
            </Grid>
            <Grid item xs={12}>
              <Select
                label="Parent Category"
                value={formData.parentCategoryId || ""}
                onChange={(e) => setFormField("parentCategoryId")(e.target.value || null)}
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
        selectedIds={getSelectedIds(categories)}
        itemName="categories"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default CategoriesMenu;