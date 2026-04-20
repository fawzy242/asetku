using Whitebird.App.Features.Common.Service;
using Whitebird.Domain.Features.Reports.View;

namespace Whitebird.App.Features.Reports.Interfaces;

public interface IReportsService
{
    Task<ServiceResult<IEnumerable<ReportsAssetTransactionViewModel>>> GetAssetTransactionReportsAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);
    Task<ServiceResult<IEnumerable<ReportsAssetInventoryViewModel>>> GetAssetInventoryReportsAsync(string? status = null, int? categoryId = null, int? supplierId = null);
    Task<ServiceResult<IEnumerable<ReportsEmployeeAssetViewModel>>> GetEmployeeAssetReportsAsync(int? employeeId = null, string? department = null);
    Task<ServiceResult<IEnumerable<ReportsMaintenanceViewModel>>> GetMaintenanceReportsAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);
    Task<ServiceResult<IEnumerable<ReportsFinancialViewModel>>> GetFinancialReportsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync();
    Task<ServiceResult<byte[]>> GenerateAssetTransactionExcelAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);
    Task<ServiceResult<byte[]>> GenerateAssetInventoryExcelAsync(string? status = null, int? categoryId = null, int? supplierId = null);
    Task<ServiceResult<byte[]>> GenerateEmployeeAssetExcelAsync(int? employeeId = null, string? department = null);
    Task<ServiceResult<byte[]>> GenerateMaintenanceExcelAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);
    Task<ServiceResult<byte[]>> GenerateFinancialExcelAsync(DateTime? startDate = null, DateTime? endDate = null);
}