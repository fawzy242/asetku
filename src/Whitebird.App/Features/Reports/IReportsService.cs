using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Reports;
using Whitebird.Domain.Features.Supplier;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Reports;

/// <summary>
/// Service interface for Reports business logic
/// </summary>
public interface IReportsService
{
    // ============================================================
    // LIST ENDPOINTS (for dropdown parameters)
    // ============================================================

    Task<ServiceResult<IEnumerable<EmployeeDropdownView>>> GetEmployeeListAsync();
    Task<ServiceResult<IEnumerable<CategoryDropdownView>>> GetCategoryListAsync();
    Task<ServiceResult<IEnumerable<SupplierDropdownView>>> GetSupplierListAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypeListAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMaintenanceTypeListAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetConditionListAsync();
    Task<ServiceResult<IEnumerable<StatusListDto>>> GetStatusListAsync();

    // ============================================================
    // EXPORT ENDPOINTS
    // ============================================================

    Task<ServiceResult<byte[]>> ExportAssetTransactionAsync(string format, DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null);
    Task<ServiceResult<byte[]>> ExportAssetInventoryAsync(string format, string? status = null, int? categoryId = null, int? supplierId = null);
    Task<ServiceResult<byte[]>> ExportEmployeeAssetAsync(string format, int? employeeId = null);
    Task<ServiceResult<byte[]>> ExportMaintenanceAsync(string format, DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null);
    Task<ServiceResult<byte[]>> ExportFinancialAsync(string format, DateTime? startDate = null, DateTime? endDate = null);
}