import React, { useState, useMemo, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box, Chip } from "@mui/material";
import LocationsData from "./Locations.data";
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
import "./Locations.scss";

const locationsData = new LocationsData();

const INITIAL_FORM_DATA = {
  locationName: "", locationType: "", address: "", city: "", parentLocationId: "",
};

const LOCATION_TYPE_OPTIONS = [
  { value: "", label: "Select" },
  { value: "Building", label: "Building" },
  { value: "Floor", label: "Floor" },
  { value: "Room", label: "Room" },
  { value: "Warehouse", label: "Warehouse" },
];

const TABS = [
  { id: "all", label: "All" },
  { id: "active", label: "Active" },
  { id: "inactive", label: "Inactive" },
];

const LocationsMenu = () => {
  const [activeTab, setActiveTab] = useState("all");

  const { locations: parentLocations } = useReferenceData();

  const {
    showModal,
    editingRecord: editingLocation,
    isSubmitting,
    formData,
    setFormData,
    handleCreate,
    handleEdit,
    handleClose,
  } = useCrudForm(INITIAL_FORM_DATA, locationsData, {
    idField: 'locationId',
  });

  const fetchGridData = useCallback(async (params) => {
    const result = await locationsData.fetchGridData(params);
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
        dataArray = dataArray.filter(l => l.isActive === true);
      } else if (activeTab === "inactive") {
        dataArray = dataArray.filter(l => l.isActive === false);
      }

      return { success: true, data: { data: dataArray, totalCount: dataArray.length } };
    }
    return result;
  }, [activeTab]);

  const {
    data: locations,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    updateFilters,
    reload
  } = useGridData(['locations', activeTab], fetchGridData);

  const handleSearch = useCallback((search) => updateFilters({ search }), [updateFilters]);

  const handleDelete = useCallback(async (loc) => {
    const r = await locationsData.delete(loc.locationId);
    if (r.success) reload();
  }, [reload]);

  const onSubmit = useCallback(async (e) => {
    e.preventDefault();
    if (!formData.locationName.trim()) return;
    if (isSubmitting) return;
    const data = { ...formData, isActive: editingLocation ? editingLocation.isActive : true };
    const r = editingLocation
      ? await locationsData.update(editingLocation.locationId, data)
      : await locationsData.create(data);
    if (r.success) { handleClose(); reload(); }
  }, [formData, isSubmitting, editingLocation, handleClose, reload]);

  const columns = useMemo(() => [
    { field: "locationCode", headerName: "Code", width: 120 },
    { field: "locationName", headerName: "Name", flex: 1, minWidth: 180 },
    { field: "locationType", headerName: "Type", width: 130 },
    { field: "city", headerName: "City", width: 140 },
    { field: "parentLocationName", headerName: "Parent", width: 170 },
    { field: "childCount", headerName: "Sub-locations", width: 130 },
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
          <IconButton onClick={() => handleEdit(p.row)} title="Edit location" size="lg">
            <FiEdit2 size={18} />
          </IconButton>
          <IconButton onClick={() => handleDelete(p.row)} title="Delete location" variant="danger" size="lg">
            <FiTrash2 size={18} />
          </IconButton>
        </div>
      )
    },
  ], [handleEdit, handleDelete]);

  const parentLocationOptions = useMemo(() => [
    { value: "", label: "None" },
    ...parentLocations
      .filter(l => !editingLocation || l.value !== editingLocation.locationId)
      .map(l => ({ value: l.value, label: l.label }))
  ], [parentLocations, editingLocation]);

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
  }, [setPage]);

  if (loading && !locations.length) return <div className="page-loading"><Spinner size="lg" /></div>;

  return (
    <div className="locations-menu">
      <PageHeader title="Location Management" buttonText="Add Location" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />
      <SearchToolbar onSearch={handleSearch} placeholder="Search by name, code..." />
      <div className="locations-menu__table">
        <DataTable
          rows={locations}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.locationId}
          hideFooter={true}
          ariaLabel="Locations data table"
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

      <Modal isOpen={showModal} onClose={handleClose} title={editingLocation ? "Edit Location" : "Add Location"} size="lg">
        <form onSubmit={onSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Input label="Location Name" value={formData.locationName} onChange={e => setFormData({ ...formData, locationName: e.target.value })} required />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Location Type" value={formData.locationType} onChange={e => setFormData({ ...formData, locationType: e.target.value })} options={LOCATION_TYPE_OPTIONS} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Input label="City" value={formData.city} onChange={e => setFormData({ ...formData, city: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Select label="Parent Location" value={formData.parentLocationId} onChange={e => setFormData({ ...formData, parentLocationId: e.target.value })} options={parentLocationOptions} />
            </Grid>
            <Grid item xs={12}>
              <Input label="Address" value={formData.address} onChange={e => setFormData({ ...formData, address: e.target.value })} multiline rows={2} />
            </Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button variant="outline" onClick={handleClose} type="button">Cancel</Button>
            <Button type="submit" variant="primary" loading={isSubmitting}>
              {editingLocation ? "Update" : "Create"}
            </Button>
          </Box>
        </form>
      </Modal>
    </div>
  );
};

export default LocationsMenu;