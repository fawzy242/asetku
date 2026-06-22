using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Reports;

namespace Whitebird.App.Features.Reports;

/// <summary>
/// Service interface for Reports business logic
/// </summary>
public interface IReportsService
{
    // ============================================================
    // DATA QUERIES
    // ============================================================

    /// <summary>
    /// Gets asset transaction report data
    /// </summary>
    Task<ServiceResult<IEnumerable<ReportsAssetTransactionViewModel>>> GetAssetTransactionReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);

    /// <summary>
    /// Gets asset inventory report data
    /// </summary>
    Task<ServiceResult<IEnumerable<ReportsAssetInventoryViewModel>>> GetAssetInventoryReportsAsync(
        string? status = null, int? categoryId = null, int? supplierId = null);

    /// <summary>
    /// Gets employee asset report data
    /// </summary>
    Task<ServiceResult<IEnumerable<ReportsEmployeeAssetViewModel>>> GetEmployeeAssetReportsAsync(
        int? employeeId = null);

    /// <summary>
    /// Gets maintenance report data
    /// </summary>
    Task<ServiceResult<IEnumerable<ReportsMaintenanceViewModel>>> GetMaintenanceReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);

    /// <summary>
    /// Gets financial report data
    /// </summary>
    Task<ServiceResult<IEnumerable<ReportsFinancialViewModel>>> GetFinancialReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null);

    // ============================================================
    // DASHBOARD
    // ============================================================

    /// <summary>
    /// Gets dashboard statistics for the main dashboard
    /// </summary>
    Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync();

    /// <summary>
    /// Gets monthly statistics for dashboard chart
    /// </summary>
    Task<ServiceResult<IEnumerable<MonthlyStatDto>>> GetMonthlyStatsAsync(int year);

    /// <summary>
    /// Gets category breakdown for dashboard chart
    /// </summary>
    Task<ServiceResult<IEnumerable<CategoryBreakdownDto>>> GetCategoryBreakdownAsync();

    // ============================================================
    // EXCEL EXPORTS
    // ============================================================

    /// <summary>
    /// Generates Excel file for asset transaction report
    /// </summary>
    Task<ServiceResult<byte[]>> GenerateAssetTransactionExcelAsync(
        DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);

    /// <summary>
    /// Generates Excel file for asset inventory report
    /// </summary>
    Task<ServiceResult<byte[]>> GenerateAssetInventoryExcelAsync(
        string? status = null, int? categoryId = null, int? supplierId = null);

    /// <summary>
    /// Generates Excel file for employee asset report
    /// </summary>
    Task<ServiceResult<byte[]>> GenerateEmployeeAssetExcelAsync(
        int? employeeId = null);

    /// <summary>
    /// Generates Excel file for maintenance report
    /// </summary>
    Task<ServiceResult<byte[]>> GenerateMaintenanceExcelAsync(
        DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);

    /// <summary>
    /// Generates Excel file for financial report
    /// </summary>
    Task<ServiceResult<byte[]>> GenerateFinancialExcelAsync(
        DateTime? startDate = null, DateTime? endDate = null);
}