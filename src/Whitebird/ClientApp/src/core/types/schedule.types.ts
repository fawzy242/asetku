// src/core/types/schedule.types.ts
import { AuditableEntity, GridRequestBase } from './common.types';

// ============================================================
// SCHEDULE HANDOVER
// ============================================================

/**
 * Schedule handover entity
 */
export interface ScheduleHandoverEntity extends AuditableEntity {
  scheduleHandoverId: number;
  scheduleDate: string;
  assetId: number;
  employeeId: number;
  handoverAssetTransactionId?: number | null;
}

/**
 * Schedule handover list view - for grid display
 */
export interface ScheduleHandoverListView extends Partial<ScheduleHandoverEntity> {
  scheduleHandoverId: number;
  scheduleDate: string;
  assetId: number;
  assetCode?: string | null;
  assetName?: string | null;
  employeeId: number;
  employeeName?: string | null;
  handoverAssetTransactionId?: number | null;
  isActive: boolean;
  createdDate: string;
  createdBy: string;
  // Grid action flags
  canEdit?: boolean;
  canDelete?: boolean;
}

/**
 * Schedule handover view
 */
export interface ScheduleHandoverView extends Partial<ScheduleHandoverListView> {
  employeeCode?: string | null;
  transactionCode?: string | null;
}

/**
 * Schedule handover create DTO
 */
export interface ScheduleHandoverCreateDto {
  scheduleDate: string;
  assetId: number;
  employeeId: number;
}

/**
 * Schedule handover update DTO
 */
export interface ScheduleHandoverUpdateDto {
  scheduleDate: string;
  assetId: number;
  employeeId: number;
  isActive: boolean;
}

/**
 * Schedule handover generate DTO
 */
export interface ScheduleHandoverGenerateDto {
  dateFrom: string;
  dateTo: string;
  assetId: number;
  employeeId: number;
}

// ============================================================
// SCHEDULE RETURN
// ============================================================

/**
 * Schedule return entity
 */
export interface ScheduleReturnEntity extends AuditableEntity {
  scheduleReturnId: number;
  scheduleDate: string;
  assetId: number;
  employeeId: number;
  returnAssetTransactionId?: number | null;
}

/**
 * Schedule return list view - for grid display
 */
export interface ScheduleReturnListView extends Partial<ScheduleReturnEntity> {
  scheduleReturnId: number;
  scheduleDate: string;
  assetId: number;
  assetCode?: string | null;
  assetName?: string | null;
  employeeId: number;
  employeeName?: string | null;
  returnAssetTransactionId?: number | null;
  isActive: boolean;
  createdDate: string;
  createdBy: string;
  // Grid action flags
  canEdit?: boolean;
  canDelete?: boolean;
}

/**
 * Schedule return view
 */
export interface ScheduleReturnView extends Partial<ScheduleReturnListView> {
  employeeCode?: string | null;
  transactionCode?: string | null;
}

/**
 * Schedule return create DTO
 */
export interface ScheduleReturnCreateDto {
  scheduleDate: string;
  assetId: number;
  employeeId: number;
}

/**
 * Schedule return update DTO
 */
export interface ScheduleReturnUpdateDto {
  scheduleDate: string;
  assetId: number;
  employeeId: number;
  isActive: boolean;
}

/**
 * Schedule return generate DTO
 */
export interface ScheduleReturnGenerateDto {
  dateFrom: string;
  dateTo: string;
  assetId: number;
  employeeId: number;
}

// ============================================================
// SCHEDULE MAINTENANCE
// ============================================================

/**
 * Schedule maintenance entity
 */
export interface ScheduleMaintenanceEntity extends AuditableEntity {
  scheduleMaintenanceId: number;
  scheduleDate: string;
  assetId: number;
  maintenanceType: number;
  maintenanceAssetTransactionId?: number | null;
  postAssetTransactionId?: number | null;
}

/**
 * Schedule maintenance list view - for grid display
 */
export interface ScheduleMaintenanceListView extends Partial<ScheduleMaintenanceEntity> {
  scheduleMaintenanceId: number;
  scheduleDate: string;
  assetId: number;
  assetCode?: string | null;
  assetName?: string | null;
  maintenanceType: number;
  maintenanceTypeName?: string | null;
  maintenanceAssetTransactionId?: number | null;
  postAssetTransactionId?: number | null;
  isActive: boolean;
  createdDate: string;
  createdBy: string;
  // Grid action flags
  canEdit?: boolean;
  canDelete?: boolean;
}

/**
 * Schedule maintenance view
 */
export interface ScheduleMaintenanceView extends Partial<ScheduleMaintenanceListView> {}

/**
 * Schedule maintenance create DTO
 */
export interface ScheduleMaintenanceCreateDto {
  scheduleDate: string;
  assetId: number;
  maintenanceType: number;
}

/**
 * Schedule maintenance update DTO
 */
export interface ScheduleMaintenanceUpdateDto {
  scheduleDate: string;
  assetId: number;
  maintenanceType: number;
  isActive: boolean;
}

/**
 * Schedule maintenance generate DTO
 */
export interface ScheduleMaintenanceGenerateDto {
  dateFrom: string;
  dateTo: string;
  assetId: number;
  maintenanceType: number;
}

/**
 * Schedule generate result DTO
 */
export interface ScheduleGenerateResultDto {
  totalGenerated: number;
  totalSkipped: number;
  generatedDates: string[];
  skippedDates: string[];
  message?: string | null;
}

/**
 * Schedule grid request
 */
export interface ScheduleGridRequest extends GridRequestBase {
  assetId?: number | null;
  employeeId?: number | null;
  maintenanceType?: number | null;
  dateFrom?: string | null;
  dateTo?: string | null;
}