// src/core/types/api.types.ts
import { ApiResponse, PaginatedResult } from './common.types';
import { AssetListView } from './asset.types';
import { EmployeeListView } from './employee.types';
import { AssetTransactionListView } from './transaction.types';
import { OfficeListView } from './office.types';
import { CategoryListView } from './category.types';
import { SupplierListView } from './supplier.types';
import { DepartmentListView } from './department.types';

// ============================================================
// ASSET API TYPES
// ============================================================

export type AssetGridResponse = ApiResponse<PaginatedResult<AssetListView>>;
export type AssetDetailResponse = ApiResponse<AssetListView>;
export type AssetCreateResponse = ApiResponse<AssetListView>;
export type AssetUpdateResponse = ApiResponse<AssetListView>;
export type AssetDeleteResponse = ApiResponse<null>;
export type AssetDropdownResponse = ApiResponse<AssetDropdownView[]>;
export type AssetActivateResponse = ApiResponse<number>;

// ============================================================
// EMPLOYEE API TYPES
// ============================================================

export type EmployeeGridResponse = ApiResponse<PaginatedResult<EmployeeListView>>;
export type EmployeeDetailResponse = ApiResponse<EmployeeDetailView>;
export type EmployeeCreateResponse = ApiResponse<EmployeeDetailView>;
export type EmployeeUpdateResponse = ApiResponse<EmployeeDetailView>;
export type EmployeeDeleteResponse = ApiResponse<null>;
export type EmployeeDropdownResponse = ApiResponse<EmployeeDropdownView[]>;
export type EmployeeActivateResponse = ApiResponse<number>;

// ============================================================
// TRANSACTION API TYPES
// ============================================================

export type TransactionGridResponse = ApiResponse<PaginatedResult<AssetTransactionListView>>;
export type TransactionDetailResponse = ApiResponse<AssetTransactionDetailView>;
export type TransactionCreateResponse = ApiResponse<AssetTransactionDetailView>;
export type TransactionUpdateResponse = ApiResponse<AssetTransactionDetailView>;
export type TransactionDeleteResponse = ApiResponse<null>;
export type TransactionBulkResponse = ApiResponse<number>;
export type TransactionCompleteResponse = ApiResponse<null>;

// ============================================================
// OFFICE API TYPES
// ============================================================

export type OfficeGridResponse = ApiResponse<PaginatedResult<OfficeListView>>;
export type OfficeDetailResponse = ApiResponse<OfficeDetailView>;
export type OfficeCreateResponse = ApiResponse<OfficeDetailView>;
export type OfficeUpdateResponse = ApiResponse<OfficeDetailView>;
export type OfficeDeleteResponse = ApiResponse<null>;
export type OfficeDropdownResponse = ApiResponse<OfficeDropdownView[]>;
export type OfficeActivateResponse = ApiResponse<number>;

// ============================================================
// CATEGORY API TYPES
// ============================================================

export type CategoryGridResponse = ApiResponse<PaginatedResult<CategoryListView>>;
export type CategoryDetailResponse = ApiResponse<CategoryDetailView>;
export type CategoryCreateResponse = ApiResponse<CategoryDetailView>;
export type CategoryUpdateResponse = ApiResponse<CategoryDetailView>;
export type CategoryDeleteResponse = ApiResponse<null>;
export type CategoryDropdownResponse = ApiResponse<CategoryDropdownView[]>;
export type CategoryActivateResponse = ApiResponse<number>;

// ============================================================
// SUPPLIER API TYPES
// ============================================================

export type SupplierGridResponse = ApiResponse<PaginatedResult<SupplierListView>>;
export type SupplierDetailResponse = ApiResponse<SupplierDetailView>;
export type SupplierCreateResponse = ApiResponse<SupplierDetailView>;
export type SupplierUpdateResponse = ApiResponse<SupplierDetailView>;
export type SupplierDeleteResponse = ApiResponse<null>;
export type SupplierDropdownResponse = ApiResponse<SupplierDropdownView[]>;
export type SupplierActivateResponse = ApiResponse<number>;

// ============================================================
// DEPARTMENT API TYPES
// ============================================================

export type DepartmentGridResponse = ApiResponse<PaginatedResult<DepartmentListView>>;
export type DepartmentDetailResponse = ApiResponse<DepartmentDetailView>;
export type DepartmentCreateResponse = ApiResponse<DepartmentDetailView>;
export type DepartmentUpdateResponse = ApiResponse<DepartmentDetailView>;
export type DepartmentDeleteResponse = ApiResponse<null>;
export type DepartmentDropdownResponse = ApiResponse<DepartmentDropdownView[]>;
export type DepartmentActivateResponse = ApiResponse<number>;