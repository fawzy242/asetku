import { useQuery, useQueryClient } from '@tanstack/react-query';
import apiService from '../core/services/api.service';
import masterDataService from '../core/services/masterData.service';

// ============================================================
// INDIVIDUAL FETCHERS (Reusable)
// ============================================================

const fetchCategories = async () => {
  const r = await apiService.get('/Category/active');
  return (r.data?.data || []).map(c => ({ value: c.categoryId, label: c.categoryName }));
};

const fetchSuppliers = async () => {
  const r = await apiService.get('/Supplier/active');
  return (r.data?.data || []).map(s => ({ value: s.supplierId, label: s.supplierName }));
};

const fetchEmployees = async () => {
  const r = await apiService.get('/Employee');
  return (r.data?.data || []).map(e => ({ 
    value: e.employeeId, 
    label: `${e.employeeCode} - ${e.fullName}`,
    fullName: e.fullName,
    employeeCode: e.employeeCode
  }));
};

const fetchDepartments = async () => {
  const r = await apiService.get('/Department/active');
  return (r.data?.data || []).map(d => ({ value: d.departmentId, label: d.departmentName }));
};

const fetchOffices = async () => {
  const r = await apiService.get('/Office/active');
  return (r.data?.data || []).map(o => ({ value: o.officeId, label: o.officeName }));
};

const fetchAssets = async () => {
  const r = await apiService.get('/Asset');
  return (r.data?.data || []).map(a => ({ 
    value: a.assetId, 
    label: `${a.assetCode} - ${a.assetName}`,
    assetCode: a.assetCode,
    assetName: a.assetName
  }));
};

// ============================================================
// MASTER DATA FETCHERS (from MasterData API)
// ============================================================

const fetchMasterDataTypes = async (referenceName) => {
  const r = await masterDataService.getByReferenceName(referenceName);
  return (r.data || []).map(m => ({ code: m.code, name: m.name }));
};

// ============================================================
// CACHE KEYS
// ============================================================

const CACHE_KEYS = {
  CATEGORIES: ['reference', 'categories'],
  SUPPLIERS: ['reference', 'suppliers'],
  EMPLOYEES: ['reference', 'employees'],
  DEPARTMENTS: ['reference', 'departments'],
  OFFICES: ['reference', 'offices'],
  ASSETS: ['reference', 'assets'],
  TRANSACTION_TYPES: ['reference', 'transactionTypes'],
  ASSET_CONDITIONS: ['reference', 'assetConditions'],
  EMPLOYEE_POSITIONS: ['reference', 'employeePositions'],
  EMPLOYEE_STATUSES: ['reference', 'employeeStatuses'],
  OFFICE_TYPES: ['reference', 'officeTypes'],
  MAINTENANCE_TYPES: ['reference', 'maintenanceTypes'],
  ASSET_CONDITION_PURCHASES: ['reference', 'assetConditionPurchases'],
};

const CACHE_STALE_TIME = 30 * 60 * 1000; // 30 minutes
const MASTER_DATA_STALE_TIME = 24 * 60 * 60 * 1000; // 24 hours

// ============================================================
// MAIN HOOK
// ============================================================

export const useReferenceData = () => {
  const queryClient = useQueryClient();

  // Basic Reference Data
  const categories = useQuery({
    queryKey: CACHE_KEYS.CATEGORIES,
    queryFn: fetchCategories,
    staleTime: CACHE_STALE_TIME,
  });

  const suppliers = useQuery({
    queryKey: CACHE_KEYS.SUPPLIERS,
    queryFn: fetchSuppliers,
    staleTime: CACHE_STALE_TIME,
  });

  const employees = useQuery({
    queryKey: CACHE_KEYS.EMPLOYEES,
    queryFn: fetchEmployees,
    staleTime: CACHE_STALE_TIME,
  });

  const departments = useQuery({
    queryKey: CACHE_KEYS.DEPARTMENTS,
    queryFn: fetchDepartments,
    staleTime: CACHE_STALE_TIME,
  });

  const offices = useQuery({
    queryKey: CACHE_KEYS.OFFICES,
    queryFn: fetchOffices,
    staleTime: CACHE_STALE_TIME,
  });

  const assets = useQuery({
    queryKey: CACHE_KEYS.ASSETS,
    queryFn: fetchAssets,
    staleTime: CACHE_STALE_TIME,
  });

  // Master Data (from MasterData API)
  const transactionTypes = useQuery({
    queryKey: CACHE_KEYS.TRANSACTION_TYPES,
    queryFn: () => fetchMasterDataTypes('TransactionType'),
    staleTime: MASTER_DATA_STALE_TIME,
  });

  const assetConditions = useQuery({
    queryKey: CACHE_KEYS.ASSET_CONDITIONS,
    queryFn: () => fetchMasterDataTypes('AssetCondition'),
    staleTime: MASTER_DATA_STALE_TIME,
  });

  const employeePositions = useQuery({
    queryKey: CACHE_KEYS.EMPLOYEE_POSITIONS,
    queryFn: () => fetchMasterDataTypes('Position'),
    staleTime: MASTER_DATA_STALE_TIME,
  });

  const employeeStatuses = useQuery({
    queryKey: CACHE_KEYS.EMPLOYEE_STATUSES,
    queryFn: () => fetchMasterDataTypes('EmployeeStatus'),
    staleTime: MASTER_DATA_STALE_TIME,
  });

  const officeTypes = useQuery({
    queryKey: CACHE_KEYS.OFFICE_TYPES,
    queryFn: () => fetchMasterDataTypes('OfficeType'),
    staleTime: MASTER_DATA_STALE_TIME,
  });

  const maintenanceTypes = useQuery({
    queryKey: CACHE_KEYS.MAINTENANCE_TYPES,
    queryFn: () => fetchMasterDataTypes('MaintenanceType'),
    staleTime: MASTER_DATA_STALE_TIME,
  });

  const assetConditionPurchases = useQuery({
    queryKey: CACHE_KEYS.ASSET_CONDITION_PURCHASES,
    queryFn: () => fetchMasterDataTypes('AssetConditionPurchase'),
    staleTime: MASTER_DATA_STALE_TIME,
  });

  // Loading & Error States
  const isLoading = 
    categories.isLoading || suppliers.isLoading || employees.isLoading ||
    departments.isLoading || offices.isLoading || assets.isLoading ||
    transactionTypes.isLoading || assetConditions.isLoading ||
    employeePositions.isLoading || employeeStatuses.isLoading ||
    officeTypes.isLoading || maintenanceTypes.isLoading ||
    assetConditionPurchases.isLoading;

  const isError = 
    categories.isError || suppliers.isError || employees.isError ||
    departments.isError || offices.isError || assets.isError ||
    transactionTypes.isError || assetConditions.isError ||
    employeePositions.isError || employeeStatuses.isError ||
    officeTypes.isError || maintenanceTypes.isError ||
    assetConditionPurchases.isError;

  // Invalidation Helpers
  const invalidateAll = () => {
    queryClient.invalidateQueries({ queryKey: ['reference'] });
  };

  const invalidateCategories = () => {
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.CATEGORIES });
  };

  const invalidateSuppliers = () => {
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.SUPPLIERS });
  };

  const invalidateEmployees = () => {
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.EMPLOYEES });
  };

  const invalidateDepartments = () => {
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.DEPARTMENTS });
  };

  const invalidateOffices = () => {
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.OFFICES });
  };

  const invalidateAssets = () => {
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.ASSETS });
  };

  const invalidateMasterData = () => {
    queryClient.invalidateQueries({ queryKey: ['reference', 'transactionTypes'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'assetConditions'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'employeePositions'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'employeeStatuses'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'officeTypes'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'maintenanceTypes'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'assetConditionPurchases'] });
  };

  // Selector Helpers
  const getCategoryLabel = (value) => {
    const category = categories.data?.find(c => c.value === value);
    return category?.label || 'Unknown';
  };

  const getSupplierLabel = (value) => {
    const supplier = suppliers.data?.find(s => s.value === value);
    return supplier?.label || 'Unknown';
  };

  const getEmployeeLabel = (value) => {
    const employee = employees.data?.find(e => e.value === value);
    return employee?.label || 'Unknown';
  };

  const getDepartmentLabel = (value) => {
    const department = departments.data?.find(d => d.value === value);
    return department?.label || 'Unknown';
  };

  const getOfficeLabel = (value) => {
    const office = offices.data?.find(o => o.value === value);
    return office?.label || 'Unknown';
  };

  const getTransactionTypeLabel = (value) => {
    const type = transactionTypes.data?.find(t => t.code === value);
    return type?.name || 'Unknown';
  };

  const getAssetConditionLabel = (value) => {
    const condition = assetConditions.data?.find(c => c.code === value);
    return condition?.name || 'Unknown';
  };

  return {
    // Data
    categories: categories.data || [],
    suppliers: suppliers.data || [],
    employees: employees.data || [],
    departments: departments.data || [],
    offices: offices.data || [],
    assets: assets.data || [],
    transactionTypes: transactionTypes.data || [],
    assetConditions: assetConditions.data || [],
    employeePositions: employeePositions.data || [],
    employeeStatuses: employeeStatuses.data || [],
    officeTypes: officeTypes.data || [],
    maintenanceTypes: maintenanceTypes.data || [],
    assetConditionPurchases: assetConditionPurchases.data || [],
    
    // Status
    isLoading,
    isError,
    
    // Invalidation
    invalidateAll,
    invalidateCategories,
    invalidateSuppliers,
    invalidateEmployees,
    invalidateDepartments,
    invalidateOffices,
    invalidateAssets,
    invalidateMasterData,
    
    // Selector Helpers
    getCategoryLabel,
    getSupplierLabel,
    getEmployeeLabel,
    getDepartmentLabel,
    getOfficeLabel,
    getTransactionTypeLabel,
    getAssetConditionLabel,
  };
};

export default useReferenceData;