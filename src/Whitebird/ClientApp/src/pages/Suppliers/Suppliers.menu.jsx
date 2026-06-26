import React, { useState, useMemo, useCallback, useEffect } from "react";
import { Grid } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import SuppliersData from "./Suppliers.data";
import GridView from "../../components/organisms/GridView/GridView";
import CrudModal from "../../components/molecules/CrudModal/CrudModal";
import FormSection from "../../components/atoms/FormSection/FormSection";
import Input from "../../components/atoms/Input/Input";
import Spinner from "../../components/atoms/Spinner/Spinner";
import FileUploader from "../../components/molecules/FileUploader/FileUploader";
import BulkActivateModal from "../../components/molecules/BulkActivateModal/BulkActivateModal";
import StatusChip from "../../components/atoms/StatusChip/StatusChip";
import Button from "../../components/atoms/Button/Button";
import { FiCheckSquare } from "react-icons/fi";
import { ACTION_TYPES, useGridActions } from "../../hooks/useGridActions";
import { useBulkSelection } from "../../hooks/useBulkSelection";
import { useSweetAlert } from "../../hooks/useSweetAlert";
import { useGridData } from "../../hooks/useGridData";
import { useCrudFormBase } from "../../hooks/useCrudFormBase";
import { cleanSupplierFormData } from "../../core/utils/formHelpers";
import "./Suppliers.scss";

const suppliersData = new SuppliersData();
suppliersData.transformFormData = cleanSupplierFormData;

const INITIAL_FORM_DATA = {
  supplierName: "",
  contactPerson: "",
  phoneNumber: "",
  email: "",
  address: ""
};

const TABS = [
  { id: "all", label: "All" },
  { id: "active", label: "Active" },
  { id: "inactive", label: "Inactive" },
];

const SuppliersMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");
  const [showBulkActivateModal, setShowBulkActivateModal] = useState(false);
  const queryClient = useQueryClient();
  const { confirm, modal } = useSweetAlert(); // HAPUS confirmDelete

  const {
    showModal,
    editingRecord: editingSupplier,
    isSubmitting,
    formData,
    setFormField,
    handleCreate,
    handleEdit,
    handleClose,
    handleSubmit: crudHandleSubmit,
  } = useCrudFormBase(INITIAL_FORM_DATA, suppliersData, {
    idField: "supplierId",
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reference", "suppliers"] });
    },
  });

  const { selectedRowIds, selectionCount, hasSelection, handleSelectionChange, clearSelection, getSelectedIds } = useBulkSelection({ idField: "supplierId" });

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
    const result = await suppliersData.fetchGridData(requestParams);
    return result;
  }, [buildFilters]);

  const {
    data: suppliers,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    reload
  } = useGridData(["suppliers", activeTab, searchTerm], fetchGridData);

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
  const handleDelete = useCallback(async (sup) => {
    const r = await suppliersData.delete(sup.supplierId);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ["reference", "suppliers"] });
      reload();
      clearSelection();
    }
  }, [reload, queryClient, clearSelection]);

  const handleBulkActivate = useCallback(async (ids, activate) => {
    const actionText = activate ? "activate" : "deactivate";
    const confirmed = await confirm({
      title: activate ? "Activate Suppliers" : "Deactivate Suppliers",
      text: `Are you sure you want to ${actionText} ${ids.length} supplier(s)?`,
      confirmButtonText: activate ? "Yes, Activate" : "Yes, Deactivate",
    });
    if (!confirmed) return;

    let successCount = 0;
    for (const id of ids) {
      const result = await suppliersData.fetchById(id);
      if (result.success && result.data) {
        const updateData = { ...result.data, isActive: activate };
        const updateResult = await suppliersData.update(id, updateData);
        if (updateResult.success) successCount++;
      }
    }
    if (successCount > 0) {
      queryClient.invalidateQueries({ queryKey: ["reference", "suppliers"] });
      reload();
      clearSelection();
    }
  }, [reload, queryClient, confirm, clearSelection]);

  const onSubmit = useCallback(async () => {
    const submitData = {
      ...formData,
      isActive: editingSupplier ? editingSupplier.isActive : true
    };
    if (submitData.contactPerson === "") submitData.contactPerson = null;
    if (submitData.phoneNumber === "") submitData.phoneNumber = null;
    if (submitData.email === "") submitData.email = null;
    if (submitData.address === "") submitData.address = null;
    Object.keys(submitData).forEach(key => {
      setFormField(key)(submitData[key]);
    });
    const success = await crudHandleSubmit();
    if (success) {
      reload();
      clearSelection();
    }
    return success;
  }, [formData, editingSupplier, crudHandleSubmit, setFormField, reload, clearSelection]);

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
    rowIdField: "supplierId",
  });

  const columns = useMemo(() => [
    { field: "supplierName", headerName: "Name", flex: 1, minWidth: 180 },
    { field: "contactPerson", headerName: "Contact", width: 160 },
    { field: "phoneNumber", headerName: "Phone", width: 140 },
    { field: "email", headerName: "Email", width: 200 },
    { field: "assetCount", headerName: "Assets", width: 90 },
    {
      field: "isActive",
      headerName: "Status",
      width: 110,
      renderCell: (p) => {
        const status = p?.value ? "Active" : "Inactive";
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

  if (loading && !suppliers.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  const bulkActivateValue = activeTab === "active" ? false : true;
  const bulkButtonText = activeTab === "active" ? "Deactivate" : "Activate";
  const bulkTitle = bulkButtonText === "Activate" ? "Activate Suppliers" : "Deactivate Suppliers";
  const bulkDescription = `This action will ${bulkButtonText.toLowerCase()} the selected suppliers.`;

  return (
    <div className="suppliers-menu">
      <GridView
        title="Supplier Management"
        tabs={TABS}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onCreate={handleCreate}
        columns={columns}
        data={suppliers}
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
        createButtonText="Add Supplier"
        ariaLabel="Suppliers data table"
        extraActions={extraActions}
      />

      <CrudModal
        isOpen={showModal}
        onClose={handleClose}
        title={editingSupplier ? "Edit Supplier" : "Add Supplier"}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingSupplier ? "Update" : "Create"}
        size="lg"
      >
        <FormSection title="Basic Information" description="Supplier name and contact person">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input
                label="Supplier Name"
                value={formData.supplierName || ""}
                onChange={(e) => setFormField("supplierName")(e.target.value)}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Contact Person"
                value={formData.contactPerson || ""}
                onChange={(e) => setFormField("contactPerson")(e.target.value)}
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Contact Information" description="Phone, email, and address">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input
                label="Phone Number"
                value={formData.phoneNumber || ""}
                onChange={(e) => setFormField("phoneNumber")(e.target.value)}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input
                label="Email"
                type="email"
                value={formData.email || ""}
                onChange={(e) => setFormField("email")(e.target.value)}
              />
            </Grid>
            <Grid item xs={12}>
              <Input
                label="Address"
                value={formData.address || ""}
                onChange={(e) => setFormField("address")(e.target.value)}
                multiline
                rows={2}
              />
            </Grid>
          </Grid>
        </FormSection>

        <FormSection title="Attachments" description="Supporting documents">
          <FileUploader
            referenceTable="Supplier"
            referenceId={editingSupplier?.supplierId}
            onUploadComplete={reload}
          />
        </FormSection>
      </CrudModal>

      <BulkActivateModal
        isOpen={showBulkActivateModal}
        onClose={() => setShowBulkActivateModal(false)}
        onConfirm={(ids) => handleBulkActivate(ids, bulkActivateValue)}
        selectedIds={getSelectedIds(suppliers)}
        itemName="suppliers"
        title={bulkTitle}
        description={bulkDescription}
      />
    </div>
  );
};

export default SuppliersMenu;