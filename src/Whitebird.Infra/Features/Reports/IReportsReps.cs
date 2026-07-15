using Whitebird.Domain.Features.Reports;

namespace Whitebird.Infra.Features.Reports;

/// <summary>
/// Repository interface for Reports operations
/// </summary>
public interface IReportsReps
{
    // Data queries for export
    Task<IEnumerable<ReportsAssetTransactionViewModel>> GetAssetTransactionDataAsync(
        DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);

    Task<IEnumerable<ReportsAssetInventoryViewModel>> GetAssetInventoryDataAsync(
        string? status = null, int? categoryId = null, int? supplierId = null);

    Task<IEnumerable<ReportsEmployeeAssetViewModel>> GetEmployeeAssetDataAsync(
        int? employeeId = null);

    Task<IEnumerable<ReportsMaintenanceViewModel>> GetMaintenanceDataAsync(
        DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);

    Task<IEnumerable<ReportsFinancialViewModel>> GetFinancialDataAsync(
        DateTime? startDate = null, DateTime? endDate = null);
}