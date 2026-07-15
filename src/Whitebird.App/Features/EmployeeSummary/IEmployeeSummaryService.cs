using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.EmployeeSummary;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.EmployeeSummary;

/// <summary>
/// Service interface for Employee Summary business logic
/// </summary>
public interface IEmployeeSummaryService
{
    // Lookup/Dropdown
    Task<ServiceResult<EmployeeSummaryGetDataDto>> GetEmployeeDataAsync(int employeeId);

    // Complete Summary
    Task<ServiceResult<EmployeeSummaryView>> GetSummaryAsync(int employeeId);

    // Statistics
    Task<ServiceResult<EmployeeSummaryStatsDto>> GetStatsAsync(int employeeId);

    // Current Holding
    Task<ServiceResult<PaginatedResult<EmployeeCurrentHoldingDto>>> GetCurrentHoldingAsync(
        int employeeId, EmployeeSummaryRequest request);

    // Assets History
    Task<ServiceResult<PaginatedResult<EmployeeAssetsHistoryDto>>> GetAssetsHistoryAsync(
        int employeeId, EmployeeSummaryRequest request);

    // Transaction History
    Task<ServiceResult<PaginatedResult<EmployeeTransactionHistoryDto>>> GetTransactionHistoryAsync(
        int employeeId, EmployeeSummaryRequest request);

    // Current Holding Stats (Category breakdown)
    Task<ServiceResult<EmployeeCategoryStatsResponse>> GetCurrentHoldingStatsAsync(int employeeId);

    // Transaction Stats
    Task<ServiceResult<EmployeeTransactionStatsResponse>> GetTransactionStatsAsync(int employeeId);

    // Schedule Stats
    Task<ServiceResult<EmployeeScheduleStatsDto>> GetScheduleStatsAsync(int employeeId);
}