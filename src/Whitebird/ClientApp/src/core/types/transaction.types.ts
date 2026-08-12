// src/core/types/transaction.types.ts
import { AuditableEntity, GridRequestBase } from './common.types';

/**
 * Asset transaction entity
 */
export interface AssetTransactionEntity extends AuditableEntity {
  assetTransactionId: number;
  notes?: string | null;
  approved?: boolean | null;
  approvedBy?: string | null;
  completed?: boolean | null;
  completedBy?: string | null;
  assetTransactionCode?: string | null;
  transactionDate: string;
  transactionType: number;
  assetId: number;
  employeeId?: number | null;
  officeId?: number | null;
  condition?: number | null;
  fromAssetTransactionId?: number | null;
  expectedLoanReturnDate?: string | null;
  maintenanceEmployeeId?: number | null;
  maintenanceType?: number | null;
  maintenanceCost?: number | null;
}

/**
 * Asset transaction list view - for grid display
 */
export interface AssetTransactionListView extends Partial<AssetTransactionEntity> {
  assetTransactionId: number;
  assetTransactionCode?: string | null;
  assetId: number;
  assetCode?: string | null;
  assetName?: string | null;
  transactionType: number;
  transactionTypeName?: string | null;
  employeeId?: number | null;
  employeeCode?: string | null;
  employeeName?: string | null;
  officeId?: number | null;
  officeName?: string | null;
  transactionDate: string;
  expectedLoanReturnDate?: string | null;
  notes?: string | null;
  condition?: number | null;
  conditionName?: string | null;
  approved?: boolean | null;
  approvedBy?: string | null;
  completed?: boolean | null;
  completedBy?: string | null;
  maintenanceType?: number | null;
  maintenanceTypeName?: string | null;
  maintenanceCost?: number | null;
  fromAssetTransactionId?: number | null;
  isActive: boolean;
  createdDate: string;
  createdBy: string;
  modifiedDate?: string | null;
  modifiedBy?: string | null;
  // Additional computed fields
  isOverdue?: boolean;
  // Grid action flags
  canEdit?: boolean;
  canDelete?: boolean;
  canReturn?: boolean;
  canPost?: boolean;
}

/**
 * Asset transaction detail view
 */
export interface AssetTransactionDetailView extends Partial<AssetTransactionListView> {}

/**
 * Asset transaction pending approval view
 */
export interface AssetTransactionPendingApprovalView {
  assetTransactionId: number;
  assetTransactionCode?: string | null;
  assetCode?: string | null;
  assetName?: string | null;
  employeeCode?: string | null;
  employeeName?: string | null;
  transactionTypeName?: string | null;
  transactionDate: string;
  conditionName?: string | null;
  approved?: boolean | null;
  completed?: boolean | null;
  allAttachmentsApproved?: boolean;
  hasNeedToChange?: boolean;
  attachments?: ApproveAttachmentStatusDto[];
  // Grid action flags
  canEdit?: boolean;
  canDelete?: boolean;
  canReturn?: boolean;
  canPost?: boolean;
}

/**
 * Approve attachment status DTO
 */
export interface ApproveAttachmentStatusDto {
  attachmentId: number;
  documentType: number;
  documentTypeName?: string | null;
  isRequired: boolean;
  isUploaded: boolean;
  needToChange: boolean;
  approved: boolean;
  fileName?: string | null;
}

/**
 * Asset transaction create view model
 */
export interface AssetTransactionCreateViewModel {
  assetId: number;
  transactionType: number;
  employeeId?: number | null;
  officeId?: number | null;
  transactionDate: string;
  expectedLoanReturnDate?: string | null;
  notes?: string | null;
  condition?: number | null;
  maintenanceType?: number | null;
  maintenanceCost?: number | null;
  fromAssetTransactionId?: number | null;
}

/**
 * Asset transaction update view model
 */
export interface AssetTransactionUpdateViewModel {
  assetId: number;
  transactionType: number;
  employeeId?: number | null;
  officeId?: number | null;
  transactionDate: string;
  expectedLoanReturnDate?: string | null;
  notes?: string | null;
  condition?: number | null;
  approved?: boolean | null;
  approvedBy?: string | null;
  completed?: boolean | null;
  completedBy?: string | null;
  maintenanceType?: number | null;
  maintenanceCost?: number | null;
}

/**
 * Asset return view model
 */
export interface AssetReturnViewModel {
  actualReturnDate: string;
  condition?: number | null;
  notes?: string | null;
}

/**
 * Post maintenance view model
 */
export interface PostMaintenanceViewModel {
  completionDate: string;
  condition?: number | null;
  maintenanceCost?: number | null;
  notes?: string | null;
}

/**
 * Approve attachment action DTO
 */
export interface ApproveAttachmentActionDto {
  attachmentId: number;
  action: 'NeedToChange' | 'Approve';
  notes?: string | null;
}

/**
 * Required document type DTO
 */
export interface RequiredDocumentTypeDto {
  documentType: number;
  documentTypeName?: string | null;
  isRequired: boolean;
  isUploaded: boolean;
  attachmentId?: number | null;
}

/**
 * Asset transaction grid request
 */
export interface AssetTransactionGridRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
  dateFrom?: string | null;
  dateTo?: string | null;
  employeeId?: number | null;
  assetId?: number | null;
  transactionType?: number | null;
  sortBy?: string | null;
  sortDescending?: boolean;
}