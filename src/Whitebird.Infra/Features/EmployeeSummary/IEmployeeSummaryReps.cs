using Whitebird.Domain.Features.EmployeeSummary;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.EmployeeSummary;

/// <summary>
/// Repository interface for Employee Summary operations
/// </summary>
public interface IEmployeeSummaryReps
{
    // ============================================================
    // LOOKUP
    // ============================================================

    Task<EmployeeSummaryGetDataDto?> GetEmployeeDataAsync(int employeeId);

    // ============================================================
    // COMPLETE SUMMARY
    // ============================================================

    Task<EmployeeSummaryView?> GetSummaryAsync(int employeeId);

    // ============================================================
    // STATISTICS
    // ============================================================

    Task<EmployeeSummaryStatsDto> GetStatsAsync(int employeeId);

    // ============================================================
    // CURRENT HOLDING - Using GridRequest
    // ============================================================

    Task<PaginatedResult<EmployeeCurrentHoldingDto>> GetCurrentHoldingAsync(EmployeeSummaryGridRequest request);

    // ============================================================
    // ASSETS HISTORY - Using GridRequest
    // ============================================================

    Task<PaginatedResult<EmployeeAssetsHistoryDto>> GetAssetsHistoryAsync(EmployeeSummaryGridRequest request);

    // ============================================================
    // TRANSACTION HISTORY - Using GridRequest
    // ============================================================

    Task<PaginatedResult<EmployeeTransactionHistoryDto>> GetTransactionHistoryAsync(EmployeeSummaryGridRequest request);

    // ============================================================
    // CURRENT HOLDING STATS (Category breakdown)
    // ============================================================

    Task<EmployeeCategoryStatsResponse> GetCurrentHoldingStatsAsync(int employeeId);

    // ============================================================
    // TRANSACTION STATS
    // ============================================================

    Task<EmployeeTransactionStatsResponse> GetTransactionStatsAsync(int employeeId);

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    Task<EmployeeScheduleStatsDto> GetScheduleStatsAsync(int employeeId);

    // ============================================================
    // ATTACHMENTS
    // ============================================================

    Task<IEnumerable<EmployeeSummaryAttachmentDto>> GetAttachmentsByEmployeeIdAsync(int employeeId);

    // ============================================================
    // PHOTO PROFILE
    // ============================================================

    Task<EmployeeSummaryAttachmentDto?> GetPhotoProfileByEmployeeIdAsync(int employeeId);
}