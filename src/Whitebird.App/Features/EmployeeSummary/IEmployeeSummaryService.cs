using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.EmployeeSummary;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.EmployeeSummary;

/// <summary>
/// Service interface for Employee Summary business logic
/// </summary>
public interface IEmployeeSummaryService
{
    // ============================================================
    // LOOKUP
    // ============================================================

    Task<ServiceResult<EmployeeSummaryGetDataDto>> GetEmployeeDataAsync(int employeeId);

    // ============================================================
    // COMPLETE SUMMARY
    // ============================================================

    Task<ServiceResult<EmployeeSummaryView>> GetSummaryAsync(int employeeId);

    // ============================================================
    // STATISTICS
    // ============================================================

    Task<ServiceResult<EmployeeSummaryStatsDto>> GetStatsAsync(int employeeId);

    // ============================================================
    // CURRENT HOLDING - Using GridRequest
    // ============================================================

    Task<ServiceResult<PaginatedResult<EmployeeCurrentHoldingDto>>> GetCurrentHoldingAsync(EmployeeSummaryGridRequest request);

    // ============================================================
    // ASSETS HISTORY - Using GridRequest
    // ============================================================

    Task<ServiceResult<PaginatedResult<EmployeeAssetsHistoryDto>>> GetAssetsHistoryAsync(EmployeeSummaryGridRequest request);

    // ============================================================
    // TRANSACTION HISTORY - Using GridRequest
    // ============================================================

    Task<ServiceResult<PaginatedResult<EmployeeTransactionHistoryDto>>> GetTransactionHistoryAsync(EmployeeSummaryGridRequest request);

    // ============================================================
    // CURRENT HOLDING STATS (Category breakdown)
    // ============================================================

    Task<ServiceResult<EmployeeCategoryStatsResponse>> GetCurrentHoldingStatsAsync(int employeeId);

    // ============================================================
    // TRANSACTION STATS
    // ============================================================

    Task<ServiceResult<EmployeeTransactionStatsResponse>> GetTransactionStatsAsync(int employeeId);

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    Task<ServiceResult<EmployeeScheduleStatsDto>> GetScheduleStatsAsync(int employeeId);
}