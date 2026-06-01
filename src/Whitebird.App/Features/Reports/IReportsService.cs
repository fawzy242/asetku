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
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="transactionType">Optional transaction type filter</param>
    /// <returns>Collection of asset transaction report view models</returns>
    Task<ServiceResult<IEnumerable<ReportsAssetTransactionViewModel>>> GetAssetTransactionReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);

    /// <summary>
    /// Gets asset inventory report data
    /// </summary>
    /// <param name="status">Optional status filter (Available, Assigned, On Loan, In Maintenance)</param>
    /// <param name="categoryId">Optional category ID filter</param>
    /// <param name="supplierId">Optional supplier ID filter</param>
    /// <returns>Collection of asset inventory report view models</returns>
    Task<ServiceResult<IEnumerable<ReportsAssetInventoryViewModel>>> GetAssetInventoryReportsAsync(
        string? status = null, int? categoryId = null, int? supplierId = null);

    /// <summary>
    /// Gets employee asset report data
    /// </summary>
    /// <param name="employeeId">Optional employee ID filter</param>
    /// <param name="department">Optional department name filter</param>
    /// <returns>Collection of employee asset report view models</returns>
    Task<ServiceResult<IEnumerable<ReportsEmployeeAssetViewModel>>> GetEmployeeAssetReportsAsync(
        int? employeeId = null, string? department = null);

    /// <summary>
    /// Gets maintenance report data
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="isUpcoming">True for upcoming maintenance, false for history</param>
    /// <returns>Collection of maintenance report view models</returns>
    Task<ServiceResult<IEnumerable<ReportsMaintenanceViewModel>>> GetMaintenanceReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);

    /// <summary>
    /// Gets financial report data
    /// </summary>
    /// <param name="startDate">Optional purchase start date filter</param>
    /// <param name="endDate">Optional purchase end date filter</param>
    /// <returns>Collection of financial report view models</returns>
    Task<ServiceResult<IEnumerable<ReportsFinancialViewModel>>> GetFinancialReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null);

    // ============================================================
    // DASHBOARD
    // ============================================================

    /// <summary>
    /// Gets dashboard statistics for the main dashboard
    /// </summary>
    /// <returns>Dashboard statistics view model</returns>
    Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync();

    // ============================================================
    // EXCEL EXPORTS
    // ============================================================

    /// <summary>
    /// Generates Excel file for asset transaction report
    /// </summary>
    /// <returns>Excel file bytes</returns>
    Task<ServiceResult<byte[]>> GenerateAssetTransactionExcelAsync(
        DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);

    /// <summary>
    /// Generates Excel file for asset inventory report
    /// </summary>
    /// <returns>Excel file bytes</returns>
    Task<ServiceResult<byte[]>> GenerateAssetInventoryExcelAsync(
        string? status = null, int? categoryId = null, int? supplierId = null);

    /// <summary>
    /// Generates Excel file for employee asset report
    /// </summary>
    /// <returns>Excel file bytes</returns>
    Task<ServiceResult<byte[]>> GenerateEmployeeAssetExcelAsync(
        int? employeeId = null, string? department = null);

    /// <summary>
    /// Generates Excel file for maintenance report
    /// </summary>
    /// <returns>Excel file bytes</returns>
    Task<ServiceResult<byte[]>> GenerateMaintenanceExcelAsync(
        DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);

    /// <summary>
    /// Generates Excel file for financial report
    /// </summary>
    /// <returns>Excel file bytes</returns>
    Task<ServiceResult<byte[]>> GenerateFinancialExcelAsync(
        DateTime? startDate = null, DateTime? endDate = null);
}