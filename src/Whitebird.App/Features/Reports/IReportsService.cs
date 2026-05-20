using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Reports;

namespace Whitebird.App.Features.Reports;

public interface IReportsService
{
    // Data queries
    Task<ServiceResult<IEnumerable<ReportsAssetTransactionViewModel>>> GetAssetTransactionReportsAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);
    Task<ServiceResult<IEnumerable<ReportsAssetInventoryViewModel>>> GetAssetInventoryReportsAsync(string? status = null, int? categoryId = null, int? supplierId = null);
    Task<ServiceResult<IEnumerable<ReportsEmployeeAssetViewModel>>> GetEmployeeAssetReportsAsync(int? employeeId = null, string? department = null);
    Task<ServiceResult<IEnumerable<ReportsMaintenanceViewModel>>> GetMaintenanceReportsAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);
    Task<ServiceResult<IEnumerable<ReportsFinancialViewModel>>> GetFinancialReportsAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Dashboard
    Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync();

    // Excel exports
    Task<ServiceResult<byte[]>> GenerateAssetTransactionExcelAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);
    Task<ServiceResult<byte[]>> GenerateAssetInventoryExcelAsync(string? status = null, int? categoryId = null, int? supplierId = null);
    Task<ServiceResult<byte[]>> GenerateEmployeeAssetExcelAsync(int? employeeId = null, string? department = null);
    Task<ServiceResult<byte[]>> GenerateMaintenanceExcelAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);
    Task<ServiceResult<byte[]>> GenerateFinancialExcelAsync(DateTime? startDate = null, DateTime? endDate = null);
}