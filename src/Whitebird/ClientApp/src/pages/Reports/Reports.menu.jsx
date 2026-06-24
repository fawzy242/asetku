import React, { useState, useCallback, useEffect, useRef } from "react";
import { FiDownload, FiFileText, FiBox, FiUsers, FiTool, FiTrendingUp } from "react-icons/fi";
import { Grid, Typography, Chip } from "@mui/material";
import ReportsData from "./Reports.data";
import Card from "../../components/atoms/Card/Card";
import Button from "../../components/atoms/Button/Button";
import Select from "../../components/atoms/Select/Select";
import DatePickerInput from "../../components/atoms/Input/DatePickerInput";
import Spinner from "../../components/atoms/Spinner/Spinner";
import { useReferenceData } from "../../hooks/useReferenceData";
import { useOptions } from "../../hooks/useOptions";
import { useSweetAlert } from "../../hooks/useSweetAlert";
import "./Reports.scss";

const reportsData = new ReportsData();

const REPORT_TYPES = [
  { id: "transactions", label: "Transaction Report", icon: <FiFileText />, description: "Export asset transaction history", hasDateRange: true, hasTransactionTypeFilter: true },
  { id: "inventory", label: "Inventory Report", icon: <FiBox />, description: "Export current asset inventory", hasStatusFilter: true, hasCategoryFilter: true, hasSupplierFilter: true },
  { id: "employee", label: "Employee Asset Report", icon: <FiUsers />, description: "Export assets assigned to employees", hasEmployeeFilter: true },
  { id: "maintenance", label: "Maintenance Report", icon: <FiTool />, description: "Export maintenance history", hasDateRange: true },
  { id: "financial", label: "Financial Report", icon: <FiTrendingUp />, description: "Export asset financial summary", hasDateRange: true },
];

const INVENTORY_STATUS_OPTIONS = [
  { value: "", label: "All Statuses" },
  { value: "Available", label: "Available" },
  { value: "Assigned", label: "Assigned" },
  { value: "Damaged", label: "Damaged" },
];

const TRANSACTION_TYPE_OPTIONS = [
  { value: "", label: "All Types" },
  { value: "HANDOVER", label: "HANDOVER" },
  { value: "TRANSFER", label: "TRANSFER" },
  { value: "LOAN", label: "LOAN" },
  { value: "RETURN", label: "RETURN" },
  { value: "LOAN_RETURN", label: "LOAN_RETURN" },
  { value: "MAINTENANCE", label: "MAINTENANCE" },
  { value: "POST_MAINTENANCE", label: "POST_MAINTENANCE" },
  { value: "DISPOSAL", label: "DISPOSAL" },
];

const ReportsMenu = () => {
  const [selectedReport, setSelectedReport] = useState("transactions");
  const [isExporting, setIsExporting] = useState(false);
  const [dateRange, setDateRange] = useState({ startDate: "", endDate: "" });
  const [statusFilter, setStatusFilter] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [supplierFilter, setSupplierFilter] = useState("");
  const [transactionTypeFilter, setTransactionTypeFilter] = useState("");
  const [employeeFilter, setEmployeeFilter] = useState("");

  const { categories, suppliers, employees, isLoading: loadingRefData } = useReferenceData();
  const { toast } = useSweetAlert();
  const isMountedRef = useRef(true);

  useEffect(() => {
    isMountedRef.current = true;
    return () => { isMountedRef.current = false; };
  }, []);

  const currentReport = REPORT_TYPES.find(r => r.id === selectedReport) || REPORT_TYPES[0];

  const categoryOptions = useOptions(categories, "All Categories");
  const supplierOptions = useOptions(suppliers, "All Suppliers");
  const employeeOptions = useOptions(
    employees.map(e => ({ value: e.value, label: e.label })),
    "All Employees"
  );

  const buildParams = useCallback(() => {
    const params = {};
    if (currentReport.hasDateRange) {
      if (dateRange.startDate) params.startDate = dateRange.startDate;
      if (dateRange.endDate) params.endDate = dateRange.endDate;
    }
    if (currentReport.hasStatusFilter && statusFilter) params.status = statusFilter;
    if (currentReport.hasCategoryFilter && categoryFilter) params.categoryId = categoryFilter;
    if (currentReport.hasSupplierFilter && supplierFilter) params.supplierId = supplierFilter;
    if (currentReport.hasTransactionTypeFilter && transactionTypeFilter) params.transactionType = transactionTypeFilter;
    if (currentReport.hasEmployeeFilter && employeeFilter) params.employeeId = employeeFilter;
    return params;
  }, [currentReport, dateRange, statusFilter, categoryFilter, supplierFilter, transactionTypeFilter, employeeFilter]);

  const handleExport = async () => {
    setIsExporting(true);
    const params = buildParams();
    let result = { success: false };
    try {
      switch (selectedReport) {
        case "transactions": result = await reportsData.exportTransaction(params); break;
        case "inventory": result = await reportsData.exportInventory(params); break;
        case "employee": result = await reportsData.exportEmployee(params); break;
        case "maintenance": result = await reportsData.exportMaintenance(params); break;
        case "financial": result = await reportsData.exportFinancial(params); break;
        default: break;
      }
    } catch (error) {
      toast.error("Failed to generate report. Please try again.");
    }
    setIsExporting(false);
    if (!result.success) toast.error("Failed to generate report. Please try again.");
  };

  const resetFilters = () => {
    setDateRange({ startDate: "", endDate: "" });
    setStatusFilter("");
    setCategoryFilter("");
    setSupplierFilter("");
    setTransactionTypeFilter("");
    setEmployeeFilter("");
  };

  if (loadingRefData) {
    return (
      <div className="reports-menu">
        <div className="page-header"><h1 className="page-title">Reports & Analytics</h1></div>
        <div className="page-loading"><Spinner size="lg" /></div>
      </div>
    );
  }

  return (
    <div className="reports-menu fade-transition">
      <div className="page-header"><h1 className="page-title">Reports & Analytics</h1></div>
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Card title="Select Report Type">
            <div className="reports-menu__report-types">
              {REPORT_TYPES.map(report => (
                <button key={report.id} className={`reports-menu__report-card ${selectedReport === report.id ? "reports-menu__report-card--active" : ""}`} onClick={() => setSelectedReport(report.id)}>
                  <div className="reports-menu__report-icon">{report.icon}</div>
                  <div className="reports-menu__report-info">
                    <h3 className="reports-menu__report-label">{report.label}</h3>
                    <p className="reports-menu__report-description">{report.description}</p>
                  </div>
                  {selectedReport === report.id && <Chip label="Selected" size="small" color="primary" />}
                </button>
              ))}
            </div>
          </Card>
        </Grid>
        <Grid item xs={12}>
          <Card title="Filter Options">
            <div className="reports-menu__filters">
              {currentReport.hasDateRange && (
                <>
                  <DatePickerInput
                    label="Start Date"
                    value={dateRange.startDate}
                    onChange={e => setDateRange(prev => ({ ...prev, startDate: e.target.value }))}
                  />
                  <DatePickerInput
                    label="End Date"
                    value={dateRange.endDate}
                    onChange={e => setDateRange(prev => ({ ...prev, endDate: e.target.value }))}
                  />
                </>
              )}
              {currentReport.hasStatusFilter && (
                <Select
                  label="Status"
                  value={statusFilter}
                  onChange={e => setStatusFilter(e.target.value)}
                  options={INVENTORY_STATUS_OPTIONS}
                />
              )}
              {currentReport.hasCategoryFilter && (
                <Select
                  label="Category"
                  value={categoryFilter}
                  onChange={e => setCategoryFilter(e.target.value)}
                  options={categoryOptions}
                />
              )}
              {currentReport.hasSupplierFilter && (
                <Select
                  label="Supplier"
                  value={supplierFilter}
                  onChange={e => setSupplierFilter(e.target.value)}
                  options={supplierOptions}
                />
              )}
              {currentReport.hasTransactionTypeFilter && (
                <Select
                  label="Transaction Type"
                  value={transactionTypeFilter}
                  onChange={e => setTransactionTypeFilter(e.target.value)}
                  options={TRANSACTION_TYPE_OPTIONS}
                />
              )}
              {currentReport.hasEmployeeFilter && (
                <Select
                  label="Employee"
                  value={employeeFilter}
                  onChange={e => setEmployeeFilter(e.target.value)}
                  options={employeeOptions}
                />
              )}
              <div className="reports-menu__filter-actions">
                <Button variant="outline" onClick={resetFilters}>Reset Filters</Button>
              </div>
            </div>
          </Card>
        </Grid>
        <Grid item xs={12}>
          <Card className="reports-menu__export-card">
            <div className="reports-menu__export-content">
              <div className="reports-menu__export-icon"><FiDownload size={32} /></div>
              <div className="reports-menu__export-info">
                <Typography variant="h6" fontWeight={600}>Download {currentReport.label}</Typography>
                <Typography variant="body2" color="text.secondary">{currentReport.description} will be exported as Excel file (.xlsx)</Typography>
              </div>
              <Button variant="primary" size="lg" onClick={handleExport} loading={isExporting} startIcon={<FiDownload />}>
                Generate Report
              </Button>
            </div>
          </Card>
        </Grid>
      </Grid>
    </div>
  );
};

export default ReportsMenu;