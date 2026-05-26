import React, { useState, useCallback, useEffect, useRef } from "react";
import { FiDownload, FiFileText, FiCalendar, FiBox, FiUsers, FiTool, FiTrendingUp } from "react-icons/fi";
import { Grid, Box, Typography, Chip } from "@mui/material";
import ReportsData from "./Reports.data";
import Card from "../../components/atoms/Card/Card";
import Button from "../../components/atoms/Button/Button";
import Select from "../../components/atoms/Select/Select";
import DatePickerInput from "../../components/atoms/Input/DatePickerInput";
import Spinner from "../../components/atoms/Spinner/Spinner";
import ConfirmDialog from "../../components/molecules/ConfirmDialog/ConfirmDialog";
import "./Reports.scss";

const reportsData = new ReportsData();

const REPORT_TYPES = [
  { 
    id: "transactions", 
    label: "Transaction Report", 
    icon: <FiFileText />,
    description: "Export asset transaction history",
    hasDateRange: true,
    hasTypeFilter: true,
  },
  { 
    id: "inventory", 
    label: "Inventory Report", 
    icon: <FiBox />,
    description: "Export current asset inventory",
    hasStatusFilter: true,
    hasCategoryFilter: true,
  },
  { 
    id: "employee", 
    label: "Employee Asset Report", 
    icon: <FiUsers />,
    description: "Export assets assigned to employees",
    hasEmployeeFilter: true,
    hasDepartmentFilter: true,
  },
  { 
    id: "maintenance", 
    label: "Maintenance Report", 
    icon: <FiTool />,
    description: "Export maintenance history",
    hasDateRange: true,
  },
  { 
    id: "financial", 
    label: "Financial Report", 
    icon: <FiTrendingUp />,
    description: "Export asset financial summary",
    hasDateRange: true,
  },
];

const ReportsMenu = () => {
  const [selectedReport, setSelectedReport] = useState("transactions");
  const [isExporting, setIsExporting] = useState(false);
  const [dateRange, setDateRange] = useState({ startDate: "", endDate: "" });
  const [statusFilter, setStatusFilter] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [typeFilter, setTypeFilter] = useState("");
  const [employeeFilter, setEmployeeFilter] = useState("");
  const [departmentFilter, setDepartmentFilter] = useState("");
  
  const [categories, setCategories] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [loadingFilters, setLoadingFilters] = useState(true);
  
  const isMountedRef = useRef(true);
  const abortControllerRef = useRef(null);

  useEffect(() => {
    isMountedRef.current = true;
    
    const loadFilters = async () => {
      // Cancel previous request if exists
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
      
      // Create new AbortController
      abortControllerRef.current = new AbortController();
      
      setLoadingFilters(true);
      
      try {
        // Use Promise.allSettled instead of Promise.all to handle individual failures
        const results = await Promise.allSettled([
          reportsData.api.getCategories({ signal: abortControllerRef.current.signal }),
          reportsData.api.getEmployees({ signal: abortControllerRef.current.signal })
        ]);
        
        // Only update state if component is still mounted
        if (!isMountedRef.current) return;
        
        // Handle categories result
        if (results[0].status === 'fulfilled' && results[0].value?.data) {
          setCategories(results[0].value.data);
        } else {
          console.warn("Failed to load categories:", results[0].reason);
          setCategories([]);
        }
        
        // Handle employees result
        if (results[1].status === 'fulfilled' && results[1].value?.data) {
          const empData = results[1].value.data;
          setEmployees(empData);
          const depts = [...new Set(empData.map(e => e.departmentName || e.department).filter(Boolean))];
          setDepartments(depts);
        } else {
          console.warn("Failed to load employees:", results[1].reason);
          setEmployees([]);
          setDepartments([]);
        }
        
      } catch (error) {
        // Ignore cancel errors (they are expected)
        if (error?.name === 'CanceledError' || error?.message === 'canceled') {
          console.log("Request was cancelled - this is normal when component unmounts");
        } else {
          console.error("Failed to load filters:", error);
        }
      } finally {
        if (isMountedRef.current) {
          setLoadingFilters(false);
        }
      }
    };
    
    loadFilters();
    
    // Cleanup function: cancel requests and mark component as unmounted
    return () => {
      isMountedRef.current = false;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, []);

  const currentReport = REPORT_TYPES.find(r => r.id === selectedReport) || REPORT_TYPES[0];

  const buildParams = useCallback(() => {
    const params = {};
    if (currentReport.hasDateRange) {
      if (dateRange.startDate) params.startDate = dateRange.startDate;
      if (dateRange.endDate) params.endDate = dateRange.endDate;
    }
    if (currentReport.hasStatusFilter && statusFilter) params.status = statusFilter;
    if (currentReport.hasCategoryFilter && categoryFilter) params.categoryId = categoryFilter;
    if (currentReport.hasTypeFilter && typeFilter) params.transactionType = typeFilter;
    if (currentReport.hasEmployeeFilter && employeeFilter) params.employeeId = employeeFilter;
    if (currentReport.hasDepartmentFilter && departmentFilter) params.department = departmentFilter;
    return params;
  }, [currentReport, dateRange, statusFilter, categoryFilter, typeFilter, employeeFilter, departmentFilter]);

  const handleExport = async () => {
    setIsExporting(true);
    
    const params = buildParams();
    let result = { success: false };
    
    try {
      switch (selectedReport) {
        case "transactions":
          result = await reportsData.exportTransaction(params, []);
          break;
        case "inventory":
          result = await reportsData.exportInventory(params, []);
          break;
        case "employee":
          result = await reportsData.exportEmployee(params, []);
          break;
        case "maintenance":
          result = await reportsData.exportMaintenance(params, []);
          break;
        case "financial":
          result = await reportsData.exportFinancial(params, []);
          break;
        default:
          break;
      }
    } catch (error) {
      console.error("Export failed:", error);
      ConfirmDialog.toast.error("Failed to generate report. Please try again.");
    }
    
    setIsExporting(false);
    
    if (!result.success) {
      ConfirmDialog.toast.error("Failed to generate report. Please try again.");
    }
  };

  const resetFilters = () => {
    setDateRange({ startDate: "", endDate: "" });
    setStatusFilter("");
    setCategoryFilter("");
    setTypeFilter("");
    setEmployeeFilter("");
    setDepartmentFilter("");
  };

  const categoryOptions = [
    { value: "", label: "All Categories" },
    ...(categories || []).map(c => ({ value: c.categoryId, label: c.categoryName }))
  ];
  
  const employeeOptions = [
    { value: "", label: "All Employees" },
    ...(employees || []).map(e => ({ value: e.employeeId, label: `${e.employeeCode} - ${e.fullName}` }))
  ];
  
  const departmentOptions = [
    { value: "", label: "All Departments" },
    ...departments.map(d => ({ value: d, label: d }))
  ];
  
  const statusOptions = [
    { value: "", label: "All Statuses" },
    { value: "Active", label: "Active" },
    { value: "Inactive", label: "Inactive" },
  ];
  
  const transactionTypeOptions = [
    { value: "", label: "All Types" },
    { value: "1", label: "HANDOVER" },
    { value: "2", label: "TRANSFER" },
    { value: "3", label: "LOAN" },
    { value: "4", label: "RETURN" },
    { value: "5", label: "LOAN_RETURN" },
    { value: "6", label: "MAINTENANCE" },
    { value: "7", label: "POST_MAINTENANCE" },
    { value: "8", label: "DISPOSAL" },
  ];

  if (loadingFilters) {
    return (
      <div className="reports-menu">
        <div className="page-header">
          <h1 className="page-title">Reports & Analytics</h1>
        </div>
        <div className="page-loading"><Spinner size="lg" /></div>
      </div>
    );
  }

  return (
    <div className="reports-menu fade-transition">
      <div className="page-header">
        <h1 className="page-title">Reports & Analytics</h1>
      </div>
      
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Card title="Select Report Type">
            <div className="reports-menu__report-types">
              {REPORT_TYPES.map(report => (
                <button
                  key={report.id}
                  className={`reports-menu__report-card ${selectedReport === report.id ? 'reports-menu__report-card--active' : ''}`}
                  onClick={() => setSelectedReport(report.id)}
                >
                  <div className="reports-menu__report-icon">{report.icon}</div>
                  <div className="reports-menu__report-info">
                    <h3 className="reports-menu__report-label">{report.label}</h3>
                    <p className="reports-menu__report-description">{report.description}</p>
                  </div>
                  {selectedReport === report.id && (
                    <Chip label="Selected" size="small" color="primary" />
                  )}
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
                  options={statusOptions}
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
              
              {currentReport.hasTypeFilter && (
                <Select
                  label="Transaction Type"
                  value={typeFilter}
                  onChange={e => setTypeFilter(e.target.value)}
                  options={transactionTypeOptions}
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
              
              {currentReport.hasDepartmentFilter && (
                <Select
                  label="Department"
                  value={departmentFilter}
                  onChange={e => setDepartmentFilter(e.target.value)}
                  options={departmentOptions}
                />
              )}
              
              <div className="reports-menu__filter-actions">
                <Button variant="outline" onClick={resetFilters}>
                  Reset Filters
                </Button>
              </div>
            </div>
          </Card>
        </Grid>

        <Grid item xs={12}>
          <Card className="reports-menu__export-card">
            <div className="reports-menu__export-content">
              <div className="reports-menu__export-icon">
                <FiDownload size={32} />
              </div>
              <div className="reports-menu__export-info">
                <Typography variant="h6" fontWeight={600}>
                  Download {currentReport.label}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {currentReport.description} will be exported as Excel file (.xlsx)
                </Typography>
              </div>
              <Button
                variant="primary"
                size="lg"
                onClick={handleExport}
                loading={isExporting}
                startIcon={<FiDownload />}
              >
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