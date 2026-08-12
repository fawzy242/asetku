// src/core/types/department.types.ts
import { AuditableEntity, GridRequestBase } from './common.types';

/**
 * Department entity
 */
export interface DepartmentEntity extends AuditableEntity {
  departmentId: number;
  departmentCode?: string | null;
  departmentName: string;
  description?: string | null;
}

/**
 * Department list view - for grid display
 */
export interface DepartmentListView extends Partial<DepartmentEntity> {
  departmentId: number;
  departmentCode?: string | null;
  departmentName: string;
  description?: string | null;
  employeeCount: number;
  isActive: boolean;
  createdDate: string;
  createdBy: string;
  modifiedDate?: string | null;
  modifiedBy?: string | null;
  // Grid action flags
  canEdit?: boolean;
  canDelete?: boolean;
}

/**
 * Department detail view
 */
export interface DepartmentDetailView extends Partial<DepartmentListView> {}

/**
 * Department create view model
 */
export interface DepartmentCreateViewModel {
  departmentName: string;
  departmentCode?: string | null;
  description?: string | null;
}

/**
 * Department update view model
 */
export interface DepartmentUpdateViewModel extends DepartmentCreateViewModel {
  isActive: boolean;
}

/**
 * Department dropdown view
 */
export interface DepartmentDropdownView {
  departmentId: number;
  departmentName: string;
  departmentCode?: string | null;
}

/**
 * Department grid request
 */
export interface DepartmentGridRequest extends GridRequestBase {}