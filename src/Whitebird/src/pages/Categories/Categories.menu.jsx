import React, { useState, useEffect, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus, FiSearch, FiX } from "react-icons/fi";
import CategoriesData from "./Categories.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Badge from "../../components/atoms/Badge/Badge";
import Modal from "../../components/molecules/Modal/Modal";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import ConfirmDialog from "../../components/molecules/ConfirmDialog/ConfirmDialog";
import "./Categories.scss";

const CategoriesMenu = () => {
  const [loading, setLoading] = useState(true);
  const [categories, setCategories] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState("");
  const [searchInput, setSearchInput] = useState("");
  const [activeTab, setActiveTab] = useState("all");

  const [showModal, setShowModal] = useState(false);
  const [editingCategory, setEditingCategory] = useState(null);
  const [formData, setFormData] = useState({
    categoryName: "",
    description: "",
    parentCategoryId: "",
  });
  const [parentCategories, setParentCategories] = useState([]);

  const categoriesData = new CategoriesData();

  const loadData = useCallback(async () => {
    setLoading(true);

    const result = await categoriesData.loadGridData(page, pageSize, search);

    if (result.success) {
      let data = result.data.data || [];
      if (activeTab === "active") {
        data = data.filter((c) => c.isActive);
      } else if (activeTab === "inactive") {
        data = data.filter((c) => !c.isActive);
      }
      setCategories(data);
      setTotalCount(data.length);
    }

    setLoading(false);
  }, [page, pageSize, search, activeTab]);

  const loadParentCategories = async () => {
    const result = await categoriesData.loadParentCategories();
    if (result.success) {
      setParentCategories(result.data);
    }
  };

  useEffect(() => {
    loadData();
    loadParentCategories();
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
    setEditingCategory(null);
    setFormData({
      categoryName: "",
      description: "",
      parentCategoryId: "",
    });
    setShowModal(true);
  };

  const handleEdit = async (category) => {
    const result = await categoriesData.loadCategory(category.categoryId);
    if (result.success) {
      setEditingCategory(result.data);
      setFormData({
        categoryName: result.data.categoryName || "",
        description: result.data.description || "",
        parentCategoryId: result.data.parentCategoryId || "",
      });
      setShowModal(true);
    }
  };

  const handleDelete = async (category) => {
    const confirmed = await ConfirmDialog.showDelete(
      "Delete Category",
      `Are you sure you want to delete ${category.categoryName}?`
    );

    if (confirmed) {
      const result = await categoriesData.deleteCategory(category.categoryId);
      if (result.success) {
        loadData();
      }
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.categoryName.trim()) {
      ConfirmDialog.showError("Error", "Category name is required");
      return;
    }

    let result;
    if (editingCategory) {
      result = await categoriesData.updateCategory(editingCategory.categoryId, {
        ...formData,
        isActive: editingCategory.isActive,
      });
    } else {
      result = await categoriesData.createCategory(formData);
    }

    if (result.success) {
      setShowModal(false);
      loadData();
    }
  };

  const columns = [
    { field: "categoryName", headerName: "Name", width: 250 },
    { field: "description", headerName: "Description", width: 300 },
    { field: "parentCategoryName", headerName: "Parent Category", width: 200 },
    { field: "childCount", headerName: "Subcategories", width: 120 },
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
    { id: "all", label: "All Categories" },
    { id: "active", label: "Active" },
    { id: "inactive", label: "Inactive" },
  ];

  if (loading && categories.length === 0) {
    return (
      <div className="categories-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="categories-menu">
      <div className="page-header">
        <h1 className="page-title">Category Management</h1>
        <Button variant="primary" onClick={handleCreate} startIcon={<FiPlus />}>
          Add Category
        </Button>
      </div>

      <div className="categories-menu__tabs">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            className={`categories-menu__tab ${activeTab === tab.id ? "active" : ""}`}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className="categories-menu__toolbar">
        <form className="categories-menu__search" onSubmit={handleSearch}>
          <FiSearch className="categories-menu__search-icon" />
          <input
            type="text"
            className="categories-menu__search-input"
            placeholder="Search by name or description..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
          {searchInput && (
            <button type="button" className="categories-menu__search-clear" onClick={handleClearSearch}>
              <FiX />
            </button>
          )}
          <Button type="submit" variant="primary" size="sm">
            Search
          </Button>
        </form>
      </div>

      <div className="categories-menu__table">
        <DataTable
          rows={categories}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.categoryId}
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
        title={editingCategory ? "Edit Category" : "Add New Category"}
        size="md"
      >
        <form className="category-form" onSubmit={handleSubmit}>
          <Input
            label="Category Name"
            value={formData.categoryName}
            onChange={(e) => setFormData({ ...formData, categoryName: e.target.value })}
            required
          />

          <Input
            label="Description"
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            placeholder="Optional description"
          />

          <Select
            label="Parent Category"
            value={formData.parentCategoryId}
            onChange={(e) => setFormData({ ...formData, parentCategoryId: e.target.value })}
            options={[
              { value: "", label: "None (Top Level)" },
              ...parentCategories
                .filter((c) => !editingCategory || c.categoryId !== editingCategory.categoryId)
                .map((c) => ({ value: c.categoryId, label: c.categoryName })),
            ]}
          />

          <div className="modal-actions">
            <Button variant="outline" onClick={() => setShowModal(false)}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              {editingCategory ? "Update" : "Create"} Category
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default CategoriesMenu;