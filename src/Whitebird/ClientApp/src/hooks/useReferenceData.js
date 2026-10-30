// src/hooks/useReferenceData.js
import { useQueries, useQueryClient } from '@tanstack/react-query';
import {
  categoryApi,
  supplierApi,
  employeeApi,
  departmentApi,
  officeApi,
  assetApi,
  transactionApi,
  rolesApi,
} from '../core/api/api';
import { useIsAdmin } from './useIsAdmin';

const CACHE_KEYS = {
  CATEGORIES: ['reference', 'categories'],
  SUPPLIERS: ['reference', 'suppliers'],
  EMPLOYEES: ['reference', 'employees'],
  DEPARTMENTS: ['reference', 'departments'],
  OFFICES: ['reference', 'offices'],
  ASSETS: ['reference', 'assets'],
  TRANSACTION_TYPES: ['reference', 'transactionTypes'],
  CONDITIONS: ['reference', 'conditions'],
  EMPLOYEE_POSITIONS: ['reference', 'employeePositions'],
  EMPLOYEE_STATUSES: ['reference', 'employeeStatuses'],
  OFFICE_TYPES: ['reference', 'officeTypes'],
  MAINTENANCE_TYPES: ['reference', 'maintenanceTypes'],
  ASSET_CONDITION_PURCHASES: ['reference', 'assetConditionPurchases'],
  ROLES: ['reference', 'roles'],
};

const CACHE_STALE_TIME = 5 * 60 * 1000;
const MASTER_DATA_STALE_TIME = 24 * 60 * 60 * 1000;

// ============================================================
// FETCHERS
// ============================================================
const fetchCategories = async () => {
  const res = await categoryApi.dropdown();
  return (res.data?.data || []).map((c) => ({
    value: c.categoryId,
    label: c.categoryName,
  }));
};

const fetchSuppliers = async () => {
  const res = await supplierApi.dropdown();
  return (res.data?.data || []).map((s) => ({
    value: s.supplierId,
    label: s.supplierName,
  }));
};

const fetchEmployees = async () => {
  const res = await employeeApi.dropdown();
  return (res.data?.data || []).map((e) => ({
    value: e.employeeId,
    label: `${e.employeeCode} - ${e.fullName}`,
    fullName: e.fullName,
    employeeCode: e.employeeCode,
  }));
};

const fetchDepartments = async () => {
  const res = await departmentApi.dropdown();
  return (res.data?.data || []).map((d) => ({
    value: d.departmentId,
    label: d.departmentName,
  }));
};

const fetchOffices = async () => {
  const res = await officeApi.dropdown();
  return (res.data?.data || []).map((o) => ({
    value: o.officeId,
    label: o.officeName,
  }));
};

const fetchAssets = async () => {
  try {
    const res = await assetApi.getGrid({ page: 1, pageSize: 500 });
    const data = res.data?.data?.data || res.data?.data || res.data || [];
    return Array.isArray(data)
      ? data.map((a) => ({
          value: a.assetId,
          label: `${a.assetCode} - ${a.assetName}`,
          assetCode: a.assetCode,
          assetName: a.assetName,
        }))
      : [];
  } catch (error) {
    console.error('Failed to fetch assets:', error);
    return [];
  }
};

const fetchTransactionTypes = async () => {
  const res = await transactionApi.listTransactionTypeAdd();
  return (res.data?.data || []).map((m) => ({
    code: m.code,
    name: m.name,
    value: m.code,
    label: m.name,
  }));
};

const fetchConditions = async () => {
  const res = await transactionApi.listCondition();
  return (res.data?.data || []).map((m) => ({
    code: m.code,
    name: m.name,
    value: m.code,
    label: m.name,
  }));
};

const fetchEmployeePositions = async () => {
  const res = await employeeApi.masterDataList('Position');
  return (res.data?.data || []).map((m) => ({
    code: m.code,
    name: m.name,
    value: m.code,
    label: m.name,
  }));
};

const fetchEmployeeStatuses = async () => {
  const res = await employeeApi.masterDataList('EmployeeStatus');
  return (res.data?.data || []).map((m) => ({
    code: m.code,
    name: m.name,
    value: m.code,
    label: m.name,
  }));
};

const fetchOfficeTypes = async () => {
  return [
    { code: 1, name: 'Head Office', value: 1, label: 'Head Office' },
    { code: 2, name: 'Branch Office', value: 2, label: 'Branch Office' },
  ];
};

const fetchMaintenanceTypes = async () => {
  const res = await transactionApi.listMaintenanceType();
  return (res.data?.data || []).map((m) => ({
    code: m.code,
    name: m.name,
    value: m.code,
    label: m.name,
  }));
};

const fetchAssetConditionPurchases = async () => {
  return [
    { code: 1, name: 'New', value: 1, label: 'New' },
    { code: 2, name: 'Second Hand', value: 2, label: 'Second Hand' },
  ];
};

const fetchRoles = async () => {
  const res = await rolesApi.dropdown();
  return (res.data?.data || []).map((r) => ({
    value: r.roleId,
    label: r.roleName,
  }));
};

// ============================================================
// MAIN HOOK
// ============================================================
export const useReferenceData = () => {
  const queryClient = useQueryClient();
  const isAdmin = useIsAdmin();

  const results = useQueries({
    queries: [
      { queryKey: CACHE_KEYS.CATEGORIES, queryFn: fetchCategories, staleTime: CACHE_STALE_TIME },
      { queryKey: CACHE_KEYS.SUPPLIERS, queryFn: fetchSuppliers, staleTime: CACHE_STALE_TIME },
      { queryKey: CACHE_KEYS.EMPLOYEES, queryFn: fetchEmployees, staleTime: CACHE_STALE_TIME },
      { queryKey: CACHE_KEYS.DEPARTMENTS, queryFn: fetchDepartments, staleTime: CACHE_STALE_TIME },
      { queryKey: CACHE_KEYS.OFFICES, queryFn: fetchOffices, staleTime: CACHE_STALE_TIME },
      { queryKey: CACHE_KEYS.ASSETS, queryFn: fetchAssets, staleTime: CACHE_STALE_TIME },
      {
        queryKey: CACHE_KEYS.TRANSACTION_TYPES,
        queryFn: fetchTransactionTypes,
        staleTime: MASTER_DATA_STALE_TIME,
      },
      {
        queryKey: CACHE_KEYS.CONDITIONS,
        queryFn: fetchConditions,
        staleTime: MASTER_DATA_STALE_TIME,
      },
      {
        queryKey: CACHE_KEYS.EMPLOYEE_POSITIONS,
        queryFn: fetchEmployeePositions,
        staleTime: MASTER_DATA_STALE_TIME,
      },
      {
        queryKey: CACHE_KEYS.EMPLOYEE_STATUSES,
        queryFn: fetchEmployeeStatuses,
        staleTime: MASTER_DATA_STALE_TIME,
      },
      {
        queryKey: CACHE_KEYS.OFFICE_TYPES,
        queryFn: fetchOfficeTypes,
        staleTime: MASTER_DATA_STALE_TIME,
      },
      {
        queryKey: CACHE_KEYS.MAINTENANCE_TYPES,
        queryFn: fetchMaintenanceTypes,
        staleTime: MASTER_DATA_STALE_TIME,
      },
      {
        queryKey: CACHE_KEYS.ASSET_CONDITION_PURCHASES,
        queryFn: fetchAssetConditionPurchases,
        staleTime: MASTER_DATA_STALE_TIME,
      },
      {
        queryKey: CACHE_KEYS.ROLES,
        queryFn: fetchRoles,
        staleTime: CACHE_STALE_TIME,
        // /api/Roles/* requires ADMIN or Administrator (backend change log §4.10).
        // Non-admin sessions must not fire this request.
        enabled: isAdmin,
      },
    ],
  });

  const [
    categories,
    suppliers,
    employees,
    departments,
    offices,
    assets,
    transactionTypes,
    conditions,
    employeePositions,
    employeeStatuses,
    officeTypes,
    maintenanceTypes,
    assetConditionPurchases,
    roles,
  ] = results;

  const isLoading = results.some((r) => r.isLoading);
  const isError = results.some((r) => r.isError);

  const invalidateAll = () => queryClient.invalidateQueries({ queryKey: ['reference'] });
  const invalidateCategories = () =>
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.CATEGORIES });
  const invalidateSuppliers = () =>
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.SUPPLIERS });
  const invalidateEmployees = () =>
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.EMPLOYEES });
  const invalidateDepartments = () =>
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.DEPARTMENTS });
  const invalidateOffices = () =>
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.OFFICES });
  const invalidateAssets = () =>
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.ASSETS });
  const invalidateRoles = () =>
    queryClient.invalidateQueries({ queryKey: CACHE_KEYS.ROLES });
  const invalidateMasterData = () => {
    queryClient.invalidateQueries({ queryKey: ['reference', 'transactionTypes'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'conditions'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'employeePositions'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'employeeStatuses'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'officeTypes'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'maintenanceTypes'] });
    queryClient.invalidateQueries({ queryKey: ['reference', 'assetConditionPurchases'] });
  };

  return {
    categories: categories.data || [],
    suppliers: suppliers.data || [],
    employees: employees.data || [],
    departments: departments.data || [],
    offices: offices.data || [],
    assets: assets.data || [],
    transactionTypes: transactionTypes.data || [],
    conditions: conditions.data || [],
    employeePositions: employeePositions.data || [],
    employeeStatuses: employeeStatuses.data || [],
    officeTypes: officeTypes.data || [],
    maintenanceTypes: maintenanceTypes.data || [],
    assetConditionPurchases: assetConditionPurchases.data || [],
    roles: roles.data || [],
    isLoading,
    isError,
    invalidateAll,
    invalidateCategories,
    invalidateSuppliers,
    invalidateEmployees,
    invalidateDepartments,
    invalidateOffices,
    invalidateAssets,
    invalidateRoles,
    invalidateMasterData,
  };
};

export default useReferenceData;