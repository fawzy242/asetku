// src/core/types/employee.types.ts
import { AuditableEntity, GridRequestBase } from './common.types';

/**
 * Employee entity
 */
export interface EmployeeEntity extends AuditableEntity {
  employeeId: number;
  employeeCode: string;
  fullName: string;
  address?: string | null;
  departmentId?: number | null;
  position?: number | null;
  employmentStatus?: number | null;
  phoneNumber?: string | null;
  email?: string | null;
  officeId?: number | null;
  joinDate?: string | null;
  resignDate?: string | null;
}

/**
 * Employee list view - for grid display
 */
export interface EmployeeListView extends Partial<EmployeeEntity> {
  employeeId: number;
  employeeCode: string;
  fullName: string;
  address?: string | null;
  departmentId?: number | null;
  departmentName?: string | null;
  position?: number | null;
  positionName?: string | null;
  employmentStatus?: number | null;
  employmentStatusName?: string | null;
  phoneNumber?: string | null;
  email?: string | null;
  officeId?: number | null;
  officeName?: string | null;
  joinDate?: string | null;
  resignDate?: string | null;
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
 * Employee detail view
 */
export interface EmployeeDetailView extends Partial<EmployeeListView> {}

/**
 * Employee create view model
 */
export interface EmployeeCreateViewModel {
  employeeCode: string;
  fullName: string;
  address?: string | null;
  departmentId?: number | null;
  position?: number | null;
  employmentStatus?: number | null;
  phoneNumber?: string | null;
  email?: string | null;
  officeId?: number | null;
  joinDate?: string | null;
}

/**
 * Employee update view model
 */
export interface EmployeeUpdateViewModel extends EmployeeCreateViewModel {
  resignDate?: string | null;
  isActive: boolean;
}

/**
 * Employee dropdown view
 */
export interface EmployeeDropdownView {
  employeeId: number;
  employeeCode: string;
  fullName: string;
}

/**
 * Employee grid request
 */
export interface EmployeeGridRequest extends GridRequestBase {
  departmentId?: number | null;
  officeId?: number | null;
  employmentStatus?: number | null;
}