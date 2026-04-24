import React, { useState, useEffect, useCallback } from "react";
import { FiPackage, FiDollarSign, FiUsers, FiClock, FiDownload, FiRefreshCw, FiBox, FiTool, FiTrendingUp, FiFilter, FiX, FiSearch } from "react-icons/fi";
import ReportsData from "./Reports.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Card from "../../components/atoms/Card/Card";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import Tabs from "../../components/molecules/Tabs/Tabs";
import utilsHelper from "../../core/utils/utils.helper";
import "./Reports.scss";

const reportsData = new ReportsData();

const ReportsMenu = () => {
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("transactions");
  const [stats, setStats] = useState({});
  const [reportData, setReportData] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [showFilters, setShowFilters] = useState(false);
  const [filters, setFilters] = useState({ startDate: "", endDate: "", transactionType: "", status: "", categoryId: "", supplierId: "", employeeId: "", department: "", isUpcoming: "" });
  const [filterOptions, setFilterOptions] = useState({ categories: [], suppliers: [], employees: [], departments: [] });

  useEffect(() => { loadStats(); loadFilterOptions(); }, []);
  useEffect(() => { loadReportData(); }, [activeTab, filters, page, pageSize]);

  const loadStats = async () => { const r = await reportsData.fetchDashboardStats(); if (r.success) setStats(r.data); };
  const loadFilterOptions = async () => { const r = await reportsData.fetchFilterOptions(); if (r.success) setFilterOptions(r.data); };

  const loadReportData = async () => {
    setLoading(true);
    let r;
    const params = { ...filters, page, pageSize };
    if (activeTab === "transactions") r = await reportsData.fetchTransactionData(params);
    else if (activeTab === "inventory") r = await reportsData.fetchInventoryData(params);
    else if (activeTab === "employee") r = await reportsData.fetchEmployeeAssetData(params);
    else if (activeTab === "maintenance") r = await reportsData.fetchMaintenanceData(params);
    else r = await reportsData.fetchFinancialData(params);
    if (r.success) { setReportData(r.data); setTotalCount(r.data.length); }
    setLoading(false);
  };

  const handleExport = async () => {
    if (activeTab === "transactions") await reportsData.exportTransaction(filters);
    else if (activeTab === "inventory") await reportsData.exportInventory(filters);
    else if (activeTab === "employee") await reportsData.exportEmployee(filters);
    else if (activeTab === "maintenance") await reportsData.exportMaintenance(filters);
    else await reportsData.exportFinancial(filters);
  };

  const handleFilterChange = (key, value) => { setFilters(p => ({ ...p, [key]: value })); setPage(1); };
  const handleClearFilters = () => { setFilters({ startDate: "", endDate: "", transactionType: "", status: "", categoryId: "", supplierId: "", employeeId: "", department: "", isUpcoming: "" }); setPage(1); };

  const tabs = [{ id: "transactions", label: "Transactions", icon: <FiRefreshCw /> }, { id: "inventory", label: "Inventory", icon: <FiBox /> }, { id: "employee", label: "Employee Assets", icon: <FiUsers /> }, { id: "maintenance", label: "Maintenance", icon: <FiTool /> }, { id: "financial", label: "Financial", icon: <FiTrendingUp /> }];

  const getColumns = () => {
    if (activeTab === "transactions") return [{ field: "assetCode", headerName: "Code", width: 120 }, { field: "assetName", headerName: "Name", width: 180 }, { field: "fullName", headerName: "Employee", width: 160 }, { field: "transactionType", headerName: "Type", width: 120 }, { field: "transactionStatus", headerName: "Status", width: 110 }, { field: "transactionDate", headerName: "Date", width: 160, valueFormatter: p => utilsHelper.formatDateTime(p.value) }];
    if (activeTab === "inventory") return [{ field: "assetCode", headerName: "Code", width: 120 }, { field: "assetName", headerName: "Name", width: 200 }, { field: "categoryName", headerName: "Category", width: 150 }, { field: "status", headerName: "Status", width: 110 }, { field: "currentHolderName", headerName: "Holder", width: 150 }];
    if (activeTab === "employee") return [{ field: "employeeCode", headerName: "Emp Code", width: 110 }, { field: "fullName", headerName: "Employee", width: 180 }, { field: "department", headerName: "Dept", width: 140 }, { field: "assetCode", headerName: "Asset", width: 120 }, { field: "assetName", headerName: "Name", width: 180 }];
    if (activeTab === "maintenance") return [{ field: "assetCode", headerName: "Code", width: 120 }, { field: "assetName", headerName: "Name", width: 200 }, { field: "maintenanceCount", headerName: "Count", width: 90 }, { field: "lastMaintenanceDate", headerName: "Last", width: 120, valueFormatter: p => utilsHelper.formatDate(p.value) }, { field: "nextMaintenanceDate", headerName: "Next", width: 120, valueFormatter: p => utilsHelper.formatDate(p.value) }];
    return [{ field: "assetCode", headerName: "Code", width: 120 }, { field: "assetName", headerName: "Name", width: 200 }, { field: "categoryName", headerName: "Category", width: 150 }, { field: "purchasePrice", headerName: "Price", width: 130, valueFormatter: p => utilsHelper.formatCurrency(p.value) }, { field: "totalMaintenanceCost", headerName: "Maint. Cost", width: 130, valueFormatter: p => utilsHelper.formatCurrency(p.value) }];
  };

  const renderFilters = () => {
    if (activeTab === "transactions") return (<><Input label="Start" type="date" value={filters.startDate} onChange={e => handleFilterChange("startDate", e.target.value)} /><Input label="End" type="date" value={filters.endDate} onChange={e => handleFilterChange("endDate", e.target.value)} /><Select label="Type" value={filters.transactionType} onChange={e => handleFilterChange("transactionType", e.target.value)} options={[{value:"",label:"All"},{value:"Assignment",label:"Assignment"},{value:"Return",label:"Return"}]} /></>);
    if (activeTab === "inventory") return (<><Select label="Status" value={filters.status} onChange={e => handleFilterChange("status", e.target.value)} options={[{value:"",label:"All"},{value:"Available",label:"Available"},{value:"Assigned",label:"Assigned"}]} /><Select label="Category" value={filters.categoryId} onChange={e => handleFilterChange("categoryId", e.target.value)} options={[{value:"",label:"All"},...filterOptions.categories.map(c=>({value:c.categoryId,label:c.categoryName}))]} /></>);
    if (activeTab === "employee") return (<><Select label="Employee" value={filters.employeeId} onChange={e => handleFilterChange("employeeId", e.target.value)} options={[{value:"",label:"All"},...filterOptions.employees.map(e=>({value:e.employeeId,label:e.fullName}))]} /><Select label="Department" value={filters.department} onChange={e => handleFilterChange("department", e.target.value)} options={[{value:"",label:"All"},...filterOptions.departments.map(d=>({value:d,label:d}))]} /></>);
    if (activeTab === "maintenance") return (<><Input label="Start" type="date" value={filters.startDate} onChange={e => handleFilterChange("startDate", e.target.value)} /><Input label="End" type="date" value={filters.endDate} onChange={e => handleFilterChange("endDate", e.target.value)} /><Select label="Type" value={filters.isUpcoming} onChange={e => handleFilterChange("isUpcoming", e.target.value)} options={[{value:"",label:"All"},{value:"true",label:"Upcoming"}]} /></>);
    return (<><Input label="Start" type="date" value={filters.startDate} onChange={e => handleFilterChange("startDate", e.target.value)} /><Input label="End" type="date" value={filters.endDate} onChange={e => handleFilterChange("endDate", e.target.value)} /></>);
  };

  return (
    <div className="reports-menu fade-transition">
      <div className="page-header"><h1 className="page-title">Reports & Analytics</h1><Button variant="outline" onClick={handleExport} startIcon={<FiDownload />}>Export</Button></div>

      <div className="reports-menu__stats">
        <Card className="reports-menu__stat-card"><FiPackage /><div><h3>Total Assets</h3><p>{stats.totalAssets || 0}</p></div></Card>
        <Card className="reports-menu__stat-card"><FiDollarSign /><div><h3>Total Value</h3><p>{utilsHelper.formatCurrency(stats.totalAssetValue)}</p></div></Card>
        <Card className="reports-menu__stat-card"><FiUsers /><div><h3>Employees</h3><p>{stats.totalEmployees || 0}</p></div></Card>
        <Card className="reports-menu__stat-card"><FiClock /><div><h3>Pending</h3><p>{stats.pendingTransactions || 0}</p></div></Card>
      </div>

      <Tabs tabs={tabs} activeTab={activeTab} onTabChange={setActiveTab} />

      <Card className="reports-menu__filters">
        <div className="reports-menu__filter-header">
          <div className="reports-menu__filter-actions-left">
            <Button variant="text" size="sm" onClick={() => setShowFilters(!showFilters)} startIcon={<FiFilter />}>{showFilters ? "Hide" : "Show"} Filters</Button>
            {showFilters && <Button variant="text" size="sm" onClick={handleClearFilters} startIcon={<FiX />}>Clear</Button>}
          </div>
          <Button variant="primary" onClick={loadReportData} startIcon={<FiSearch />}>Generate</Button>
        </div>
        {showFilters && <div className="reports-menu__filter-grid">{renderFilters()}</div>}
      </Card>

      <Card className="reports-menu__table">
        {loading ? <div className="reports-menu__loading"><Spinner size="lg" /></div> : (<><DataTable rows={reportData} columns={getColumns()} loading={loading} pageSize={pageSize} hideFooter={true} getRowId={(_, i) => i} /><Pagination currentPage={page} totalPages={Math.ceil(totalCount / pageSize)} pageSize={pageSize} totalItems={totalCount} onPageChange={setPage} onPageSizeChange={setPageSize} /></>)}
      </Card>
    </div>
  );
};

export default ReportsMenu;