import React, { useState, useMemo, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box, Chip } from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import OfficesData from "./Offices.data";
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
import ModalActions from "../../components/molecules/ModalActions/ModalActions";
import { useGridData } from "../../hooks/useGridData";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useCrudForm } from "../../hooks/useCrudForm";
import { cleanNullableStrings, cleanIdFields } from "../../core/utils/formHelpers";
import { STATUS_TABS } from "../../core/constants/tabs";
import "./Offices.scss";

const officesData = new OfficesData();

// OFFICE TYPE OPTIONS - hardcoded, bisa pindah ke constants jika perlu
const OFFICE_TYPE_OPTIONS = [
  { value: "", label: "Select Type" },
  { value: "1", label: "Head Office" },
  { value: "2", label: "Branch Office" },
];

const INITIAL_FORM_DATA = {
  officeName: "",
  officeCode: "",
  officeType: "",
  city: "",
  address: "",
  phone: "",
  parentOfficeId: null,  // IMPORTANT: gunakan null, BUKAN string kosong
};

/**
 * Transform form data untuk dikirim ke API
 * Pastikan parentOfficeId dikirim sebagai null (bukan string kosong)
 */
const transformOfficeFormData = (data) => {
  // Step 1: Clean nullable string fields (empty string -> null)
  let result = cleanNullableStrings(data, ['officeCode', 'city', 'address', 'phone']);
  
  // Step 2: Clean ID fields (string -> int or null)
  // Pastikan parentOfficeId yang kosong jadi null, BUKAN string kosong ""
  result = cleanIdFields(result, ['officeType', 'parentOfficeId']);
  
  // Step 3: Pastikan officeName tidak kosong
  if (!result.officeName || result.officeName.trim() === '') {
    result.officeName = null;
  }
  
  // Step 4: VERIFIKASI - parentOfficeId HARUS null atau number, BUKAN string
  if (result.parentOfficeId === '' || result.parentOfficeId === undefined) {
    result.parentOfficeId = null;
  }
  
  return result;
};

officesData.transformFormData = transformOfficeFormData;

const CRUD_OPTIONS = { 
  idField: 'officeId',
};

const OfficesMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const queryClient = useQueryClient();
  const { offices: parentOffices } = useReferenceData();

  const {
    showModal,
    editingRecord: editingOffice,
    isSubmitting,
    formData,
    setFormData,
    handleCreate,
    handleEdit,
    handleClose,
  } = useCrudForm(INITIAL_FORM_DATA, officesData, CRUD_OPTIONS);

  const fetchGridData = useCallback(async (params) => {
    const result = await officesData.fetchGridData(params);
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
        dataArray = dataArray.filter(o => o.isActive === true);
      } else if (activeTab === "inactive") {
        dataArray = dataArray.filter(o => o.isActive === false);
      }

      return { success: true, data: { data: dataArray, totalCount: dataArray.length } };
    }
    return result;
  }, [activeTab]);

  const {
    data: offices,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    updateFilters,
    reload
  } = useGridData(['offices', activeTab], fetchGridData);

  const handleSearch = useCallback((search) => updateFilters({ search }), [updateFilters]);

  const handleDelete = useCallback(async (office) => {
    const r = await officesData.delete(office.officeId);
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'offices'] });
      reload();
    }
  }, [reload, queryClient]);

  const onSubmit = useCallback(async (e) => {
    e.preventDefault();
    if (!formData.officeName?.trim()) return;
    if (isSubmitting) return;
    
    // Prepare data with isActive flag
    const data = { 
      ...formData, 
      isActive: editingOffice ? editingOffice.isActive : true 
    };
    
    // Pastikan parentOfficeId tidak pernah string kosong
    if (data.parentOfficeId === '' || data.parentOfficeId === undefined) {
      data.parentOfficeId = null;
    }
    
    const r = editingOffice
      ? await officesData.update(editingOffice.officeId, data)
      : await officesData.create(data);
      
    if (r.success) {
      queryClient.invalidateQueries({ queryKey: ['reference', 'offices'] });
      handleClose();
      reload();
    }
  }, [formData, isSubmitting, editingOffice, handleClose, reload, queryClient]);

  // Handler untuk perubahan parentOfficeId di form
  const handleParentOfficeChange = useCallback((value) => {
    // Jika value adalah string kosong, set ke null
    const newValue = (value === '' || value === undefined || value === null) ? null : value;
    setFormData(prev => ({ ...prev, parentOfficeId: newValue }));
  }, [setFormData]);

  const parentOfficeOptions = useMemo(() => [
    { value: "", label: "None (Top Level)" },
    ...parentOffices
      .filter(o => !editingOffice || o.value !== editingOffice.officeId)
      .map(o => ({ value: o.value, label: o.label }))
  ], [parentOffices, editingOffice]);

  const columns = useMemo(() => [
    { field: "officeCode", headerName: "Code", width: 120 },
    { field: "officeName", headerName: "Name", flex: 1, minWidth: 180 },
    { field: "officeTypeName", headerName: "Type", width: 130 },
    { field: "city", headerName: "City", width: 140 },
    { field: "parentOfficeName", headerName: "Parent", width: 170 },
    { field: "childCount", headerName: "Sub-offices", width: 130 },
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
          <IconButton onClick={() => handleEdit(p.row)} title="Edit office" size="lg">
            <FiEdit2 size={18} />
          </IconButton>
          <IconButton onClick={() => handleDelete(p.row)} title="Delete office" variant="danger" size="lg">
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

  if (loading && !offices.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  // Nilai parentOfficeId untuk form - pastikan null menjadi "" untuk display di Select
  const parentOfficeValue = formData.parentOfficeId === null ? "" : (formData.parentOfficeId || "");

  return (
    <div className="offices-menu">
      <div className="page-header">
        <h1 className="page-title">Office</h1>
        <PageHeader 
          title="Office Management" 
          buttonText="Add Office" 
          onButtonClick={handleCreate} 
          buttonIcon={<FiPlus />} 
        />
      </div>

      <Tabs tabs={STATUS_TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} placeholder="Search by name, code, city..." />
      
<div className="offices-menu__table" style={{ width: '100%', minWidth: 0 }}>
  <DataTable
    rows={offices}
    columns={columns}
    loading={loading}
    pageSize={pageSize}
    getRowId={(row) => row.officeId}
    hideFooter={true}
    autoHeight={true}  // TAMBAHKAN INI
    ariaLabel="Offices data table"
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

      <Modal isOpen={showModal} onClose={handleClose} title={editingOffice ? "Edit Office" : "Add Office"} size="lg">
        <form onSubmit={onSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input 
                label="Office Name" 
                value={formData.officeName || ""} 
                onChange={e => setFormData({ ...formData, officeName: e.target.value })} 
                required 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input 
                label="Office Code" 
                value={formData.officeCode || ""} 
                onChange={e => setFormData({ ...formData, officeCode: e.target.value })} 
                placeholder="Optional"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Office Type" 
                value={formData.officeType || ""} 
                onChange={e => setFormData({ ...formData, officeType: e.target.value })} 
                options={OFFICE_TYPE_OPTIONS} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select 
                label="Parent Office" 
                value={parentOfficeValue}
                onChange={e => handleParentOfficeChange(e.target.value)} 
                options={parentOfficeOptions} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input 
                label="City" 
                value={formData.city || ""} 
                onChange={e => setFormData({ ...formData, city: e.target.value })} 
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input 
                label="Phone" 
                value={formData.phone || ""} 
                onChange={e => setFormData({ ...formData, phone: e.target.value })} 
              />
            </Grid>
            <Grid item xs={12}>
              <Input 
                label="Address" 
                value={formData.address || ""} 
                onChange={e => setFormData({ ...formData, address: e.target.value })} 
                multiline 
                rows={2} 
              />
            </Grid>
          </Grid>
          <ModalActions 
            onCancel={handleClose} 
            isSubmitting={isSubmitting}
            submitText={editingOffice ? "Update" : "Create"}
          />
        </form>
      </Modal>
    </div>
  );
};

export default OfficesMenu;