import { useQuery, useQueryClient } from '@tanstack/react-query';
import apiService from '../core/services/api.service';

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
  return (r.data?.data || []).map(e => ({ value: e.employeeId, label: e.fullName }));
};

const fetchLocations = async () => {
  const r = await apiService.get('/Location/active');
  return (r.data?.data || []).map(l => ({ value: l.locationId, label: l.locationName }));
};

const fetchAllAssets = async () => {
  const r = await apiService.get('/Asset');
  return (r.data?.data || []).map(a => ({ value: a.assetId, label: `${a.assetCode} - ${a.assetName}` }));
};

/**
 * Custom hook untuk mengambil reference data dengan caching React Query
 * Data di-cache selama 30 menit (staleTime)
 * 
 * @returns {{ categories, suppliers, employees, locations, assets, isLoading, isError, invalidateAll }}
 */
export const useReferenceData = () => {
  const queryClient = useQueryClient();

  const categories = useQuery({
    queryKey: ['reference', 'categories'],
    queryFn: fetchCategories,
    staleTime: 30 * 60 * 1000,
  });

  const suppliers = useQuery({
    queryKey: ['reference', 'suppliers'],
    queryFn: fetchSuppliers,
    staleTime: 30 * 60 * 1000,
  });

  const employees = useQuery({
    queryKey: ['reference', 'employees'],
    queryFn: fetchEmployees,
    staleTime: 30 * 60 * 1000,
  });

  const locations = useQuery({
    queryKey: ['reference', 'locations'],
    queryFn: fetchLocations,
    staleTime: 30 * 60 * 1000,
  });

  const assets = useQuery({
    queryKey: ['reference', 'assets'],
    queryFn: fetchAllAssets,
    staleTime: 10 * 60 * 1000,
  });

  const invalidateAll = () => {
    queryClient.invalidateQueries({ queryKey: ['reference'] });
  };

  return {
    categories: categories.data || [],
    suppliers: suppliers.data || [],
    employees: employees.data || [],
    locations: locations.data || [],
    assets: assets.data || [],
    isLoading: categories.isLoading || suppliers.isLoading || employees.isLoading || locations.isLoading || assets.isLoading,
    isError: categories.isError || suppliers.isError || employees.isError || locations.isError || assets.isError,
    invalidateAll,
  };
};