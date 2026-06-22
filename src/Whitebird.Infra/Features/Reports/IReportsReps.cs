using Whitebird.Domain.Features.Reports;

namespace Whitebird.Infra.Features.Reports;

/// <summary>
/// Repository interface for Reports operations
/// </summary>
public interface IReportsReps
{
    // Report data
    Task<IEnumerable<ReportsAssetTransactionViewModel>> GetAssetTransactionReportsAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);
    Task<IEnumerable<ReportsAssetInventoryViewModel>> GetAssetInventoryReportsAsync(string? status = null, int? categoryId = null, int? supplierId = null);
    Task<IEnumerable<ReportsEmployeeAssetViewModel>> GetEmployeeAssetReportsAsync(int? employeeId = null);
    Task<IEnumerable<ReportsMaintenanceViewModel>> GetMaintenanceReportsAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);
    Task<IEnumerable<ReportsFinancialViewModel>> GetFinancialReportsAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Dashboard stats
    Task<DashboardStatsViewModel> GetDashboardStatsAsync();
    Task<int> GetPendingApprovalsCountAsync();
    Task<int> GetActiveEmployeesCountAsync();
    Task<int> GetTotalOfficesCountAsync();
    Task<int> GetTotalDepartmentsCountAsync();

    // NEW: Monthly stats for dashboard
    Task<IEnumerable<MonthlyStatDto>> GetMonthlyStatsAsync(int year);
    Task<IEnumerable<CategoryBreakdownDto>> GetCategoryBreakdownAsync();

    // Excel export (return raw data for client-side export)
    Task<IEnumerable<ReportsAssetTransactionViewModel>> ExportAssetTransactionReportsAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);
    Task<IEnumerable<ReportsAssetInventoryViewModel>> ExportAssetInventoryReportsAsync(string? status = null, int? categoryId = null, int? supplierId = null);
    Task<IEnumerable<ReportsEmployeeAssetViewModel>> ExportEmployeeAssetReportsAsync(int? employeeId = null);
    Task<IEnumerable<ReportsMaintenanceViewModel>> ExportMaintenanceReportsAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);
    Task<IEnumerable<ReportsFinancialViewModel>> ExportFinancialReportsAsync(DateTime? startDate = null, DateTime? endDate = null);
}