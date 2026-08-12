// src/core/types/supplier.types.ts
import { AuditableEntity, GridRequestBase } from './common.types';

/**
 * Supplier entity
 */
export interface SupplierEntity extends AuditableEntity {
  supplierId: number;
  supplierName: string;
  contactPerson?: string | null;
  phoneNumber?: string | null;
  email?: string | null;
  address?: string | null;
}

/**
 * Supplier list view - for grid display
 */
export interface SupplierListView extends Partial<SupplierEntity> {
  supplierId: number;
  supplierName: string;
  contactPerson?: string | null;
  phoneNumber?: string | null;
  email?: string | null;
  address?: string | null;
  assetCount: number;
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
 * Supplier detail view
 */
export interface SupplierDetailView extends Partial<SupplierListView> {}

/**
 * Supplier create view model
 */
export interface SupplierCreateViewModel {
  supplierName: string;
  contactPerson?: string | null;
  phoneNumber?: string | null;
  email?: string | null;
  address?: string | null;
}

/**
 * Supplier update view model
 */
export interface SupplierUpdateViewModel extends SupplierCreateViewModel {
  isActive: boolean;
}

/**
 * Supplier dropdown view
 */
export interface SupplierDropdownView {
  supplierId: number;
  supplierName: string;
  contactPerson?: string | null;
}

/**
 * Supplier grid request
 */
export interface SupplierGridRequest extends GridRequestBase {}