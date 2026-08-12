// src/core/types/employeeSummary.types.ts
import { PaginatedResult, GridRequestBase } from './common.types';

/**
 * Employee summary get data DTO - for dropdown
 */
export interface EmployeeSummaryGetDataDto {
  employeeId: number;
  employeeCode: string;
  fullName: string;
  departmentName?: string | null;
  isActive: boolean;
}

/**
 * Employee summary stats DTO
 */
export interface EmployeeSummaryStatsDto {
  currentlyHeldAssets: number;
  assetsOnLoan: number;
  overdueLoans: number;
  totalHistoricalAssets: number;
  returnedAssets: number;
  damagedReturns: number;
}

/**
 * Employee current holding DTO
 */
export interface EmployeeCurrentHoldingDto {
  assetId: number;
  assetCode: string;
  assetName: string;
  categoryName: string;
  status: string;
  associationType: string;
  sinceDate: string;
  expectedReturnDate?: string | null;
  isOverdue: boolean;
  conditionName?: string | null;
}

/**
 * Employee assets history DTO
 */
export interface EmployeeAssetsHistoryDto {
  assetId: number;
  assetCode: string;
  assetName: string;
  categoryName: string;
  lastHeldDate: string;
  currentHolderName?: string | null;
  currentHolderId?: number | null;
  conditionName?: string | null;
}

/**
 * Employee transaction history DTO
 */
export interface EmployeeTransactionHistoryDto {
  assetTransactionId: number;
  assetId: number;
  assetCode: string;
  assetName: string;
  transactionTypeName: string;
  transactionDate: string;
  fromEmployeeName?: string | null;
  toEmployeeName?: string | null;
  conditionAfterName?: string | null;
  notes?: string | null;
}

/**
 * Employee category stats DTO
 */
export interface EmployeeCategoryStatsDto {
  category: string;
  count: number;
  percentage: number;
}

/**
 * Employee category stats response
 */
export interface EmployeeCategoryStatsResponse {
  totalAssets: number;
  data: EmployeeCategoryStatsDto[];
}

/**
 * Employee transaction stats DTO
 */
export interface EmployeeTransactionStatsDto {
  transactionType: string;
  count: number;
  percentage: number;
}

/**
 * Employee transaction stats response
 */
export interface EmployeeTransactionStatsResponse {
  totalTransactions: number;
  data: EmployeeTransactionStatsDto[];
}

/**
 * Employee schedule stats DTO
 */
export interface EmployeeScheduleStatsDto {
  totalHandover: number;
  totalReturn: number;
  upcomingHandover: number;
  upcomingReturn: number;
  handoverList: EmployeeScheduleItemDto[];
  returnList: EmployeeScheduleItemDto[];
}

/**
 * Employee schedule item DTO
 */
export interface EmployeeScheduleItemDto {
  scheduleId: number;
  scheduleDate: string;
  assetCode: string;
  assetName: string;
  status: string;
}

/**
 * Employee summary view - complete detail
 */
export interface EmployeeSummaryView {
  // Employee Info
  employeeId: number;
  employeeCode: string;
  fullName: string;
  departmentName?: string | null;
  positionName?: string | null;
  employmentStatusName?: string | null;
  joinDate?: string | null;
  resignDate?: string | null;
  isActive: boolean;
  // Photo Profile
  photoProfileUrl?: string | null;
  // Attachments
  attachments: EmployeeSummaryAttachmentDto[];
  // Statistics
  stats: EmployeeSummaryStatsDto;
  // Current Holding
  currentHolding: EmployeeCurrentHoldingDto[];
  // Assets History
  assetsHistory: EmployeeAssetsHistoryDto[];
  // Transaction History
  transactionHistory: EmployeeTransactionHistoryDto[];
}

/**
 * Employee summary attachment DTO
 */
export interface EmployeeSummaryAttachmentDto {
  fileAttachmentId: number;
  fileName: string;
  originalFileName: string;
  fileUrl?: string | null;
  fileCategory?: string | null;
  isPrimary: boolean;
}

/**
 * Employee summary grid request
 */
export interface EmployeeSummaryGridRequest extends GridRequestBase {
  employeeId: number;
}