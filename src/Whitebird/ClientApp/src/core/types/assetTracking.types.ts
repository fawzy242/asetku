// src/core/types/assetTracking.types.ts
import { PaginatedResult, GridRequestBase } from './common.types';

/**
 * Asset tracking get data DTO - for dropdown
 */
export interface AssetTrackingGetDataDto {
  assetId: number;
  assetCode: string;
  assetName: string;
  categoryName: string;
  isActive: boolean;
}

/**
 * Asset stats view
 */
export interface AssetStatsView {
  // Asset Info
  assetId: number;
  assetCode: string;
  assetName: string;
  categoryName: string;
  brand?: string | null;
  model?: string | null;
  serialNumber?: string | null;
  // Current Status
  currentStatus: string;
  currentHolderName?: string | null;
  currentLocation?: string | null;
  conditionName?: string | null;
  // Flags
  isOnLoan: boolean;
  isInMaintenance: boolean;
  isOverdue: boolean;
  loanDueDate?: string | null;
  // Purchase Info
  purchaseDate?: string | null;
  purchasePrice?: number | null;
  warrantyExpiryDate?: string | null;
  // Photo Profile
  photoProfileUrl?: string | null;
  // Attachments
  attachments: AssetTrackingAttachmentDto[];
  // Software Installation
  softwareInstallation?: AssetTrackingSoftwareInstallationDto | null;
}

/**
 * Asset tracking view - complete detail
 */
export interface AssetTrackingView extends AssetStatsView {
  totalTransactions: number;
  timeline: AssetTimelineEntry[];
}

/**
 * Asset timeline entry
 */
export interface AssetTimelineEntry {
  id: number;
  date: string;
  activityType: string;
  description: string;
  previousHolder?: string | null;
  newHolder?: string | null;
  previousStatus?: string | null;
  newStatus?: string | null;
  notes?: string | null;
}

/**
 * Asset tracking software installation DTO
 */
export interface AssetTrackingSoftwareInstallationDto {
  operatingSystem?: number | null;
  operatingSystemName?: string | null;
  microsoftOffice?: number | null;
  microsoftOfficeName?: string | null;
  visualStudio?: number | null;
  visualStudioName?: string | null;
  dbms?: number | null;
  dbmsName?: string | null;
  userPassword?: string | null;
  adminOperationalPassword?: string | null;
  connectToNAS: boolean;
}

/**
 * Asset tracking attachment DTO
 */
export interface AssetTrackingAttachmentDto {
  fileAttachmentId: number;
  fileName: string;
  originalFileName: string;
  fileUrl?: string | null;
  fileCategory?: string | null;
  isPrimary: boolean;
}

/**
 * Asset tracking schedule stats DTO
 */
export interface AssetTrackingScheduleStatsDto {
  scheduleId: number;
  scheduleDate: string;
  assetCode: string;
  assetName: string;
  employeeName?: string | null;
  status?: string | null;
  type?: string | null;
  relatedTransactionId?: number | null;
  isActive: boolean;
}

/**
 * Asset tracking transaction type stats DTO
 */
export interface AssetTrackingTransactionTypeStatsDto {
  transactionType: string;
  count: number;
  percentage: number;
}

/**
 * Asset tracking transaction type stats response
 */
export interface AssetTrackingTransactionTypeStatsResponse {
  totalTransactions: number;
  data: AssetTrackingTransactionTypeStatsDto[];
}

/**
 * Asset tracking employee history DTO
 */
export interface AssetTrackingEmployeeHistoryDto {
  employeeId: number;
  employeeCode: string;
  fullName: string;
  departmentName?: string | null;
  positionName?: string | null;
  lastTransactionDate?: string | null;
  totalTransactions: number;
  assetsHeld: number;
}

/**
 * Asset tracking grid request
 */
export interface AssetTrackingGridRequest extends GridRequestBase {
  assetId: number;
}