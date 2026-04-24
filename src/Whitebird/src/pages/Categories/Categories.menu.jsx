import React, { useState, useEffect, useMemo, useCallback } from "react";
import { FiEdit2, FiTrash2, FiPlus } from "react-icons/fi";
import { Grid, Box } from "@mui/material";
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
import { useGridData } from "../../hooks/useGridData";
import "./Categories.scss";

const categoriesData = new CategoriesData();

const CategoriesMenu = () => {
  const [activeTab, setActiveTab] = useState("all");
  const [showModal, setShowModal] = useState(false);
  const [editingCategory, setEditingCategory] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [parentCategories, setParentCategories] = useState([]);
  const [formData, setFormData] = useState({ categoryName: "", description: "", parentCategoryId: "" });

  const fetchGridData = useCallback(async (params) => {
    const result = await categoriesData.fetchGridData(params);
    if (result.success) { let data = result.data.data || []; if (activeTab === "active") data = data.filter(c => c.isActive); else if (activeTab === "inactive") data = data.filter(c => !c.isActive); return { success: true, data: { data, totalCount: data.length } }; }
    return result;
  }, [activeTab]);

  const { data: categories, totalCount, loading, page, setPage, pageSize, setPageSize, updateFilters, reload } = useGridData(fetchGridData);

  useEffect(() => { categoriesData.fetchParentCategories().then(r => { if (r.success) setParentCategories(r.data); }); }, []);

  const handleSearch = (search) => updateFilters({ search });
  const handleCreate = () => { setEditingCategory(null); setFormData({ categoryName: "", description: "", parentCategoryId: "" }); setShowModal(true); };
  const handleEdit = async (cat) => { const r = await categoriesData.fetchById(cat.categoryId); if (r.success) { setEditingCategory(r.data); setFormData({ categoryName: r.data.categoryName, description: r.data.description || "", parentCategoryId: r.data.parentCategoryId || "" }); setShowModal(true); } };
  const handleDelete = async (cat) => { const r = await categoriesData.delete(cat.categoryId); if (r.success) reload(); };
  const handleSubmit = async (e) => { e.preventDefault(); if (!formData.categoryName.trim()) return; if (isSubmitting) return; setIsSubmitting(true); const r = editingCategory ? await categoriesData.update(editingCategory.categoryId, { ...formData, isActive: editingCategory.isActive }) : await categoriesData.create(formData); setIsSubmitting(false); if (r.success) { setShowModal(false); reload(); } };

  const columns = useMemo(() => [
    { field: "categoryName", headerName: "Name", width: 250 }, { field: "description", headerName: "Description", width: 300 }, { field: "parentCategoryName", headerName: "Parent", width: 200 }, { field: "childCount", headerName: "Subcategories", width: 120 },
    { field: "isActive", headerName: "Status", width: 100, renderCell: (p) => <span className={`status-badge ${p.value ? 'status-badge--success' : 'status-badge--secondary'}`}>{p.value ? 'Active' : 'Inactive'}</span> },
    { field: "actions", headerName: "Actions", width: 100, sortable: false, renderCell: (p) => (<div className="table-actions"><button className="icon-btn icon-btn--lg" onClick={() => handleEdit(p.row)} title="Edit"><FiEdit2 size={18} /></button><button className="icon-btn icon-btn--danger icon-btn--lg" onClick={() => handleDelete(p.row)} title="Delete"><FiTrash2 size={18} /></button></div>) },
  ], []);

  const tabs = [{ id: "all", label: "All" }, { id: "active", label: "Active" }, { id: "inactive", label: "Inactive" }];
  if (loading && !categories.length) return <div className="categories-loading"><Spinner size="lg" /></div>;

  return (
    <div className="categories-menu">
      <PageHeader title="Category Management" buttonText="Add Category" onButtonClick={handleCreate} buttonIcon={<FiPlus />} />
      <Tabs tabs={tabs} activeTab={activeTab} onTabChange={(tab) => { setActiveTab(tab); setPage(1); }} />
      <SearchToolbar onSearch={handleSearch} placeholder="Search by name..." />
      <div className="categories-menu__table"><DataTable rows={categories} columns={columns} loading={loading} pageSize={pageSize} getRowId={(row) => row.categoryId} hideFooter={true} /></div>
      <Pagination currentPage={page} totalPages={Math.ceil(totalCount / pageSize)} pageSize={pageSize} totalItems={totalCount} onPageChange={setPage} onPageSizeChange={setPageSize} />
      <Modal isOpen={showModal} onClose={() => !isSubmitting && setShowModal(false)} title={editingCategory ? "Edit Category" : "Add Category"} size="md">
        <form onSubmit={handleSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12}><Input label="Category Name" value={formData.categoryName} onChange={e => setFormData({...formData, categoryName: e.target.value})} required /></Grid>
            <Grid item xs={12}><Input label="Description" value={formData.description} onChange={e => setFormData({...formData, description: e.target.value})} multiline rows={2} /></Grid>
            <Grid item xs={12}><Select label="Parent Category" value={formData.parentCategoryId} onChange={e => setFormData({...formData, parentCategoryId: e.target.value})} options={[{value: "", label: "None (Top Level)"}, ...parentCategories.filter(c => !editingCategory || c.categoryId !== editingCategory.categoryId).map(c => ({value: c.categoryId, label: c.categoryName}))]} /></Grid>
          </Grid>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}><Button variant="outline" onClick={() => setShowModal(false)} type="button">Cancel</Button><Button type="submit" variant="primary" loading={isSubmitting}>{editingCategory ? "Update" : "Create"}</Button></Box>
        </form>
      </Modal>
    </div>
  );
};

export default CategoriesMenu;