using Whitebird.Domain.Features.Reports.View;

namespace Whitebird.Infra.Features.Reports;

public interface IReportsReps
{
    Task<IEnumerable<ReportsAssetTransactionViewModel>> GetAssetTransactionReportsAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);
    Task<IEnumerable<ReportsAssetInventoryViewModel>> GetAssetInventoryReportsAsync(string? status = null, int? categoryId = null, int? supplierId = null);
    Task<IEnumerable<ReportsEmployeeAssetViewModel>> GetEmployeeAssetReportsAsync(int? employeeId = null, string? department = null);
    Task<IEnumerable<ReportsMaintenanceViewModel>> GetMaintenanceReportsAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);
    Task<IEnumerable<ReportsFinancialViewModel>> GetFinancialReportsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<DashboardStatsViewModel> GetDashboardStatsAsync();
}