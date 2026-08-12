// src/core/types/common.types.ts

/**
 * Base response from all API endpoints
 */
export interface ApiResponse<T = any> {
  isSuccess: boolean;
  message?: string;
  data?: T;
  errors?: string[];
  errorType?: 'NotFound' | 'Conflict' | 'BadRequest' | 'Error';
}

/**
 * Paginated result from grid endpoints
 */
export interface PaginatedResult<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  filters?: Record<string, any>;
  sortBy?: string;
  sortDescending?: boolean;
}

/**
 * Base grid request parameters
 */
export interface GridRequestBase {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
}

/**
 * Auditable entity fields
 */
export interface AuditableEntity {
  isActive: boolean;
  createdDate: string;
  createdBy: string;
  modifiedDate?: string | null;
  modifiedBy?: string | null;
}

/**
 * Select option type for dropdowns
 */
export interface SelectOption {
  value: string | number;
  label: string;
}

/**
 * Master data DTO from backend
 */
export interface MasterDataDto {
  code: number;
  name: string;
}

/**
 * Master data group DTO
 */
export interface MasterDataGroupDto {
  referenceName: string;
  values: MasterDataDto[];
}