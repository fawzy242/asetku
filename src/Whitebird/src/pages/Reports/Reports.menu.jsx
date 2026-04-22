import React, { useState, useEffect, useCallback } from "react";
import {
  FiPackage,
  FiDollarSign,
  FiUsers,
  FiClock,
  FiDownload,
  FiRefreshCw,
  FiBox,
  FiTool,
  FiTrendingUp,
  FiSearch,
  FiFilter,
  FiX,
} from "react-icons/fi";
import ReportsData from "./Reports.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Card from "../../components/atoms/Card/Card";
import Input from "../../components/atoms/Input/Input";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import utilsHelper from "../../core/utils/utils.helper";
import "./Reports.scss";

const ReportsMenu = () => {
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("transactions");
  const [stats, setStats] = useState({});
  const [reportData, setReportData] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [showFilters, setShowFilters] = useState(false);

  const [filters, setFilters] = useState({
    startDate: "",
    endDate: "",
    transactionType: "",
    status: "",
    categoryId: "",
    supplierId: "",
    employeeId: "",
    department: "",
    isUpcoming: "",
  });

  const [filterOptions, setFilterOptions] = useState({
    categories: [],
    suppliers: [],
    employees: [],
    departments: [],
  });

  const reportsData = new ReportsData();

  const loadStats = useCallback(async () => {
    const result = await reportsData.loadDashboardStats();
    if (result.success) setStats(result.data);
  }, []);

  const loadFilterOptions = useCallback(async () => {
    const result = await reportsData.loadFilterOptions();
    if (result.success) setFilterOptions(result.data);
  }, []);

  const loadReportData = useCallback(async () => {
    setLoading(true);
    let result;
    switch (activeTab) {
      case "transactions":
        result = await reportsData.loadAssetTransactionData({ ...filters, page, pageSize });
        break;
      case "inventory":
        result = await reportsData.loadAssetInventoryData({ ...filters, page, pageSize });
        break;
      case "employee":
        result = await reportsData.loadEmployeeAssetData({ ...filters, page, pageSize });
        break;
      case "maintenance":
        result = await reportsData.loadMaintenanceData({ ...filters, page, pageSize });
        break;
      case "financial":
        result = await reportsData.loadFinancialData({ ...filters, page, pageSize });
        break;
      default:
        result = { success: false, data: [] };
    }
    if (result.success) {
      setReportData(result.data || []);
      setTotalCount(result.totalCount || result.data?.length || 0);
    }
    setLoading(false);
  }, [activeTab, filters, page, pageSize]);

  useEffect(() => {
    loadStats();
    loadFilterOptions();
  }, [loadStats, loadFilterOptions]);

  useEffect(() => {
    loadReportData();
  }, [loadReportData]);

  const handleExport = async () => {
    switch (activeTab) {
      case "transactions":
        await reportsData.exportAssetTransaction(filters);
        break;
      case "inventory":
        await reportsData.exportAssetInventory(filters);
        break;
      case "employee":
        await reportsData.exportEmployeeAsset(filters);
        break;
      case "maintenance":
        await reportsData.exportMaintenance(filters);
        break;
      case "financial":
        await reportsData.exportFinancial(filters);
        break;
    }
  };

  const handleFilterChange = (key, value) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
    setPage(1);
  };

  const handleClearFilters = () => {
    setFilters({
      startDate: "",
      endDate: "",
      transactionType: "",
      status: "",
      categoryId: "",
      supplierId: "",
      employeeId: "",
      department: "",
      isUpcoming: "",
    });
    setPage(1);
  };

  const tabs = [
    { id: "transactions", label: "Transactions", icon: <FiRefreshCw /> },
    { id: "inventory", label: "Inventory", icon: <FiBox /> },
    { id: "employee", label: "Employee Assets", icon: <FiUsers /> },
    { id: "maintenance", label: "Maintenance", icon: <FiTool /> },
    { id: "financial", label: "Financial", icon: <FiTrendingUp /> },
  ];

  const getColumns = () => {
    switch (activeTab) {
      case "transactions":
        return [
          { field: "assetCode", headerName: "Asset Code", width: 120 },
          { field: "assetName", headerName: "Asset Name", width: 180 },
          { field: "fullName", headerName: "Employee", width: 160 },
          { field: "transactionType", headerName: "Type", width: 120 },
          { field: "transactionStatus", headerName: "Status", width: 110 },
          { field: "transactionDate", headerName: "Date", width: 160, valueFormatter: (p) => utilsHelper.formatDateTime(p.value) },
          { field: "purchasePrice", headerName: "Price", width: 130, valueFormatter: (p) => utilsHelper.formatCurrency(p.value) },
        ];
      case "inventory":
        return [
          { field: "assetCode", headerName: "Code", width: 120 },
          { field: "assetName", headerName: "Name", width: 200 },
          { field: "categoryName", headerName: "Category", width: 150 },
          { field: "status", headerName: "Status", width: 110 },
          { field: "condition", headerName: "Condition", width: 100 },
          { field: "currentHolderName", headerName: "Holder", width: 150 },
          { field: "purchasePrice", headerName: "Price", width: 130, valueFormatter: (p) => utilsHelper.formatCurrency(p.value) },
        ];
      case "employee":
        return [
          { field: "employeeCode", headerName: "Emp Code", width: 110 },
          { field: "fullName", headerName: "Employee", width: 180 },
          { field: "department", headerName: "Department", width: 140 },
          { field: "assetCode", headerName: "Asset Code", width: 120 },
          { field: "assetName", headerName: "Asset Name", width: 180 },
          { field: "categoryName", headerName: "Category", width: 140 },
          { field: "assignmentDate", headerName: "Assigned", width: 120, valueFormatter: (p) => utilsHelper.formatDate(p.value) },
        ];
      case "maintenance":
        return [
          { field: "assetCode", headerName: "Code", width: 120 },
          { field: "assetName", headerName: "Name", width: 200 },
          { field: "categoryName", headerName: "Category", width: 150 },
          { field: "maintenanceCount", headerName: "Count", width: 90 },
          { field: "lastMaintenanceDate", headerName: "Last", width: 120, valueFormatter: (p) => utilsHelper.formatDate(p.value) },
          { field: "nextMaintenanceDate", headerName: "Next", width: 120, valueFormatter: (p) => utilsHelper.formatDate(p.value) },
        ];
      case "financial":
        return [
          { field: "assetCode", headerName: "Code", width: 120 },
          { field: "assetName", headerName: "Name", width: 200 },
          { field: "categoryName", headerName: "Category", width: 150 },
          { field: "supplierName", headerName: "Supplier", width: 150 },
          { field: "purchasePrice", headerName: "Price", width: 130, valueFormatter: (p) => utilsHelper.formatCurrency(p.value) },
          { field: "totalMaintenanceCost", headerName: "Maint. Cost", width: 130, valueFormatter: (p) => utilsHelper.formatCurrency(p.value) },
        ];
      default:
        return [];
    }
  };

  const renderFilters = () => {
    switch (activeTab) {
      case "transactions":
        return (
          <>
            <Input label="Start Date" type="date" value={filters.startDate} onChange={(e) => handleFilterChange("startDate", e.target.value)} />
            <Input label="End Date" type="date" value={filters.endDate} onChange={(e) => handleFilterChange("endDate", e.target.value)} />
            <Select label="Type" value={filters.transactionType} onChange={(e) => handleFilterChange("transactionType", e.target.value)} options={[{ value: "", label: "All" }, { value: "Assignment", label: "Assignment" }, { value: "Return", label: "Return" }]} />
          </>
        );
      case "inventory":
        return (
          <>
            <Select label="Status" value={filters.status} onChange={(e) => handleFilterChange("status", e.target.value)} options={[{ value: "", label: "All" }, { value: "Available", label: "Available" }, { value: "Assigned", label: "Assigned" }]} />
            <Select label="Category" value={filters.categoryId} onChange={(e) => handleFilterChange("categoryId", e.target.value)} options={[{ value: "", label: "All" }, ...filterOptions.categories.map((c) => ({ value: c.categoryId, label: c.categoryName }))]} />
          </>
        );
      case "employee":
        return (
          <>
            <Select label="Employee" value={filters.employeeId} onChange={(e) => handleFilterChange("employeeId", e.target.value)} options={[{ value: "", label: "All" }, ...filterOptions.employees.map((e) => ({ value: e.employeeId, label: e.fullName }))]} />
            <Select label="Department" value={filters.department} onChange={(e) => handleFilterChange("department", e.target.value)} options={[{ value: "", label: "All" }, ...filterOptions.departments.map((d) => ({ value: d, label: d }))]} />
          </>
        );
      case "maintenance":
        return (
          <>
            <Input label="Start Date" type="date" value={filters.startDate} onChange={(e) => handleFilterChange("startDate", e.target.value)} />
            <Input label="End Date" type="date" value={filters.endDate} onChange={(e) => handleFilterChange("endDate", e.target.value)} />
            <Select label="Type" value={filters.isUpcoming} onChange={(e) => handleFilterChange("isUpcoming", e.target.value)} options={[{ value: "", label: "All" }, { value: "true", label: "Upcoming Only" }]} />
          </>
        );
      case "financial":
        return (
          <>
            <Input label="Start Date" type="date" value={filters.startDate} onChange={(e) => handleFilterChange("startDate", e.target.value)} />
            <Input label="End Date" type="date" value={filters.endDate} onChange={(e) => handleFilterChange("endDate", e.target.value)} />
          </>
        );
      default:
        return null;
    }
  };

  return (
    <div className="reports-menu fade-transition">
      <div className="page-header">
        <h1 className="page-title">Reports & Analytics</h1>
        <Button variant="outline" onClick={handleExport} startIcon={<FiDownload />}>
          Export
        </Button>
      </div>

      <div className="reports-menu__stats">
        <Card className="reports-menu__stat-card">
          <FiPackage className="reports-menu__stat-icon" />
          <div><h3>Total Assets</h3><p>{stats.totalAssets || 0}</p></div>
        </Card>
        <Card className="reports-menu__stat-card">
          <FiDollarSign className="reports-menu__stat-icon" />
          <div><h3>Total Value</h3><p>{utilsHelper.formatCurrency(stats.totalAssetValue)}</p></div>
        </Card>
        <Card className="reports-menu__stat-card">
          <FiUsers className="reports-menu__stat-icon" />
          <div><h3>Employees</h3><p>{stats.totalEmployees || 0}</p></div>
        </Card>
        <Card className="reports-menu__stat-card">
          <FiClock className="reports-menu__stat-icon" />
          <div><h3>Pending</h3><p>{stats.pendingTransactions || 0}</p></div>
        </Card>
      </div>

      <div className="reports-menu__tabs">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            className={`reports-menu__tab ${activeTab === tab.id ? "active" : ""}`}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.icon} <span>{tab.label}</span>
          </button>
        ))}
      </div>

      <Card className="reports-menu__filters">
        <div className="reports-menu__filter-header">
          <Button variant="text" size="sm" onClick={() => setShowFilters(!showFilters)} startIcon={<FiFilter />}>
            {showFilters ? "Hide Filters" : "Show Filters"}
          </Button>
          {showFilters && (
            <Button variant="text" size="sm" onClick={handleClearFilters} startIcon={<FiX />}>
              Clear
            </Button>
          )}
        </div>
        {showFilters && <div className="reports-menu__filter-grid">{renderFilters()}</div>}
        <div className="reports-menu__filter-actions">
          <Button variant="primary" onClick={loadReportData} startIcon={<FiSearch />}>
            Generate Report
          </Button>
        </div>
      </Card>

      <Card className="reports-menu__table">
        {loading ? (
          <div className="reports-menu__loading"><Spinner size="lg" /></div>
        ) : (
          <>
            <DataTable rows={reportData} columns={getColumns()} loading={loading} pageSize={pageSize} hideFooter={true} getRowId={(row, index) => index} />
            <Pagination
              currentPage={page}
              totalPages={Math.ceil(totalCount / pageSize)}
              pageSize={pageSize}
              totalItems={totalCount}
              onPageChange={setPage}
              onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
            />
          </>
        )}
      </Card>
    </div>
  );
};

export default ReportsMenu;