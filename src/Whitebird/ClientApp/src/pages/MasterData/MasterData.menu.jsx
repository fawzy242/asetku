import React, { useState } from "react";
import { FiDatabase, FiTag, FiUsers, FiBriefcase, FiMapPin, FiTool, FiPackage, FiRefreshCw } from "react-icons/fi";
import { Box, Chip, Typography } from "@mui/material";
import { useReferenceData } from "../../hooks/useReferenceData";
import Card from "../../components/atoms/Card/Card";
import Spinner from "../../components/atoms/Spinner/Spinner";
import Tabs from "../../components/molecules/Tabs/Tabs";
import "./MasterData.scss";

const TABS = [
  { id: "transaction-types", label: "Transaction Types", icon: <FiRefreshCw /> },
  { id: "asset-conditions", label: "Asset Conditions", icon: <FiPackage /> },
  { id: "employee-positions", label: "Employee Positions", icon: <FiBriefcase /> },
  { id: "employee-statuses", label: "Employee Statuses", icon: <FiUsers /> },
  { id: "office-types", label: "Office Types", icon: <FiMapPin /> },
  { id: "maintenance-types", label: "Maintenance Types", icon: <FiTool /> },
  { id: "asset-condition-purchases", label: "Purchase Conditions", icon: <FiTag /> },
];

const MasterDataMenu = () => {
  const [activeTab, setActiveTab] = useState("transaction-types");
  
  const { 
    transactionTypes, 
    assetConditions, 
    employeePositions, 
    employeeStatuses, 
    officeTypes, 
    maintenanceTypes, 
    assetConditionPurchases,
    isLoading,
    isError
  } = useReferenceData();

  const getDataForTab = () => {
    switch (activeTab) {
      case "transaction-types":
        return { data: transactionTypes, title: "Transaction Types", icon: <FiRefreshCw />, color: "#3b82f6" };
      case "asset-conditions":
        return { data: assetConditions, title: "Asset Conditions", icon: <FiPackage />, color: "#10b981" };
      case "employee-positions":
        return { data: employeePositions, title: "Employee Positions", icon: <FiBriefcase />, color: "#f59e0b" };
      case "employee-statuses":
        return { data: employeeStatuses, title: "Employee Statuses", icon: <FiUsers />, color: "#8b5cf6" };
      case "office-types":
        return { data: officeTypes, title: "Office Types", icon: <FiMapPin />, color: "#06b6d4" };
      case "maintenance-types":
        return { data: maintenanceTypes, title: "Maintenance Types", icon: <FiTool />, color: "#ef4444" };
      case "asset-condition-purchases":
        return { data: assetConditionPurchases, title: "Purchase Conditions", icon: <FiTag />, color: "#dc2626" };
      default:
        return { data: [], title: "Unknown", icon: null, color: "#6b7280" };
    }
  };

  const handleTabChange = (tab) => {
    setActiveTab(tab);
  };

  if (isLoading) {
    return (
      <div className="master-data">
        <div className="page-header">
          <h1 className="page-title">Master Data</h1>
        </div>
        <div className="page-loading"><Spinner size="lg" /></div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="master-data">
        <div className="page-header">
          <h1 className="page-title">Master Data</h1>
        </div>
        <Card>
          <Box sx={{ textAlign: 'center', py: 6, color: 'var(--error)' }}>
            <FiDatabase size={48} style={{ marginBottom: 16 }} />
            <Typography variant="h6" sx={{ mb: 1 }}>Failed to load master data</Typography>
            <Typography variant="body2" color="text.secondary">
              Please refresh the page or contact support if the problem persists.
            </Typography>
          </Box>
        </Card>
      </div>
    );
  }

  const { data, title, icon, color } = getDataForTab();

  return (
    <div className="master-data fade-transition">
      <div className="page-header">
        <h1 className="page-title">Master Data</h1>
      </div>

      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />

      <Card>
        <div className="master-data__header">
          <div className="master-data__header-icon" style={{ backgroundColor: `${color}15`, color }}>
            {icon}
          </div>
          <h2 className="master-data__header-title">{title}</h2>
          <div className="master-data__header-count">
            <Chip 
              label={`${data.length} item${data.length !== 1 ? 's' : ''}`} 
              size="small" 
              sx={{ bgcolor: `${color}15`, color }} 
            />
          </div>
        </div>

        {data.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 6, color: 'var(--text-secondary)' }}>
            <FiDatabase size={48} style={{ marginBottom: 16, opacity: 0.5 }} />
            <Typography variant="h6" gutterBottom>No Data Available</Typography>
            <Typography variant="body2">
              No reference data found for {title.toLowerCase()}.
            </Typography>
          </Box>
        ) : (
          <div className="master-data__grid">
            {data.map((item) => (
              <div key={item.code} className="master-data__item">
                <div className="master-data__item-code" style={{ backgroundColor: `${color}15`, color }}>
                  {item.code}
                </div>
                <div className="master-data__item-name">{item.name}</div>
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
};

export default MasterDataMenu;