// src/core/types/office.types.ts
import { AuditableEntity, GridRequestBase } from './common.types';

/**
 * Office entity
 */
export interface OfficeEntity extends AuditableEntity {
  officeId: number;
  officeCode?: string | null;
  officeName: string;
  officeType?: number | null;
  city?: string | null;
  address?: string | null;
  phone?: string | null;
  parentOfficeId?: number | null;
}

/**
 * Office list view - for grid display
 */
export interface OfficeListView extends Partial<OfficeEntity> {
  officeId: number;
  officeCode?: string | null;
  officeName: string;
  officeType?: number | null;
  officeTypeName?: string | null;
  city?: string | null;
  address?: string | null;
  phone?: string | null;
  parentOfficeId?: number | null;
  parentOfficeName?: string | null;
  childCount: number;
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
 * Office detail view
 */
export interface OfficeDetailView extends Partial<OfficeListView> {}

/**
 * Office create view model
 */
export interface OfficeCreateViewModel {
  officeName: string;
  officeCode?: string | null;
  officeType?: number | null;
  city?: string | null;
  address?: string | null;
  phone?: string | null;
  parentOfficeId?: number | null;
}

/**
 * Office update view model
 */
export interface OfficeUpdateViewModel extends OfficeCreateViewModel {
  isActive: boolean;
}

/**
 * Office dropdown view
 */
export interface OfficeDropdownView {
  officeId: number;
  officeName: string;
  officeCode?: string | null;
  parentOfficeId?: number | null;
}

/**
 * Office grid request
 */
export interface OfficeGridRequest extends GridRequestBase {
  officeType?: number | null;
  parentOfficeId?: number | null;
  city?: string | null;
}