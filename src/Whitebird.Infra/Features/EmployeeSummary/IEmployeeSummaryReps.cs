using Whitebird.Domain.Features.EmployeeSummary;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.EmployeeSummary;

/// <summary>
/// Repository interface for Employee Summary operations
/// </summary>
public interface IEmployeeSummaryReps
{
    // Lookup/Dropdown
    Task<EmployeeSummaryGetDataDto?> GetEmployeeDataAsync(int employeeId);

    // Complete Summary
    Task<EmployeeSummaryView?> GetSummaryAsync(int employeeId);

    // Statistics
    Task<EmployeeSummaryStatsDto> GetStatsAsync(int employeeId);

    // Current Holding
    Task<PaginatedResult<EmployeeCurrentHoldingDto>> GetCurrentHoldingAsync(
        int employeeId, int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false);

    // Assets History (previously held, now with another employee)
    Task<PaginatedResult<EmployeeAssetsHistoryDto>> GetAssetsHistoryAsync(
        int employeeId, int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false);

    // Transaction History
    Task<PaginatedResult<EmployeeTransactionHistoryDto>> GetTransactionHistoryAsync(
        int employeeId, int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false);

    // Current Holding Stats (Category breakdown)
    Task<EmployeeCategoryStatsResponse> GetCurrentHoldingStatsAsync(int employeeId);

    // Transaction Stats
    Task<EmployeeTransactionStatsResponse> GetTransactionStatsAsync(int employeeId);

    // Schedule Stats
    Task<EmployeeScheduleStatsDto> GetScheduleStatsAsync(int employeeId);

    // Attachments
    Task<IEnumerable<EmployeeSummaryAttachmentDto>> GetAttachmentsByEmployeeIdAsync(int employeeId);

    // Photo Profile
    Task<EmployeeSummaryAttachmentDto?> GetPhotoProfileByEmployeeIdAsync(int employeeId);
}