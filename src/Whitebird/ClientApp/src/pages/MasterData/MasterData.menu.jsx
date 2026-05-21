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
        return { data: transactionTypes, title: "Transaction Types", icon: <FiRefreshCw /> };
      case "asset-conditions":
        return { data: assetConditions, title: "Asset Conditions", icon: <FiPackage /> };
      case "employee-positions":
        return { data: employeePositions, title: "Employee Positions", icon: <FiBriefcase /> };
      case "employee-statuses":
        return { data: employeeStatuses, title: "Employee Statuses", icon: <FiUsers /> };
      case "office-types":
        return { data: officeTypes, title: "Office Types", icon: <FiMapPin /> };
      case "maintenance-types":
        return { data: maintenanceTypes, title: "Maintenance Types", icon: <FiTool /> };
      case "asset-condition-purchases":
        return { data: assetConditionPurchases, title: "Purchase Conditions", icon: <FiTag /> };
      default:
        return { data: [], title: "Unknown", icon: null };
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
          <Box sx={{ textAlign: 'center', py: 4, color: 'var(--error)' }}>
            <FiDatabase size={48} />
            <Typography variant="h6" sx={{ mt: 2 }}>Failed to load master data</Typography>
            <Typography variant="body2" color="text.secondary">Please try again later.</Typography>
          </Box>
        </Card>
      </div>
    );
  }

  const { data, title, icon } = getDataForTab();

  return (
    <div className="master-data">
      <div className="page-header">
        <h1 className="page-title">Master Data</h1>
      </div>

      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />

      <Card>
        <div className="master-data__header">
          <div className="master-data__header-icon">
            {icon}
          </div>
          <h2 className="master-data__header-title">{title}</h2>
          <div className="master-data__header-count">
            <Chip 
              label={`${data.length} items`} 
              size="small" 
              sx={{ bgcolor: 'rgba(220, 38, 38, 0.1)', color: '#dc2626' }} 
            />
          </div>
        </div>

        {data.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 4, color: 'var(--text-secondary)' }}>
            <FiDatabase size={40} />
            <Typography sx={{ mt: 2 }}>No data available</Typography>
          </Box>
        ) : (
          <div className="master-data__grid">
            {data.map((item) => (
              <div key={item.code} className="master-data__item">
                <div className="master-data__item-code">{item.code}</div>
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