import React, { useState, useEffect, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus, FiSearch, FiX } from "react-icons/fi";
import SuppliersData from "./Suppliers.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Badge from "../../components/atoms/Badge/Badge";
import Modal from "../../components/molecules/Modal/Modal";
import Input from "../../components/atoms/Input/Input";
import Spinner from "../../components/atoms/Spinner/Spinner";
import ConfirmDialog from "../../components/molecules/ConfirmDialog/ConfirmDialog";
import "./Suppliers.scss";

const SuppliersMenu = () => {
  const [loading, setLoading] = useState(true);
  const [suppliers, setSuppliers] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState("");
  const [searchInput, setSearchInput] = useState("");
  const [activeTab, setActiveTab] = useState("all");

  const [showModal, setShowModal] = useState(false);
  const [editingSupplier, setEditingSupplier] = useState(null);
  const [formData, setFormData] = useState({
    supplierName: "",
    contactPerson: "",
    phoneNumber: "",
    email: "",
    address: "",
  });

  const suppliersData = new SuppliersData();

  const loadData = useCallback(async () => {
    setLoading(true);

    const result = await suppliersData.loadGridData(page, pageSize, search);

    if (result.success) {
      let data = result.data.data || [];
      if (activeTab === "active") {
        data = data.filter((s) => s.isActive);
      } else if (activeTab === "inactive") {
        data = data.filter((s) => !s.isActive);
      }
      setSuppliers(data);
      setTotalCount(data.length);
    }

    setLoading(false);
  }, [page, pageSize, search, activeTab]);

  useEffect(() => {
    loadData();
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
    setEditingSupplier(null);
    setFormData({
      supplierName: "",
      contactPerson: "",
      phoneNumber: "",
      email: "",
      address: "",
    });
    setShowModal(true);
  };

  const handleEdit = async (supplier) => {
    const result = await suppliersData.loadSupplier(supplier.supplierId);
    if (result.success) {
      setEditingSupplier(result.data);
      setFormData({
        supplierName: result.data.supplierName || "",
        contactPerson: result.data.contactPerson || "",
        phoneNumber: result.data.phoneNumber || "",
        email: result.data.email || "",
        address: result.data.address || "",
      });
      setShowModal(true);
    }
  };

  const handleDelete = async (supplier) => {
    const confirmed = await ConfirmDialog.showDelete(
      "Delete Supplier",
      `Are you sure you want to delete ${supplier.supplierName}?`
    );

    if (confirmed) {
      const result = await suppliersData.deleteSupplier(supplier.supplierId);
      if (result.success) {
        loadData();
      }
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.supplierName.trim()) {
      ConfirmDialog.showError("Error", "Supplier name is required");
      return;
    }

    let result;
    if (editingSupplier) {
      result = await suppliersData.updateSupplier(editingSupplier.supplierId, {
        ...formData,
        isActive: editingSupplier.isActive,
      });
    } else {
      result = await suppliersData.createSupplier(formData);
    }

    if (result.success) {
      setShowModal(false);
      loadData();
    }
  };

  const columns = [
    { field: "supplierName", headerName: "Name", width: 200 },
    { field: "contactPerson", headerName: "Contact Person", width: 180 },
    { field: "phoneNumber", headerName: "Phone", width: 150 },
    { field: "email", headerName: "Email", width: 220 },
    { field: "assetCount", headerName: "Assets", width: 100 },
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
    { id: "all", label: "All Suppliers" },
    { id: "active", label: "Active" },
    { id: "inactive", label: "Inactive" },
  ];

  if (loading && suppliers.length === 0) {
    return (
      <div className="suppliers-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="suppliers-menu">
      <div className="page-header">
        <h1 className="page-title">Supplier Management</h1>
        <Button variant="primary" onClick={handleCreate} startIcon={<FiPlus />}>
          Add Supplier
        </Button>
      </div>

      <div className="suppliers-menu__tabs">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            className={`suppliers-menu__tab ${activeTab === tab.id ? "active" : ""}`}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className="suppliers-menu__toolbar">
        <form className="suppliers-menu__search" onSubmit={handleSearch}>
          <FiSearch className="suppliers-menu__search-icon" />
          <input
            type="text"
            className="suppliers-menu__search-input"
            placeholder="Search by name, contact, email..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
          {searchInput && (
            <button type="button" className="suppliers-menu__search-clear" onClick={handleClearSearch}>
              <FiX />
            </button>
          )}
          <Button type="submit" variant="primary" size="sm">
            Search
          </Button>
        </form>
      </div>

      <div className="suppliers-menu__table">
        <DataTable
          rows={suppliers}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.supplierId}
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
        title={editingSupplier ? "Edit Supplier" : "Add New Supplier"}
        size="lg"
      >
        <form className="supplier-form" onSubmit={handleSubmit}>
          <div className="supplier-form__grid">
            <Input
              label="Supplier Name"
              value={formData.supplierName}
              onChange={(e) => setFormData({ ...formData, supplierName: e.target.value })}
              required
            />

            <Input
              label="Contact Person"
              value={formData.contactPerson}
              onChange={(e) => setFormData({ ...formData, contactPerson: e.target.value })}
            />

            <Input
              label="Phone Number"
              value={formData.phoneNumber}
              onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
            />

            <Input
              label="Email"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            />

            <div className="supplier-form__full-width">
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
              {editingSupplier ? "Update" : "Create"} Supplier
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default SuppliersMenu;