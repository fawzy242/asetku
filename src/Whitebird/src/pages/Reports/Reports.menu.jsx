import React, { useState, useEffect, useCallback, useMemo } from "react";
import { FiPackage, FiDollarSign, FiUsers, FiClock, FiDownload, FiRefreshCw, FiBox, FiTool, FiTrendingUp, FiSearch } from "react-icons/fi";
import ReportsData from "./Reports.data";
import DataTable from "../../components/molecules/DataTable/DataTable";
import Pagination from "../../components/molecules/Pagination/Pagination";
import Button from "../../components/atoms/Button/Button";
import Card from "../../components/atoms/Card/Card";
import Select from "../../components/atoms/Select/Select";
import Spinner from "../../components/atoms/Spinner/Spinner";
import Tabs from "../../components/molecules/Tabs/Tabs";
import utilsHelper from "../../core/utils/utils.helper";
import "./Reports.scss";

const reportsData = new ReportsData();

const TABS = [
  { id: "transactions", label: "Transactions", icon: <FiRefreshCw /> },
  { id: "inventory", label: "Inventory", icon: <FiBox /> },
  { id: "employee", label: "Employee Assets", icon: <FiUsers /> },
  { id: "maintenance", label: "Maintenance", icon: <FiTool /> },
  { id: "financial", label: "Financial", icon: <FiTrendingUp /> },
];

const EXPORT_REPORT_OPTIONS = [
  { value: "transactions", label: "Transaction Report" },
  { value: "inventory", label: "Inventory Report" },
  { value: "employee", label: "Employee Asset Report" },
  { value: "maintenance", label: "Maintenance Report" },
  { value: "financial", label: "Financial Report" },
];

const ReportsMenu = () => {
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("transactions");
  const [stats, setStats] = useState({});
  const [reportData, setReportData] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [exportReportType, setExportReportType] = useState("");
  const [isExporting, setIsExporting] = useState(false);

  useEffect(() => {
    loadStats();
  }, []);

  useEffect(() => {
    loadReportData();
  }, [activeTab, page, pageSize]);

  const loadStats = async () => {
    const r = await reportsData.fetchDashboardStats();
    if (r.success) setStats(r.data);
  };

  const loadReportData = async () => {
    setLoading(true);
    let r;
    switch (activeTab) {
      case "transactions": r = await reportsData.fetchTransactionData({ page, pageSize }); break;
      case "inventory": r = await reportsData.fetchInventoryData({ page, pageSize }); break;
      case "employee": r = await reportsData.fetchEmployeeAssetData({ page, pageSize }); break;
      case "maintenance": r = await reportsData.fetchMaintenanceData({ page, pageSize }); break;
      default: r = await reportsData.fetchFinancialData({ page, pageSize }); break;
    }
    if (r.success) { setReportData(r.data); setTotalCount(r.data.length); }
    setLoading(false);
  };

  const handleExport = async () => {
    if (!exportReportType) return;

    setIsExporting(true);
    let dataToExport = reportData;

    // Fetch data untuk report yang dipilih jika berbeda dengan tab aktif
    if (exportReportType !== activeTab) {
      let r;
      switch (exportReportType) {
        case "transactions": r = await reportsData.fetchTransactionData({}); break;
        case "inventory": r = await reportsData.fetchInventoryData({}); break;
        case "employee": r = await reportsData.fetchEmployeeAssetData({}); break;
        case "maintenance": r = await reportsData.fetchMaintenanceData({}); break;
        default: r = await reportsData.fetchFinancialData({}); break;
      }
      if (r.success) dataToExport = r.data;
    }

    const reportNames = {
      transactions: 'Transaction_Report',
      inventory: 'Inventory_Report',
      employee: 'Employee_Asset_Report',
      maintenance: 'Maintenance_Report',
      financial: 'Financial_Report',
    };

    const params = {};
    let serverResult = { success: false };

    switch (exportReportType) {
      case "transactions": serverResult = await reportsData.exportTransaction(params, dataToExport); break;
      case "inventory": serverResult = await reportsData.exportInventory(params, dataToExport); break;
      case "employee": serverResult = await reportsData.exportEmployee(params, dataToExport); break;
      case "maintenance": serverResult = await reportsData.exportMaintenance(params, dataToExport); break;
      default: serverResult = await reportsData.exportFinancial(params, dataToExport); break;
    }

    if (!serverResult.success && dataToExport && dataToExport.length > 0) {
      await reportsData.exportToExcel(dataToExport, reportNames[exportReportType] || 'Report');
    }

    setIsExporting(false);
  };

  const handleTabChange = useCallback((tab) => {
    setActiveTab(tab);
    setPage(1);
  }, []);

  const getColumns = useMemo(() => {
    switch (activeTab) {
      case "transactions":
        return [
          { field: "assetCode", headerName: "Code", width: 120 },
          { field: "assetName", headerName: "Name", flex: 1, minWidth: 160 },
          { field: "fullName", headerName: "Employee", width: 160 },
          { field: "transactionType", headerName: "Type", width: 150 },
          { field: "transactionStatus", headerName: "Status", width: 110 },
          { field: "transactionDate", headerName: "Date", width: 160, valueFormatter: (p) => (p?.value && p.value !== '') ? utilsHelper.formatDateTime(p.value) : '-' },
        ];
      case "inventory":
        return [
          { field: "assetCode", headerName: "Code", width: 120 },
          { field: "assetName", headerName: "Name", flex: 1, minWidth: 180 },
          { field: "categoryName", headerName: "Category", width: 150 },
          { field: "status", headerName: "Status", width: 140 },
          { field: "currentHolderName", headerName: "Holder", width: 150 },
        ];
      case "employee":
        return [
          { field: "employeeCode", headerName: "Emp Code", width: 110 },
          { field: "fullName", headerName: "Employee", flex: 1, minWidth: 160 },
          { field: "department", headerName: "Dept", width: 140 },
          { field: "assetCode", headerName: "Asset", width: 120 },
          { field: "assetName", headerName: "Name", flex: 1, minWidth: 160 },
        ];
      case "maintenance":
        return [
          { field: "assetCode", headerName: "Code", width: 120 },
          { field: "assetName", headerName: "Name", flex: 1, minWidth: 180 },
          { field: "maintenanceCount", headerName: "Count", width: 90 },
          { field: "lastMaintenanceDate", headerName: "Last", width: 120, valueFormatter: (p) => (p?.value && p.value !== '') ? utilsHelper.formatDate(p.value) : '-' },
          { field: "nextMaintenanceDate", headerName: "Next", width: 120, valueFormatter: (p) => (p?.value && p.value !== '') ? utilsHelper.formatDate(p.value) : '-' },
        ];
      default:
        return [
          { field: "assetCode", headerName: "Code", width: 120 },
          { field: "assetName", headerName: "Name", flex: 1, minWidth: 180 },
          { field: "categoryName", headerName: "Category", width: 150 },
          { field: "purchasePrice", headerName: "Price", width: 130, valueFormatter: (p) => (p?.value != null) ? utilsHelper.formatCurrency(p.value) : '-' },
          { field: "totalMaintenanceCost", headerName: "Maint. Cost", width: 130, valueFormatter: (p) => (p?.value != null) ? utilsHelper.formatCurrency(p.value) : '-' },
        ];
    }
  }, [activeTab]);

  return (
    <div className="reports-menu fade-transition">
      <div className="page-header">
        <h1 className="page-title">Reports & Analytics</h1>
        <div style={{ display: 'flex', gap: '12px', alignItems: 'center' }}>
          <div style={{ minWidth: '240px' }}>
            <Select
              label="Select Report to Download"
              value={exportReportType}
              onChange={e => setExportReportType(e.target.value)}
              options={EXPORT_REPORT_OPTIONS}
              size="small"
            />
          </div>
          <Button
            variant="primary"
            onClick={handleExport}
            startIcon={<FiDownload />}
            loading={isExporting}
            disabled={!exportReportType}
          >
            Export
          </Button>
        </div>
      </div>

      <div className="reports-menu__stats">
        <Card className="reports-menu__stat-card">
          <FiPackage />
          <div><h3>Total Assets</h3><p>{stats.totalAssets || 0}</p></div>
        </Card>
        <Card className="reports-menu__stat-card">
          <FiDollarSign />
          <div><h3>Total Value</h3><p>{utilsHelper.formatCurrency(stats.totalAssetValue)}</p></div>
        </Card>
        <Card className="reports-menu__stat-card">
          <FiUsers />
          <div><h3>Employees</h3><p>{stats.totalEmployees || 0}</p></div>
        </Card>
        <Card className="reports-menu__stat-card">
          <FiClock />
          <div><h3>Pending Txns</h3><p>{stats.pendingTransactions || 0}</p></div>
        </Card>
      </div>

      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />

      <Card className="reports-menu__table">
        {loading ? (
          <div className="page-loading"><Spinner size="lg" /></div>
        ) : (
          <>
            <DataTable
              rows={reportData}
              columns={getColumns}
              loading={loading}
              pageSize={pageSize}
              hideFooter={true}
              getRowId={(_, i) => `report-row-${i}`}
              ariaLabel="Reports data table"
            />
            <Pagination
              currentPage={page}
              totalPages={Math.ceil(totalCount / pageSize) || 1}
              pageSize={pageSize}
              totalItems={totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
            />
          </>
        )}
      </Card>
    </div>
  );
};

export default ReportsMenu;