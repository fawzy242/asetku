import React, { useState, useEffect, useMemo, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box } from "@mui/material";
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
import { useGridData } from "../../hooks/useGridData";
import "./Locations.scss";

const locationsData = new LocationsData();

const LocationsMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showModal, setShowModal] = useState(false);
  const [editingLocation, setEditingLocation] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [parentLocations, setParentLocations] = useState([]);
  const [formData, setFormData] = useState({ locationName: "", locationType: "", address: "", city: "", parentLocationId: "" });

  const fetchGridData = useCallback(async (params) => {
    const result = await locationsData.fetchGridData(params);
    if (result.success) { let data = result.data.data || []; if (activeTab === "active") data = data.filter(l => l.isActive); else if (activeTab === "inactive") data = data.filter(l => !l.isActive); return { success: true, data: { data, totalCount: data.length } }; }
    return result;
  }, [activeTab]);

  const { data: locations, totalCount, loading, page, setPage, pageSize, setPageSize, updateFilters, reload } = useGridData(fetchGridData);

  useEffect(() => { locationsData.fetchParentLocations().then(r => { if (r.success) setParentLocations(r.data); }); }, []);

  const handleSearch = (search) => updateFilters({ search });
  const handleCreate = () => { setEditingLocation(null); setFormData({ locationName: "", locationType: "", address: "", city: "", parentLocationId: "" }); setShowModal(true); };
  const handleEdit = async (loc) => { const r = await locationsData.fetchById(loc.locationId); if (r.success) { setEditingLocation(r.data); setFormData({ locationName: r.data.locationName, locationType: r.data.locationType || "", address: r.data.address || "", city: r.data.city || "", parentLocationId: r.data.parentLocationId || "" }); setShowModal(true); } };
  const handleDelete = async (loc) => { const r = await locationsData.delete(loc.locationId); if (r.success) reload(); };
  const handleSubmit = async (e) => { e.preventDefault(); if (!formData.locationName.trim()) return; if (isSubmitting) return; setIsSubmitting(true); const r = editingLocation ? await locationsData.update(editingLocation.locationId, { ...formData, isActive: editingLocation.isActive }) : await locationsData.create(formData); setIsSubmitting(false); if (r.success) { setShowModal(false); reload(); } };

  const columns = useMemo(() => [
    { field: "locationCode", headerName: "Code", width: 120 }, { field: "locationName", headerName: "Name", width: 200 }, { field: "locationType", headerName: "Type", width: 130 }, { field: "city", headerName: "City", width: 150 }, { field: "parentLocationName", headerName: "Parent", width: 180 }, { field: "childCount", headerName: "Sub-locations", width: 120 },
    { field: "isActive", headerName: "Status", width: 100, renderCell: (p) => <span className={`status-badge ${p.value ? 'status-badge--success' : 'status-badge--secondary'}`}>{p.value ? 'Active' : 'Inactive'}</span> },
    { field: "actions", headerName: "Actions", width: 100, sortable: false, renderCell: (p) => (<div className="table-actions"><button className="icon-btn icon-btn--lg" onClick={() => handleEdit(p.row)} title="Edit"><FiEdit2 size={18} /></button><button className="icon-btn icon-btn--danger icon-btn--lg" onClick={() => handleDelete(p.row)} title="Delete"><FiTrash2 size={18} /></button></div>) },
  ], []);

  const tabs = [{ id: "all", label: "All" }, { id: "active", label: "Active" }, { id: "inactive", label: "Inactive" }];
  if (loading && !locations.length) return <div className="locations-loading"><Spinner size="lg" /></div>;

  return (
    <div className="locations-menu">
      <PageHeader title="Location Management" buttonText="Add Location" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      <Tabs tabs={tabs} activeTab={activeTab} onTabChange={(tab) => { setActiveTab(tab); setPage(1); }} />
      <SearchToolbar onSearch={handleSearch} placeholder="Search by name, code..." />
      <div className="locations-menu__table"><DataTable rows={locations} columns={columns} loading={loading} pageSize={pageSize} getRowId={(row) => row.locationId} hideFooter={true} /></div>
      <Pagination currentPage={page} totalPages={Math.ceil(totalCount / pageSize)} pageSize={pageSize} totalItems={totalCount} onPageChange={setPage} onPageSizeChange={setPageSize} />
      <Modal isOpen={showModal} onClose={() => !isSubmitting && setShowModal(false)} title={editingLocation ? "Edit Location" : "Add Location"} size="lg">
        <form onSubmit={handleSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}><Input label="Location Name" value={formData.locationName} onChange={e => setFormData({...formData, locationName: e.target.value})} required /></Grid>
            <Grid item xs={12} sm={6}><Select label="Location Type" value={formData.locationType} onChange={e => setFormData({...formData, locationType: e.target.value})} options={[{value: "", label: "Select"}, {value: "Building", label: "Building"}, {value: "Floor", label: "Floor"}, {value: "Room", label: "Room"}, {value: "Warehouse", label: "Warehouse"}]} /></Grid>
            <Grid item xs={12} sm={6}><Input label="City" value={formData.city} onChange={e => setFormData({...formData, city: e.target.value})} /></Grid>
            <Grid item xs={12} sm={6}><Select label="Parent Location" value={formData.parentLocationId} onChange={e => setFormData({...formData, parentLocationId: e.target.value})} options={[{value: "", label: "None"}, ...parentLocations.filter(l => !editingLocation || l.locationId !== editingLocation.locationId).map(l => ({value: l.locationId, label: l.locationName}))]} /></Grid>
            <Grid item xs={12}><Input label="Address" value={formData.address} onChange={e => setFormData({...formData, address: e.target.value})} multiline rows={2} /></Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}><Button variant="outline" onClick={() => setShowModal(false)} type="button">Cancel</Button><Button type="submit" variant="primary" loading={isSubmitting}>{editingLocation ? "Update" : "Create"}</Button></Box>
        </form>
      </Modal>
    </div>
  );
};

export default LocationsMenu;