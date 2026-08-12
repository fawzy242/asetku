// src/core/types/dashboard.types.ts

/**
 * Dashboard stats view model
 */
export interface DashboardStatsViewModel {
  // Asset counts
  totalAssets: number;
  availableAssets: number;
  assignedAssets: number;
  assetsOnLoan: number;
  assetsInMaintenance: number;
  disposedAssets: number;
  damagedAssets: number;
  // Asset alerts
  expiredWarrantyCount: number;
  overdueLoanCount: number;
  // Financial
  totalAssetValue: number;
  // Employee counts
  totalEmployees: number;
  activeEmployees: number;
  // Transaction stats
  pendingApprovals: number;
  approvedTransactions: number;
  rejectedTransactions: number;
  last30DaysTransactions: number;
  // Organization stats
  totalOffices: number;
  totalDepartments: number;
  // Derived metrics
  assetUtilizationRate: number;
  assetAvailabilityRate: number;
  averageAssetValue: number;
}

/**
 * Monthly stat DTO
 */
export interface MonthlyStatDto {
  month: number;
  transactionCount: number;
  uniqueAssetsCount: number;
}

/**
 * Category breakdown DTO
 */
export interface CategoryBreakdownDto {
  category: string;
  assetCount: number;
  totalValue: number;
}

/**
 * Recent transaction DTO
 */
export interface RecentTransactionDto {
  assetTransactionId: number;
  assetCode: string;
  assetName: string;
  transactionType: number;
  transactionTypeName: string;
  fromEmployeeName?: string | null;
  toEmployeeName?: string | null;
  transactionDate: string;
  approved?: boolean | null;
  expectedReturnDate?: string | null;
  notes?: string | null;
}

/**
 * Expired warranty DTO
 */
export interface ExpiredWarrantyDto {
  assetId: number;
  assetCode: string;
  assetName: string;
  categoryName?: string | null;
  purchaseDate?: string | null;
  warrantyExpiryDate?: string | null;
  supplierName?: string | null;
  currentStatus?: string | null;
}

/**
 * Schedule stats DTO
 */
export interface ScheduleStatsDto {
  totalHandover: number;
  totalReturn: number;
  totalMaintenance: number;
  upcomingHandover: number;
  upcomingReturn: number;
  upcomingMaintenance: number;
}

/**
 * Asset detail DTO - for drilldown
 */
export interface AssetDetailDto {
  assetId: number;
  assetCode: string;
  assetName: string;
  categoryName?: string | null;
  brand?: string | null;
  model?: string | null;
  serialNumber?: string | null;
  currentStatus?: string | null;
  currentHolderName?: string | null;
  purchaseDate?: string | null;
  purchasePrice?: number | null;
  warrantyExpiryDate?: string | null;
  isActive: boolean;
}

/**
 * Overdue loan detail DTO
 */
export interface OverdueLoanDetailDto {
  assetTransactionId: number;
  assetId: number;
  assetCode: string;
  assetName: string;
  employeeName: string;
  transactionDate: string;
  expectedLoanReturnDate: string;
  daysOverdue: number;
  notes?: string | null;
}

/**
 * Dashboard detail stats request
 */
export interface DashboardDetailStatsRequest extends GridRequestBase {
  type: 'Available' | 'Assigned' | 'OnLoan' | 'InMaintenance' | 'Disposed' | 'Damaged' | 'ExpiredWarranty' | 'OverdueLoans';
}

/**
 * Detail stats response
 */
export interface DetailStatsResponse {
  type: string;
  title: string;
  totalCount: number;
  data: PaginatedResult<AssetDetailDto>;
}