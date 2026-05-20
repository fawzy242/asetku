using Whitebird.Domain.Features.Reports;

namespace Whitebird.Infra.Features.Reports;

public interface IReportsReps
{
    // Report data
    Task<IEnumerable<ReportsAssetTransactionViewModel>> GetAssetTransactionReportsAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);
    Task<IEnumerable<ReportsAssetInventoryViewModel>> GetAssetInventoryReportsAsync(string? status = null, int? categoryId = null, int? supplierId = null);
    Task<IEnumerable<ReportsEmployeeAssetViewModel>> GetEmployeeAssetReportsAsync(int? employeeId = null, string? department = null);
    Task<IEnumerable<ReportsMaintenanceViewModel>> GetMaintenanceReportsAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);
    Task<IEnumerable<ReportsFinancialViewModel>> GetFinancialReportsAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Dashboard stats
    Task<DashboardStatsViewModel> GetDashboardStatsAsync();
    Task<int> GetPendingApprovalsCountAsync();
    Task<int> GetActiveEmployeesCountAsync();
    Task<int> GetTotalOfficesCountAsync();
    Task<int> GetTotalDepartmentsCountAsync();
}