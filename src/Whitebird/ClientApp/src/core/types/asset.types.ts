// src/core/types/asset.types.ts
import { AuditableEntity } from './common.types';

/**
 * Asset entity - matching AssetEntity from backend
 */
export interface AssetEntity extends AuditableEntity {
  assetId: number;
  notes?: string | null;
  assetCode: string;
  assetName: string;
  categoryId: number;
  supplierId?: number | null;
  brand?: string | null;
  model?: string | null;
  serialNumber?: string | null;
  imei?: string | null;
  hostname?: string | null;
  ipAddress?: string | null;
  macAddress?: string | null;
  purchaseDate?: string | null;
  purchasePrice?: number | null;
  invoiceNumber?: string | null;
  warrantyPeriod?: number | null;
  warrantyExpiryDate?: string | null;
  assetConditionPurchase?: number | null;
}

/**
 * Asset list view - for grid display
 */
export interface AssetListView extends Partial<AssetEntity> {
  assetId: number;
  assetCode: string;
  assetName: string;
  categoryId: number;
  categoryName?: string | null;
  brand?: string | null;
  model?: string | null;
  serialNumber?: string | null;
  imei?: string | null;
  macAddress?: string | null;
  hostname?: string | null;
  ipAddress?: string | null;
  purchaseDate?: string | null;
  purchasePrice?: number | null;
  invoiceNumber?: string | null;
  supplierId?: number | null;
  supplierName?: string | null;
  warrantyPeriod?: number | null;
  warrantyExpiryDate?: string | null;
  assetConditionPurchase?: number | null;
  assetConditionPurchaseName?: string | null;
  notes?: string | null;
  isActive: boolean;
  currentStatus?: string | null;
  createdDate: string;
  createdBy: string;
  modifiedDate?: string | null;
  modifiedBy?: string | null;
  // Grid action flags
  canEdit?: boolean;
  canDelete?: boolean;
}

/**
 * Asset detail view
 */
export interface AssetDetailView extends Partial<AssetListView> {
  softwareInstallation?: SoftwareInstallationView | null;
}

/**
 * Asset create view model
 */
export interface AssetCreateViewModel {
  assetCode: string;
  assetName: string;
  categoryId: number;
  brand?: string | null;
  model?: string | null;
  serialNumber?: string | null;
  imei?: string | null;
  macAddress?: string | null;
  purchaseDate?: string | null;
  purchasePrice?: number | null;
  invoiceNumber?: string | null;
  supplierId?: number | null;
  warrantyPeriod?: number | null;
  assetConditionPurchase?: number | null;
  hostname?: string | null;
  ipAddress?: string | null;
  notes?: string | null;
  softwareInstallation?: SoftwareInstallationDto | null;
}

/**
 * Asset update view model
 */
export interface AssetUpdateViewModel extends AssetCreateViewModel {
  isActive: boolean;
}

/**
 * Asset dropdown view
 */
export interface AssetDropdownView {
  assetId: number;
  assetCode: string;
  assetName: string;
}

/**
 * Software installation entity
 */
export interface SoftwareInstallationEntity extends AuditableEntity {
  softwareInstallationId: number;
  assetId: number;
  userPassword?: string | null;
  adminOperationalPassword?: string | null;
  operatingSystem?: number | null;
  microsoftOffice?: number | null;
  visualStudio?: number | null;
  dbms?: number | null;
  connectToNAS: boolean;
}

/**
 * Software installation view
 */
export interface SoftwareInstallationView {
  softwareInstallationId: number;
  assetId: number;
  userPassword?: string | null;
  adminOperationalPassword?: string | null;
  operatingSystem?: number | null;
  operatingSystemName?: string | null;
  microsoftOffice?: number | null;
  microsoftOfficeName?: string | null;
  visualStudio?: number | null;
  visualStudioName?: string | null;
  dbms?: number | null;
  dbmsName?: string | null;
  connectToNAS: boolean;
  isActive: boolean;
  createdDate: string;
  createdBy: string;
  modifiedDate?: string | null;
  modifiedBy?: string | null;
}

/**
 * Software installation DTO
 */
export interface SoftwareInstallationDto {
  userPassword?: string | null;
  adminOperationalPassword?: string | null;
  operatingSystem?: number | null;
  microsoftOffice?: number | null;
  visualStudio?: number | null;
  dbms?: number | null;
  connectToNAS: boolean;
}

/**
 * Asset grid request
 */
export interface AssetGridRequest extends GridRequestBase {
  status?: string | null;
  categoryId?: number | null;
  officeId?: number | null;
  supplierId?: number | null;
}