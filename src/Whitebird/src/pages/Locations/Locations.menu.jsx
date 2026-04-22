import React, { useState, useEffect, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus, FiSearch, FiX } from "react-icons/fi";
import LocationsData from "./Locations.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Badge from "../../components/atoms/Badge/Badge";
import Modal from "../../components/molecules/Modal/Modal";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import ConfirmDialog from "../../components/molecules/ConfirmDialog/ConfirmDialog";
import "./Locations.scss";

const LocationsMenu = () => {
  const [loading, setLoading] = useState(true);
  const [locations, setLocations] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState("");
  const [searchInput, setSearchInput] = useState("");
  const [activeTab, setActiveTab] = useState("all");

  const [showModal, setShowModal] = useState(false);
  const [editingLocation, setEditingLocation] = useState(null);
  const [formData, setFormData] = useState({
    locationName: "",
    locationType: "",
    address: "",
    city: "",
    parentLocationId: "",
  });
  const [parentLocations, setParentLocations] = useState([]);

  const locationsData = new LocationsData();

  const loadData = useCallback(async () => {
    setLoading(true);

    const result = await locationsData.loadGridData(page, pageSize, search);

    if (result.success) {
      let data = result.data.data || [];
      if (activeTab === "active") {
        data = data.filter((l) => l.isActive);
      } else if (activeTab === "inactive") {
        data = data.filter((l) => !l.isActive);
      }
      setLocations(data);
      setTotalCount(data.length);
    }

    setLoading(false);
  }, [page, pageSize, search, activeTab]);

  const loadParentLocations = async () => {
    const result = await locationsData.loadParentLocations();
    if (result.success) {
      setParentLocations(result.data);
    }
  };

  useEffect(() => {
    loadData();
    loadParentLocations();
  }, [loadData]);

  const handleSearch = (e) => {
    e.preventDefault();
    setSearch(searchInput);
    setPage(1);
  };

  const handleClearSearch = () => {
    setSearchInput("");
    setSearch("");
    setPage(1);
  };

  const handleCreate = () => {
    setEditingLocation(null);
    setFormData({
      locationName: "",
      locationType: "",
      address: "",
      city: "",
      parentLocationId: "",
    });
    setShowModal(true);
  };

  const handleEdit = async (location) => {
    const result = await locationsData.loadLocation(location.locationId);
    if (result.success) {
      setEditingLocation(result.data);
      setFormData({
        locationName: result.data.locationName || "",
        locationType: result.data.locationType || "",
        address: result.data.address || "",
        city: result.data.city || "",
        parentLocationId: result.data.parentLocationId || "",
      });
      setShowModal(true);
    }
  };

  const handleDelete = async (location) => {
    const confirmed = await ConfirmDialog.showDelete(
      "Delete Location",
      `Are you sure you want to delete ${location.locationName}?`
    );

    if (confirmed) {
      const result = await locationsData.deleteLocation(location.locationId);
      if (result.success) {
        loadData();
      }
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.locationName.trim()) {
      ConfirmDialog.showError("Error", "Location name is required");
      return;
    }

    let result;
    if (editingLocation) {
      result = await locationsData.updateLocation(editingLocation.locationId, {
        ...formData,
        isActive: editingLocation.isActive,
      });
    } else {
      result = await locationsData.createLocation(formData);
    }

    if (result.success) {
      setShowModal(false);
      loadData();
    }
  };

  const columns = [
    { field: "locationCode", headerName: "Code", width: 120 },
    { field: "locationName", headerName: "Name", width: 200 },
    { field: "locationType", headerName: "Type", width: 130 },
    { field: "city", headerName: "City", width: 150 },
    { field: "parentLocationName", headerName: "Parent Location", width: 180 },
    { field: "childCount", headerName: "Sub-locations", width: 120 },
    {
      field: "isActive",
      headerName: "Status",
      width: 100,
      renderCell: (params) => (
        <Badge variant={params.value ? "success" : "secondary"}>
          {params.value ? "Active" : "Inactive"}
        </Badge>
      ),
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 120,
      sortable: false,
      renderCell: (params) => (
        <div className="table-actions">
          <Button variant="text" size="sm" onClick={() => handleEdit(params.row)} title="Edit">
            <FiEdit2 />
          </Button>
          <Button variant="text" size="sm" onClick={() => handleDelete(params.row)} title="Delete">
            <FiTrash2 />
          </Button>
        </div>
      ),
    },
  ];

  const tabs = [
    { id: "all", label: "All Locations" },
    { id: "active", label: "Active" },
    { id: "inactive", label: "Inactive" },
  ];

  if (loading && locations.length === 0) {
    return (
      <div className="locations-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="locations-menu">
      <div className="page-header">
        <h1 className="page-title">Location Management</h1>
        <Button variant="primary" onClick={handleCreate} startIcon={<FiPlus />}>
          Add Location
        </Button>
      </div>

      <div className="locations-menu__tabs">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            className={`locations-menu__tab ${activeTab === tab.id ? "active" : ""}`}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className="locations-menu__toolbar">
        <form className="locations-menu__search" onSubmit={handleSearch}>
          <FiSearch className="locations-menu__search-icon" />
          <input
            type="text"
            className="locations-menu__search-input"
            placeholder="Search by name, code, city..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
          {searchInput && (
            <button type="button" className="locations-menu__search-clear" onClick={handleClearSearch}>
              <FiX />
            </button>
          )}
          <Button type="submit" variant="primary" size="sm">
            Search
          </Button>
        </form>
      </div>

      <div className="locations-menu__table">
        <DataTable
          rows={locations}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.locationId}
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

      <Modal
        isOpen={showModal}
        onClose={() => setShowModal(false)}
        title={editingLocation ? "Edit Location" : "Add New Location"}
        size="lg"
      >
        <form className="location-form" onSubmit={handleSubmit}>
          <div className="location-form__grid">
            <Input
              label="Location Name"
              value={formData.locationName}
              onChange={(e) => setFormData({ ...formData, locationName: e.target.value })}
              required
            />

            <Select
              label="Location Type"
              value={formData.locationType}
              onChange={(e) => setFormData({ ...formData, locationType: e.target.value })}
              options={[
                { value: "", label: "Select Type" },
                { value: "Building", label: "Building" },
                { value: "Floor", label: "Floor" },
                { value: "Room", label: "Room" },
                { value: "Warehouse", label: "Warehouse" },
                { value: "Office", label: "Office" },
              ]}
            />

            <Input
              label="City"
              value={formData.city}
              onChange={(e) => setFormData({ ...formData, city: e.target.value })}
            />

            <Select
              label="Parent Location"
              value={formData.parentLocationId}
              onChange={(e) => setFormData({ ...formData, parentLocationId: e.target.value })}
              options={[
                { value: "", label: "None (Top Level)" },
                ...parentLocations
                  .filter((l) => !editingLocation || l.locationId !== editingLocation.locationId)
                  .map((l) => ({ value: l.locationId, label: l.locationName })),
              ]}
            />

            <div className="location-form__full-width">
              <Input
                label="Address"
                value={formData.address}
                onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                placeholder="Full address"
              />
            </div>
          </div>

          <div className="modal-actions">
            <Button variant="outline" onClick={() => setShowModal(false)}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              {editingLocation ? "Update" : "Create"} Location
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default LocationsMenu;